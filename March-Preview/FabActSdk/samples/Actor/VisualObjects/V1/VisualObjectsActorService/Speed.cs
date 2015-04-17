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

    [DataContract]
    public class Speed
    {
        public Speed(Speed other)
        {
            XSpeed = other.XSpeed;
            YSpeed = other.YSpeed;
            ZSpeed = other.ZSpeed;
        }

        public Speed(double x, double y, double z)
        {
            XSpeed = x;
            YSpeed = y;
            ZSpeed = z;
        }

        public static Speed CreateRandom(Random rand = null)
        {
            if (rand == null)
            {
                rand = new Random((int)DateTime.Now.Ticks);
            }

            return new Speed(rand.NextDouble() * 0.03, rand.NextDouble() * 0.03, rand.NextDouble() * 0.03);
        }

        [DataMember]
        public double XSpeed { get; private set; }

        [DataMember]
        public double YSpeed { get; private set; }

        [DataMember]
        public double ZSpeed { get; private set; }
    }
}