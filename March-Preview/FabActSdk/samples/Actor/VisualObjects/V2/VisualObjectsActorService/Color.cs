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
    public sealed class Color
    {
        public static double[][] CurrentColorsPalette = {
            new[] { 0.0, 0.0, 1.0, 0.0 },
            new[] { 0.0, 1.0, 0.0, 0.0 },
            new[] { 1.0, 0.0, 0.0, 0.0 }
        };

        public static double[][] HistoryColorsPalette = {
            new[] { 1.0, 0.0, 0.0, 0.0 },
            new[] { 1.0, 1.0, 0.0, 0.0 },
            new[] { 1.0, 1.0, 1.0, 0.0 }
        };

        public Color(double r, double g, double b, double a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public Color(Color other)
        {
            R = other.R;
            G = other.G;
            B = other.B;
            A = other.A;
        }

        public static Color CreateRandom(double[][] colorPalette, Random rand = null)
        {
            if (rand == null)
            {
                rand = new Random();
            }

            var colorIndex = rand.Next(colorPalette.GetLength(0));

            return new Color(
                r: colorPalette[colorIndex][0] + rand.NextDouble(),
                g: colorPalette[colorIndex][1] + rand.NextDouble(),
                b: colorPalette[colorIndex][2] + rand.NextDouble(),
                a: colorPalette[colorIndex][3] + rand.NextDouble());
        }

        [DataMember]
        public double R { get; private set; }

        [DataMember]
        public double G { get; private set; }

        [DataMember]
        public double B { get; private set; }

        [DataMember]
        public double A { get; private set; }


        public string ToJson()
        {
            var sb = new StringBuilder();
            ToJson(sb);

            return sb.ToString();
        }

        public void ToJson(StringBuilder builder)
        {
            builder.AppendFormat(
                "{{ \"r\":{0}, \"g\":{1}, \"b\":{2}, \"a\":{3} }}",
                R,
                G,
                B,
                A);
        }
    }
}