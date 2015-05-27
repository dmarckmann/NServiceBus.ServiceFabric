using NServiceBus;
using NServiceBus.ServiceFabric.Sample.Messages;
using System;

namespace NServiceBus.ServiceFabric.Sample.Client
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
