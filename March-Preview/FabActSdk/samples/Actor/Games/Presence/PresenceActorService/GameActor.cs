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
    using System.Threading.Tasks;

    class GameActor : Actor<GameState>, IGameActor
    {
        public override Task OnActivateAsync()
        {
            if (State == null)
            {
                State = new GameState();
            }

            State.Players = new HashSet<Guid>();
            State.Status = new GameStatus {Score = "0.0"};

            return base.OnActivateAsync();
        }

        public async Task UpdateGameStatus(GameStatus newStatus)
        {
            State.Status = newStatus;

            // Check for new players that joined since last update
            foreach (var player in newStatus.Players)
            {
                if (!State.Players.Contains(player))
                {
                    try
                    {
                        // Here we call player grains serially, which is less efficient than a fan-out but simpler to express.
                        await ActorProxy.Create<IPlayerActor>(new ActorId(player)).JoinGame(this);
                        State.Players.Add(player);
                    }
                    catch
                    {
                        // Ignore exceptions while telling player grains to join the game. 
                        // Since we didn't add the player to the list, this will be tried again with next update.
                    }
                }
            }

            // Check for players that left the game since last update
            var promises = new List<Task>();
            foreach (var player in State.Players)
            {
                if (!newStatus.Players.Contains(player))
                {
                    try
                    {
                        // Here we do a fan-out with multiple calls going out in parallel. We join the promisses later.
                        // More code to write but we get lower latency when calling multiple player grains.
                        promises.Add(ActorProxy.Create<IPlayerActor>(new ActorId(player)).LeaveGame(this));
                        State.Players.Remove(player);
                    }
                    catch
                    {
                        // Ignore exceptions while telling player grains to leave the game.
                        // Since we didn't remove the player from the list, this will be tried again with next update.
                    }
                }
            }

            // Joining promises
            await Task.WhenAll(promises);
            
            // send score updated events to the subscribers
            // uncomment the line below to ensure that only the quorum acked score is published
            // await this.SaveStateAsync();

            var ev = GetEvent<IGameEvents>();
            ev.GameScoreUpdated(Id.GetGuidId(), State.Status.Score);
        }

        public Task<string> GetGameScore()
        {
            return Task.FromResult(State.Status.Score);
        }
    }
}