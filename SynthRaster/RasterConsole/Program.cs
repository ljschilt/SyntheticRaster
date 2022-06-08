using RasterCore;
using System;
using System.Collections.Generic;

namespace RasterConsole
{
	class Point : RCPoint
	{
		public double X { get; set; }
		public double Y { get; set; }
		public double Z { get; set; }

		Point(double X, double Y, double Z = 0)
		{
			this.X = X;
			this.Y = Y;
			this.Z = Z;
		}
	}

	class Program
	{
		//List <RCPoint> Points = {new Point() }

		static void Main(string[] args)
		{
			double cellSize = 50;
			int numColumns = 748;
			int numRows = 598;
			double leftXCoordinate = 1277550.01;
			double bottomYCoordinate = 690050.02;

			//var coreRas = new RasterCore.RasterCore("C:\\Users\\lukes\\Downloads\\test.asc");
			var coreRas = RasterCore.RasterCore.Zeroes(cellSize, numColumns, numRows, leftXCoordinate, bottomYCoordinate);
			// Add Gradient
			coreRas.AddSimpleGradient();

			// Output to ASC File
			coreRas.WriteToFile("C:\\Users\\lukes\\OneDrive\\Documents\\Research Files\\SyntheticRaster\\SynthRaster\\Raster Files", "TestRun.asc");

			Console.WriteLine("Hello World!");
		}
	}
}
