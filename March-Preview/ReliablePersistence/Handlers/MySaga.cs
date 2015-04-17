using NServiceBus.Saga;
using ReliablePersistence.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliablePersistence
{
    public class MySaga : Saga<MySagaData>,
        IAmStartedByMessages<DeferredMessage>,
        IHandleTimeouts<TimeoutExpired>
    {

        public void Handle(DeferredMessage message)
        {
            Data.SomeCorrelationId = message.SomeCorrelationId;

            ServiceEventSource.Current.Message("Received defered message, starting saga");

            RequestTimeout(TimeSpan.FromSeconds(5), new TimeoutExpired());
        }

        public void Timeout(TimeoutExpired message)
        {
            ServiceEventSource.Current.Message("Timeout expired");

            Bus.Publish(new SagaCompleted());

            MarkAsComplete();
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<MySagaData> mapper)
        {
            mapper.ConfigureMapping<DeferredMessage>(m => m.SomeCorrelationId).ToSaga(s => s.SomeCorrelationId);
        }
    }
}
