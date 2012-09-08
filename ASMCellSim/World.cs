using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASMCellSim
{
    public class World
    {
        public class NearbyCellEnumerator : IEnumerator<Cell>
        {
            private readonly int myMinX;
            private readonly int myMinY;

            private readonly int myMaxX;
            private readonly int myMaxY;

            private int myX;
            private int myY;
            private int myI;

            public readonly World World;
            public readonly Vector2 Location;
            public readonly float Radius;

            public NearbyCellEnumerator( World world, float minX, float minY, float width, float height )
            {
                World = world;
                Radius = 0f;

                myMinX = (int) Math.Floor( minX / World.stGridSize );
                myMinY = (int) Math.Floor( minY / World.stGridSize );

                myMaxX = (int) Math.Ceiling( ( minX + width ) / World.stGridSize );
                myMaxY = (int) Math.Ceiling( ( minY + height ) / World.stGridSize );

                myMinX -= (int) Math.Floor( (double) myMinX / World.myCols ) * World.myCols;
                myMinY -= (int) Math.Floor( (double) myMinX / World.myRows ) * World.myRows;

                myMaxX -= (int) Math.Floor( (double) myMaxX / World.myCols ) * World.myCols;
                myMaxY -= (int) Math.Floor( (double) myMaxY / World.myRows ) * World.myRows;
            }

            public NearbyCellEnumerator( World world, Vector2 location, float radius )
                : this( world, location.X - radius, location.Y - radius, radius * 2f, radius * 2f )
            {
                Location = location;
                Radius = radius;
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

        public readonly float Width;
        public readonly float Height;

        public readonly bool WrapHorz;
        public readonly bool WrapVert;

        public float Friction;

        public World( float size, bool wrap = true )
            : this( size, size, wrap, wrap ) { }

        public World( float width, float height, bool wrapHorz = true, bool wrapVert = true )
        {
            Width = width;
            Height = height;

            myHalfWidth = Width / 2.0f;
            myHalfHeight = Height / 2.0f;

            WrapHorz = wrapHorz;
            WrapVert = wrapVert;

            Friction = 0.9f;

            myCols = (int) Math.Ceiling( width / stGridSize );
            myRows = (int) Math.Ceiling( height / stGridSize );

            myCellGrid = new List<Cell>[ myCols, myRows ];

            for ( int c = 0; c < myCols; ++c )
                for ( int r = 0; r < myRows; ++r )
                    myCellGrid[ c, r ] = new List<Cell>();
        }

        public Cell AddCell( Vector2 pos )
        {
            Cell cell = new Cell( Wrap( pos ) );
            myCellGrid[ (int) ( cell.Position.X / stGridSize ), (int) ( cell.Position.Y / stGridSize ) ].Add( cell );
            return cell;
        }

        public void Clear()
        {
            for ( int c = 0; c < myCols; ++c )
                for ( int r = 0; r < myRows; ++r )
                    myCellGrid[ c, r ].Clear();
        }

        public Vector2 Difference( Vector2 vec0, Vector2 vec1 )
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

        public Vector2 Wrap( Vector2 vec )
        {
            vec.X -= (int) Math.Floor( vec.X / Width ) * Width;
            vec.Y -= (int) Math.Floor( vec.Y / Height ) * Height;

            return vec;
        }

        public void Step()
        {
            for ( int c = 0; c < myCols; ++c )
                for ( int r = 0; r < myRows; ++r )
                    foreach ( Cell cell in myCellGrid[ c, r ] )
                        cell.Step( this );

            List<Cell> displaced = new List<Cell>();

            for ( int c = 0; c < myCols; ++c )
            {
                for ( int r = 0; r < myRows; ++r )
                {
                    for ( int i = myCellGrid[ c, r ].Count - 1; i >= 0; --i )
                    {
                        Cell cell = myCellGrid[ c, r ][ i ];
                        cell.StepPhysics( this );
                        int gc = (int) ( cell.Position.X / stGridSize );
                        int gr = (int) ( cell.Position.Y / stGridSize );
                        if ( gc != c || gr != r )
                        {
                            myCellGrid[ c, r ].RemoveAt( i );
                            displaced.Add( cell );
                        }
                    }
                }
            }

            foreach ( Cell cell in displaced )
            {
                int gc = (int) ( cell.Position.X / stGridSize ) % myCols;
                int gr = (int) ( cell.Position.Y / stGridSize ) % myRows;
                myCellGrid[ gc, gr ].Add( cell );
            }
        }
    }
}
