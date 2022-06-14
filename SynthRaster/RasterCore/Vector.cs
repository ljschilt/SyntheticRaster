using System;
using System.Collections.Generic;
using System.Text;

namespace RasterCore
{
    class Vector
    {
        private double X { get; set; }
        private double Y { get; set; }

        public Vector()
        {
        }

        public Vector(double X, double Y)
        {
            this.X = X;
            this.Y = Y;
        }

        // TODO: Utilize operator overloading

        public Vector(RCPoint startPoint, RCPoint endPoint)
        {
            this.X = endPoint.X - startPoint.X;
            this.Y = endPoint.Y - startPoint.Y;
        }

        public double Magnitude(Vector vector)
        {
            return Math.Sqrt(Math.Pow(vector.X, 2) + Math.Pow(vector.Y, 2));
        }

        public Vector UnitVector(Vector vector)
        {
            double magnitude = Magnitude(vector);
            return Divide(vector, magnitude);
        }

        public Vector Add(Vector vector1, Vector vector2)
        {
            return new Vector(vector1.X + vector2.X, vector1.Y + vector2.Y);
        }

        public Vector Subtract(Vector vector1, Vector vector2)
        {
            return new Vector(vector1.X - vector2.X, vector1.Y - vector2.Y);
        }

        public Vector Multiply(Vector vector, double scalar)
        {
            return new Vector(vector.X * scalar, vector.Y * scalar);
        }

        public Vector Divide(Vector vector, double scalar)
        {
            if (scalar == 0)
            {
                // Note: Include an exception here.
            }
            return new Vector(vector.X / scalar, vector.Y / scalar);
        }

        public double CrossProductMagnitude(Vector vector1, Vector vector2)
        {
            return Math.Abs((vector1.X * vector2.Y) - (vector2.X * vector1.Y)); 
        }

        public double DotProduct(Vector vector1, Vector vector2)
        {
            return (vector1.X * vector2.X) + (vector2.Y * vector1.Y);
        }

        public override Boolean Equals(Object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                Vector vector2 = (Vector) obj;
                return (X == vector2.X) && (Y == vector2.Y);
            }
        }
    }
}
