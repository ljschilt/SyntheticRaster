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
            // Read the layer present in the active map with the given name.
            List<LineSegment> Lines = new List<LineSegment>();
            FeatureLayer layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>()
                .FirstOrDefault(L => L.Name == layerName);

            if (!(layer.GetTable() is FeatureClass fc)) { return; }

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

                        // Iterate through the network to create a list of line segments.
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


        /// <summary>
        /// Geometrically sort the list of line segments using intersection points. Return a list of reaches (lists of line segments).
        /// </summary>
        /// <param name="cellSize"></param>
        /// <param name="Lines"></param>
        public void SortSegmentsGeometrically(double cellSize, List<LineSegment> Lines)
        {
            RoadNetwork = new List<List<LineSegment>>();

            // Create the dictionary
            Dictionary<(double X, double Y), List<LineSegment>> roadPointDict = CreateRoadDictionary(Lines, cellSize);

            // Start at a terminal point by declaring a first list of line segments and a line segment.
            List<LineSegment> currentLine = new List<LineSegment>();
            LineSegment currentSegment = new LineSegment(new Point(0, 0), new Point(0, 0));
            (double X, double Y) currentKey = (0, 0);

            foreach (KeyValuePair<(double X, double Y), List<LineSegment>> roadPointItem in roadPointDict)
            {
                if (roadPointItem.Value.Count == 1)
                {
                    currentSegment = roadPointItem.Value[0];
                    roadPointItem.Value[0].IsChecked = true;
                    currentKey = roadPointItem.Key;
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
                foreach (KeyValuePair<(double, double), List<LineSegment>> roadPointItem in roadPointDict)
                {
                    if (hitIntersection) { break; }

                    // Look at a road point present in the dictionary.
                    int segmentCount = roadPointItem.Value.Count;

                    // If the road point is part of two lines, one of those lines is the same as the current segment, and the road point is not the current road point, then set this road point as the current point and add the line to the list.
                    if (segmentCount == 2 && (roadPointItem.Value[0] == currentSegment || roadPointItem.Value[1] == currentSegment) && roadPointItem.Key != currentKey)
                    {
                        currentKey = roadPointItem.Key;
                        currentPoint = new Point(currentKey.X, currentKey.Y);

                        // Swap the direction if needed
                        if (currentSegment == roadPointItem.Value[0])
                        {
                            currentSegment = roadPointItem.Value[1];
                            roadPointItem.Value[1].IsChecked = true;
                        }
                        else
                        {
                            currentSegment = roadPointItem.Value[0];
                            roadPointItem.Value[0].IsChecked = true;
                        }

                        if (currentPoint == currentSegment.EndPoint) { currentSegment.SwapDirection(); }
                        currentLine.Add(currentSegment);
                        continue;
                    }

                    else if (segmentCount > 2 && roadPointItem.Key != currentKey)
                    {
                        if (hitIntersection) { break; }

                        // Check every segment
                        for (int i = 0; i < segmentCount; i++) 
                        {
                            LineSegment segment = roadPointItem.Value[i];

                            // For the segment that shares the point.
                            if (segment == currentSegment) 
                            {
                                currentKey = roadPointItem.Key;
                                currentPoint = new Point(currentKey.X, currentKey.Y);
                                currentList = roadPointItem.Value;
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
                /* 
                 * The following program uses a form of Depth-First search to read the road network.
                 * It is recommend to research this topic to maximize understanding of the following code.
                 */

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
                            if (!segment.IsChecked)
                            {
                                currentSegment = segment;

                                // Check the direction and swap if needed
                                Point AnchorPointCheck = new Point(AnchorPoints[currentAnchorPoint].Location.X, AnchorPoints[currentAnchorPoint].Location.Y);
                                Point simplifiedEndPoint = new Point((double) (currentSegment.EndPoint.X / cellSize * 100), (double)(currentSegment.EndPoint.Y / cellSize * 100));

                                if (simplifiedEndPoint.Equals(AnchorPointCheck)) { currentSegment.SwapDirection(); }

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
                            foreach (KeyValuePair<(double, double), List<LineSegment>> roadPointItem in roadPointDict)
                            {
                                if (!notACriticalPoint) { break; }

                                // Look at a road point present in the dictionary.
                                segmentCount = roadPointItem.Value.Count;

                                // If the road point is part of two lines, one of those lines is the same as the current segment, and the road point is not the current road point, then set this road point as the current point and add the line to the list.
                                if (segmentCount == 2 && (roadPointItem.Value[0] == currentSegment ||
                                    roadPointItem.Value[1] == currentSegment) && roadPointItem.Key != currentKey)
                                {
                                    currentKey = roadPointItem.Key;
                                    currentPoint = new Point(currentKey.X, currentKey.Y);

                                    if (currentSegment == roadPointItem.Value[0])
                                    {
                                        currentSegment = roadPointItem.Value[1];
                                        roadPointItem.Value[1].IsChecked = true;
                                    }
                                    else
                                    {
                                        currentSegment = roadPointItem.Value[0];
                                        roadPointItem.Value[0].IsChecked = true;
                                    }

                                    // Swap the direction if needed
                                    Point simplifiedEndPoint = new Point((double)(currentSegment.EndPoint.X / cellSize * 100), (double)(currentSegment.EndPoint.Y / cellSize * 100));
                                    if (currentPoint.X == simplifiedEndPoint.X && currentPoint.Y == simplifiedEndPoint.Y)
                                    {
                                        currentSegment.SwapDirection();
                                    }
                                    currentLine.Add(currentSegment);
                                    continue;
                                }
                                else if (segmentCount != 2 && roadPointItem.Key != currentKey)
                                {
                                    for (int i = 0; i < segmentCount; i++)
                                    {
                                        LineSegment segment = roadPointItem.Value[i];
                                        if (segment == currentSegment)
                                        {
                                            currentKey = roadPointItem.Key;
                                            currentList = roadPointItem.Value;
                                            notACriticalPoint = false;
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        // If the next point is a terminal point
                        if (segmentCount == 1) 
                        {
                            // Add the list to the road network.
                            RoadNetwork.Add(currentLine);

                            // Subtract one from unchecked branches for the current anchor point.
                            AnchorPoints[currentAnchorPoint].UncheckedBranches -= 1;
                        }
                        // If the next point is an anchor point
                        else if (segmentCount >= 2 && CheckAnchorPoint(currentKey) != -1) 
                        {
                            // Add the list to the road network.
                            RoadNetwork.Add(currentLine);

                            // Identify the anchor point that was hit
                            // Subtract one from unchecked branches for both the current anchor point and the anchor point hit.
                            AnchorPoints[currentAnchorPoint].UncheckedBranches -= 1;
                            AnchorPoints[CheckAnchorPoint(currentKey)].UncheckedBranches -= 1;
                        }
                        // The next point is an intersection point
                        else
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


        /// <summary>
        /// Creates a dictionary based on a list of line segments where the keys are unique road points and their values are lists of line segments that contain the respective road point.
        /// </summary>
        /// <param name="Lines"></param>
        /// <param name="cellSize"></param>
        /// <returns></returns>
        public Dictionary<(double X, double Y), List<LineSegment>> CreateRoadDictionary(List<LineSegment> Lines, double cellSize)
        {
            Dictionary<(double X, double Y), List<LineSegment>> roadPointDict = new Dictionary<(double X, double Y), List<LineSegment>>();

            double beginX;
            double beginY;
            double endX;
            double endY;
            (double X, double Y) key;

            //Iterate through the list of line segments. Add all of the unique road points to the dictionary, noting their location and the segments that contain the point.
            foreach (LineSegment aLine in Lines)
            {
                beginX = (double)(aLine.BeginPoint.X / cellSize * 100);
                beginY = (double)(aLine.BeginPoint.Y / cellSize * 100);
                key = (beginX, beginY);
                if (!roadPointDict.ContainsKey(key)) { roadPointDict[key] = new List<LineSegment>(); }

                bool alreadyInThere = Convert.ToBoolean(roadPointDict[key].Where(item => item.UniqueString == aLine.UniqueString).Count());
                if (!alreadyInThere) { roadPointDict[key].Add(aLine); }

                endX = (double)(aLine.EndPoint.X / cellSize * 100);
                endY = (double)(aLine.EndPoint.Y / cellSize * 100);
                key = (endX, endY);
                if (!roadPointDict.ContainsKey(key)) { roadPointDict[key] = new List<LineSegment>(); }

                alreadyInThere = Convert.ToBoolean(roadPointDict[key].Where(item => item.UniqueString == aLine.UniqueString).Count());
                if (!alreadyInThere) { roadPointDict[key].Add(aLine); }
            }

            return roadPointDict;
        }


        /// <summary>
        /// Returns the list value of the AnchorPoint with the same key. Returns -1 if no key was found. 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int CheckAnchorPoint((double, double) key)
        {
            return AnchorPoints.FindIndex(a => a.OnLocation(key));
        }


        /// <summary>
        /// Convert the list of list of line segments into a list of list of road points.
        /// </summary>
        /// <returns></returns>
        public List<List<RCPoint>> CreateRoadNetworkList()
        {
            List<List<RCPoint>> RoadNetworkPoints = new List<List<RCPoint>>();

            // Iterate through the road network list to save a list of road points.
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
