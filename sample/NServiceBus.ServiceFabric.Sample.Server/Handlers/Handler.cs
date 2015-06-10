using NServiceBus;
using NServiceBus.ServiceFabric.Sample.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NServiceBus.ServiceFabric.Sample.Server
{
    public class Handler : IHandleMessages<TestCommand>
    {
        private IBus _bus;

        public Handler(IBus bus)
        {
            _bus = bus;
        }

        public void Handle(TestCommand message)
        {
            ServiceEventSource.Current.Message("Received start command");

            ServiceEventSource.Current.Message("Publishing test event");
            _bus.Publish(new TestEvent());

            ServiceEventSource.Current.Message("Deferring deferred message");
            _bus.Defer(TimeSpan.FromSeconds(5), new DeferredMessage()
            {
                SomeCorrelationId = Guid.NewGuid()
            });
        }
    }
}
