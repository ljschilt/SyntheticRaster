using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RasterCore
{
    /// <summary>
    /// Anchor Points represent intersection points in the road network. They are called "Anchor Points"
    /// because they save their location to mark where the program needs to return to when iterating
    /// through the network, hence "anchoring" the important points in the program. They also save their
    /// branches and note which ones have been checked so that the program knows when to stop searching
    /// with the current anchor point to return to a previous anchor point.
    /// </summary>
    public class AnchorPoint
    {
        public (double X, double Y) Location { get; set; }
        public List<LineSegment> IntersectingLines { get; set; }
        public double UncheckedBranches { get; set; }

        public AnchorPoint((double, double) Location, List<LineSegment> IntersectingLines)
        {
            this.Location = Location;
            this.IntersectingLines = IntersectingLines;
            UncheckedBranches = IntersectingLines.Count - 1;
        }

        /// <summary>
        /// Returns a boolean that is true if the anchor point still has branches left to be checked.
        /// </summary>
        /// <returns></returns>
        public bool BranchesLeft()
        {
            return UncheckedBranches != 0;
        }

        /// <summary>
        /// Returns a boolean if the location inputted is the same as the anchor point's location.
        /// </summary>
        /// <param name="Location"></param>
        /// <returns></returns>
        public bool OnLocation((double, double) Location)
        {
            return this.Location == Location;
        }
    }
}
