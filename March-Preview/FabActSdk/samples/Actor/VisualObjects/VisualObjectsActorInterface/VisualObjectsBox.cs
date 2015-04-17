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
    using System.Linq;
    using System.Collections.Generic;
    using System.Fabric.Actors;
    using System.Globalization;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class VisualObjectsBox
    {
        private readonly Dictionary<ActorId, string> _objectBuffers;
        private readonly string _applicationName;
        private readonly IEnumerable<ActorId> _objectIds;
        private static readonly TimeSpan RefreshInterval = TimeSpan.FromMilliseconds(15);
        private static ReaderWriterLockSlim _rwLock;

        public VisualObjectsBox(string applicationName, int numObjects = 5)
        {
            _objectBuffers = new Dictionary<ActorId, string>();
            _rwLock = new ReaderWriterLockSlim();
            _applicationName = applicationName;
            _objectIds = GetVisualObjectIds(numObjects);
        }

        public string GetContents()
        {
            var builder = new StringBuilder();
            builder.Append("[ ");
            var first = true;
            try
            {
                _rwLock.EnterReadLock();
                foreach (var val in _objectBuffers.Values)
                {
                    if (!first)
                    {
                        builder.Append(", ");
                    }
                    else
                    {
                        first = false;
                    }

                    builder.Append(val);
                }
            }
            finally
            {
                _rwLock.ExitReadLock();
            }

            builder.Append("]");
            return builder.ToString();
        }

        public void GetContentsTable(Dictionary<ActorId, string> contentsTable)
        {
            try
            {
                _rwLock.EnterReadLock();
                foreach (var k in _objectBuffers.Keys)
                {
                    contentsTable[k] = _objectBuffers[k];
                }
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }

        public Task RefreshContentsAsync(CancellationToken cancellationToken)
        {
            var tasks = _objectIds.Select(objectId => RefreshObjectAsync(objectId, cancellationToken)).ToList();
            return Task.WhenAll(tasks);
        }

        private async Task RefreshObjectAsync(ActorId objectId, CancellationToken cancellationToken)
        {
            var actorProxy = ActorProxy.Create<IVisualObject>(objectId, _applicationName);
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var buffer = await actorProxy.GetStateAsJsonAsync();
                    if (!string.IsNullOrEmpty(buffer))
                    {
                        Update(objectId, buffer);
                    }
                    await Task.Delay(RefreshInterval, cancellationToken);
                }
                catch (Exception)
                {
                    // ignore the exceptions
                }
            }
        }

        private void Update(ActorId actorId, string buffer)
        {
            try
            {
                _rwLock.EnterWriteLock();
                _objectBuffers[actorId] = buffer;
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }

        }

        private static IEnumerable<ActorId> GetVisualObjectIds(int numObjects)
        {
            var actorIds = new ActorId[numObjects];
            for (var i = 0; i < actorIds.Length; i++)
            {
                actorIds[i] = new ActorId(string.Format(CultureInfo.InvariantCulture, "Visual Object # {0}", i));
            }

            return actorIds;
        }
    }
}