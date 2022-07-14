using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

        // Computation Functions: Calculates the values for the given raster point
        private Func<double, double, double, double> SecperbolaFunc =
            (a, Pm, x) => (a * a * Pm /
                ((x * x + a * a) * Math.Sqrt(((x * x) / (a * a)) + 1.0)));

        protected RasterCore() { }

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
                    if (line == null) { break; }
                    string[] lineList = line.Split(' ');
                    rowCounter++;
                    int columnCounter = -1;

                    foreach (string entry in lineList)
                    {
                        columnCounter++;
                        if (entry == NoDataValue)
                        {
                            RasterGrid[rowCounter, columnCounter] = double.NaN;
                        }
                        else
                        {
                            try { RasterGrid[rowCounter, columnCounter] = double.Parse(entry); }
                            catch { }
                        }
                    }
                }

            }
        }

        public void WriteToFile(string PathToWriteTo, string fileName)
        {
            try
            {
                string filePath = PathToWriteTo + @"\" + fileName;

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
                            if (RasterGrid[currentRow, currentColumn] == double.NaN)
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
            catch { }
        }

        public void ComputeParametricSurface(List<List<RCPoint>> RoadNetwork, double inflectionWidth, double maxProb, double maxWidth, double widthToPeak, double RoadWidth, Func<double, double, double, double> theFunc = null)
        {
            //int pointCounter = 0;
            //foreach (List<RCPoint> Reach in RoadNetwork)
            //{
            //    pointCounter += Reach.Count;
            //}

            Func<double, double, double, double> theFunction = theFunc;
            if (theFunction == null) { theFunction = SecperbolaFunc; }

            double a = inflectionWidth;
            double inflectionWidthSquared = a * a;
            double maxWidthSquared = Math.Pow(maxWidth - widthToPeak, 2);
            double baseProb = -1.0 * inflectionWidthSquared * maxProb /
                ((inflectionWidthSquared + maxWidthSquared) * Math.Sqrt(1.0 + (maxWidthSquared / inflectionWidthSquared)));
            double ProbDifference = maxProb - baseProb;

            Parallel.For(0, NumRows, currentRow => 
            //for (int currentRow = 0; currentRow < NumRows; currentRow++) //
            {
                for (int currentColumn = 0; currentColumn < NumColumns; currentColumn++)
                {
                    double currentOffset = 0;
                    bool FirstRoadList = true;
                    RCPoint rasterPoint = new Point(
                        LeftXCoordinate + ((currentColumn + 0.5) * CellSize), 
                        BottomYCoordinate + (NumRows * CellSize) - ((currentRow + 0.5) * CellSize));
                    //progressCounter++;
                    foreach (List<RCPoint> road in RoadNetwork)
                    {
                        double existingCellValue = RasterGrid[currentRow, currentColumn];
                        if (existingCellValue == int.Parse(NoDataValue)) { break; }

                        IReadOnlyList<StationAndOffset> allSOs = StationAndOffset.CreateSOList(rasterPoint, road);

                        StationAndOffset closestStationAndOffset = allSOs.OrderBy(so => Math.Abs(so.Offset)).FirstOrDefault();

                        if (FirstRoadList || closestStationAndOffset.Offset <= currentOffset)
                        {
                            currentOffset = closestStationAndOffset.Offset;

                            if (closestStationAndOffset.Offset <= RoadWidth)
                            {
                                RasterGrid[currentRow, currentColumn] = int.Parse(NoDataValue);
                            }
                            else if (!closestStationAndOffset.ProjectsOnEndCap)
                            {
                                double x = closestStationAndOffset.Offset;
                                x -= widthToPeak;
                                double xSquared = x * x;

                                double probabilityOfDevelopment;
                                probabilityOfDevelopment =
                                   theFunction(a, maxProb - baseProb, x)
                                   + baseProb;

                                RasterGrid[currentRow, currentColumn] = probabilityOfDevelopment;
                            }

                            FirstRoadList = false;
                        }
                    }
                }
            } ); //

            for (int currentRow = 0; currentRow < NumRows; currentRow++)
            {
                for (int currentColumn = 0; currentColumn < NumColumns; currentColumn++)
                {
                    if (RasterGrid[currentRow, currentColumn] <= 0) { RasterGrid[currentRow, currentColumn] = int.Parse(NoDataValue); }
                }
            }
        }

        public static RasterCore Zeroes(double cellSize, int numColumns, int numRows, double leftXCoordinate, double bottomYCoordinate, string noDataValue = "-9999")
        {
            RasterCore newRaster = new RasterCore
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

        public Point(double X, double Y)
        {
            this.X = X;
            this.Y = Y;
        }
    }
}
