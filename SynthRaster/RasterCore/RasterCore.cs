using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            using (StreamReader sr = new StreamReader(PathToOpen))
            {
                numColumns = int.Parse(sr.ReadLine().Split(' ')[1]);
                numRows = int.Parse(sr.ReadLine().Split(' ')[1]);
                leftXCoordinate = double.Parse(sr.ReadLine().Split(' ')[1]);
                bottomYCoordinate = double.Parse(sr.ReadLine().Split(' ')[1]);
                cellSize = double.Parse(sr.ReadLine().Split(' ')[1]);
                NoDataValue = sr.ReadLine().Split(' ')[1];
                rasterGrid = new double[numRows, numColumns];
                string line;
                int rowCounter = -1;

                while (true)
                {
                    line = sr.ReadLine();
                    if (line == null) break;
                    var lineList = line.Split(' ');
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
                    writer.WriteLine("NODATA_value  " + NoDataValue);

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

        public void ComputeParametricSurface(List<RCPoint> RoadPoints)
        {
            int RoadWidth = 50;
            double maxValue = 0.0;
            for (int currentRow = 0; currentRow < numRows; currentRow++)
            {
                for (int currentColumn = 0; currentColumn < numColumns; currentColumn++)
                {
                    RCPoint rasterPoint = new Point(leftXCoordinate + ((currentColumn + 0.5) * cellSize), bottomYCoordinate + ((currentRow + 0.5) * cellSize));
                    IReadOnlyList<StationAndOffset> allSOs = StationAndOffset.CreateSOList(rasterPoint, RoadPoints);

                    var stationAndOffset = allSOs.Where(so => so.ProjectsOnSegment == true)
                        .OrderBy(so => Math.Abs(so.offset))
                        .FirstOrDefault();

                    if (stationAndOffset == null)
                    {
                        rasterGrid[currentRow, currentColumn] = Int32.Parse(NoDataValue);
                    }
                    else
                    {
                        if (stationAndOffset.offset >= RoadWidth)
                        {
                            rasterGrid[currentRow, currentColumn] = stationAndOffset.offset;
                            maxValue = rasterGrid[currentRow, currentColumn] > maxValue ? rasterGrid[currentRow, currentColumn] : maxValue;
                        }
                        else
                        {
                            rasterGrid[currentRow, currentColumn] = Int32.Parse(NoDataValue);
                        }
                    }
                }
            }
            for (int currentRow = 0; currentRow < numRows; currentRow++)
            {
                for (int currentColumn = 0; currentColumn < numColumns; currentColumn++)
                {
                    var val = rasterGrid[currentRow, currentColumn];
                    if (val != Int32.Parse(NoDataValue))
                    {
                        val *= 498;
                        val /= maxValue;
                        rasterGrid[currentRow, currentColumn] = val;
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
                    newRaster.rasterGrid[currentRow, currentColumn] = 0;
                }
            }

            return newRaster;
        }
    }
}
