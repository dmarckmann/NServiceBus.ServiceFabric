using NServiceBus;
using Preview1.Messages;
using System;

namespace Preview1.Client
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
