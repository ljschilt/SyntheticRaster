using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using RasterArc.Models;
using System.Diagnostics;
using System.Windows.Input;

namespace RasterArc
{
    internal class ArcDockpaneViewModel : DockPane
    {
        private const string _dockPaneID = "RasterArc_ArcDockpane";

        protected ArcDockpaneViewModel() 
        {
            Raster = new BasicRaster() { };
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
                        // Check for exceptions in the user inputs.
                        // If there are user inputs, output the exceptions and let the user try again.
                        // Otherwise, begin creating and displaying the raster.
                        if (Raster.HandleExceptions().Equals("No exceptions"))
                        {
                            Raster.CreateAndDisplayRaster();
                        }
                        else { _ = MessageBox.Show(Raster.HandleExceptions()); }
                    }, () => true);
                    
                }
                return _displayRaster;
            }
        }

        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null) { return; }
            pane.Activate();
        }

        private string _heading = "";
        public string Heading
        {
            get => _heading;
            set => SetProperty(ref _heading, value, () => Heading);
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
