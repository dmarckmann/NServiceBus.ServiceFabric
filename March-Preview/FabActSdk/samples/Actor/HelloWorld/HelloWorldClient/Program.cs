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
    using System.Fabric.Actors;
    using System.Threading;

    class Program
    {
        private const string ApplicationName = "fabric:/HelloWorldActorApp";

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                for (; ; )
                {
                    var friend = ActorProxy.Create<IHello>(ActorId.NewId(), ApplicationName);
                    Console.WriteLine(@"\n\nFrom Actor {1}: {0}\n\n", friend.SayHello("Good morning!").Result, friend.GetActorId());
                    Thread.Sleep(50);
                }
            }
            else
            {
                var friend = ActorProxy.Create<IHello>(ActorId.NewId(), ApplicationName);
                Console.WriteLine(@"\n\nFrom Actor {1}: {0}\n\n", friend.SayHello("Good morning!").Result, friend.GetActorId());
            }

            Console.WriteLine(@"Press enter to exit ...");
            Console.ReadLine();
        }
    }
}
