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
    using System.Threading.Tasks;

    class VisualObjectActor : Actor<VisualObject>, IVisualObject
    {
        private IActorTimer _updateTimer;
        private string _jsonString;

        public override Task OnActivateAsync()
        {
            if (State == null)
            {
                State = VisualObject.CreateRandom(Id.ToString(), new Random(Id.ToString().GetHashCode()));
            }

            _jsonString = State.ToJson();
            _updateTimer = RegisterTimer(
                MoveObject,                     // callback method
                null,                           // state to be passed to the callback method
                TimeSpan.FromMilliseconds(15),  // amount of time to delay before callback is invoked
                TimeSpan.FromMilliseconds(15)); // time interval between invocation of the callback method
            return base.OnActivateAsync();
        }

        public Task<string> GetStateAsJsonAsync()
        {
            return Task.FromResult(_jsonString);
        }

        public override Task OnDeactivateAsync()
        {
            if (_updateTimer != null)
            {
                UnregisterTimer(_updateTimer);
            }

            return base.OnDeactivateAsync();
        }

        private Task MoveObject(object state)
        {
            State.Move();
            _jsonString = State.ToJson();

            return Task.FromResult(true);
        }
    }
}
