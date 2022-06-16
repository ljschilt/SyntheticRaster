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
        private double _leftXCoordinate;
        private double _bottomYCoordinate;
        private double _a;
        private double _maxProb;
        private double _baseProb;
        private double _widthToPeak;
        private double _roadWidth;
        private string _rasterOutputDirectory;
        private string _rasterFilename;

        public BasicRaster()
        {
            _cellSize = 50;
            _numColumns = 748;
            _numRows = 598;
            _leftXCoordinate = 1277550.01;
            _bottomYCoordinate = 690050.02;
            _a = 100;
            _maxProb = 0.15;
            _baseProb = -0.0005;
            _widthToPeak = 300;
            _roadWidth = 50;
            _rasterOutputDirectory = @"C:\Users\lukes\OneDrive\Documents\Research Files\SyntheticRaster\SynthRaster\Raster Files";
            _rasterFilename = "TestRun.asc";
        }

        public double CellSize { get { return _cellSize; } set { SetProperty(ref _cellSize, value, () => CellSize); } }
        public int NumColumns { get { return _numColumns; } set { SetProperty(ref _numColumns, value, () => NumColumns); } }
        public int NumRows { get { return _numRows; } set { SetProperty(ref _numRows, value, () => NumRows); } }
        public double LeftCoordinatesX { get { return _leftXCoordinate; } set { SetProperty(ref _leftXCoordinate, value, () => LeftCoordinatesX); } }
        public double BottomCoordinatesY { get { return _bottomYCoordinate; } set { SetProperty(ref _bottomYCoordinate, value, () => BottomCoordinatesY); } }
        public double A { get { return _a; } set { SetProperty(ref _a, value, () => A); } }
        public double MaxProb { get { return _maxProb; } set { SetProperty(ref _maxProb, value, () => MaxProb); } }
        public double BaseProb { get { return _baseProb; } set { SetProperty(ref _baseProb, value, () => BaseProb); } }
        public double WidthToPeak { get { return _widthToPeak; } set { SetProperty(ref _widthToPeak, value, () => WidthToPeak); } }
        public double RoadWidth { get { return _roadWidth; } set { SetProperty(ref _roadWidth, value, () => RoadWidth); } }
        public string RasterOutputDirectory { get { return _rasterOutputDirectory; } set { SetProperty(ref _rasterOutputDirectory, value, () => RasterOutputDirectory); } }
        public string RasterFilename { get { return _rasterFilename; } set { SetProperty(ref _rasterFilename, value, () => RasterFilename); } }

        public async void CreateAndDisplayRaster(string rasterName = "TestRun.asc", 
            string rasterOutputDirectory = @"C:\Users\lukes\OneDrive\Documents\Research Files\SyntheticRaster\SynthRaster\Raster Files")
        {
            GeometryReader geometryReader = new GeometryReader();
            List<RCPoint> Points = geometryReader.CreateRoadPointList();
            //List<RCPoint> Points = new List<RCPoint> { new Point(1289502.41, 696521.78), new Point(1300086.67, 708023.33), new Point(1285440.00, 716776.67) };
            RasterCore.RasterCore coreRas = RasterCore.RasterCore.Zeroes(_cellSize, _numColumns, _numRows, _leftXCoordinate, _bottomYCoordinate);
            coreRas.ComputeParametricSurface(Points, _a, _maxProb, _baseProb, _widthToPeak, _roadWidth);
            coreRas.WriteToFile(rasterOutputDirectory, rasterName);
            var map = MapView.Active.Map;
            string url = @rasterOutputDirectory + @"\" + @rasterName;

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
            if (!is_disposed)
            {
                if (disposing)
                {
                }
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
        }
    }
}
