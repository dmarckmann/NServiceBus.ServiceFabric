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
    using System.Fabric.Services;
    using System.Fabric.Actors;
    using System.Threading.Tasks;
    
    class PlayerActor : Actor<PlayerState>, IPlayerActor
    {
        public override Task OnActivateAsync()
        {
            if (State == null)
            {
                State = new PlayerState();
            }

            return base.OnActivateAsync();
        } 

        public Task<IGameActor> GetCurrentGame()
        {
            return Task.FromResult(State.CurrentGame);
        }

        public Task JoinGame(IGameActor game)
        {
            State.CurrentGame = game;
            return Task.FromResult(true);
        }

        public Task LeaveGame(IGameActor game)
        {
            State.CurrentGame = null;
            return Task.FromResult(true);
        }
    }
}
