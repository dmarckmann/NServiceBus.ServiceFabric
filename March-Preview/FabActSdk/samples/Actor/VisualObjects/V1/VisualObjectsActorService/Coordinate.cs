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
    using System.Runtime.Serialization;
    using System.Text;

    [DataContract]
    public sealed class Coordinate
    {
        public Coordinate(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Coordinate(Coordinate other)
        {
            X = other.X;
            Y = other.Y;
            Z = other.Z;
        }

        public static Coordinate CreateRandom(Random rand = null)
        {
            if (rand == null)
            {
                rand = new Random((int)DateTime.Now.Ticks);
            }

            return new Coordinate(rand.NextDouble(), rand.NextDouble(), rand.NextDouble());
        }

        [DataMember]
        public double X { get; private set; }

        [DataMember]
        public double Y { get; private set; }

        [DataMember]
        public double Z { get; private set; }

        public string ToJson()
        {
            var sb = new StringBuilder();
            ToJson(sb);

            return sb.ToString();
        }

        public void ToJson(StringBuilder builder)
        {
            builder.AppendFormat(
                "{{ \"x\":{0}, \"y\":{1}, \"z\":{2} }}",
                X,
                Y,
                Z);
        }
    }
}