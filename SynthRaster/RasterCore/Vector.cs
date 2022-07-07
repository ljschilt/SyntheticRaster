using System;

namespace RasterCore
{
    internal class Vector
    {
        private double X { get; set; }
        private double Y { get; set; }

        public Vector() { }

        public Vector(double X, double Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public Vector(RCPoint startPoint, RCPoint endPoint)
        {
            X = endPoint.X - startPoint.X;
            Y = endPoint.Y - startPoint.Y;
        }

        public double Magnitude(Vector vector)
        {
            return Math.Sqrt(Math.Pow(vector.X, 2) + Math.Pow(vector.Y, 2));
        }

        public double DotProduct(Vector vector1, Vector vector2)
        {
            return (vector1.X * vector2.X) + (vector2.Y * vector1.Y);
        }
    }
}
