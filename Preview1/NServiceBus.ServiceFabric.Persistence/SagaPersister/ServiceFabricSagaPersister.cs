namespace NServiceBus.ServiceFabric.SagaPersister
{
    using System;
    using System.Linq;
    using System.Threading;
    using Saga;
    using Serializers.Json;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using NServiceBus.ServiceFabric.Persistence;

    /// <summary>
    /// ServiceFabric implementation of ISagaPersister.
    /// </summary>
    class ServiceFabricSagaPersister : ISagaPersister
    {
        private ServiceFabricStorageContext _context;
        //readonly SagaMetaModel sagaModel;
        int version;

        JsonMessageSerializer serializer = new JsonMessageSerializer(null);
        IReliableDictionary<Guid, VersionedSagaEntity> data;

        public ServiceFabricSagaPersister(/*SagaMetaModel sagaModel,*/ ServiceFabricStorageContext context)
        {
            //this.sagaModel = sagaModel;
            _context = context;
            data = context.StateManager.GetOrAddAsync<IReliableDictionary<Guid, VersionedSagaEntity>>("nservicebusSagas").Result;
        }

        public void Complete(IContainSagaData saga)
        {
            try
            {
                data.TryRemoveAsync(_context.CurrentTransaction, saga.Id).Wait();                
            }
            catch (Exception ex)
            {
                throw ex;
            }            
        }

        public TSagaData Get<TSagaData>(string propertyName, object propertyValue) where TSagaData : IContainSagaData
        {
            var values = data.Select(x => x.Value).Where(x => x.SagaType == typeof(TSagaData).FullName);
            foreach (var entity in values)
            {
                var prop = entity.SagaEntity.GetType().GetProperty(propertyName);
                if (prop == null)
                {
                    continue;
                }
                if (!prop.GetValue(entity.SagaEntity, null).Equals(propertyValue))
                {
                    continue;
                }
                var clone = (TSagaData)Deserialize(entity.SagaEntity, GetType(entity.SagaType));
                entity.RecordRead(clone, version);
                return clone;
            }
            return default(TSagaData);
        }

        public TSagaData Get<TSagaData>(Guid sagaId) where TSagaData : IContainSagaData
        {
            using (ITransaction tx = _context.StateManager.CreateTransaction())
            {
                var r = data.TryGetValueAsync(tx, sagaId).Result;
                if (r.HasValue) {
                    VersionedSagaEntity result = r.Value;
                    if (result != null && result.SagaType == typeof(TSagaData).FullName)
                    {
                        var clone = (TSagaData)Deserialize(result.SagaEntity, GetType(result.SagaType));
                        result.RecordRead(clone, version);
                        return clone;
                    }
                }
                return default(TSagaData);
            }
        }

        public static Type GetType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null) return type;
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = a.GetType(typeName);
                if (type != null)
                    return type;
            }
            return null;
        }

        public void Save(IContainSagaData saga)
        {
            //ValidateUniqueProperties(saga);

            try
            {
                var tx = _context.CurrentTransaction;
                VersionedSagaEntity sagaEntity;
                var res = data.TryGetValueAsync(tx, saga.Id).Result;
                if (res.HasValue)
                {
                    sagaEntity = res.Value;
                    sagaEntity.ConcurrencyCheck(saga, version);
                }

                data.AddOrUpdateAsync(tx, saga.Id, id => new VersionedSagaEntity { SagaEntity = Serialize(saga), SagaType = saga.GetType().FullName }, (id, original) => new VersionedSagaEntity { SagaEntity = Serialize(saga), SagaType = saga.GetType().FullName, VersionCache = original.VersionCache }).Wait();

                Interlocked.Increment(ref version);
            }
            catch (Exception ex)
            {
                throw ex;
            }           
        }

        public void Update(IContainSagaData saga)
        {
            Save(saga);
        }

        // sagaModel will be injected as from v6

        //void ValidateUniqueProperties(IContainSagaData saga)
        //{

        //    var sagaMetaData = sagaModel.FindByEntityName(saga.GetType().FullName);

        //    if (!sagaMetaData.CorrelationProperties.Any()) return;

        //    var sagasFromSameType = from s in data
        //                            where
        //                                (s.Value.SagaEntity.GetType() == saga.GetType() && (s.Key != saga.Id))
        //                            select s.Value;

        //    foreach (var storedSaga in sagasFromSameType)
        //    {
        //        foreach (var correlationProperty in sagaMetaData.CorrelationProperties)
        //        {
        //            var uniqueProperty = saga.GetType().GetProperty(correlationProperty.Name);
        //            if (!uniqueProperty.CanRead)
        //            {
        //                continue;
        //            }
        //            var inComingSagaPropertyValue = uniqueProperty.GetValue(saga, null);
        //            var storedSagaPropertyValue = uniqueProperty.GetValue(storedSaga.SagaEntity, null);
        //            if (inComingSagaPropertyValue.Equals(storedSagaPropertyValue))
        //            {
        //                var message = string.Format("Cannot store a saga. The saga with id '{0}' already has property '{1}' with value '{2}'.", storedSaga.SagaEntity.Id, uniqueProperty, storedSagaPropertyValue);
        //                throw new InvalidOperationException(message);
        //            }
        //        }
        //    }
        //}

        string Serialize(IContainSagaData source)
        {
            return serializer.SerializeObject(source);
        }

        IContainSagaData Deserialize(string json, Type type)
        {
            return (IContainSagaData)serializer.DeserializeObject(json, type);
        }

        [DataContractAttribute]
        class VersionedSagaEntity
        {
            [DataMember]
            public string SagaEntity;

            [DataMember]
            public string SagaType;

            [DataMember]
            public Dictionary<Guid, int> VersionCache = new Dictionary<Guid, int>();

            public void RecordRead(IContainSagaData sagaEntity, int currentVersion)
            {
                VersionCache[sagaEntity.Id] = currentVersion;
            }

            public void ConcurrencyCheck(IContainSagaData sagaEntity, int currentVersion)
            {
                int v;
                if (!VersionCache.TryGetValue(sagaEntity.Id, out v))
                    throw new Exception(string.Format("InMemorySagaPersister in an inconsistent state: entity Id[{0}] not read.", sagaEntity.Id));

                if (v != currentVersion)
                    throw new Exception(string.Format("InMemorySagaPersister concurrency violation: saga entity Id[{0}] already saved.", sagaEntity.Id));
            }
        }
    }

}
