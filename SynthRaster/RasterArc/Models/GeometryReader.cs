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
        public List<AnchorPoint> AnchorPoints { get; private set; }

        public List<List<LineSegment>> RoadNetwork { get; private set; }

        public GeometryReader(double cellSize)
        {
            List<LineSegment> Lines = new List<LineSegment>();
            FeatureLayer layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>()
                .FirstOrDefault(L => L.Name == "Roads3");

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
            Dictionary<(int, int), List<LineSegment>> d = new Dictionary<(int, int), List<LineSegment>>();

            int beginX;
            int beginY;
            int endX;
            int endY;
            (int, int) key;

            // Populate the dictionary
            foreach (LineSegment aLine in Lines)
            {
                beginX = (int)(aLine.BeginPoint.X / cellSize);
                beginY = (int)(aLine.BeginPoint.Y / cellSize);
                key = (beginX, beginY);
                if (!d.ContainsKey(key))
                {
                    d[key] = new List<LineSegment>();
                }

                bool alreadyInThere = Convert.ToBoolean(d[key].Where(item => item.UniqueString == aLine.UniqueString).Count());
                if(!alreadyInThere)
                {
                    d[key].Add(aLine);
                }

                endX = (int)(aLine.EndPoint.X / cellSize);
                endY = (int)(aLine.EndPoint.Y / cellSize);
                key = (endX, endY);
                if (!d.ContainsKey(key))
                {
                    d[key] = new List<LineSegment>();
                }

                alreadyInThere = Convert.ToBoolean(d[key].Where(item => item.UniqueString == aLine.UniqueString).Count());
                if (!alreadyInThere)
                {
                    d[key].Add(aLine);
                }
            }

            // Start at a terminal point by declaring a first list of line segments and a line segment.
            List<LineSegment> currentLine = new List<LineSegment>();
            LineSegment currentSegment = new LineSegment(new Point(0, 0), new Point(0, 0));
            (int, int) currentKey = (0, 0);

            foreach (KeyValuePair<(int, int), List<LineSegment>> dItem in d)
            {
                if (dItem.Value.Count == 1)
                {
                    currentSegment = dItem.Value[0];
                    currentKey = dItem.Key;
                    break;
                }
            }

            Point currentPoint = new Point(currentKey.Item1, currentKey.Item2);
            if (currentPoint == currentSegment.EndPoint) { currentSegment.SwapDirection(); }
            currentLine.Add(currentSegment);

            // Continue on the path until an intersection point is reached.
            bool hitIntersection = false;
            for (int counter = 1; counter < Lines.Count; counter++)
            {
                foreach (KeyValuePair<(int, int), List<LineSegment>> dItem in d)
                {
                    int segmentCount = dItem.Value.Count;

                    if (segmentCount == 2 && (dItem.Value[0] == currentSegment || dItem.Value[1] == currentSegment) && dItem.Key != currentKey)
                    {
                        currentKey = dItem.Key;
                        currentPoint = new Point(currentKey.Item1, currentKey.Item2);

                        currentSegment = dItem.Value[0] == currentSegment ? dItem.Value[1] : dItem.Value[0];

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
                            if ((segment == currentSegment)) // For the segment that shares the point.
                            {
                                hitIntersection = true;
                                break;
                            }
                        }
                    }
                }
            }

            RoadNetwork.Add(currentLine);

            // If the previous line did not hit an intersection, then let the fun begin!
            if (!hitIntersection) { return; }

            // Set the first anchor point!
            List<AnchorPoint> AnchorPoints = new List<AnchorPoint>();
            AnchorPoint firstAnchorPoint = new AnchorPoint();
            AnchorPoints.Add(firstAnchorPoint);
            int currentAnchorPoint = 0;

            while (AnchorPoints[0].BranchesLeft())
            {

            }
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
