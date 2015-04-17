namespace NServiceBus.ServiceFabric.Outbox
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using NServiceBus.Outbox;
    using System.Runtime.Serialization;
    using NServiceBus.Serializers.Json;
    using System.Fabric.Data.Collections;
    using System.Fabric.Data;
    using NServiceBus.ServiceFabric.Persistence;

    class ServiceFabricOutboxStorage : IOutboxStorage
    {
        private ServiceFabricStorageContext _context;

        public ServiceFabricOutboxStorage(/*SagaMetaModel sagaModel,*/ ServiceFabricStorageContext context)
        {
            //this.sagaModel = sagaModel;
            _context = context;
            storage = _context.StateManager.GetOrAddAsync<IReliableDictionary<string, StoredMessage>>("nservicebusOutbox").Result;
        }

        public bool TryGet(string messageId, out OutboxMessage message)
        {
            StoredMessage storedMessage;
            message = null;

            var res = storage.TryGetValueAsync(_context.CurrentTransaction, messageId).Result;
            if (!res.HasValue)
            {
                return false;
            }

            storedMessage = res.Value;
            message = new OutboxMessage(messageId);
            message.TransportOperations.AddRange(ConvertStringToObject(storedMessage.TransportOperations));
                       
            return true;
        }

        public void Store(string messageId, IEnumerable<TransportOperation> transportOperations)
        {           
            if (!storage.TryAddAsync(_context.CurrentTransaction, messageId, new StoredMessage(messageId, ConvertObjectToString(transportOperations.ToList()))).Result)
            {
                throw new Exception(string.Format("Outbox message with id '{0}' is already present in storage.", messageId));
            }             
            
        }

        public void SetAsDispatched(string messageId)
        {
            var tx = _context.CurrentTransaction;
            StoredMessage storedMessage;

            var res = storage.TryGetValueAsync(tx, messageId).Result;
            if (!res.HasValue)
            {
                return;
            }

            storedMessage = res.Value;

            storedMessage.TransportOperations = string.Empty;
            storedMessage.Dispatched = true;

            storage.AddOrUpdateAsync(tx, messageId, id => storedMessage, (id, original) => storedMessage).Wait();
        }

        IReliableDictionary<string, StoredMessage> storage;

        [DataContractAttribute]
        class StoredMessage
        {
            public StoredMessage(string messageId, string transportOperations)
            {
                TransportOperations = transportOperations;
                Id = messageId;
                StoredAt = DateTime.UtcNow;
            }

            [DataMember]
            public string Id { get; private set; }

            [DataMember]
            public bool Dispatched { get; set; }

            [DataMember]
            public DateTime StoredAt { get; set; }

            [DataMember]
            public string TransportOperations { get; set; }

            protected bool Equals(StoredMessage other)
            {
                return string.Equals(Id, other.Id) && Dispatched.Equals(other.Dispatched);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }
                if (ReferenceEquals(this, obj))
                {
                    return true;
                }
                if (obj.GetType() != GetType())
                {
                    return false;
                }
                return Equals((StoredMessage)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Id != null ? Id.GetHashCode() : 0) * 397) ^ Dispatched.GetHashCode();
                }
            }

        }

        public void RemoveEntriesOlderThan(DateTime dateTime)
        {
            using (ITransaction tx = _context.StateManager.CreateTransaction())
            {
                var entriesToRemove = storage
               .Where(e => e.Value.Dispatched && e.Value.StoredAt < dateTime)
               .Select(e => e.Key)
               .ToList();

                foreach (var entry in entriesToRemove)
                {
                    storage.TryRemoveAsync(tx, entry).Wait();
                }

                tx.CommitAsync().Wait();
            }

           
        }

        static IEnumerable<TransportOperation> ConvertStringToObject(string data)
        {
            if (String.IsNullOrEmpty(data))
            {
                return Enumerable.Empty<TransportOperation>();
            }

            return (IEnumerable<TransportOperation>)serializer.DeserializeObject(data, typeof(IEnumerable<TransportOperation>));
        }

        static string ConvertObjectToString(IEnumerable<TransportOperation> operations)
        {
            if (operations == null || !operations.Any())
            {
                return null;
            }

            return serializer.SerializeObject(operations);
        }

        static readonly JsonMessageSerializer serializer = new JsonMessageSerializer(null);
    }
}