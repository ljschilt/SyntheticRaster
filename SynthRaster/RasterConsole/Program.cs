using RasterCore;
using System;
using System.Collections.Generic;

namespace RasterConsole
{
    internal class Program
    {
        public static List<RCPoint> Points = new List <RCPoint> {new Point(1289502.41, 696521.78), new Point(1300086.67, 708023.33), new Point(1285440.00, 716776.67)};
        public static List<List<RCPoint>> RoadNetwork = new List<List<RCPoint>>();

        private static void Main(string[] args)
        {
            if (args is null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            RoadNetwork.Add(Points);
            double cellSize = 50;
            int numColumns = 748;
            int numRows = 598;
            double leftXCoordinate = 1277550.01;
            double bottomYCoordinate = 690050.02;
            double a = 100;
            double maxProb = 0.15;
            double maxWidth = -0.0005;
            double widthToPeak = 300;
            double roadWidth = 50;

            RasterCore.RasterCore coreRas = RasterCore.RasterCore.Zeroes(cellSize, numColumns, numRows, leftXCoordinate, bottomYCoordinate);
            coreRas.ComputeParametricSurface(RoadNetwork, a, maxProb, maxWidth, widthToPeak, roadWidth);

            coreRas.WriteToFile("C:\\Users\\lukes\\OneDrive\\Documents\\Research Files\\SyntheticRaster\\SynthRaster\\Raster Files", "TestRun.asc");
        }
    }
}
