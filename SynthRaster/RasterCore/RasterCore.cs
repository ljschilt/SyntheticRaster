using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RasterCore
{

    public class RasterCore
    {
        private double CellSize { get; set; }
        private int NumColumns { get; set; }
        private int NumRows { get; set; }
        private double LeftXCoordinate { get; set; }
        private double BottomYCoordinate { get; set; }
        private string NoDataValue { get; set; }
        private double[,] RasterGrid { get; set; }

        protected RasterCore()
        {
        }

        public RasterCore(string PathToOpen)
        {
            using (StreamReader sr = new StreamReader(PathToOpen))
            {
                NumColumns = int.Parse(sr.ReadLine().Split(' ')[1]);
                NumRows = int.Parse(sr.ReadLine().Split(' ')[1]);
                LeftXCoordinate = double.Parse(sr.ReadLine().Split(' ')[1]);
                BottomYCoordinate = double.Parse(sr.ReadLine().Split(' ')[1]);
                CellSize = double.Parse(sr.ReadLine().Split(' ')[1]);
                NoDataValue = sr.ReadLine().Split(' ')[1];
                RasterGrid = new double[NumRows, NumColumns];
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
                            RasterGrid[rowCounter, columnCounter] = double.NaN;
                        }
                        else
                        {
                            try
                            {
                                RasterGrid[rowCounter, columnCounter] = double.Parse(entry);
                            }
                            catch
                            {
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
                var filePath = PathToWriteTo + @"\" + fileName;

                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("ncols         " + NumColumns);
                    writer.WriteLine("nrows         " + NumRows);
                    writer.WriteLine("xllcorner     " + LeftXCoordinate);
                    writer.WriteLine("yllcorner     " + BottomYCoordinate);
                    writer.WriteLine("cellsize      " + CellSize);
                    writer.WriteLine("NODATA_value  " + NoDataValue);

                    for (int currentRow = 0; currentRow < NumRows; currentRow++)
                    {
                        for (int currentColumn = 0; currentColumn < NumColumns; currentColumn++)
                        {
                            if (this.RasterGrid[currentRow, currentColumn] == double.NaN)
                            {
                                writer.Write(NoDataValue);
                            }
                            else
                            {
                                writer.Write(RasterGrid[currentRow, currentColumn]);
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

        public void ComputeParametricSurface(List<RCPoint> RoadPoints, double a, double maxProb, double baseProb, double widthToPeak, double RoadWidth)
        {
            double maxValue = 0.0;
            double aSquared = a * a;
            double ProbDifference = maxProb - baseProb;

            for (int currentRow = 0; currentRow < NumRows; currentRow++)
            {
                for (int currentColumn = 0; currentColumn < NumColumns; currentColumn++)
                {
                    RCPoint rasterPoint = new Point(LeftXCoordinate + ((currentColumn + 0.5) * CellSize), (BottomYCoordinate + (NumRows * CellSize)) - ((currentRow + 0.5) * CellSize));
                    IReadOnlyList<StationAndOffset> allSOs = StationAndOffset.CreateSOList(rasterPoint, RoadPoints);

                    var stationAndOffset = allSOs.Where(so => so.ProjectsOnSegment == true)
                        .OrderBy(so => Math.Abs(so.Offset))
                        .FirstOrDefault();

                    if (stationAndOffset == null)
                    {
                        RasterGrid[currentRow, currentColumn] = Int32.Parse(NoDataValue);
                    }
                    else
                    {
                        double x = stationAndOffset.Offset;
                        if (x >= RoadWidth)
                        {
                            x -= widthToPeak;
                            double xSquared = x * x;

                            double probabilityOfDevelopment = ( (aSquared * ProbDifference) / 
                                ((xSquared + aSquared) * Math.Sqrt((xSquared / aSquared) + 1.0))) + baseProb;

                            if (probabilityOfDevelopment >= 0)
                            {
                                RasterGrid[currentRow, currentColumn] = probabilityOfDevelopment;
                            }
                            else
                            {
                                RasterGrid[currentRow, currentColumn] = Int32.Parse(NoDataValue);
                            }

                            maxValue = RasterGrid[currentRow, currentColumn] > maxValue ? RasterGrid[currentRow, currentColumn] : maxValue;
                        }
                        else
                        {
                            RasterGrid[currentRow, currentColumn] = Int32.Parse(NoDataValue);
                        }
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
            var newRaster = new RasterCore
            {
                CellSize = cellSize,
                NumColumns = numColumns,
                NumRows = numRows,
                LeftXCoordinate = leftXCoordinate,
                BottomYCoordinate = bottomYCoordinate,
                NoDataValue = noDataValue,
                RasterGrid = new double[numRows, numColumns]
            };

            for (int currentRow = 0; currentRow < numRows; currentRow++)
            {
                for (int currentColumn = 0; currentColumn < numColumns; currentColumn++)
                {
                    newRaster.RasterGrid[currentRow, currentColumn] = 0;
                }
            }

            return newRaster;
        }
    }
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
}
