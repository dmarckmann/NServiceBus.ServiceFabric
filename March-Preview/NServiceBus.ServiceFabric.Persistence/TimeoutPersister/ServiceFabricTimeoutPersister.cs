namespace NServiceBus.ServiceFabric.TimeoutPersister
{
    using NServiceBus.ServiceFabric.Persistence;
    using System;
    using System.Collections.Generic;
    using System.Fabric.Data;
    using System.Fabric.Data.Collections;
    using System.Threading;
    using Timeout.Core;

    class ServiceFabricTimeoutPersister : IPersistTimeouts
    {
        ServiceFabricStorageContext _context;
        IReliableDictionary<string, List<TimeoutData>> storages;
        ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim();

        public string EndpointName { get; set; }
        
        public ServiceFabricTimeoutPersister(ServiceFabricStorageContext context)
        {
            _context = context;
            storages = context.StateManager.GetOrAddAsync<IReliableDictionary<string, List<TimeoutData>>>("nservicebusTimeout").Result;
        }

        public IEnumerable<Tuple<string, DateTime>> GetNextChunk(DateTime startSlice, out DateTime nextTimeToRunQuery)
        {
            var now = DateTime.UtcNow;
            nextTimeToRunQuery = DateTime.MaxValue;

            var tuples = new List<Tuple<string, DateTime>>();

            try
            {
                readerWriterLock.EnterReadLock();

                using (ITransaction tx = _context.StateManager.CreateTransaction())
                {
                    var storage = storages.GetOrAddAsync(tx, EndpointName, type => new List<TimeoutData>()).Result;

                    foreach (var data in storage)
                    {
                        if (data.Time > now && data.Time < nextTimeToRunQuery)
                        {
                            nextTimeToRunQuery = data.Time;
                        }
                        if (data.Time > startSlice && data.Time <= now)
                        {
                            tuples.Add(new Tuple<string, DateTime>(data.Id, data.Time));
                        }
                    }
                }
            }
            finally
            {
                readerWriterLock.ExitReadLock();
            }
            if (nextTimeToRunQuery == DateTime.MaxValue)
            {
                nextTimeToRunQuery = now.AddMinutes(1);
            }
            return tuples;
        }

        public void Add(TimeoutData timeout)
        {
            timeout.Id = Guid.NewGuid().ToString();
            try
            {
                readerWriterLock.EnterWriteLock();

                using (ITransaction tx = _context.StateManager.CreateTransaction())
                {
                    var storage = storages.GetOrAddAsync(tx, EndpointName, type => new List<TimeoutData>()).Result;

                    storage.Add(timeout);

                    storages.AddOrUpdateAsync(tx, EndpointName, type => new List<TimeoutData>(), (t, o) => storage).Wait();

                    tx.CommitAsync().Wait();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                readerWriterLock.ExitWriteLock();
            }
        }

        public bool TryRemove(string timeoutId, out TimeoutData timeoutData)
        {
            try
            {
                bool result = false;
                timeoutData = null;

                readerWriterLock.EnterWriteLock();

                using (ITransaction tx = _context.StateManager.CreateTransaction())
                {
                    var storage = storages.GetOrAddAsync(tx, EndpointName, type => new List<TimeoutData>()).Result;

                    for (var index = 0; index < storage.Count; index++)
                    {
                        var data = storage[index];
                        if (data.Id == timeoutId)
                        {
                            timeoutData = data;
                            storage.RemoveAt(index);
                            result = true;
                            break;
                        }
                    }

                    storages.AddOrUpdateAsync(tx, EndpointName, type => new List<TimeoutData>(), (t, o) => storage).Wait();

                    tx.CommitAsync().Wait();                    
                }
                
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                readerWriterLock.ExitWriteLock();
            }
        }

        public void RemoveTimeoutBy(Guid sagaId)
        {
            try
            {
                readerWriterLock.EnterWriteLock();

                using (ITransaction tx = _context.StateManager.CreateTransaction())
                {
                    var storage = storages.GetOrAddAsync(tx, EndpointName, type => new List<TimeoutData>()).Result;
                    for (var index = 0; index < storage.Count; )
                    {
                        var timeoutData = storage[index];
                        if (timeoutData.SagaId == sagaId)
                        {
                            storage.RemoveAt(index);
                            continue;
                        }
                        index++;
                    }

                    storages.AddOrUpdateAsync(tx, EndpointName, type => new List<TimeoutData>(), (t, o) => storage).Wait();

                    tx.CommitAsync().Wait();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                readerWriterLock.ExitWriteLock();
            }
        }
    }
}