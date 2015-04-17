//-----------------------------------------------------------------------
// <copyright file="ServiceHost.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
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
// </copyright>
//-----------------------------------------------------------------------

namespace VisualObjects.WebService
{
    using System;
    using System.Diagnostics;
    using System.Fabric;

    /// <summary>
    /// The service host is the executable that hosts the Service instances.
    /// </summary>
    public class ServiceHost
    {
        /// <summary>
        /// The console executable entry point.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            // Create a Windows Fabric Runtime
            using (var fabricRuntime = FabricRuntime.Create())
            {
                Trace.Listeners.Add(new ConsoleTraceListener { Name = "ConsoleTraceListener" } );
                Trace.Listeners.Add(new TextWriterTraceListener(@"C:\log\visualobjectsweb\output.log"));
                Trace.AutoFlush = true;

                try
                {
                    Trace.WriteLine("Starting Service Host for Web Service.");
                    fabricRuntime.RegisterServiceType(Service.ServiceName, typeof(Service));

                    // Wait for WindowsFabric to place services in this process
                    // Using ReadLine() makes it easy to work with a console host in a development environment.
                    Console.ReadLine();
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e.Message);
                    Console.ReadLine();
                }
            }
        }
    }
}
