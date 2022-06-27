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
        public List<LineSegment> Lines { get; private set; }

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
            this.Lines = Lines;
            SortSegmentsGeometrically(cellSize);
        }

        public void SortSegmentsGeometrically(double cellSize)
        {
            Dictionary<(int, int), List<LineSegment>> d = new Dictionary<(int, int), List<LineSegment>>();

            int beginX;
            int beginY;
            int endX;
            int endY;
            (int, int) key;

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

            int linesCounter = Lines.Count;
            List<LineSegment> sortedLines = new List<LineSegment>();

            // Does not account for closed loops
            LineSegment currentSegment = new LineSegment(new Point(0,0), new Point(0,0));
            (int, int) currentKey = (0,0);

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
            if(currentPoint == currentSegment.EndPoint) { currentSegment.SwapDirection(); }
            sortedLines.Add(currentSegment);

            for (int counter = 1; counter < linesCounter; counter++)
            {
                foreach (KeyValuePair<(int, int), List<LineSegment>> dItem in d)
                {
                    if (dItem.Value.Count == 2 && (dItem.Value[0] == currentSegment || dItem.Value[1] == currentSegment) && dItem.Key != currentKey)
                    {
                        currentKey = dItem.Key;
                        currentPoint = new Point(currentKey.Item1, currentKey.Item2);

                        currentSegment = dItem.Value[0] == currentSegment ? dItem.Value[1] : dItem.Value[0];

                        if (currentPoint == currentSegment.EndPoint) { currentSegment.SwapDirection(); }
                        sortedLines.Add(currentSegment);

                        continue;
                    }
                }
            }

            this.Lines = sortedLines;
        }

        public List<RCPoint> CreateRoadPointList()
        {
            List<RCPoint> RoadPoints = new List<RCPoint>
            {
                Lines[0].BeginPoint
            };

            for (int i = 0; i < Lines.Count; i++)
            {
                RoadPoints.Add(Lines[i].EndPoint);
            }

            return RoadPoints;
        }

        public int CheckIntersectionCount(LineSegment segment)
        {
            int count = 0;

            for (int i = 0; i < Lines.Count; i++)
            {
                if (segment.CheckForIntersection(Lines[i]) && !(segment.SameBeginPoints(Lines[i]) && segment.SameEndPoints(Lines[i])))
                {
                    count++;
                }
            }

            return count;
        }

        public int CheckIntersectionCount(RCPoint point)
        {
            int count = 0;

            for (int i = 0; i < Lines.Count; i++)
            {
                if (point == Lines[i].BeginPoint || point == Lines[i].EndPoint)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
