using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework.Contracts;

namespace SynthRaster.Models
{
	class BasicRaster : PropertyChangedBase , IDisposable
	{
		private float _cellSize;
		private float _numColumns;
		private float _numRows;
		private float _leftCoordinatesX;
		private float _bottomCoordinatesY;
		private string _rasterOutputDirectory;

		public BasicRaster()
		{
			_cellSize = 1;
			_numColumns = 1;
			_numRows = 1;
			_leftCoordinatesX = 0;
			_bottomCoordinatesY = 0;
			_rasterOutputDirectory = "Blank output directory";
		}

		public float CellSize { get { return _cellSize; } set { SetProperty(ref _cellSize, value, () => CellSize); } }

		public float NumColumns { get { return _numColumns; } set { SetProperty(ref _numColumns, value, () => NumColumns); } }

		public float NumRows { get { return _numRows; } set { SetProperty(ref _numRows, value, () => NumRows); } }

		public float LeftCoordinatesX { get { return _leftCoordinatesX; } set { SetProperty(ref _leftCoordinatesX, value, () => LeftCoordinatesX); } }

		public float BottomCoordinatesY { get { return _bottomCoordinatesY; } set { SetProperty(ref _bottomCoordinatesY, value, () => BottomCoordinatesY); } }

		public string RasterOutputDirectory { get { return _rasterOutputDirectory; } set { SetProperty(ref _rasterOutputDirectory, value, () => RasterOutputDirectory); } }

		public void CreateRaster(double cellSize, double numColumns, double numRows, double leftXCoordinate, double bottomYCoordinate, string rasterName = "Test Run.asc")
		{

		}

		bool is_disposed = false;
		protected virtual void Dispose(bool disposing)
		{
			if (!is_disposed) // only dispose once!
			{
				if (disposing)
				{
				}
				// perform cleanup for this object
			}
			this.is_disposed = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~BasicRaster()
		{
			Dispose(false);
			// TODO: Close every file and folder and delete it
		}
	}
}
