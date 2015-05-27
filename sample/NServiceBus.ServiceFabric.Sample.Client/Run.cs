using NServiceBus;
using NServiceBus.ServiceFabric.Sample.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NServiceBus.ServiceFabric.Sample.Client
{
    public class Run : IWantToRunWhenBusStartsAndStops
    {

        private IBus _bus;

        public Run(IBus bus)
        {
            _bus = bus;
        }

        public void Start()
        {
            Console.WriteLine("Press enter to send a message, x to quit");
            var key = Console.ReadLine();

            while (key != "x")
            {
                _bus.Send(new TestCommand());

                Console.WriteLine("Sent a message, wait for event received...");
                key = Console.ReadLine();
            }
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
