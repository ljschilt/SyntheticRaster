using System;
using System.Collections.Generic;
using System.Text;

namespace RasterCore
{
    class StationAndOffset : Vector
    {
        public double station { get; private set; }
        public double offset { get; private set; }
        public bool ProjectsOnSegment { get; private set; } = true;

        public StationAndOffset(RCPoint rasterPoint, int firstSegmentPoint, List<RCPoint> RoadPoints)
        {
            // Step 1: Calculate the vector and unit vector of the road segment's points.
            Vector roadVector = new Vector(RoadPoints[firstSegmentPoint], RoadPoints[firstSegmentPoint + 1]);
            double roadSegmentLength = Magnitude(roadVector);
            _ = new Vector();
            Vector roadUnitVector = UnitVector(roadVector);

            Vector rasterVector = new Vector(RoadPoints[firstSegmentPoint], rasterPoint);
            double rasterVectorLength = Magnitude(rasterVector);

            // Step 2: Calculate the angle in between the road vector and raster vector.
            double angle = Math.Acos(DotProduct(rasterVector, roadVector) / (Magnitude(rasterVector) * Magnitude(roadVector)));

            // Step 3: Calculate the offset and station.
            offset = 0;
            station = CalculateStation(firstSegmentPoint, RoadPoints);
            if (angle <= (Math.PI / 2))
            {
                offset = rasterVectorLength * Math.Sin(angle);
                station += rasterVectorLength * Math.Cos(angle);
            }
            else
            {
                angle = Math.PI - angle;
                offset = rasterVectorLength * Math.Sin(angle);
                station -= rasterVectorLength * Math.Cos(angle);
            }

            // Step 4: Determine if the point is projected off of the line segment. Update the boolean ProjectsOnSegment
            double beginStation = CalculateStation(firstSegmentPoint, RoadPoints);
            double endStation = CalculateStation(firstSegmentPoint + 1, RoadPoints);

            ProjectsOnSegment = station >= beginStation && station <= endStation;
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

        public static IReadOnlyList<StationAndOffset> CreateSOList(RCPoint rasterPoint, List<RCPoint> RoadPoints) {
            var soList = new List<StationAndOffset>();
            for (int i = 0; i < RoadPoints.Count - 1; i++) {
                var aSO = new StationAndOffset(rasterPoint, i, RoadPoints);

                double testOffset = aSO.offset;
                double testStation = aSO.station;

                soList.Add(aSO);
            }
            return soList;
        }
    }
}
