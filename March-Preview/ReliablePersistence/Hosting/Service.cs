using System.Fabric.Services;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.ServiceFabric.Persistence;

namespace ReliablePersistence
{
    public class ReliablePersistenceService : StatefulService
    {
        public const string ServiceTypeName = "ReliablePersistenceType";

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            ServiceEventSource.Current.Message("Starting bus");

            BusConfiguration configuration = new BusConfiguration();
            
            configuration.UsePersistence<ServiceFabricPersistence>()
                .UseStateManager(this.StateManager);

            //configuration.UseTransport<AzureServiceBusTransport>();
            configuration.UseTransport<AzureStorageQueueTransport>();
            configuration.ScaleOut().UseSingleBrokerQueue();
            configuration.EnableOutbox();

            using (IStartableBus bus = Bus.Create(configuration))
            {
                bus.Start();

                ServiceEventSource.Current.Message("Started the bus");

                await Task.Delay(Timeout.Infinite, cancellationToken);                
            }
        }
    }   
}
