namespace NServiceBus.Features
{
    using NServiceBus.ServiceFabric.TimeoutPersister;

    /// <summary>
    /// Used to configure service fabric timeout persistence.
    /// </summary>
    public class ServiceFabricTimeoutPersistence : Feature
    {
        internal ServiceFabricTimeoutPersistence()
        {
            DependsOn<TimeoutManager>();
        }

        /// <summary>
        /// See <see cref="Feature.Setup"/>
        /// </summary>
        protected override void Setup(FeatureConfigurationContext context)
        {
            context.Container.ConfigureComponent<ServiceFabricTimeoutPersister>(DependencyLifecycle.SingleInstance);
            context.Container.ConfigureProperty<ServiceFabricTimeoutPersister>("EndpointName", context.Settings.EndpointName());
        }
    }
}