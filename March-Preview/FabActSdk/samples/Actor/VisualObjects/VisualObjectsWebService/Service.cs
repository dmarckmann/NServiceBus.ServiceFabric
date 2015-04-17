//-----------------------------------------------------------------------
// <copyright file="Service.cs" company="Microsoft">
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
    using Microsoft.Fabric.Actor.Samples;
    using Microsoft.Owin.Hosting;
    using System;
    using System.Diagnostics;
    using System.Fabric;
    using System.Net;
    using System.Net.WebSockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// This Web Service is the Visual Objects middle-tier.
    /// It handles front-end web requests and acts as a proxy to the back-end data for the UI web page.
    /// This service is a stateless service that hosts both a Web API application on OWIN,and a Web Socket listener.
    /// The Web API application serves up the UI web page, which then uses the web socket listener to render the animation.
    /// </summary>
    public class Service : IStatelessServiceInstance
    {
        /// <summary>
        /// Name of the service type.
        /// </summary>
        public const string ServiceName = "VisualObjectsWebService";
        
        /// <summary>
        /// OWIN server handle.
        /// </summary>
        private IDisposable _serverHandle;

        /// <summary>
        /// Web Socket listener
        /// </summary>
        private HttpListener _httpListener;

        /// <summary>
        /// For cancelling operations when the service is closed or aborted.
        /// </summary>
        private CancellationTokenSource _cancellationSource;

        /// <summary>
        /// This is the service name of the VisualObjectsActor application
        /// </summary>
        private const string ApplicationName = "fabric:/VisualObjectsActorApp";

        /// <summary>
        /// Initialize is called once by the fabric runtime at the beginning to initialize the service instance.
        /// </summary>
        public void Initialize(StatelessServiceInitializationParameters initializationParameters)
        {
            Trace.WriteLine("Initialize");
        }

        /// <summary>
        /// OpenAsync is called after Initialize when the service instance is ready to be opened by the runtime.
        /// This should start any service listeners without blocking the call for an extended period of time.
        /// Here we start Web API and the web socket listener.
        /// </summary>
        /// <param name="partition"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<string> OpenAsync(IStatelessServicePartition partition, CancellationToken cancellationToken)
        {
            Trace.WriteLine("Open");

            try
            {
                _cancellationSource = new CancellationTokenSource();

                // create the HTTP address for the web server and the web socket endpoints.
                var node = FabricRuntime.GetNodeContext();

                var webServerUrl = new UriBuilder(Uri.UriSchemeHttp, node.IPAddressOrFQDN, 8505);

                var webSocketServerUrl = new UriBuilder(webServerUrl.Uri) {Path = "data"};

                // start the web server and the web socket listener
                StartWebServer(webServerUrl.Uri.ToString());
                StartWebSocketServer(webSocketServerUrl.Uri.ToString());

                return Task.FromResult(webServerUrl.Uri.ToString());
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());

                StopAll();

                return null;
            }
        }

        /// <summary>
        /// CloseAsync is called when the service instance is closed.
        /// This should shut down any processing, close any network connections or listeners, and cancel any pending tasks
        /// without blocking the call for an extended period of time.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task CloseAsync(CancellationToken cancellationToken)
        {
            Trace.WriteLine("Close");

            StopAll();

            return Task.FromResult(true);
        }

        /// <summary>
        /// Abort is called when the service instance needs to stop abrubtly.
        /// This should immediately stop everything.
        /// </summary>
        public void Abort()
        {
            Trace.WriteLine("Abort");

            StopAll();
        }


        /// <summary>
        /// This method starts the Web API server.
        /// </summary>
        public void StartWebServer(string url)
        {
            Trace.WriteLine("Starting web server on " + url);

            _serverHandle = WebApp.Start<ServiceHttpStartup>(url);

            // Katana adds its own console trace listener. Remove this one so we don't get duplicate output.
            Trace.Listeners.Remove("ConsoleTraceListener");
        }

        /// <summary>
        /// This method starts listening for web socket requests on the given URL.
        /// </summary>
        /// <param name="url"></param>
        public void StartWebSocketServer(string url)
        {
            if (!url.EndsWith("/"))
            {
                url += "/";
            }

            Trace.WriteLine("Starting web socket listener on " + url);

            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add(url);
            _httpListener.Start();

            Task.Run(async () =>
            {
                var cancellationToken = _cancellationSource.Token;

                var box = new VisualObjectsBox(ApplicationName);
#pragma warning disable 4014
                box.RefreshContentsAsync(cancellationToken);
#pragma warning restore 4014

                // This loop continuously listens for incoming client connections
                // as you might normally do with a web socket server.
                while (true)
                {
                    Trace.WriteLine("Waiting for connection..");

                    cancellationToken.ThrowIfCancellationRequested();

                    var context = await _httpListener.GetContextAsync();

#pragma warning disable 4014
                    context.AcceptWebSocketAsync(null).ContinueWith(async task =>
#pragma warning restore 4014
                    {
                        var websocketContext = task.Result;

                        Trace.WriteLine("Connection from " + websocketContext.Origin);

                        using (var browserSocket = websocketContext.WebSocket)
                        {
                            while (true)
                            {
                                var response = box.GetContents();
                                var buffer = Encoding.UTF8.GetBytes(response);

                                try
                                {
                                    await browserSocket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), WebSocketMessageType.Text, true, cancellationToken);

                                    if (browserSocket.State != WebSocketState.Open)
                                    {
                                        break;
                                    }
                                }
                                catch (WebSocketException ex)
                                {
                                    // If the browser quit or the socket was closed, exit this loop so we can get a new browser socket.
                                    Trace.WriteLine(ex.InnerException != null ? ex.InnerException.Message : ex.Message);

                                    break;
                                }

                                await Task.Delay(TimeSpan.FromMilliseconds(10), cancellationToken);
                            }
                        }
                        Trace.WriteLine("Client disconnected.");

                    }, TaskContinuationOptions.OnlyOnRanToCompletion);
                }
            },
            _cancellationSource.Token);
        }

        /// <summary>
        /// Stops, cancels, and disposes everything.
        /// </summary>
        public void StopAll()
        {
            try
            {
                if (_serverHandle != null)
                {
                    Trace.WriteLine("Stopping web server.");

                    _serverHandle.Dispose();
                }

                if (_httpListener != null && _httpListener.IsListening)
                {
                    Trace.WriteLine("Stopping web socket server.");

                    _httpListener.Close();
                }

                if (_cancellationSource != null && !_cancellationSource.IsCancellationRequested)
                {
                    _cancellationSource.Cancel();
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (AggregateException ae)
            {
                ae.Handle(ex =>
                {
                    Trace.WriteLine(ex.Message);
                    return true;
                });
            }
        }
    }
}
