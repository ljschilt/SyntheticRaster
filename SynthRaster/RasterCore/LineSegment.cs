using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RasterCore
{
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

        public void SwapDirection()
        {
            RCPoint temporaryVariable = BeginPoint;
            BeginPoint = EndPoint;
            EndPoint = temporaryVariable;
        }

        public bool SameBeginPoints(LineSegment segment)
        {
            return BeginPoint == segment.BeginPoint;
        }

        public bool SameEndPoints(LineSegment segment)
        {
            return EndPoint == segment.EndPoint;
        }

        public Boolean CheckForIntersection(LineSegment segment)
        {
            return BeginPoint == segment.EndPoint || EndPoint == segment.BeginPoint 
                || BeginPoint == segment.BeginPoint || EndPoint == segment.EndPoint;
        }

        public String UniqueString
        {
            get
            {
                return String.Format($"{BeginPoint.X:.0.000}, {BeginPoint.Y:.0.000}:{EndPoint.X:.0.000}, {EndPoint.Y:.0.000}");
            }
        }
    }
}
