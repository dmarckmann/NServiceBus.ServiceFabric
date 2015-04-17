using System;
using System.Diagnostics;
using System.Fabric;
using System.Threading;

namespace ReliablePersistence
{
    public class ServiceHost
    {
        public static void Main(string[] args)
        {
            try
            {
                using (FabricRuntime fabricRuntime = FabricRuntime.Create())
                {
                    fabricRuntime.RegisterServiceType(ReliablePersistenceService.ServiceTypeName, typeof(ReliablePersistenceService));

                    ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, ReliablePersistenceService.ServiceTypeName);

                    Thread.Sleep(Timeout.Infinite);
                }
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e);
            }
        }
    }
}
