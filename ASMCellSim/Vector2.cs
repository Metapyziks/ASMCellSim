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

        public static Vector2 operator *( Vector2 vec, float scalar )
        {
            return new Vector2( vec.X * scalar, vec.Y * scalar );
        }

        public static float operator *( Vector2 vec0, Vector2 vec1 )
        {
            return vec0.X * vec1.X + vec0.Y * vec1.Y;
        }

        public static Vector2 operator /( Vector2 vec, float scalar )
        {
            return new Vector2( vec.X / scalar, vec.Y / scalar );
        }

        public float X;
        public float Y;

        public float Length
        {
            get { return (float) Math.Sqrt( X * X + Y * Y ); }
        }

        public float Length2
        {
            get { return X * X + Y * Y; }
        }

        public Vector2 Normal
        {
            get
            {
                float length = Length;
                return new Vector2( X / length, Y / length );
            }
        }

        public Vector2( float x, float y )
        {
            X = x;
            Y = y;
        }
    }
}
