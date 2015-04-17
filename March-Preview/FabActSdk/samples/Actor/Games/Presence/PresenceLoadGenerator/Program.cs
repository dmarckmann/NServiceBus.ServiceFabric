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

        private static void Main()
        {
            const int nGames = 10; // number of games to simulate
            const int nPlayersPerGame = 4; // number of players in each game
            var sendInterval = TimeSpan.FromSeconds(5); // interval for sending updates

            // Precreate base heartbeat data objects for each of the games.
            // We'll modify them before every time before sending.
            var heartbeats = new HeartbeatData[nGames];
            for (var i = 0; i < nGames; i++)
            {
                heartbeats[i] = new HeartbeatData();
                heartbeats[i].Game = Guid.NewGuid();
                for (var j = 0; j < nPlayersPerGame; j++)
                {
                    var playerId = Guid.NewGuid();
                    heartbeats[i].Status.Players.Add(playerId);
                }
            }

            var outstandingUpdates = new List<Task>();
            var outstandingScoreReads = new List<Task<string>>();
            var iteration = 0;
            while (true)
            {
                iteration++;
                Console.WriteLine();
                Console.WriteLine("Sending heartbeat series # {0}", iteration);

                var score = String.Format("{0}:{1}", iteration, iteration > 5 ? iteration - 5 : 0);
                var presence = ActorProxy.Create<IPresenceActor>(ActorId.NewId(), ApplicationName); // get any stateless actor
                outstandingUpdates.Clear();
                try
                {
                    for (var i = 0; i < nGames; i++)
                    {
                        heartbeats[i].Status.Score = score;
                        var t = presence.Heartbeat(HeartbeatDataDotNetSerializer.Serialize(heartbeats[i]));
                        outstandingUpdates.Add(t);
                    }

                    // Wait for all calls to finish.
                    // It is okay to block the thread here because it's a client program with no parallelism.
                    // One should never block a thread in grain code.
                    Console.WriteLine("Wating for the tasks to finish");
                    Task.WaitAll(outstandingUpdates.ToArray());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: {0}", e);
                }

                Console.WriteLine();
                Console.WriteLine("Getting game scores: ");
                outstandingScoreReads.Clear();
                try
                {
                    for (var i = 0; i < nGames; i++)
                    {
                        var t = ActorProxy.Create<IGameActor>(new ActorId(heartbeats[i].Game), ApplicationName).GetGameScore();
                        outstandingScoreReads.Add(t);
                    }

                    // Wait for all calls to finish.
                    // It is okay to block the thread here because it's a client program with no parallelism.
                    // One should never block a thread in grain code.
                    Task.WhenAll(outstandingScoreReads.ToArray()).Wait();

                    for (var i = 0; i < nGames; i++)
                    {
                        Console.WriteLine("Game: {0}, Score: {1}", heartbeats[i].Game, outstandingScoreReads[i].Result);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: {0}", e);
                }

                Console.WriteLine();
                Console.WriteLine("Sleeping for {0} seconds.", sendInterval.TotalSeconds);
                Console.WriteLine("Press CTRL-C to exit");
                Thread.Sleep(sendInterval);
            }
        }
    }
}