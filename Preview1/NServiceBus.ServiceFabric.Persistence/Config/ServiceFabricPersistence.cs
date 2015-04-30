namespace NServiceBus
{
    using NServiceBus.Configuration.AdvanceExtensibility;
    using Features;
    using Persistence;
    using Pipeline;
    using Microsoft.ServiceFabric.Data;
    using NServiceBus.Pipeline.Contexts;
    using System;
    using NServiceBus.ServiceFabric.Persistence;


    /// <summary>
    /// Used to enable ServiceFabric persistence.
    /// </summary>
    public class ServiceFabricPersistence : PersistenceDefinition
    {
        internal ServiceFabricPersistence()
        {
            Defaults(s => s.EnableFeatureByDefault<ServiceFabricSharedTransaction>());
            Supports<StorageType.Sagas>(s => s.EnableFeatureByDefault<ServiceFabricSagaPersistence>());
            Supports<StorageType.Timeouts>(s => s.EnableFeatureByDefault<ServiceFabricTimeoutPersistence>());
            Supports<StorageType.Subscriptions>(s => s.EnableFeatureByDefault<ServiceFabricSubscriptionPersistence>());
            Supports<StorageType.Outbox>(s => s.EnableFeatureByDefault<ServiceFabricOutboxPersistence>());
            Supports<StorageType.GatewayDeduplication>(s => s.EnableFeatureByDefault<InMemoryGatewayPersistence>());
        }        
    }  
}