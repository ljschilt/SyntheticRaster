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

        // Probability Functions: Calculates the values for the given raster point
        /// <summary>
        /// 
        /// </summary>
        private Func<double, double, double, double> SecperbolaFunc =
            (a, Pm, x) => (a * a * Pm /
                ((x * x + a * a) * Math.Sqrt(((x * x) / (a * a)) + 1.0)));

        protected RasterCore() { }

        /// <summary>
        /// Read the raster file with the given file path and set the parameters to what is on the file.
        /// </summary>
        /// <param name="PathToOpen"></param>
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

        /// <summary>
        /// Write to the raster file in proper ASC format.
        /// </summary>
        /// <param name="PathToWriteTo"></param>
        /// <param name="fileName"></param>
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

        /// <summary>
        /// Using the road network and user inputs, populate the raster to represent the relationship between road network proximity and probability of urban development.
        /// </summary>
        /// <param name="RoadNetwork"></param>
        /// <param name="inflectionWidth"></param>
        /// <param name="maxProb"></param>
        /// <param name="maxWidth"></param>
        /// <param name="widthToPeak"></param>
        /// <param name="RoadWidth"></param>
        /// <param name="theFunc"></param>
        public void ComputeParametricSurface(List<List<RCPoint>> RoadNetwork, double inflectionWidth, double maxProb, double maxWidth, double widthToPeak, double RoadWidth, Func<double, double, double, double> theFunc = null)
        {
            // Set the probability function. If no probability function was given, default to SecPerbola.
            Func<double, double, double, double> theFunction = theFunc;
            if (theFunction == null) { theFunction = SecperbolaFunc; }

            double a = inflectionWidth;
            double inflectionWidthSquared = a * a;
            double maxWidthSquared = Math.Pow(maxWidth - widthToPeak, 2);
            double baseProb = -1.0 * inflectionWidthSquared * maxProb /
                ((inflectionWidthSquared + maxWidthSquared) * Math.Sqrt(1.0 + (maxWidthSquared / inflectionWidthSquared)));
            double ProbDifference = maxProb - baseProb;

            Parallel.For(0, NumRows, currentRow => //
            //for (int currentRow = 0; currentRow < NumRows; currentRow++) //
            {
                for (int currentColumn = 0; currentColumn < NumColumns; currentColumn++)
                {
                    double currentOffset = 0;
                    bool FirstRoadList = true;
                    RCPoint rasterPoint = new Point(
                        LeftXCoordinate + ((currentColumn + 0.5) * CellSize), 
                        BottomYCoordinate + (NumRows * CellSize) - ((currentRow + 0.5) * CellSize));
                    foreach (List<RCPoint> road in RoadNetwork)
                    {
                        double existingCellValue = RasterGrid[currentRow, currentColumn];
                        if (existingCellValue == int.Parse(NoDataValue)) { break; }

                        // Calculate all of the stations and offsets by the current reach for the raster point and save the value with the smallest offset.
                        IReadOnlyList<StationAndOffset> allSOs = StationAndOffset.CreateSOList(rasterPoint, road);
                        StationAndOffset closestStationAndOffset = allSOs.OrderBy(so => Math.Abs(so.Offset)).FirstOrDefault();

                        // If this offset is less than the current value's offset, continue.
                        if (FirstRoadList || closestStationAndOffset.Offset <= currentOffset)
                        {
                            currentOffset = closestStationAndOffset.Offset;

                            // If the offset is less than the road width, then assign a NoDataValue to the raster cell, else calculate the probability at that cell.
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

            // Iterate through the array to assign NoDataValues to any cells that have a value less than or equal to zero.
            for (int currentRow = 0; currentRow < NumRows; currentRow++)
            {
                for (int currentColumn = 0; currentColumn < NumColumns; currentColumn++)
                {
                    if (RasterGrid[currentRow, currentColumn] <= 0) { RasterGrid[currentRow, currentColumn] = int.Parse(NoDataValue); }
                }
            }
        }

        /// <summary>
        /// A default constructor that sets the raster array to be empty 
        /// (where every cell has a value of 0)
        /// </summary>
        /// <param name="cellSize"></param>
        /// <param name="numColumns"></param>
        /// <param name="numRows"></param>
        /// <param name="leftXCoordinate"></param>
        /// <param name="bottomYCoordinate"></param>
        /// <param name="noDataValue"></param>
        /// <returns></returns>
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
}
