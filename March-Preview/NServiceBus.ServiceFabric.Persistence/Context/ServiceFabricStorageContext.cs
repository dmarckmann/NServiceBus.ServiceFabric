using NServiceBus.Pipeline;
using System;
using System.Collections.Generic;
using System.Fabric.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NServiceBus.ServiceFabric.Persistence
{
    public class ServiceFabricStorageContext
    {
        readonly PipelineExecutor pipelineExecutor;

        internal ServiceFabricStorageContext(PipelineExecutor pipelineExecutor, IReliableObjectStateManager stateManager)
        {
            this.pipelineExecutor = pipelineExecutor;
            StateManager = stateManager;
        }

        public IReliableObjectStateManager StateManager { get; }

        public ITransaction CurrentTransaction
        {
            get
            {
                Lazy<ITransaction> lazy;
                if (pipelineExecutor.CurrentContext.TryGet("ServiceFabricSharedTransaction", out lazy))
                {
                    return lazy.Value;
                }

                throw new InvalidOperationException("No transaction available");
            }
        }
    }
}
