using System;
using System.Collections.Generic;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using RasterCore;
using System.IO;
using System.Linq;
using ArcGIS.Desktop.Editing;
using System.Diagnostics;
using ArcGIS.Desktop.Framework.Dialogs;

namespace RasterArc.Models
{
    class BasicRaster : PropertyChangedBase, IDisposable
    {
        private double _cellSize;
        private int _numColumns;
        private int _numRows;
        private double _leftXCoordinate;
        private double _bottomYCoordinate;
        private double _inflectionWidth;
        private double _maxProb;
        private double _maxWidth;
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
            _inflectionWidth = 100;
            _maxProb = 0.15;
            _maxWidth = 962.673;
            _widthToPeak = 300;
            _roadWidth = 50;
            _rasterOutputDirectory = @"";
            _rasterFilename = "TestRun.asc";
            _layerName = "Bethel&WhiteEagleIntersection";
        }

        public double CellSize { get { return _cellSize; } set { SetProperty(ref _cellSize, value, () => CellSize); } }
        public int NumColumns { get { return _numColumns; } set { SetProperty(ref _numColumns, value, () => NumColumns); } }
        public int NumRows { get { return _numRows; } set { SetProperty(ref _numRows, value, () => NumRows); } }
        public double LeftXCoordinate { get { return _leftXCoordinate; } set { SetProperty(ref _leftXCoordinate, value, () => LeftXCoordinate); } }
        public double BottomYCoordinate { get { return _bottomYCoordinate; } set { SetProperty(ref _bottomYCoordinate, value, () => BottomYCoordinate); } }
        public double InflectionWidth { get { return _inflectionWidth; } set { SetProperty(ref _inflectionWidth, value, () => InflectionWidth); } }
        public double MaxProb { get { return _maxProb; } set { SetProperty(ref _maxProb, value, () => MaxProb); } }
        public double MaxWidth { get { return _maxWidth; } set { SetProperty(ref _maxWidth, value, () => MaxWidth); } }
        public double WidthToPeak { get { return _widthToPeak; } set { SetProperty(ref _widthToPeak, value, () => WidthToPeak); } }
        public double RoadWidth { get { return _roadWidth; } set { SetProperty(ref _roadWidth, value, () => RoadWidth); } }
        public string RasterOutputDirectory { get { return _rasterOutputDirectory; } set { SetProperty(ref _rasterOutputDirectory, value, () => RasterOutputDirectory); } }
        public string RasterFilename { get { return _rasterFilename; } set { SetProperty(ref _rasterFilename, value, () => RasterFilename); } }
        public string LayerName { get { return _layerName; } set { SetProperty(ref _layerName, value, () => LayerName); } }

        /// <summary>
        /// Creates the raster file and outputs it into the current Contents pane in ArcGIS, while noting the time it takes to do so.
        /// </summary>
        public async void CreateAndDisplayRaster()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            StretchColorizerDefinition stretchColorizerDef = new StretchColorizerDefinition();
            await QueuedTask.Run(() =>
            {
                // #1: Read through the road network.
                GeometryReader geometryReader = new GeometryReader(CellSize, LayerName);
                List<List<RCPoint>> RoadNetwork = geometryReader.CreateRoadNetworkList();

                // #2: Populate the raster.
                RasterCore.RasterCore coreRas = RasterCore.RasterCore.Zeroes(CellSize, NumColumns, NumRows, LeftXCoordinate, BottomYCoordinate);
                coreRas.ComputeParametricSurface(RoadNetwork, InflectionWidth, MaxProb, MaxWidth, WidthToPeak, RoadWidth);

                // #3: Write to the file and output it in ArcGIS.
                coreRas.WriteToFile(RasterOutputDirectory, RasterFilename);
                Map map = MapView.Active.Map;
                string url = @RasterOutputDirectory + @"\" + @RasterFilename;
                Uri uri = new Uri(url);
                RasterLayer rasterLayerfromURL =
                  LayerFactory.Instance.CreateRasterLayer(uri, map, 0, RasterFilename, stretchColorizerDef) as RasterLayer;
            });
            stopwatch.Stop();

            double elapsedSeconds = stopwatch.ElapsedMilliseconds / 1000.00;
            string stopWatchMessage = "Elapsed Time is " + elapsedSeconds + " seconds.";
            _ = MessageBox.Show(stopWatchMessage);
        }

        /// <summary>
        /// Checks for argument exceptions in the user inputs and creates a string with messages representing the errors.
        /// </summary>
        /// <returns></returns>
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
            if (MaxWidth <= WidthToPeak)
            {
                if (exception) { exceptionString += "\n"; }
                exception = true;
                exceptionString += "Invalid max width. Max width must be greater than the width to peak.";
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
                exceptionString += "Invalid Output Directory. Check if the file path exists!";
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
