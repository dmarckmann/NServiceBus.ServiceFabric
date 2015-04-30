using NServiceBus;
using Preview1.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Preview1.Client
{
    public class EventHandler : IHandleMessages<TestEvent>
    {
        public void Handle(TestEvent message)
        {
            Console.WriteLine("Test event received, wait for saga complete...");
        }
    }
}
