using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Templates;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Core;

namespace RasterArc.Models
{
    class GeometryReader
    {
        // Class goal: Read the road shapefile present in ArcGIS to return a list of road points.
        // Input: Layer
        // Output: List <RCPoint>

        public void Dummy()
        {
            Layer pointLayer = MapView.Active.Map.GetLayersAsFlattenedList()
                .Where(L => L.Name == "My Layer").FirstOrDefault();
        }
    }
}
