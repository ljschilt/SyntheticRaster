using System;
using System.Collections.Generic;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using RasterCore;
using System.IO;
using System.Linq;
using ArcGIS.Desktop.Editing;

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
        private string _layerName;

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
            _layerName = "Bethel&WhiteEagleIntersection";

        }

        public double CellSize { get { return _cellSize; } set { SetProperty(ref _cellSize, value, () => CellSize); } }
        public int NumColumns { get { return _numColumns; } set { SetProperty(ref _numColumns, value, () => NumColumns); } }
        public int NumRows { get { return _numRows; } set { SetProperty(ref _numRows, value, () => NumRows); } }
        public double LeftXCoordinate { get { return _leftXCoordinate; } set { SetProperty(ref _leftXCoordinate, value, () => LeftXCoordinate); } }
        public double BottomYCoordinate { get { return _bottomYCoordinate; } set { SetProperty(ref _bottomYCoordinate, value, () => BottomYCoordinate); } }
        public double A { get { return _a; } set { SetProperty(ref _a, value, () => A); } }
        public double MaxProb { get { return _maxProb; } set { SetProperty(ref _maxProb, value, () => MaxProb); } }
        public double BaseProb { get { return _baseProb; } set { SetProperty(ref _baseProb, value, () => BaseProb); } }
        public double WidthToPeak { get { return _widthToPeak; } set { SetProperty(ref _widthToPeak, value, () => WidthToPeak); } }
        public double RoadWidth { get { return _roadWidth; } set { SetProperty(ref _roadWidth, value, () => RoadWidth); } }
        public string RasterOutputDirectory { get { return _rasterOutputDirectory; } set { SetProperty(ref _rasterOutputDirectory, value, () => RasterOutputDirectory); } }
        public string RasterFilename { get { return _rasterFilename; } set { SetProperty(ref _rasterFilename, value, () => RasterFilename); } }
        public string LayerName { get { return _layerName; } set { SetProperty(ref _layerName, value, () => LayerName); } }

        public async void CreateAndDisplayRaster()
        {
            StretchColorizerDefinition stretchColorizerDef = new StretchColorizerDefinition();
            await QueuedTask.Run(() =>
            {
                GeometryReader geometryReader = new GeometryReader(CellSize, LayerName);
                
                List<List<RCPoint>> RoadNetwork = geometryReader.CreateRoadNetworkList();
                RasterCore.RasterCore coreRas = RasterCore.RasterCore.Zeroes(CellSize, NumColumns, NumRows, LeftXCoordinate, BottomYCoordinate);
                coreRas.ComputeParametricSurface(RoadNetwork, A, MaxProb, BaseProb, WidthToPeak, RoadWidth);
                coreRas.WriteToFile(RasterOutputDirectory, RasterFilename);
                Map map = MapView.Active.Map;
                string url = @RasterOutputDirectory + @"\" + @RasterFilename;
                Uri uri = new Uri(url);
                RasterLayer rasterLayerfromURL =
                  LayerFactory.Instance.CreateRasterLayer(uri, map, 0, RasterFilename, stretchColorizerDef) as RasterLayer;
            });
        }

        public string HandleExceptions()
        {
            string exceptionString = "";
            bool exception = false;
            if (CellSize <= 0)
            {
                if (exception) { exceptionString += "\n"; }
                exception = true;
                exceptionString += "Invalid cell size. Cell size must be a non-zero positive value.";
            }
            if (NumColumns <= 0)
            {
                if (exception) { exceptionString += "\n"; }
                exception = true;
                exceptionString += "Invalid number of columns. The number of columns must be a non-zero positive integer.";
            }
            if (NumRows <= 0)
            {
                if (exception) { exceptionString += "\n"; }
                exception = true;
                exceptionString += "Invalid number of rows. The number of rows must be a non-zero positive integer.";
            }
            if (RoadWidth <= 0)
            {
                if (exception) { exceptionString += "\n"; }
                exception = true;
                exceptionString += "Invalid road width. Road width must be a non-zero positive value.";
            }
            if (!Directory.Exists(RasterOutputDirectory))
            {
                if (exception) { exceptionString += "\n"; }
                exception = true;
                exceptionString += "Invalid output directory. Check if the file path exists!";
            }

            FeatureLayer layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>()
                .FirstOrDefault(L => L.Name == LayerName);
            if (layer == null)
            {
                if (exception) { exceptionString += "\n"; }
                exception = true;
                exceptionString += "Invalid layer name. Check if the layer exists and is present in the Contents pane!";
            }

            return exception ? exceptionString : "No exceptions";
        }

        private bool is_disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!is_disposed)
            {
                if (disposing)
                {
                }
            }
            is_disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~BasicRaster() { Dispose(false); }
    }
}
