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
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;

    [DataContract]
    public class HeartbeatData
    {
        [DataMember]
        public Guid Game { get; set; }

        [DataMember]
        public GameStatus Status { get; private set; }

        public HeartbeatData()
        {
            Status = new GameStatus();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("Heartbeat:");
            sb.Append(",Game=").Append(Game);
            var playerList = Status.Players.ToArray();
            for (int i = 0; i < playerList.Length; i++)
            {
                sb.AppendFormat(",Player{0}=", i + 1).Append(playerList[i]);
            }
            sb.AppendFormat(",Score={0}", Status.Score);
            return sb.ToString();
        }
    }

    public static class HeartbeatDataDotNetSerializer
    {
        static readonly DataContractSerializer Serializer = new DataContractSerializer(typeof(HeartbeatData));

        public static byte[] Serialize(object o)
        {
            using (var memoryStream = new MemoryStream())
            {
                Serializer.WriteObject(memoryStream, o);
                memoryStream.Flush();
                return memoryStream.ToArray();
            }
        }

        public static HeartbeatData Deserialize(byte[] data)
        {
            using (var memoryStream = new MemoryStream(data))
            {
                return (HeartbeatData)Serializer.ReadObject(memoryStream);
            }
        }
    }
}
