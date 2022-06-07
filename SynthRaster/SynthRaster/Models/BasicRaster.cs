using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Core;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.Raster;

// Note: This might have to be renamed to SynthRaster

namespace SynthRaster.Models
{
	class BasicRaster : PropertyChangedBase , IDisposable
	{
		private float _xCellSize;
		private float _yCellSize;
		private float _numColumns;
		private float _numRows;
		private float _topLeftCoordinatesX;
		private float _topLeftCoordinatesY;
		private string _rasterOutputDirectory;

		public BasicRaster()
		{
			_xCellSize = 1;
			_yCellSize = 1;
			_numColumns = 1;
			_numRows = 1;
			_topLeftCoordinatesX = 0;
			_topLeftCoordinatesY = 0;
			_rasterOutputDirectory = "Blank output directory";
		}

		public float XCellSize { get { return _xCellSize; } set { SetProperty(ref _xCellSize, value, () => XCellSize); } }

		public float YCellSize { get { return _yCellSize; } set { SetProperty(ref _yCellSize, value, () => YCellSize); } }

		public float NumColumns { get { return _numColumns; } set { SetProperty(ref _numColumns, value, () => NumColumns); } }

		public float NumRows { get { return _numRows; } set { SetProperty(ref _numRows, value, () => NumRows); } }

		public float TopLeftCoordinatesX { get { return _topLeftCoordinatesX; } set { SetProperty(ref _topLeftCoordinatesX, value, () => TopLeftCoordinatesX); } }

		public float TopLeftCoordinatesY { get { return _topLeftCoordinatesY; } set { SetProperty(ref _topLeftCoordinatesY, value, () => TopLeftCoordinatesY); } }

		public string RasterOutputDirectory { get { return _rasterOutputDirectory; } set { SetProperty(ref _rasterOutputDirectory, value, () => RasterOutputDirectory); } }

		public void CreateRaster()
		{
			// Create a temporary folder
			CreateTempFolder();

			// Create a geodatabase in the folder created via CreateTempFolder()

			// Create a raster dataset

			// Create a raster
		}

		private void CreateTempFolder()
		{
			// Set the base folder to be the user's temporary folder.
			string folderName = System.IO.Path.GetTempPath();

			// Create a subfolder in the temporary folder named "Temporary Raster Files" if the file path does not already exist.
			string pathString = System.IO.Path.Combine(folderName, "Temporary Raster Files");
			if (!System.IO.File.Exists(pathString))
			{
				System.IO.Directory.CreateDirectory(pathString);
			}
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
