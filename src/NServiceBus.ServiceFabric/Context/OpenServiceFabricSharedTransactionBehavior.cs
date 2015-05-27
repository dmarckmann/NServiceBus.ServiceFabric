using NServiceBus.Pipeline;
using NServiceBus.Pipeline.Contexts;
using System;
using System.Collections.Generic;
using Microsoft.ServiceFabric.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NServiceBus.ServiceFabric.Persistence
{
    class OpenServiceFabricSharedTransactionBehavior : IBehavior<IncomingContext>
    {
        ServiceFabricStorageContext _context;

        public OpenServiceFabricSharedTransactionBehavior(ServiceFabricStorageContext context)
        {
            _context = context;
        }

        public void Invoke(IncomingContext context, Action next)
        {
            var lazyTransaction = new Lazy<ITransaction>(() =>
            {
                return _context.StateManager.CreateTransaction();
            });

            context.Set("ServiceFabricSharedTransaction", lazyTransaction);
            try
            {
                next();

                lazyTransaction.Value.CommitAsync().Wait();
            }
            finally
            {
                if (lazyTransaction.IsValueCreated)
                {
                    lazyTransaction.Value.Dispose();
                }

                context.Remove("ServiceFabricSharedTransaction");
            }
        }

        public class Registration : RegisterStep
        {
            public Registration()
                : base("OpenServiceFabricSharedTransactionBehavior", typeof(OpenServiceFabricSharedTransactionBehavior), "Makes sure that there is a transaction available on the pipeline")
            {
                InsertAfter(WellKnownStep.CreateChildContainer);
                InsertBeforeIfExists("OutboxDeduplication");
                InsertBeforeIfExists(WellKnownStep.InvokeSaga);

            }
        }
    }
}
