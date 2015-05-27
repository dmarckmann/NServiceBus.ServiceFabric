namespace NServiceBus.Features
{
    using System;
    using System.Threading;
    using NServiceBus.ServiceFabric.Outbox;

    /// <summary>
    /// Used to configure in service fabric outbox persistence.
    /// </summary>
    public class ServiceFabricOutboxPersistence : Feature
    {
        internal ServiceFabricOutboxPersistence()
        {
            DependsOn<Outbox>();
            RegisterStartupTask<OutboxCleaner>();
        }

        /// <summary>
        /// See <see cref="Feature.Setup"/>
        /// </summary>
        protected override void Setup(FeatureConfigurationContext context)
        {
            context.Container.ConfigureComponent<ServiceFabricOutboxStorage>(DependencyLifecycle.SingleInstance);
            context.Container.ConfigureComponent<OutboxCleaner>(DependencyLifecycle.SingleInstance)
                .ConfigureProperty(t => t.TimeToKeepDeduplicationData, TimeSpan.FromDays(5));
        }

        class OutboxCleaner : FeatureStartupTask
        {
            public ServiceFabricOutboxStorage ServiceFabricOutboxStorage { get; set; }

            public TimeSpan TimeToKeepDeduplicationData { get; set; }

            protected override void OnStart()
            {
                cleanupTimer = new Timer(PerformCleanup, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
            }

            protected override void OnStop()
            {
                using (var waitHandle = new ManualResetEvent(false))
                {
                    cleanupTimer.Dispose(waitHandle);

                    waitHandle.WaitOne();
                }
            }

            void PerformCleanup(object state)
            {
                ServiceFabricOutboxStorage.RemoveEntriesOlderThan(DateTime.UtcNow - TimeToKeepDeduplicationData);
            }

// ReSharper disable once NotAccessedField.Local
            Timer cleanupTimer;
        }
    }
}