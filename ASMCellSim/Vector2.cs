using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASMCellSim
{
    public struct Vector2
    {
        public static Vector2 operator +( Vector2 vec0, Vector2 vec1 )
        {
            return new Vector2( vec0.X + vec1.X, vec0.Y + vec1.Y );
        }

        public static Vector2 operator -( Vector2 vec0, Vector2 vec1 )
        {
            return new Vector2( vec0.X - vec1.X, vec0.Y - vec1.Y );
        }

        public static Vector2 operator *( Vector2 vec, double scalar )
        {
            return new Vector2( vec.X * scalar, vec.Y * scalar );
        }

        public static double operator *( Vector2 vec0, Vector2 vec1 )
        {
            return vec0.X * vec1.X + vec0.Y * vec1.Y;
        }

        public static Vector2 operator /( Vector2 vec, double scalar )
        {
            return new Vector2( vec.X / scalar, vec.Y / scalar );
        }

        public double X;
        public double Y;

        public double Length
        {
            get { return (double) Math.Sqrt( X * X + Y * Y ); }
        }

        public double Length2
        {
            get { return X * X + Y * Y; }
        }

        public Vector2 Normal
        {
            get
            {
                double length = Length;
                return new Vector2( X / length, Y / length );
            }
        }

        public Vector2( double x, double y )
        {
            X = x;
            Y = y;
        }
    }
}
