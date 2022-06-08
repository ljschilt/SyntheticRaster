﻿using RasterCore;
using System;

namespace RasterConsole
{
	class Program
	{
		static void Main(string[] args)
		{
			double cellSize = 1;
			int numColumns = 250;
			int numRows = 250;
			double topLeftXCoordinate = 1286273.9;
			double topLeftYCoordinate = 703871.54;

			//var coreRas = new RasterCore.RasterCore("C:\\Users\\lukes\\Downloads\\test.asc");
			var coreRas = RasterCore.RasterCore.Zeroes(cellSize, numColumns, numRows, topLeftXCoordinate, topLeftYCoordinate);
			// Add Gradient
			coreRas.AddSimpleGradient();

			// Output to ASC File
			coreRas.WriteToFile("C:\\Users\\lukes\\OneDrive\\Documents\\Research Files\\SyntheticRaster\\SynthRaster\\Raster Files", "TestRun.asc");

			Console.WriteLine("Hello World!");
		}
	}
}
