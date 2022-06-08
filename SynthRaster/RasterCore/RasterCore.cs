using System;
using System.IO;
using System.Text;

namespace RasterCore
{
	public class RasterCore
	{
		// Instance Variables
		private double cellSize { get; set; }
		private int numColumns { get; set; }
		private int numRows { get; set; }
		private double topLeftCoordinatesX { get; set; }
		private double topLeftCoordinatesY { get; set; }
		private string NoDataValue { get; set; }
		private double[,] rasterGrid { get; set; }

		protected RasterCore()
		{
		}

		public RasterCore(string PathToOpen)
		{
			// Open the file for reading
			using(StreamReader sr = new StreamReader(PathToOpen))
			{

				// Read the tokens to acquire various data information.
				// Item #1: Number of Columns
				numColumns = int.Parse(sr.ReadLine().Split(" ")[1]);

				// Item #2: Number of Rows
				numRows = int.Parse(sr.ReadLine().Split(" ")[1]);

				// Item #3: X Lower Left Corner 
				double lowerleftXCoordinate = double.Parse(sr.ReadLine().Split(" ")[1]);
				topLeftCoordinatesX = lowerleftXCoordinate;

				// Item #4: Y Lower Left Corner
				double lowerleftYCoordinate = double.Parse(sr.ReadLine().Split(" ")[1]);
				topLeftCoordinatesY = lowerleftYCoordinate;

				// Item #5: Cell Size
				cellSize = double.Parse(sr.ReadLine().Split(" ")[1]);

				// Item #5: No Data Value
				NoDataValue = sr.ReadLine().Split(" ")[1];

				// Iteam #6: Raster Grid
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
					// Line #1: Number of columns
					writer.WriteLine("ncols         " + numColumns);

					// Line #2: Number of rows
					writer.WriteLine("nrows         " + numRows);

					// Line #3: X Lower Left Corner Coordinate
					// TODO: Add method that changes top left coordinates to lower left coordinates
					writer.WriteLine("xllcorner     " + topLeftCoordinatesX);

					// Line #4: Y Lower Left Corner Coordinate
					// TODO: Add method that changes top left coordinates to lower left coordinates
					writer.WriteLine("yllcorner     " + topLeftCoordinatesY);

					// Line #5: Cell Size
					writer.WriteLine("cellsize      " + cellSize);

					// Line #6: No Data Value
					writer.WriteLine("ncols         " + NoDataValue);

					// Remaining Lines: Output Array
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

		public void AddSimpleGradient()
		{
			// Set the array cells to be their column value plus their row value
			for (int currentRow = 0; currentRow < numRows; currentRow++)
			{
				for (int currentColumn = 0; currentColumn < numColumns; currentColumn++)
				{
					this.rasterGrid[currentRow, currentColumn] = currentColumn + currentRow;
				}
			}
		}

		public static RasterCore Zeroes(
			double cellSize, 
			int numColumns, 
			int numRows, 
			double topLeftXCoordinate, 
			double topLeftYCoordinate, 
			string noDataValue = "-9999")
		{
			var newRaster = new RasterCore();
			newRaster.cellSize = cellSize;
			newRaster.numColumns = numColumns;
			newRaster.numRows = numRows;
			newRaster.topLeftCoordinatesX = topLeftXCoordinate;
			newRaster.topLeftCoordinatesY = topLeftYCoordinate;
			newRaster.NoDataValue = noDataValue;
			newRaster.rasterGrid = new double[numRows, numColumns];

			// Set the array to be empty.
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
