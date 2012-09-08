using System;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using OpenTKTools;

using System.Diagnostics;

namespace ASMCellSim.Visualizer
{
    internal class Program : GameWindow
    {
        internal static void Main( String[] args )
        {
            Program program = new Program();
            program.Run();
            program.Dispose();
        }

        private World myWorld;
        private Vector2 myCamPos;
        private float myCamScale;

        private Cell myDraggedCell;

        private Stopwatch myTimer;
        private double myUpdatePeriod;

        private SpriteShader mySpriteShader;
        private Sprite myCellSprite;

        internal Program()
            : base( 800, 600, new GraphicsMode( new ColorFormat( 8, 8, 8, 0 ), 0, 0, 4 ), "ASMCellSim Visualizer" )
        {
            myWorld = new World( 256f, true );
            myCamPos = new Vector2( 128f, 128f );
            myCamScale = 16.0f;

            myUpdatePeriod = 1f / 60f;

            myTimer = new Stopwatch();

            Cell[] cells = new Cell[ 16 ];
            for ( int i = 0; i < 16; ++i )
            {
                cells[ i ] = myWorld.AddCell( new Vector2( 128f + i, 128f ) );

                if ( i > 0 )
                    cells[ i ].Attach( cells[ i - 1 ], Hectant.Back, Hectant.Front );
            }

            myDraggedCell = cells[ 0 ];
        }

        protected override void OnLoad( EventArgs e )
        {
            mySpriteShader = new SpriteShader( Width, Height );
            myCellSprite = new Sprite( new BitmapTexture2D( ASMCellSim.Visualizer.Properties.Resources.cell, TextureMinFilter.LinearMipmapLinear, TextureMagFilter.Linear ), myCamScale / 32.0f );
            myCellSprite.UseCentreAsOrigin = true;

            GL.ClearColor( Color4.LightGray );

            myTimer.Start();
        }

        protected override void OnUpdateFrame( FrameEventArgs e )
        {
            if ( myTimer.Elapsed.TotalSeconds >= myUpdatePeriod )
            {
                myWorld.Step();
                myTimer.Restart();

                if ( myDraggedCell != null && Mouse[ OpenTK.Input.MouseButton.Left ] )
                {
                    float mouseX = ( Mouse.X - Width / 2f ) / myCamScale + myCamPos.X;
                    float mouseY = ( Mouse.Y - Height / 2f ) / myCamScale + myCamPos.Y;

                    myDraggedCell.Position = new Vector2( mouseX, mouseY );
                }
            }
        }

        protected override void OnRenderFrame( FrameEventArgs e )
        {
            GL.Clear( ClearBufferMask.ColorBufferBit );

            World.NearbyCellEnumerator iter = new World.NearbyCellEnumerator( myWorld,
                myCamPos.X - Width / ( 2f * myCamScale ),
                myCamPos.Y - Height / ( 2f * myCamScale ),
                Width / myCamScale, Height / myCamScale );

            mySpriteShader.Begin();
            while ( iter.MoveNext() )
            {
                Cell cell = iter.Current;
                myCellSprite.X = ( cell.Position.X - myCamPos.X ) * myCamScale + Width / 2f;
                myCellSprite.Y = ( cell.Position.Y - myCamPos.Y ) * myCamScale + Height / 2f;
                myCellSprite.Rotation = cell.Rotation + MathHelper.PiOver2;
                myCellSprite.Render( mySpriteShader );
            }
            mySpriteShader.End();

            SwapBuffers();
        }
    }
}
