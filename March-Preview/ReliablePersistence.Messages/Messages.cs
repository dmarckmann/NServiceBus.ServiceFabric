using NServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliablePersistence.Messages
{
    public class TestCommand : ICommand
    {
    }

    public class TestEvent : IEvent
    {
    }

    public class SagaCompleted : IEvent
    {
    }
    

    public class DeferredMessage : IMessage {

        public Guid SomeCorrelationId { get; set; }

    }
}
