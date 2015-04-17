using NServiceBus;
using ReliablePersistence.Messages;
using System;

namespace ReliablePersistence.Client
{
    public class SagaCompletedHandler : IHandleMessages<SagaCompleted>
    {
        public void Handle(SagaCompleted message)
        {
            Console.WriteLine("Saga completed");
            Console.WriteLine("Press enter to send a message, x to quit");
        }
    }
}
