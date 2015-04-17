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
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Fabric.Actors;

    class TestContext
    {
        public TestResults Results { get; private set; }

        public TestSpecifications Specifications { get; private set; }

        public Guid[] PlayerActorIds { get; private set; }

        public Guid[] GameActorIds { get; private set; }

        public long[] PresenceActorIds { get; private set; }

        public Stopwatch Watch { get; private set; }

        public Random Rand { get; private set; }

        public string ApplicationName { get; private set; }

        public TestContext(TestSpecifications specifications)
        {
            Specifications = specifications;
            ApplicationName = specifications.ApplicationName;
            Results = new TestResults();
            PlayerActorIds = GenerateGuIds(specifications.NumActorsPerThread);
            GameActorIds = GenerateGuIds(specifications.NumActorsPerThread);
            PresenceActorIds = GenerateIds(specifications.NumActorsPerThread);
            Watch = new Stopwatch();
            Rand = new Random();
        }

        public void OnReadOperationCompleted(long timeMillis)
        {
            OnOperationCompleted(Results.ReadOperationStats, timeMillis);
        }

        public void OnWriteOperationCompleted(long timeMillis)
        {
            OnWriteOperationCompleted(timeMillis, 0, 0);
        }

        public void OnWriteOperationCompleted(long timeMillis, int additionalWriteOperationsCount, int additionalReadOperationsCount)
        {
            OnOperationCompleted(Results.WriteOperationStats, timeMillis);
            if (additionalWriteOperationsCount != 0)
            {
                Interlocked.Increment(ref Results.WriteOperationStats.TotalOperationsPerformed);
            }

            if (additionalReadOperationsCount != 0)
            {
                Interlocked.Increment(ref Results.ReadOperationStats.TotalOperationsPerformed);
            }
        }

        private static void OnOperationCompleted(OperationStats operationStats, long timeMillis)
        {
            Interlocked.Increment(ref operationStats.TotalOperationsPerformed);
            var done = false;
            while (!done)
            {
                var existing = Interlocked.Read(ref operationStats.MinElaspedTimeMillis);
                done = (timeMillis >= existing);
                if (!done)
                {
                    done = (timeMillis ==
                            (Interlocked.CompareExchange(ref operationStats.MinElaspedTimeMillis,
                                timeMillis, existing)));
                }
            }
            done = false;
            while (!done)
            {
                var existing = Interlocked.Read(ref operationStats.MaxElaspedTimeMillis);
                done = (timeMillis <= existing);
                if (!done)
                {
                    done = (timeMillis ==
                            (Interlocked.CompareExchange(ref operationStats.MaxElaspedTimeMillis,
                                timeMillis, existing)));
                }
            }
        }

        public static long[] GenerateIds(long actors)
        {
            var retVal = new long[actors];
            Parallel.For(0, actors, (i) =>
            {
                retVal[i] = (long)CRC64.ToCRC64(Guid.NewGuid().ToByteArray());
            });
            return retVal;
        }

        public static Guid[] GenerateGuIds(long actors)
        {
            var retVal = new Guid[actors];
            Parallel.For(0, actors, (i) =>
            {
                retVal[i] = Guid.NewGuid();
            });
            return retVal;
        }
    }
}
