﻿//-----------------------------------------------------------------------
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
    using System.Threading.Tasks;
    using System.Collections.Generic;

    public class VoicemailBoxActor : Actor<VoicemailBox>, IVoicemailBoxActor
    {
        public Task<List<Voicemail>> GetMessagesAsync()
        {
            return Task.FromResult(State.MessageList);
        }

        public Task<string> GetGreetingAsync()
        {
            return Task.FromResult(
                string.IsNullOrEmpty(State.Greeting) ?
                "No one is available, please leave a message after the beep." :
                State.Greeting);
        }

        public Task LeaveMessageAsync(string message)
        {
            State.MessageList.Add(
                new Voicemail
                {
                    Id = Guid.NewGuid(),
                    Message = message,
                    ReceivedAt = DateTime.Now
                });

            return Task.FromResult(true);
        }

        public Task SetGreetingAsync(string greeting)
        {
            State.Greeting = greeting;

            return Task.FromResult(true);
        }

        public Task DeleteMessageAsync(Guid messageId)
        {
            for (var i = 0; i < State.MessageList.Count; i++)
            {
                if (State.MessageList[i].Id.Equals(messageId))
                {
                    State.MessageList.RemoveAt(i);
                    break;
                }
            }

            return Task.FromResult(true);
        }

        public Task DeleteAllMessagesAsync()
        {
            State.MessageList.Clear();

            return Task.FromResult(true);
        }
    }
}