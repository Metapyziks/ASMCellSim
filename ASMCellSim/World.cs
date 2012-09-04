using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASMCellSim
{
    internal class World
    {
        internal class NearbyCellEnumerator : IEnumerator<Cell>
        {
            private readonly int myMinX;
            private readonly int myMinY;

            private readonly int myMaxX;
            private readonly int myMaxY;

            private int myX;
            private int myY;
            private int myI;

            internal readonly World World;
            internal readonly Vector2 Location;
            internal readonly float Radius;

            internal NearbyCellEnumerator( World world, Vector2 location, float radius )
            {
                World = world;

                Location = location;
                Radius = radius;

                myMinX = (int) Math.Floor( ( location.X - radius ) / World.stGridSize );
                myMinY = (int) Math.Floor( ( location.Y - radius ) / World.stGridSize );

                myMaxX = (int) Math.Ceiling ( ( location.X + radius ) / World.stGridSize );
                myMaxY = (int) Math.Ceiling( ( location.Y + radius ) / World.stGridSize );

                myMinX -= (int) Math.Floor( (double) myMinX / World.myCols ) * World.myCols;
                myMinY -= (int) Math.Floor( (double) myMinX / World.myRows ) * World.myRows;

                myMaxX -= (int) Math.Floor( (double) myMaxX / World.myCols ) * World.myCols;
                myMaxY -= (int) Math.Floor( (double) myMaxY / World.myRows ) * World.myRows;
            }

            public Cell Current
            {
                get { return World.myCellGrid[ myX, myY ][ myI ]; }
            }

            public void Dispose()
            {
                return;
            }

            object System.Collections.IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
                ++myI;

                while ( myI >= World.myCellGrid[ myX, myY ].Count )
                {
                    myI = 0;
                    ++myX;
                    myX -= (int) Math.Floor( (double) myX / World.myCols ) * World.myCols;

                    if ( myX == myMaxX )
                    {
                        myX = myMinX;
                        ++myY;
                        myY -= (int) Math.Floor( (double) myY / World.myRows ) * World.myRows;

                        if ( myY == myMaxY )
                            return false;
                    }
                }

                return true;
            }

            public void Reset()
            {
                myX = myMinX;
                myY = myMinY;
                myI = -1;
            }
        }

        private const float stGridSize = 2.0f;

        private readonly int myCols;
        private readonly int myRows;
        private List<Cell>[,] myCellGrid;

        private readonly float myHalfWidth;
        private readonly float myHalfHeight;

        internal readonly float Width;
        internal readonly float Height;

        internal readonly bool WrapHorz;
        internal readonly bool WrapVert;

        internal World( float size, bool wrap = true )
            : this( size, size, wrap, wrap ) { }

        internal World( float width, float height, bool wrapHorz = true, bool wrapVert = true )
        {
            Width = width;
            Height = height;

            myHalfWidth = Width / 2.0f;
            myHalfHeight = Height / 2.0f;

            WrapHorz = wrapHorz;
            WrapVert = wrapVert;

            myCols = (int) Math.Ceiling( width / stGridSize );
            myRows = (int) Math.Ceiling( height / stGridSize );

            myCellGrid = new List<Cell>[ myCols, myRows ];

            for ( int c = 0; c < myCols; ++c )
                for ( int r = 0; r < myRows; ++r )
                    myCellGrid[ c, r ] = new List<Cell>();
        }

        internal void Clear()
        {
            for ( int c = 0; c < myCols; ++c )
                for ( int r = 0; r < myRows; ++r )
                    myCellGrid[ c, r ].Clear();
        }

        internal Vector2 Difference( Vector2 vec0, Vector2 vec1 )
        {
            Vector2 res = vec1 - vec0;
            if ( WrapHorz )
            {
                if ( res.X >= myHalfWidth )
                    res.X -= Width;
                else if ( res.X < -myHalfWidth )
                    res.X += Width;
            }
            if ( WrapVert )
            {
                if ( res.Y >= myHalfHeight )
                    res.Y -= Height;
                else if ( res.Y < -myHalfHeight )
                    res.Y += Height;
            }
            return res;
        }
    }
}
