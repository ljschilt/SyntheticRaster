using System;
using System.Collections.Generic;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Core;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System.Collections.Specialized;
using RasterCore;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.Raster;

namespace RasterArc.Models
{
    class BasicRaster : PropertyChangedBase, IDisposable
    {
        private double _cellSize;
        private int _numColumns;
        private int _numRows;
        private double _leftCoordinatesX;
        private double _bottomCoordinatesY;
        private string _rasterOutputDirectory;
        private string _rasterFilename;

        public BasicRaster()
        {
            _cellSize = 50;
            _numColumns = 748;
            _numRows = 598;
            _leftCoordinatesX = 1277550.01;
            _bottomCoordinatesY = 690050.02;
            _rasterOutputDirectory = @"C:\Users\lukes\OneDrive\Documents\Research Files\SyntheticRaster\SynthRaster\Raster Files";
            _rasterFilename = "TestRun.asc";
        }

        public double CellSize { get { return _cellSize; } set { SetProperty(ref _cellSize, value, () => CellSize); } }

        public int NumColumns { get { return _numColumns; } set { SetProperty(ref _numColumns, value, () => NumColumns); } }

        public int NumRows { get { return _numRows; } set { SetProperty(ref _numRows, value, () => NumRows); } }

        public double LeftCoordinatesX { get { return _leftCoordinatesX; } set { SetProperty(ref _leftCoordinatesX, value, () => LeftCoordinatesX); } }

        public double BottomCoordinatesY { get { return _bottomCoordinatesY; } set { SetProperty(ref _bottomCoordinatesY, value, () => BottomCoordinatesY); } }

        public string RasterOutputDirectory { get { return _rasterOutputDirectory; } set { SetProperty(ref _rasterOutputDirectory, value, () => RasterOutputDirectory); } }

        public string RasterFilename { get { return _rasterFilename; } set { SetProperty(ref _rasterFilename, value, () => RasterFilename); } }

        public async void CreateAndDisplayRaster(double cellSize, int numColumns, int numRows, double leftXCoordinate, double bottomYCoordinate, string rasterName = "TestRun.asc", string rasterOutputDirectory = @"C:\Users\lukes\OneDrive\Documents\Research Files\SyntheticRaster\SynthRaster\Raster Files")
        {
            List<RCPoint> Points = new List<RCPoint> { new Point(1289502.41, 696521.78), new Point(1300086.67, 708023.33), new Point(1285440.00, 716776.67) };
            RasterCore.RasterCore coreRas = RasterCore.RasterCore.Zeroes(cellSize, numColumns, numRows, leftXCoordinate, bottomYCoordinate);
            coreRas.ComputeParametricSurface(Points);
            coreRas.WriteToFile(rasterOutputDirectory, rasterName);

            var map = MapView.Active.Map;
            string url = @rasterOutputDirectory + @"\" + @rasterName;
            //string url = @"C:\Users\lukes\OneDrive\Documents\Research Files\SyntheticRaster\SynthRaster\Raster Files\" + rasterName;

            StretchColorizerDefinition stretchColorizerDef = new StretchColorizerDefinition();
            await QueuedTask.Run(() =>
            {
                Uri uri = new Uri(url);
                RasterLayer rasterLayerfromURL =
                  LayerFactory.Instance.CreateRasterLayer(uri, map, 0, rasterName, stretchColorizerDef) as RasterLayer;
            });
        }

        bool is_disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!is_disposed) // only dispose once!
            {
                if (disposing)
                {
                }
                // perform cleanup for this object
            }
            this.is_disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~BasicRaster()
        {
            Dispose(false);
            // TODO: Close every file and folder and delete it
        }
    }
}
