using System.Linq;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Editing;
using ArcGIS.Core.Geometry;
using RasterCore;
using System.Collections.Generic;

namespace RasterArc.Models
{
    class GeometryReader
    {
        public List<RCPoint> CreateRoadPointList()
        {
            List<RCPoint> Points = new List<RCPoint>();
            FeatureLayer layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>()
                .FirstOrDefault(L => L.Name == "BethelChurch");

            _ = QueuedTask.Run(() =>
              {
                  if (!(layer.GetTable() is FeatureClass fc))
                  {
                      return;
                  }

                  using (RowCursor cursor = fc.Search())
                  {
                      int cursorCounter = 0;
                      while (cursor.MoveNext())
                      {
                          using (Feature feature = cursor.Current as Feature)
                          {
                              if (feature == null)
                              {
                                  continue;
                              }

                              //var outerRings = Module1.Current.GetOutermostRings(feature.GetShape() as Polygon); <-- Figure out how to change this.

                              Polyline polyline = feature.GetShape() as Polyline;
                              ReadOnlyPartCollection polylineParts = polyline.Parts;
                              IEnumerator<ReadOnlySegmentCollection> cur = polylineParts.GetEnumerator();

                              while (cur.MoveNext())
                              {
                                  ReadOnlySegmentCollection seg = cur.Current;
                                  foreach(var s in seg)
                                  {
                                      MapPoint point = s.StartPoint;
                                      Points.Add(new Point(point.X, point.Y, point.Z));
                                  }
                              }

                              
                              cursorCounter++;
                          }
                      }
                  }
              });
            return Points;
        }
    }
}
