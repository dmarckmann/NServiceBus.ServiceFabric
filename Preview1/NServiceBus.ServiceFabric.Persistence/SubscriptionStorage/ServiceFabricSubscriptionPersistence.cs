namespace NServiceBus.Features
{
    using System;
    using NServiceBus.InMemory.SubscriptionStorage;

    /// <summary>
    /// Used to configure in service fabric subscription persistence.
    /// </summary>
    public class ServiceFabricSubscriptionPersistence : Feature
    {
        internal ServiceFabricSubscriptionPersistence()
        {
            DependsOn<MessageDrivenSubscriptions>();
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            context.Container.ConfigureComponent<ServiceFabricSubscriptionStorage>(DependencyLifecycle.SingleInstance);
        }
    }
}