using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static RasterCore.StationAndOffset;

namespace RasterCore
{
	public class Point : RCPoint
	{
		public double X { get; set; }
		public double Y { get; set; }
		public double Z { get; set; }

		public Point(double X, double Y, double Z = 0)
		{
			this.X = X;
			this.Y = Y;
			this.Z = Z;
		}
	}

	public class RasterCore
	{
		private double cellSize { get; set; }
		private int numColumns { get; set; }
		private int numRows { get; set; }
		private double leftXCoordinate { get; set; }
		private double bottomYCoordinate { get; set; }
		private string NoDataValue { get; set; }
		private double[,] rasterGrid { get; set; }

		protected RasterCore()
		{
		}

		public RasterCore(string PathToOpen)
		{
			using(StreamReader sr = new StreamReader(PathToOpen))
			{
				numColumns = int.Parse(sr.ReadLine().Split(" ")[1]);

				numRows = int.Parse(sr.ReadLine().Split(" ")[1]);

				leftXCoordinate = double.Parse(sr.ReadLine().Split(" ")[1]);

				bottomYCoordinate = double.Parse(sr.ReadLine().Split(" ")[1]);

				cellSize = double.Parse(sr.ReadLine().Split(" ")[1]);

				NoDataValue = sr.ReadLine().Split(" ")[1];

				rasterGrid = new double[numRows,numColumns];

				string line;
				int rowCounter = -1;

				while (true)
				{
					line = sr.ReadLine();
					if (line == null) break;
					var lineList = line.Split(" ");
					rowCounter++;
					int columnCounter = -1;

					foreach (var entry in lineList)
					{
						columnCounter++;
						if (entry == this.NoDataValue)
						{
							rasterGrid[rowCounter, columnCounter] = double.NaN;
						}
						else
						{
							try
							{
								rasterGrid[rowCounter, columnCounter] = double.Parse(entry);
							}
							catch
							{
								// To be inputted later.
							}
						}
					}
				}

			}
		}

		public void WriteToFile(string PathToWriteTo, string fileName)
		{
			try
			{
				var filePath = PathToWriteTo + "\\" + fileName;

				using (StreamWriter writer = new StreamWriter(filePath))
				{
					writer.WriteLine("ncols         " + numColumns);

					writer.WriteLine("nrows         " + numRows);

					writer.WriteLine("xllcorner     " + leftXCoordinate);

					writer.WriteLine("yllcorner     " + bottomYCoordinate);

					writer.WriteLine("cellsize      " + cellSize);

					writer.WriteLine("ncols         " + NoDataValue);

					for (int currentRow = 0; currentRow < numRows; currentRow++)
					{
						for (int currentColumn = 0; currentColumn < numColumns; currentColumn++)
						{
							if (this.rasterGrid[currentRow, currentColumn] == double.NaN)
							{
								writer.Write(NoDataValue);
							}
							else
							{
								writer.Write(rasterGrid[currentRow, currentColumn]);
							}

							writer.Write(" ");
						}

						writer.WriteLine("");
					}

					writer.Flush();
				}
			}
			catch
			{
			}
		}

		public void ComputeParametricSurface(List <RCPoint> RoadPoints)
		{
			var stationAndOffset = new StationAndOffset();
			int RoadWidth = 50;
			for (int currentRow = 0; currentRow < numRows; currentRow++)
			{
				for (int currentColumn = 0; currentColumn < numColumns; currentColumn++)
				{
					RCPoint rasterPoint = new Point(leftXCoordinate + ((currentColumn + 0.5) * cellSize), bottomYCoordinate + ((currentRow + 0.5) * cellSize));
						
					if (stationAndOffset.CalculateStationAndOffset(rasterPoint, RoadPoints).offset >= RoadWidth)
					{
						rasterGrid[currentRow, currentColumn] = (stationAndOffset.CalculateStationAndOffset(rasterPoint, RoadPoints).offset / 1);
					}
					else
					{
						rasterGrid[currentRow, currentColumn] = Int32.Parse(NoDataValue);
					}
				}
			}
		}


		public static RasterCore Zeroes(
			double cellSize, 
			int numColumns, 
			int numRows, 
			double leftXCoordinate, 
			double bottomYCoordinate, 
			string noDataValue = "-9999")
		{
			var newRaster = new RasterCore();
			newRaster.cellSize = cellSize;
			newRaster.numColumns = numColumns;
			newRaster.numRows = numRows;
			newRaster.leftXCoordinate = leftXCoordinate;
			newRaster.bottomYCoordinate = bottomYCoordinate;
			newRaster.NoDataValue = noDataValue;
			newRaster.rasterGrid = new double[numRows, numColumns];

			for (int currentRow = 0; currentRow < numRows; currentRow++)
			{
				for (int currentColumn = 0; currentColumn < numColumns; currentColumn++)
				{
					newRaster.rasterGrid[currentRow,currentColumn] = 0;
				}
			}

			return newRaster;
		}
	}
}
