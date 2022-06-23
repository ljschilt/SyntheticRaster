using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using RasterArc.Models;
using System.Windows.Input;

namespace RasterArc
{
    internal class ArcDockpaneViewModel : DockPane
    {
        private const string _dockPaneID = "RasterArc_ArcDockpane";

        protected ArcDockpaneViewModel() 
        {
            Raster = new BasicRaster()
            {
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
                        BasicRaster raster = new BasicRaster();
                        raster.CreateAndDisplayRaster(Raster.RasterFilename, Raster.RasterOutputDirectory);
                    }, () => true);
                }
                return _displayRaster;
            }
        }

        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;

            pane.Activate();
        }

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

    internal class ArcDockpane_ShowButton : Button
    {
        protected override void OnClick()
        {
            ArcDockpaneViewModel.Show();
        }
    }
}
