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
    using System.Fabric.Services;
    using System.Fabric.Actors;

    class Program
    {
        private static readonly ActorId ActorId = ActorId.NewId();

        static void Main(string[] args)
        {
            var appName = (args.Length > 1) ? args[0] : "fabric:/CalculatorActorApp";
            var calculatorActor = ActorProxy.Create<ICalculatorActor>(ActorId, appName);

            PrintConnectionEvents(calculatorActor);

            long count = 0;
            while (true)
            {
                if (calculatorActor.AddAsync(2, 3).Result.Equals(5))
                {
                    count++;
                }
                if ((count % 500) == 0)
                {
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write(@"                    ");
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write(count);
                }
            }
        }

        private static void PrintConnectionEvents(ICalculatorActor calculatorActor)
        {
            var actorProxy = calculatorActor as IActorProxy;
            if (actorProxy != null)
            {
                actorProxy.ActorServicePartitionClient.Factory.ClientConnected += OnClientConnected;
            }
        }

        private static void OnClientConnected(object sender, CommunicationClientEventArgs e)
        {
            Console.WriteLine();
            Console.WriteLine(@"Calculator Actor {1} Connected to {0}", e.Client.ResolvedServicePartition.GetEndpoint().Address, ActorId);
            Console.WriteLine();
        }
    }
}
