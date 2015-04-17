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
    using System.Runtime.Serialization;
    using System.Text;

    [DataContract]
    public class VisualObject 
    {
        private const int HistoryLength = 20;

        public VisualObject(string name, Speed speed, Coordinate location, Color color, Color historyColor, double rotation = 0)
        {
            Name = name;
            Speed = speed;
            CurrentLocation = location;
            CurrentColor = color;
            HistoryColor = historyColor;
            Rotation = rotation;
            LocationHistory = new List<Coordinate>();
            HistoryStartIndex = -1;
        }

        public VisualObject(VisualObject other)
        {
            Name = other.Name;
            Speed = new Speed(other.Speed);

            CurrentLocation = new Coordinate(other.CurrentLocation);
            LocationHistory = new List<Coordinate>(other.LocationHistory.Count);
            foreach (var c in other.LocationHistory)
            {
                LocationHistory.Add(new Coordinate(c));
            }

            CurrentColor = new Color(other.CurrentColor);
            HistoryColor = new Color(other.HistoryColor);

            Rotation = other.Rotation;
        }

        public static VisualObject CreateRandom(string name, Random rand = null)
        {
            if (rand == null)
            {
                rand = new Random();
            }

            return new VisualObject(
                name,
                Speed.CreateRandom(rand),
                Coordinate.CreateRandom(rand),
                Color.CreateRandom(Color.CurrentColorsPalette, rand),
                Color.CreateRandom(Color.HistoryColorsPalette, rand));
        }

        public void Move()
        {
            if (LocationHistory.Count < HistoryLength)
            {
                HistoryStartIndex = (HistoryStartIndex + 1);
                LocationHistory.Add(new Coordinate(CurrentLocation));
            }
            else
            {
                HistoryStartIndex = (HistoryStartIndex + 1) % HistoryLength;
                LocationHistory[HistoryStartIndex] = new Coordinate(CurrentLocation);
            }

            var xSpeed = Speed.XSpeed;
            var ySpeed = Speed.YSpeed;
            var zSpeed = Speed.ZSpeed;

            var x = CurrentLocation.X + xSpeed;
            var y = CurrentLocation.Y + ySpeed;
            var z = CurrentLocation.Z + zSpeed;

            CurrentLocation = new Coordinate(
                CurrentLocation.X + xSpeed,
                CurrentLocation.Y + ySpeed,
                CurrentLocation.Z + zSpeed);

            // trim to edges
            Speed = new Speed(
                CheckForEdge(x, xSpeed),
                CheckForEdge(y, ySpeed),
                CheckForEdge(z, zSpeed));

            // add rotation
            Rotation += 10;
        }

        private static double CheckForEdge(double point, double speed)
        {
            if (point < -1.0 || point > 1.0)
            {
                return speed*-1.0;
            }

            return speed;
        }

        [DataMember]
        public string Name { get; private set; }

        [DataMember]
        public Speed Speed { get; private set; }

        [DataMember(Name = "current")]
        public Coordinate CurrentLocation { get; set; }

        [DataMember]
        public Color CurrentColor { get; set; }

        [DataMember]
        public Color HistoryColor { get; set; }

        [DataMember]
        public int HistoryStartIndex { get; set; }

        [DataMember(Name = "history")]
        public List<Coordinate> LocationHistory { get; private set; }

        [DataMember]
        public double Rotation { get; set; }

        public string ToJson()
        {
            var sb = new StringBuilder();
            ToJson(sb);

            return sb.ToString();
        }

        public void ToJson(StringBuilder builder)
        {
            builder.Append("{");

            {
                builder.Append("\"current\":");
                CurrentLocation.ToJson(builder);
            }

            {
                builder.Append(", \"history\":");
                builder.Append("[");
                var currentIndex = HistoryStartIndex;
                if (currentIndex != -1)
                {
                    bool first = true;
                    do
                    {
                        currentIndex++;
                        if (currentIndex == LocationHistory.Count)
                        {
                            currentIndex = 0;
                        }

                        if (first)
                        {
                            first = false;
                        }
                        else
                        {
                            builder.Append(", ");
                        }

                        LocationHistory[currentIndex].ToJson(builder);
                    }
                    while (currentIndex != HistoryStartIndex);
                }
                builder.Append("]");
            }

            {
                builder.Append(", \"currentColor\":");
                CurrentColor.ToJson(builder);
            }

            {
                builder.Append(", \"historyColor\":");
                HistoryColor.ToJson(builder);
            }

            {
                builder.Append(", \"rotation\":");
                builder.Append(Rotation);
            }

            builder.Append("}");
        }
    }
}