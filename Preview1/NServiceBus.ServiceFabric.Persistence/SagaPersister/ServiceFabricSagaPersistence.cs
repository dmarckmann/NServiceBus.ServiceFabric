namespace NServiceBus.Features
{
    using NServiceBus.ServiceFabric.SagaPersister;

    /// <summary>
    /// Used to configure service fabric saga persistence.
    /// </summary>
    public class ServiceFabricSagaPersistence : Feature
    {
        internal ServiceFabricSagaPersistence()
        {
            DependsOn<Sagas>();
        }

        /// <summary>
        /// See <see cref="Feature.Setup"/>
        /// </summary>
        protected override void Setup(FeatureConfigurationContext context)
        {
            context.Container.ConfigureComponent<ServiceFabricSagaPersister>(DependencyLifecycle.SingleInstance);
        }
    }
}