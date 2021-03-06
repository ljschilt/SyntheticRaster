using System.Linq;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Core.Geometry;
using RasterCore;
using System.Collections.Generic;
using LineSegment = RasterCore.LineSegment;
using System;

namespace RasterArc.Models
{
    internal class GeometryReader
    {
        public List<AnchorPoint> AnchorPoints { get; set; }

        public List<List<LineSegment>> RoadNetwork { get; private set; }

        public GeometryReader(double cellSize, string layerName)
        {
            List<LineSegment> Lines = new List<LineSegment>();
            FeatureLayer layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>()
                .FirstOrDefault(L => L.Name == layerName);

            if (!(layer.GetTable() is FeatureClass fc))
            {
                return;
            }

            using (RowCursor cursor = fc.Search())
            {
                int polylineCounter = 0;
                int segmentCounter = 0;
                while (cursor.MoveNext())
                {
                    using (Feature feature = cursor.Current as Feature)
                    {
                        if (feature == null)
                        {
                            continue;
                        }

                        Polyline polyline = feature.GetShape() as Polyline;
                        ReadOnlyPartCollection polylineParts = polyline.Parts;
                        IEnumerator<ReadOnlySegmentCollection> cur = polylineParts.GetEnumerator();

                        while (cur.MoveNext())
                        {
                            ReadOnlySegmentCollection seg = cur.Current;
                            foreach (Segment s in seg)
                            {
                                Point startPoint = new Point(s.StartPoint.X, s.StartPoint.Y);
                                Point endPoint = new Point(s.EndPoint.X, s.EndPoint.Y);
                                LineSegment newLine = new LineSegment(startPoint, endPoint);
                                Lines.Add(newLine);
                                segmentCounter++;
                            }
                        }
                        polylineCounter++;
                    }
                }
            }
            SortSegmentsGeometrically(cellSize, Lines);
        }

