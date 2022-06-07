using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Input;
using SynthRaster.Models;

namespace SynthRaster
{
	internal class MainDockpaneViewModel : DockPane
	{
		private const string _dockPaneID = "SynthRaster_MainDockpane";

		protected MainDockpaneViewModel() {
			Raster = new BasicRaster()
			{
				XCellSize = 1,
				YCellSize = 1,
				NumColumns = 2,
				NumRows = 2,
				TopLeftCoordinatesX = 0,
				TopLeftCoordinatesY = 0,
				RasterOutputDirectory = "C:\\Users\\lukes\\OneDrive\\Documents\\ResearchFiles\\SynthRaster"
			};
		}

		private ICommand _displayRaster = null;
		public BasicRaster Raster { get; set; }

		public ICommand DisplayRaster
		{
			get
			{
				if (_displayRaster == null)
				{
					_displayRaster = new RelayCommand(() =>
					{
						// save the info from the current user control
						var output = $@"X Cell Size: {Raster.XCellSize}, Y Cell Size: {Raster.YCellSize}, Number of Columns: {Raster.NumColumns}, Number Of Rows: {Raster.NumRows}, Top Left X Coordinate: {Raster.TopLeftCoordinatesX}, Top Left Y Coordinate: {Raster.TopLeftCoordinatesY}, Raster Output Directory: {Raster.RasterOutputDirectory}";
						ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(output);

						Raster.CreateRaster();
					}, () => true);
				}
				return _displayRaster;
			}
		}

		/// <summary>
		/// Show the DockPane.
		/// </summary>
		internal static void Show()
		{
			DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
			if (pane == null)
				return;

			pane.Activate();
		}

		/// <summary>
		/// Text shown near the top of the DockPane.
		/// </summary>
		private string _heading = "";
		public string Heading
		{
			get { return _heading; }
			set
			{
				SetProperty(ref _heading, value, () => Heading);
			}
		}
	}

	/// <summary>
	/// Button implementation to show the DockPane.
	/// </summary>
	internal class MainDockpane_ShowButton : Button
	{
		protected override void OnClick()
		{
			MainDockpaneViewModel.Show();
		}
	}
}
