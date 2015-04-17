//-----------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
// 
//      THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
//      EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
//      OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// ----------------------------------------------------------------------------------
//      The example companies, organizations, products, domain names,
//      e-mail addresses, logos, people, places, and events depicted
//      herein are fictitious.  No association with any real company,
//      organization, product, domain name, email address, logo, person,
//      places, or events is intended or should be inferred.
//-----------------------------------------------------------------------

namespace Microsoft.Fabric.Actor.Samples
{
    using System;
    using System.Fabric;
    using System.Fabric.Services;
    using System.Threading;

    class Program
    {
        static void Main(string[] args)
        {
            using (var runtime = FabricRuntime.Create())
            {
                runtime.RegisterServiceType(
                    ServiceNameFormat.GetFabricServiceTypeName(typeof(IPresenceLoadDriver)),
                    typeof(PresenceLoadDriverService));

                Thread.Sleep(Timeout.Infinite);
                GC.KeepAlive(runtime);
            }
        }
    }
}