        public void SortSegmentsGeometrically(double cellSize, List<LineSegment> Lines)
        {
            RoadNetwork = new List<List<LineSegment>>();

            // Create the dictionary
            Dictionary<(double X, double Y), List<LineSegment>> d = new Dictionary<(double X, double Y), List<LineSegment>>();

            double beginX;
            double beginY;
            double endX;
            double endY;
            (double X, double Y) key;

            // Populate the dictionary
            foreach (LineSegment aLine in Lines)
            {
                beginX = (double)(aLine.BeginPoint.X / cellSize * 100);
                beginY = (double)(aLine.BeginPoint.Y / cellSize * 100);
                key = (beginX, beginY);
                if (!d.ContainsKey(key)) { d[key] = new List<LineSegment>(); }

                bool alreadyInThere = Convert.ToBoolean(d[key].Where(item => item.UniqueString == aLine.UniqueString).Count());
                if(!alreadyInThere) { d[key].Add(aLine); }

                endX = (double)(aLine.EndPoint.X / cellSize * 100);
                endY = (double)(aLine.EndPoint.Y / cellSize * 100);
                key = (endX, endY);
                if (!d.ContainsKey(key)) { d[key] = new List<LineSegment>(); }

                alreadyInThere = Convert.ToBoolean(d[key].Where(item => item.UniqueString == aLine.UniqueString).Count());
                if (!alreadyInThere) { d[key].Add(aLine); }
            }

            // Start at a terminal point by declaring a first list of line segments and a line segment.
            List<LineSegment> currentLine = new List<LineSegment>();
            LineSegment currentSegment = new LineSegment(new Point(0, 0), new Point(0, 0));
            (double X, double Y) currentKey = (0, 0);

            foreach (KeyValuePair<(double X, double Y), List<LineSegment>> dItem in d)
            {
                if (dItem.Value.Count == 1)
                {
                    currentSegment = dItem.Value[0];
                    dItem.Value[0].IsChecked = true;
                    currentKey = dItem.Key;
                    break;
                }
            }

            Point currentPoint = new Point(currentKey.X, currentKey.Y);
            if (currentPoint == currentSegment.EndPoint) { currentSegment.SwapDirection(); }
            currentLine.Add(currentSegment);
            List<LineSegment> currentList = new List<LineSegment>();

            // Continue on the path until an intersection point is reached.
            List<LineSegment> firstAnchorList = new List<LineSegment> { new LineSegment(new Point(0,0), new Point(1,1)) };
            bool hitIntersection = false;
            for (int counter = 1; counter < Lines.Count; counter++)
            {
                foreach (KeyValuePair<(double, double), List<LineSegment>> dItem in d)
                {
                    if (hitIntersection) { break; }

                    int segmentCount = dItem.Value.Count;

                    if (segmentCount == 2 && (dItem.Value[0] == currentSegment || dItem.Value[1] == currentSegment) && dItem.Key != currentKey)
                    {
                        currentKey = dItem.Key;
                        currentPoint = new Point(currentKey.X, currentKey.Y);

                        if (currentSegment == dItem.Value[0])
                        {
                            currentSegment = dItem.Value[1];
                            dItem.Value[1].IsChecked = true;
                        }
                        else
                        {
                            currentSegment = dItem.Value[0];
                            dItem.Value[0].IsChecked = true;
                        }

                        if (currentPoint == currentSegment.EndPoint) { currentSegment.SwapDirection(); }
                        currentLine.Add(currentSegment);
                        continue;
                    }

                    else if (segmentCount > 2 && dItem.Key != currentKey)
                    {
                        if (hitIntersection) { break; }

                        for (int i = 0; i < segmentCount; i++) // Check every segment
                        {
                            LineSegment segment = dItem.Value[i];
                            if (segment == currentSegment) // For the segment that shares the point.
                            {
                                currentKey = dItem.Key;
                                currentPoint = new Point(currentKey.X, currentKey.Y);
                                currentList = dItem.Value;
                                hitIntersection = true;
                                break;
                            }
                        }
                    }
                }
            }

            RoadNetwork.Add(currentLine);

            // If the previous line did not hit an intersection, then let the fun begin!
            if (hitIntersection)
            {
                // Set the first anchor point
                AnchorPoints = new List<AnchorPoint>();
                AnchorPoint firstAnchorPoint = new AnchorPoint(currentKey, currentList);
                AnchorPoints.Add(firstAnchorPoint);
                int currentAnchorPoint = 0;

                while (AnchorPoints[0].BranchesLeft())
                {
                    if (!AnchorPoints[currentAnchorPoint].BranchesLeft())
                    {
                        // Set the current anchor point to the next point that has unchecked branches
                        do { currentAnchorPoint--; } while (!AnchorPoints[currentAnchorPoint].BranchesLeft());

                        // Subtract one from the current anchor point's unchecked branches.
                        AnchorPoints[currentAnchorPoint].UncheckedBranches -= 1;
                    }
                    else
                    {
                        // Reset the location to be the current anchor point.
                        currentKey = AnchorPoints[currentAnchorPoint].Location;
                        currentPoint = new Point(currentKey.X, currentKey.Y);

                        // Pick an unchecked branch

                        foreach (LineSegment segment in AnchorPoints[currentAnchorPoint].IntersectingLines)
                        {
                            //(int X, int Y) = ((int)(segment.BeginPoint.X / cellSize * 100), (int)(segment.BeginPoint.Y / cellSize * 100));
                            //(int X, int Y) endKey = ((int)(segment.EndPoint.X / cellSize * 100), (int)(segment.EndPoint.Y / cellSize * 100));
                            //  && !(X == endKey.X && Y == endKey.Y)
                            if (!segment.IsChecked)
                            {
                                currentSegment = segment;

                                // Check the direction and swap if needed
                                Point AnchorPointCheck = new Point(AnchorPoints[currentAnchorPoint].Location.X, AnchorPoints[currentAnchorPoint].Location.Y);
                                Point simplifiedEndPoint = new Point((double) (currentSegment.EndPoint.X / cellSize * 100), (double)(currentSegment.EndPoint.Y / cellSize * 100));
                                if (simplifiedEndPoint.X == AnchorPointCheck.X && simplifiedEndPoint.Y == AnchorPointCheck.Y)
                                {
                                    currentSegment.SwapDirection();
                                }

                                // Create a new list of line segments for the new branch.
                                currentLine = new List<LineSegment>();
                                segment.IsChecked = true;
                                // Add the first segment to the list
                                currentLine.Add(currentSegment);
                                break;
                            }
                        }

                        int segmentCount = 2;
                        bool notACriticalPoint = true;
                        while (notACriticalPoint)
                        {
                            // Continue along the path, adding segments to the list of line segments
                            int iterationCounter = 0;
                            foreach (KeyValuePair<(double, double), List<LineSegment>> dItem in d)
                            {
                                if (!notACriticalPoint) { break; }
                                if (iterationCounter == d.Count)
                                {
                                    throw new Exception("Error: System could not process the " +
                                        "intersection at" + currentKey.X + ", " + currentKey.Y + ".");
                                }
                                iterationCounter++;

                                segmentCount = dItem.Value.Count;

                                if (segmentCount == 2 && (dItem.Value[0] == currentSegment ||
                                    dItem.Value[1] == currentSegment) && dItem.Key != currentKey)
                                {
                                    currentKey = dItem.Key;
                                    currentPoint = new Point(currentKey.X, currentKey.Y);

                                    if (currentSegment == dItem.Value[0])
                                    {
                                        currentSegment = dItem.Value[1];
                                        dItem.Value[1].IsChecked = true;
                                    }
                                    else
                                    {
                                        currentSegment = dItem.Value[0];
                                        dItem.Value[0].IsChecked = true;
                                    }

                                    Point simplifiedEndPoint = new Point((double)(currentSegment.EndPoint.X / cellSize * 100), (double)(currentSegment.EndPoint.Y / cellSize * 100));
                                    if (currentPoint.X == simplifiedEndPoint.X && currentPoint.Y == simplifiedEndPoint.Y)
                                    {
                                        currentSegment.SwapDirection();
                                    }
                                    currentLine.Add(currentSegment);
                                    continue;
                                }
                                else if (segmentCount != 2 && dItem.Key != currentKey)
                                {
                                    for (int i = 0; i < segmentCount; i++)
                                    {
                                        LineSegment segment = dItem.Value[i];
                                        if (segment == currentSegment)
                                        {
                                            currentKey = dItem.Key;
                                            currentList = dItem.Value;
                                            notACriticalPoint = false;
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        if (segmentCount == 1) // If the next point is a terminal point
                        {
                            // Add the list to the road network.
                            RoadNetwork.Add(currentLine);

                            // Subtract one from unchecked branches for the current anchor point.
                            AnchorPoints[currentAnchorPoint].UncheckedBranches -= 1;
                        }
                        else if (segmentCount >= 2 && CheckAnchorPoint(currentKey) != -1) // If the next point is an anchor point
                        {
                            // Add the list to the road network.
                            RoadNetwork.Add(currentLine);

                            // Identify the anchor point that was hit
                            // Subtract one from unchecked branches for both the current anchor point and the anchor point hit.
                            AnchorPoints[currentAnchorPoint].UncheckedBranches -= 1;
                            AnchorPoints[CheckAnchorPoint(currentKey)].UncheckedBranches -= 1;
                        }
                        else // The next point is an intersection point
                        {
                            // Add the list to the road network.
                            RoadNetwork.Add(currentLine);

                            // Create a new anchor point
                            AnchorPoint newAnchorPoint = new AnchorPoint(currentKey, currentList);
                            AnchorPoints.Add(newAnchorPoint);

                            // Set the current anchor point to be this new anchor point.
                            currentAnchorPoint = AnchorPoints.FindIndex(a => a.Equals(newAnchorPoint));
                        }
                    }
                }
            }
        }

        public int CheckAnchorPoint((double, double) key)
        {
            return AnchorPoints.FindIndex(a => a.OnLocation(key));
        }

        public List<List<RCPoint>> CreateRoadNetworkList()
        {
            List<List<RCPoint>> RoadNetworkPoints = new List<List<RCPoint>>();

            for (int i = 0; i < RoadNetwork.Count; i++)
            {
                List<RCPoint> RoadPoints = new List<RCPoint>
                {
                    RoadNetwork[i][0].BeginPoint
                };

                for (int j = 0; j < RoadNetwork[i].Count; j++)
                {
                    RoadPoints.Add(RoadNetwork[i][j].EndPoint);
                }

                RoadNetworkPoints.Add(RoadPoints);
            }

            return RoadNetworkPoints;
        }
    }
}
