using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RasterCore
{
    /// <summary>
    /// A line segments notes its two points, if it has been checked by the program, and its length.
    /// </summary>
    public class LineSegment
    {
        public RCPoint BeginPoint { get; private set; }
        public RCPoint EndPoint { get; private set; }

        public bool IsChecked { get; set; }

        private double? Length_ = null;
        public double Length
        { 
            get
            {
                if (Length_ == null)
                {
                    Length_ = Math.Sqrt(Math.Pow(EndPoint.X - BeginPoint.X, 2) + Math.Pow(EndPoint.Y - BeginPoint.Y, 2));
                }
                return (double) Length_;
            }
            private set
            {

            }
        }

        public LineSegment(RCPoint BeginPoint, RCPoint EndPoint)
        {
            this.BeginPoint = BeginPoint;
            this.EndPoint = EndPoint;
            IsChecked = false;
        }

        /// <summary>
        /// The line segments have a direction indicated by which points are the begin and end points.
        /// To iterate through the branch properly, they need to be in a proper direction.
        /// </summary>
        public void SwapDirection()
        {
            RCPoint temporaryVariable = BeginPoint;
            BeginPoint = EndPoint;
            EndPoint = temporaryVariable;
        }

        /// <summary>
        /// Returns a boolean indiacting if the two segments have the same begin point 
        /// (which is used alongside the SwapDirection() method).
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public bool SameBeginPoints(LineSegment segment)
        {
            return BeginPoint == segment.BeginPoint;
        }

        /// <summary>
        /// Returns a boolean indiacting if the two segments have the same end point 
        /// (which is used alongside the SwapDirection() method).
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public bool SameEndPoints(LineSegment segment)
        {
            return EndPoint == segment.EndPoint;
        }

        /// <summary>
        /// Returns a boolean indicating if two segments share any of their points.
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public Boolean CheckForIntersection(LineSegment segment)
        {
            return BeginPoint == segment.EndPoint || EndPoint == segment.BeginPoint 
                || BeginPoint == segment.BeginPoint || EndPoint == segment.EndPoint;
        }

        /// <summary>
        /// Creates a string representing the line segment.
        /// </summary>
        public String UniqueString
        {
            get
            {
                return String.Format($"{BeginPoint.X:.0.000}, {BeginPoint.Y:.0.000}:{EndPoint.X:.0.000}, {EndPoint.Y:.0.000}");
            }
        }
    }
}
