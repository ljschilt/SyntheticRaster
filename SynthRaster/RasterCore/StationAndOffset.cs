using System;
using System.Collections.Generic;

namespace RasterCore
{
    class StationAndOffset : Vector
    {
        public double Station { get; private set; }
        public double Offset { get; private set; }
        public bool ProjectsOnSegment { get; private set; } = true;
        public bool ProjectsOnEndCap { get; private set; } = false;

        public StationAndOffset(RCPoint rasterPoint, int firstSegmentPoint, List<RCPoint> RoadPoints)
        {
            // Step 1: Calculate the vector and unit vector of the road segment's points.
            Vector roadVector = new Vector(RoadPoints[firstSegmentPoint], RoadPoints[firstSegmentPoint + 1]);
            double roadSegmentLength = Magnitude(roadVector);

            Vector rasterVector = new Vector(RoadPoints[firstSegmentPoint], rasterPoint);
            double rasterVectorLength = Magnitude(rasterVector);

            // Step 2: Calculate the angle in between the road vector and raster vector.
            double angle = Math.Acos(DotProduct(rasterVector, roadVector) / (rasterVectorLength * roadSegmentLength));

            // Step 3: Calculate the offset and station.
            Offset = 0;
            Station = CalculateStation(firstSegmentPoint, RoadPoints);
            if (angle <= (Math.PI / 2))
            {
                Offset = rasterVectorLength * Math.Sin(angle);
                Station += rasterVectorLength * Math.Cos(angle);
            }
            else
            {
                angle = Math.PI - angle;
                Offset = rasterVectorLength * Math.Sin(angle);
                Station -= rasterVectorLength * Math.Cos(angle);
            }

            // Step 4: Determine if the point is projected off of the line segment. Update the boolean ProjectsOnSegment
            double beginStation = CalculateStation(firstSegmentPoint, RoadPoints);
            double endStation = CalculateStation(firstSegmentPoint + 1, RoadPoints);

            ProjectsOnSegment = Station >= beginStation && Station <= endStation;

            if (!ProjectsOnSegment)
            {
                if (Station < beginStation)
                {
                    Offset = CalculateDistance(rasterPoint, RoadPoints[firstSegmentPoint]);

                    if (firstSegmentPoint == 0)
                    {
                        ProjectsOnEndCap = true;
                    }
                }
                else
                {
                    Offset = CalculateDistance(rasterPoint, RoadPoints[firstSegmentPoint + 1]);

                    if (firstSegmentPoint == RoadPoints.Count - 2)
                    {
                        ProjectsOnEndCap = true;
                    }
                }
            }

            
    }

        public double CalculateDistance(RCPoint point1, RCPoint point2)
        {
            return Math.Sqrt(Math.Pow(point2.X - point1.X, 2) + Math.Pow(point2.Y - point1.Y, 2));
        }

        public double CalculateStation(int RoadPoint, List<RCPoint> RoadPoints)
        {
            double station = 0;

            for (int segmentCounter = 0; segmentCounter < RoadPoint; segmentCounter++)
            {
                station += CalculateDistance(RoadPoints[segmentCounter], RoadPoints[segmentCounter + 1]);
            }

            return station;
        }

        public static IReadOnlyList<StationAndOffset> CreateSOList(RCPoint rasterPoint, List<RCPoint> RoadPoints)
        {
            var soList = new List<StationAndOffset>();
            for (int i = 0; i < RoadPoints.Count - 1; i++)
            {
                var aSO = new StationAndOffset(rasterPoint, i, RoadPoints);
                soList.Add(aSO);
            }
            return soList;
        }
    }
}
