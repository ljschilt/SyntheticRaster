using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using SynthRaster.Models;
using System.Windows.Input;

namespace SynthRaster
{
	internal class RasterDockpaneViewModel : DockPane
	{
		private const string _dockPaneID = "SynthRaster_RasterDockpane";

		protected RasterDockpaneViewModel() {
			Raster = new BasicRaster()
			{
				CellSize = 1,
				NumColumns = 2,
				NumRows = 2,
				LeftCoordinatesX = 0,
				BottomCoordinatesY = 0,
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
						var output = $@"Cell Size: {Raster.CellSize}, Number of Columns: {Raster.NumColumns}, Number Of Rows: {Raster.NumRows}, Top Left X Coordinate: {Raster.LeftCoordinatesX}, Top Left Y Coordinate: {Raster.BottomCoordinatesY}, Raster Output Directory: {Raster.RasterOutputDirectory}";
						ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(output);

						// Run Program.cs with the given inputs.
						BasicRaster raster = new BasicRaster();
						raster.CreateRaster(Raster.CellSize, Raster.NumColumns, Raster.NumRows, Raster.LeftCoordinatesX, Raster.BottomCoordinatesY);

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
		private string _heading = "My DockPane";
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
	internal class RasterDockpane_ShowButton : Button
	{
		protected override void OnClick()
		{
			RasterDockpaneViewModel.Show();
		}
	}
}
