namespace NServiceBus.InMemory.SubscriptionStorage
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using NServiceBus.Unicast.Subscriptions;
    using NServiceBus.Unicast.Subscriptions.MessageDrivenSubscriptions;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;
    using NServiceBus.ServiceFabric.Persistence;


    /// <summary>
    ///     In memory implementation of the subscription storage
    /// </summary>
    class ServiceFabricSubscriptionStorage : ISubscriptionStorage
    {
        private ServiceFabricStorageContext _context;
        public ServiceFabricSubscriptionStorage(ServiceFabricStorageContext context)
        {
            _context = context;
            storage = context.StateManager.GetOrAddAsync<IReliableDictionary<string, ConcurrentDictionary<Address, object>>>("nservicebusSubscription").Result;
        }

        void ISubscriptionStorage.Subscribe(Address address, IEnumerable<MessageType> messageTypes)
        {
            using (ITransaction tx = _context.StateManager.CreateTransaction())
            {
                foreach (var m in messageTypes)
                {
                    var dict = storage.GetOrAddAsync(tx, m.ToString(), type => new ConcurrentDictionary<Address, object>()).Result;

                    dict.AddOrUpdate(address, addValueFactory, updateValueFactory);

                    storage.AddOrUpdateAsync(tx, m.ToString(), type => new ConcurrentDictionary<Address, object>(), (t, o) => dict).Wait();
                }
                tx.CommitAsync().Wait();
            }
        }

        void ISubscriptionStorage.Unsubscribe(Address address, IEnumerable<MessageType> messageTypes)
        {
            using (ITransaction tx = _context.StateManager.CreateTransaction())
            {
                foreach (var m in messageTypes)
                {
                    ConcurrentDictionary<Address, object> dict;
                    var result = storage.TryGetValueAsync(tx, m.ToString()).Result;
                    if (result.HasValue)
                    {
                        dict = result.Value;
                        object _;
                        dict.TryRemove(address, out _);
                    }
                }
                tx.CommitAsync().Wait();
            }
        }

        IEnumerable<Address> ISubscriptionStorage.GetSubscriberAddressesForMessage(IEnumerable<MessageType> messageTypes)
        {
            var result = new HashSet<Address>();
            using (ITransaction tx = _context.StateManager.CreateTransaction())
            {
                foreach (var m in messageTypes)
                {
                    ConcurrentDictionary<Address, object> list;
                    var r = storage.TryGetValueAsync(tx, m.ToString()).Result;
                    if (r.HasValue)
                    {
                        list = r.Value;
                        result.UnionWith(list.Keys);
                    }
                }
            }
            return result;
        }

        public void Init()
        {
        }

        IReliableDictionary<string, ConcurrentDictionary<Address, object>> storage;
        Func<Address, object> addValueFactory = a => null;
        Func<Address, object, object> updateValueFactory = (a, o) => null;
    }
}