using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RasterCore
{
    public class AnchorPoint
    {
        private (int,int) Location { get; set; }
        private List<LineSegment> Intersections { get; set; }
        private double UncheckedBranches { get; set; }

        public AnchorPoint((int, int) Location, List<LineSegment> Intersections)
        {
            this.Location = Location;
            this.Intersections = Intersections;
            this.UncheckedBranches = Intersections.Count;
        }

        public bool BranchesLeft()
        {
            return UncheckedBranches != 0;
        }
    }
}
