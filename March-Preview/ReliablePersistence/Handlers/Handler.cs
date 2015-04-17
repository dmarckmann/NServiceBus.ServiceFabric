using NServiceBus;
using ReliablePersistence.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliablePersistence
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

            _bus.Publish(new TestEvent());

            _bus.Defer(TimeSpan.FromSeconds(5), new DeferredMessage()
            {
                SomeCorrelationId = Guid.NewGuid()
            });
        }
    }
}
