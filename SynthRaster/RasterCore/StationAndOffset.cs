using System;
using System.Collections.Generic;
using System.Text;

namespace RasterCore
{
	class StationAndOffset
	{
		public (double station, double offset) CalculateStationAndOffset(RCPoint rasterPoint, List<RCPoint> RoadPoints)
		{
			double minimumStation = 0;
			double minimumOffset = 0;

			for (int lowestRoadPoint = 0; lowestRoadPoint < RoadPoints.Count - 1; lowestRoadPoint++)
			{
				(double, double) roadEquation = CalculateLineEquation(RoadPoints[lowestRoadPoint], RoadPoints[lowestRoadPoint + 1]);

				(double, double) perpendicularEquation = CalculatePerpendicularLineEquation(roadEquation.Item1, roadEquation.Item2, rasterPoint.X, rasterPoint.Y);

				RCPoint intersectionPoint = CalculateIntersectionPoint(roadEquation.Item1, roadEquation.Item2, perpendicularEquation.Item1, perpendicularEquation.Item2);

				double offset = CalculateDistance(intersectionPoint, rasterPoint);

				double station = 0;
				for (int segmentCounter = 0; segmentCounter < lowestRoadPoint; segmentCounter++)
				{
					station += CalculateDistance(RoadPoints[segmentCounter], RoadPoints[segmentCounter + 1]);
				}

				station += CalculateDistance(RoadPoints[lowestRoadPoint], intersectionPoint);

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

			return (minimumStation, minimumOffset);
		}

		public (double slope, double yIntercept) CalculateLineEquation(RCPoint lowestRoadPoint, RCPoint highestRoadPoint)
		{
			// Step 1: Calculate the slope m using y = mx + b
			double slope = (highestRoadPoint.Y - lowestRoadPoint.Y) / (highestRoadPoint.X - lowestRoadPoint.X);

			// Step 2: Use the slope to calculate b, the y-intercept.
			double intercept = lowestRoadPoint.Y - slope * lowestRoadPoint.X;

			return (slope, intercept);
		}

		public (double slope, double yIntercept) CalculatePerpendicularLineEquation(double slope, double intercept, double x, double y)
		{
			// Step 1: Take the negative reciprocal of the slope.
			double perpendicularSlope = -1.0 / slope;

			// Step 2: Find the intercept using the X and Y values of the raster point.
			double perpendicularIntercept = y - perpendicularSlope * x;

			return (perpendicularSlope, perpendicularIntercept);
		}

		public RCPoint CalculateIntersectionPoint(double slope1, double intercept1, double slope2, double intercept2)
		{
			// Step 1: Calculate the X value.
			double X = -1.0 * (intercept1 - intercept2) / (slope1 - slope2);

			// Step 2: Calculate the Y value.
			double Y = (slope1 * X) + intercept1;

			RCPoint intersectionPoint = new Point(X, Y);

			return intersectionPoint;
		}

		public double CalculateDistance(RCPoint point1, RCPoint point2)
		{
			return Math.Sqrt(Math.Pow(point2.X - point1.X, 2) + Math.Pow(point2.Y - point1.Y, 2));
		}
	}
}
