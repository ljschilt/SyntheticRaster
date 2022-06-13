using System;
using System.Collections.Generic;
using System.Text;

namespace RasterCore
{
	class StationAndOffset
	{
		public double station { get; private set; }
		public double offset { get; private set; }
		public bool ProjectsOnSegment { get; private set; } = true;



		public StationAndOffset(RCPoint rasterPoint, List<RCPoint> RoadPoints)
		{
			double minimumStation = 0;
			double minimumOffset = 0;

			for (int lowestRoadPoint = 0; lowestRoadPoint < RoadPoints.Count - 1; lowestRoadPoint++)
			{
				double slope=0d, intercept=0d;
				CalculateLineEquation(RoadPoints[lowestRoadPoint], RoadPoints[lowestRoadPoint + 1], ref slope, ref intercept);

				double perpendicularSlope = 0d, perpendicularIntercept = 0d;
				CalculatePerpendicularLineEquation(slope, intercept, rasterPoint.X, rasterPoint.Y, ref perpendicularSlope, ref perpendicularIntercept);

				RCPoint intersectionPoint = CalculateIntersectionPoint(slope, intercept, perpendicularSlope, perpendicularIntercept);
				station = CalculateStation(intersectionPoint, lowestRoadPoint, RoadPoints);
				double beginStation = CalculateStation(RoadPoints[lowestRoadPoint], lowestRoadPoint, RoadPoints);
				double endStation = CalculateStation(RoadPoints[lowestRoadPoint + 1], lowestRoadPoint, RoadPoints);

				if (station > beginStation && station < endStation)
				{
					ProjectsOnSegment = true;
				}
				else
				{
					ProjectsOnSegment = false;
				}

				station = CalculateStation(intersectionPoint, lowestRoadPoint, RoadPoints);

				if (lowestRoadPoint == 0)
				{
					minimumOffset = offset;
					minimumStation = station;
				}
				else
				{
					if (offset < minimumOffset)
					{
						minimumOffset = offset;
						minimumStation = station;
					}
					else if (offset == minimumOffset && station < minimumStation)
					{
						minimumStation = station;
					}
				}
			}
		}

		public void CalculateLineEquation(RCPoint lowestRoadPoint, RCPoint highestRoadPoint,
			ref double slope, ref double intercept)
		{
			slope = (highestRoadPoint.Y - lowestRoadPoint.Y) / (highestRoadPoint.X - lowestRoadPoint.X);
			intercept = lowestRoadPoint.Y - slope * lowestRoadPoint.X;
		}

		public void CalculatePerpendicularLineEquation(double slope, double intercept, double x, double y, ref double perpendicularSlope, ref double perpendicularIntercept)
		{
			perpendicularSlope = -1.0 / slope;
			perpendicularIntercept = y - perpendicularSlope * x;
		}

		public RCPoint CalculateIntersectionPoint(double slope1, double intercept1, double slope2, double intercept2)
		{
			double X = -1.0 * (intercept1 - intercept2) / (slope1 - slope2);
			double Y = (slope1 * X) + intercept1;

			RCPoint intersectionPoint = new Point(X, Y);
			return intersectionPoint;
		}

		public double CalculateDistance(RCPoint point1, RCPoint point2)
		{
			double distance = Math.Sqrt(Math.Pow(point2.X - point1.X, 2) + Math.Pow(point2.Y - point1.Y, 2));
			if (((point2.Y - point1.Y) / (point2.X - point1.X)) > 0)
			{
				return distance;
			}
			else
			{
				return -1.0 * distance;
			}
		}

		public double CalculateStation(RCPoint Point, int RoadPoint, List<RCPoint> RoadPoints)
		{
			double station = 0;

			for (int segmentCounter = 0; segmentCounter < RoadPoint; segmentCounter++)
			{
				station += CalculateDistance(RoadPoints[segmentCounter], RoadPoints[segmentCounter + 1]);
			}

			station += CalculateDistance(RoadPoints[RoadPoint], Point);

			return station;
		}

		public static IReadOnlyList<StationAndOffset> CreateSOList(RCPoint rasterPoint, List<RCPoint> RoadPoints) {
			var soList = new List<StationAndOffset>();
			for (int i = 0; i < RoadPoints.Count - 1; i++) {
				var segment = new List<RCPoint> {RoadPoints[i], RoadPoints[i + 1] };
				var aSO = new StationAndOffset(rasterPoint, segment);
				soList.Add(aSO);
			}
			return soList;
		}
	}
}
