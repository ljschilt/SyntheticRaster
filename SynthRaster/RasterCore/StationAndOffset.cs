using System;
using System.Collections.Generic;

namespace RasterCore
{
    /// <summary>
    /// Station and Offset is a relative coordinate system in accordance with a given baseline.
    /// Station notes the distance along the baseline, and offset notes the Euclidean distance
    /// from the baseline to the given point. The baselines here are reaches, or sections of the
    /// road network split up by intersection points. This program calculates the station and offset
    /// from a given segment in the reach and the location of the given raster cell. It uses
    /// the station to calculate if the segment projects onto the raster cell.
    /// </summary>
    internal class StationAndOffset : Vector
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
                    if (firstSegmentPoint == 0) { ProjectsOnEndCap = true; }
                }
                else
                {
                    Offset = CalculateDistance(rasterPoint, RoadPoints[firstSegmentPoint + 1]);
                    if (firstSegmentPoint == RoadPoints.Count - 2) { ProjectsOnEndCap = true; }
                }
            }
        }

        /// <summary>
        /// Calculate the Euclidean distance between two points.
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
        public double CalculateDistance(RCPoint point1, RCPoint point2)
        {
            return Math.Sqrt(Math.Pow(point2.X - point1.X, 2) + Math.Pow(point2.Y - point1.Y, 2));
        }


        /// <summary>
        /// Calculate the station of a given point, where zero is the starting point of the entire reach.
        /// </summary>
        /// <param name="RoadPoint"></param>
        /// <param name="RoadPoints"></param>
        /// <returns></returns>
        public double CalculateStation(int RoadPoint, List<RCPoint> RoadPoints)
        {
            double station = 0;
            for (int segmentCounter = 0; segmentCounter < RoadPoint; segmentCounter++)
            {
                station += CalculateDistance(RoadPoints[segmentCounter], RoadPoints[segmentCounter + 1]);
            }
            return station;
        }

        /// <summary>
        /// Create a list of stations and offsets representing all of the projections from the reach
        /// onto the given raster cell.
        /// </summary>
        /// <param name="rasterPoint"></param>
        /// <param name="RoadPoints"></param>
        /// <returns></returns>
        public static IReadOnlyList<StationAndOffset> CreateSOList(RCPoint rasterPoint, List<RCPoint> RoadPoints)
        {
            List<StationAndOffset> soList = new List<StationAndOffset>();
            for (int i = 0; i < RoadPoints.Count - 1; i++)
            {
                StationAndOffset aSO = new StationAndOffset(rasterPoint, i, RoadPoints);
                soList.Add(aSO);
            }
            return soList;
        }
    }
}
