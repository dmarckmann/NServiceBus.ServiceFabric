using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Fabric;
using System.Fabric.Description;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace VisualObjectsWebService.Setup
{
    class Program
    {
        static string acl = "D:(A;;GX;;;NS)";

        static int Main(string[] args)
        {
            CodePackageActivationContext context;
            try
            {
                context = FabricRuntime.GetActivationContext();

                using (TextWriterTraceListener trace = new TextWriterTraceListener(Path.Combine(context.LogDirectory, "out.log")))
                {
                    Trace.AutoFlush = true;
                    Trace.Listeners.Add(trace);

                    if (context == null)
                    {
                        Trace.WriteLine("null activation context found");
                        return 1;
                    }

                    int rc = Http.Initialize();
                    if (rc != 0)
                    {
                        Trace.WriteLine(String.Format("http initialize failed: {0}", rc));
                        return rc;
                    }

                    try
                    {
                        var node = FabricRuntime.GetNodeContext();
                        var webServerUrl = new UriBuilder(Uri.UriSchemeHttp, node.IPAddressOrFQDN, 8505);
                        var webSocketServerUrl = new UriBuilder(webServerUrl.Uri) { Path = "data" };
                        rc = AddUrlAcl(webServerUrl.ToString());
                        rc = AddUrlAcl(webSocketServerUrl.ToString());
                    }
                    catch (InvalidOperationException e)
                    {
                        Trace.WriteLine(e.Message);
                        return rc;
                    }
                    finally
                    {
                        Http.Terminate();
                    }

                    Trace.WriteLine("Done.");
                    return 0;
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(String.Format("setup failed: {0}", e.InnerException != null ? e.InnerException.Message : e.Message));
                return 1;
            }
        }

        static int AddUrlAcl(string prefix)
        {
            Trace.WriteLine(String.Format("Adding {0} for {1}", prefix, acl));
            int rc = Http.SetAcl(prefix, acl);

            // test if already set
            if (rc == 183)
            {
                Trace.WriteLine(String.Format("acl is already set"));
            }
            else if (rc != 0)
            {
                throw new InvalidOperationException(String.Format("failed to set acl: {0}", rc));
            }

            return rc;
        }
    }
}
