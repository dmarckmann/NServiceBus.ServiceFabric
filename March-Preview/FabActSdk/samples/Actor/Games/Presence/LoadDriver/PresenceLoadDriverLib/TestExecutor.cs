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
    using System.Diagnostics;
    using System.Fabric.Services;
    using System.Fabric.Actors;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;

    public static class TestExecutor
    {
        public static Task<TestResults> RunTest(TestSpecifications specifications)
        {
            var overallResults = new TestResults();

            var contexts = new TestContext[specifications.NumThreads];
            var threads = new Thread[specifications.NumThreads];

            for (var i = 0; i < specifications.NumThreads; i++)
            {
                contexts[i] = new TestContext(specifications);
                threads[i] = new Thread(RunThread);
            }

            for (var i = 0; i < specifications.NumThreads; i++)
            {
                threads[i].Start(contexts[i]);
            }

            for (var i = 0; i < specifications.NumThreads; i++)
            {
                threads[i].Join();

                overallResults.ReadOperationStats.TotalOperationsPerformed +=
                    contexts[i].Results.ReadOperationStats.TotalOperationsPerformed;

                overallResults.WriteOperationStats.TotalOperationsPerformed +=
                    contexts[i].Results.WriteOperationStats.TotalOperationsPerformed;

                if (overallResults.WriteOperationStats.MaxElaspedTimeMillis <
                    contexts[i].Results.WriteOperationStats.MaxElaspedTimeMillis)
                {
                    overallResults.WriteOperationStats.MaxElaspedTimeMillis =
                        contexts[i].Results.WriteOperationStats.MaxElaspedTimeMillis;
                }

                if (overallResults.ReadOperationStats.MaxElaspedTimeMillis <
                    contexts[i].Results.ReadOperationStats.MaxElaspedTimeMillis)
                {
                    overallResults.ReadOperationStats.MaxElaspedTimeMillis =
                        contexts[i].Results.ReadOperationStats.MaxElaspedTimeMillis;
                }

                if (overallResults.WriteOperationStats.MinElaspedTimeMillis >
                   contexts[i].Results.WriteOperationStats.MinElaspedTimeMillis)
                {
                    overallResults.WriteOperationStats.MinElaspedTimeMillis =
                        contexts[i].Results.WriteOperationStats.MinElaspedTimeMillis;
                }

                if (overallResults.ReadOperationStats.MinElaspedTimeMillis >
                   contexts[i].Results.ReadOperationStats.MinElaspedTimeMillis)
                {
                    overallResults.ReadOperationStats.MinElaspedTimeMillis =
                        contexts[i].Results.ReadOperationStats.MinElaspedTimeMillis;
                }

                overallResults.TotalElaspedWallClockMillis += contexts[i].Results.TotalElaspedWallClockMillis;
            }

            overallResults.TotalElaspedWallClockMillis /= specifications.NumThreads;
            overallResults.TotalNumberOfActors = specifications.NumActorsPerThread * specifications.NumThreads;

            return Task.FromResult(overallResults);
        }

        private static void RunThread(object state)
        {
            var context = (TestContext)state;

            var presenceActors = new IPresenceActor[context.PresenceActorIds.Length];
            var gameActors = new IGameActor[context.PresenceActorIds.Length];

            context.Watch.Start();
            for (int i = 0; i < presenceActors.Length; i++)
            {
                presenceActors[i] = ActorProxy.Create<IPresenceActor>(new ActorId(context.PresenceActorIds[i]), context.ApplicationName);
            }
            for (int i = 0; i < presenceActors.Length; i++)
            {
                gameActors[i] = ActorProxy.Create<IGameActor>(new ActorId(context.GameActorIds[i]), context.ApplicationName);
            }
            context.Watch.Stop();

            
            var totalOperationsToBePerformed = context.Specifications.NumActorsPerThread*
                                               context.Specifications.NumOperationsPerActor;
            var outstandingOperations = new List<Task>();

            context.Watch.Start();
            for (var op = 0; op < context.Specifications.NumOutstandingOperationsPerThread; ++op)
            {
                outstandingOperations.Add(DoWork(context, presenceActors, gameActors, totalOperationsToBePerformed, op));
            }
            Task.WaitAll(outstandingOperations.ToArray());
            context.Watch.Stop();

            context.Results.TotalElaspedWallClockMillis = context.Watch.ElapsedMilliseconds;
        }

        private static async Task DoWork(
            TestContext context, 
            IPresenceActor[] presenceActors, 
            IGameActor[] gameActors,
            int totalOperationsToBePerformed,
            int opIndex)
        {
            var r = new Random();
            while (opIndex < totalOperationsToBePerformed)
            {
                var actorIndex = opIndex % context.Specifications.NumActorsPerThread;

                if (r.NextDouble() <= context.Specifications.ReadToWriteRatio)
                {
                    await DoReadOperation(gameActors[actorIndex], context, actorIndex);
                }
                else
                {
                    await DoWriteOperation(gameActors[actorIndex], context, actorIndex);
                }

                opIndex += context.Specifications.NumOutstandingOperationsPerThread;
            }
        }

        private static async Task DoWriteOperation(IPresenceActor presenceActor, TestContext context, int actorIndex)
        {
            var data = new HeartbeatData { Game = context.GameActorIds[actorIndex] };
            data.Status.Players.Add(context.PlayerActorIds[actorIndex]);
            data.Status.Score = (context.Rand.Next(0, 100).ToString(CultureInfo.InvariantCulture));

            var watch = Stopwatch.StartNew();
            var payload = HeartbeatDataDotNetSerializer.Serialize(data);
            await presenceActor.Heartbeat(payload);
            watch.Stop();

            context.OnWriteOperationCompleted(watch.ElapsedMilliseconds, 1, 1);
        }

        private static async Task DoReadOperation(IGameActor gameActor, TestContext context, int actorIndex)
        {
            var watch = Stopwatch.StartNew();
            await gameActor.GetGameScore();
            watch.Stop();

            context.OnReadOperationCompleted(watch.ElapsedMilliseconds);
        }

        private static async Task DoWriteOperation(IGameActor gameActor, TestContext context, int actorIndex)
        {
            var status = new GameStatus()
            {
                Score = (context.Rand.Next(0, 100).ToString(CultureInfo.InvariantCulture))
            };

            var watch = Stopwatch.StartNew();
            await gameActor.UpdateGameStatus(status);
            watch.Stop();

            context.OnWriteOperationCompleted(watch.ElapsedMilliseconds);
        }
    }
}
