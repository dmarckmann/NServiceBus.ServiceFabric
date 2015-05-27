using NServiceBus;
using NServiceBus.ServiceFabric.Sample.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NServiceBus.ServiceFabric.Sample.Client
{
    public class EventHandler : IHandleMessages<TestEvent>
    {
        public void Handle(TestEvent message)
        {
            Console.WriteLine("Test event received, wait for saga complete...");
        }
    }
}
