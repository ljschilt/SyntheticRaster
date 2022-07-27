namespace RasterCore
{
    public class Point : RCPoint
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Point(double X, double Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                Point p = (Point) obj;
                return (X == p.X) && (Y == p.Y);
            }
        }
    }
}
