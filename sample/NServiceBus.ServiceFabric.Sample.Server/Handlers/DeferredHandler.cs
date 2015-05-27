using NServiceBus;
using NServiceBus.ServiceFabric.Sample.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NServiceBus.ServiceFabric.Sample.Server
{
    public class DeferredHandler : IHandleMessages<DeferredMessage>
    {
        public void Handle(DeferredMessage message)
        {
            ServiceEventSource.Current.Message("Deferred message received");
        }
    }
}
