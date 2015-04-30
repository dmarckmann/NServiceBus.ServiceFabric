using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services;
using NServiceBus;
using NServiceBus.ServiceFabric.Persistence;

namespace Preview1
{
    public class Preview1 : StatefulService
    {
        protected override ICommunicationListener CreateCommunicationListener()
        {
            // TODO: Replace this with an ICommunicationListener implementation if your service needs to handle user requests.
            return base.CreateCommunicationListener();
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            ServiceEventSource.Current.Message("Starting bus");

            BusConfiguration configuration = new BusConfiguration();
            configuration.EndpointName("servicefabric");

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
