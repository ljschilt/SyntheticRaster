using System;
using System.Collections.Generic;
using System.Text;

namespace RasterCore
{
#pragma warning disable IDE1006 // Naming Styles
    public interface RCPoint
#pragma warning restore IDE1006 // Naming Styles
    {
        double X { get; set; }
        double Y { get; set; }
    }
}
