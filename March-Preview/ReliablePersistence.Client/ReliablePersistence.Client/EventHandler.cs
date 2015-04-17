using NServiceBus;
using ReliablePersistence.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliablePersistence.Client
{
    public class EventHandler : IHandleMessages<TestEvent>
    {
        public void Handle(TestEvent message)
        {
            Console.WriteLine("Test event received, wait for saga complete...");
        }
    }
}
