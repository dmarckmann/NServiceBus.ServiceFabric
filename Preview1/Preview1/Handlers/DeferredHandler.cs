using NServiceBus;
using Preview1.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Preview1
{
    public class DeferredHandler : IHandleMessages<DeferredMessage>
    {
        public void Handle(DeferredMessage message)
        {
            ServiceEventSource.Current.Message("Deferred message received");
        }
    }
}
