using NServiceBus.Saga;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliablePersistence
{
    public class MySagaData : IContainSagaData
    {
        public Guid Id { get; set; }
        public string OriginalMessageId { get; set; }
        public string Originator { get; set; }
        public Guid SomeCorrelationId { get; set; }
    }
}
