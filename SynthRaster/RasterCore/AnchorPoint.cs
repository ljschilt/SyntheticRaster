using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RasterCore
{
    public class AnchorPoint
    {
        public (int, int) Location { get; set; }
        public List<LineSegment> IntersectingLines { get; set; }
        public double UncheckedBranches { get; set; }

        public AnchorPoint((int, int) Location, List<LineSegment> IntersectingLines)
        {
            this.Location = Location;
            this.IntersectingLines = IntersectingLines;
            this.UncheckedBranches = IntersectingLines.Count - 1;
        }

        public bool BranchesLeft()
        {
            return UncheckedBranches != 0;
        }

        public bool OnLocation((int, int) Location)
        {
            return this.Location == Location;
        }
    }
}
