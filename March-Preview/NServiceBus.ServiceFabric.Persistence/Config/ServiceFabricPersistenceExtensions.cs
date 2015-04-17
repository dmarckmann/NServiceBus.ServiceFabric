using NServiceBus.Configuration.AdvanceExtensibility;
using System;
using System.Collections.Generic;
using System.Fabric.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NServiceBus.ServiceFabric.Persistence
{
    public static class ServiceFabricPersistenceExtensions
    {
        public static PersistenceExtentions<ServiceFabricPersistence> UseStateManager(this PersistenceExtentions<ServiceFabricPersistence> config, IReliableObjectStateManager stateManager)
        {
            config.GetSettings().Set("ServiceFabricStateManager", stateManager);

            return config;
        }
    }
}
