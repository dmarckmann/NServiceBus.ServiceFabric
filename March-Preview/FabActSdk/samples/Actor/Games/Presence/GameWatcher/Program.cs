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
    using System.Collections.Generic;
    using System.Fabric.Actors;
    using System.Threading;
    using System.Threading.Tasks;

    class Program
    {
        private const string ApplicationName = "fabric:/PresenceActorApp";

        private static int Main(string[] args)
        {

            if (args.Length < 1)
            {
                Console.WriteLine("Usage: GameWatcher.exe <GameId1> [<GameId2>] ..");
                return -1;
            }

            var gameProxies = new List<IGameActor>();
            foreach (var arg in args)
            {
                var proxy = ActorProxy.Create<IGameActor>(new ActorId(Guid.Parse(arg)), ApplicationName);
                proxy.SubscribeAsync(new GameEventsHandler()).Wait();
                gameProxies.Add(proxy);
            }

            Console.WriteLine("Press CTRL-C to exit ..");
            Console.ReadLine();

            return 0;
        }

    }

    class GameEventsHandler : IGameEvents
    {
        public void GameScoreUpdated(Guid gameId, string currentScore)
        {
            Console.WriteLine(@"Updates: Game: {0}, Score: {1}", gameId, currentScore);
        }
    }

}