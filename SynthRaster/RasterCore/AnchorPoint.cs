using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RasterCore
{
    public class AnchorPoint
    {
        public (double X, double Y) Location { get; set; }
        public List<LineSegment> IntersectingLines { get; set; }
        public double UncheckedBranches { get; set; }

        public AnchorPoint((double, double) Location, List<LineSegment> IntersectingLines)
        {
            this.Location = Location;
            this.IntersectingLines = IntersectingLines;
            this.UncheckedBranches = IntersectingLines.Count - 1;
        }

        public bool BranchesLeft()
        {
            return UncheckedBranches != 0;
        }

        public bool OnLocation((double, double) Location)
        {
            return this.Location == Location;
        }
    }
}
