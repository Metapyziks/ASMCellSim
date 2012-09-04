using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASMCellSim
{
    internal struct Vector2
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

        internal float X;
        internal float Y;

        internal float Length
        {
            get { return (float) Math.Sqrt( X * X + Y * Y ); }
        }

        internal float Length2
        {
            get { return X * X + Y * Y; }
        }

        internal Vector2 Normal
        {
            get
            {
                float length = Length;
                return new Vector2( X / length, Y / length );
            }
        }

        internal Vector2( float x, float y )
        {
            X = x;
            Y = y;
        }
    }
}
