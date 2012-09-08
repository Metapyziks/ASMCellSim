using System;
using System.Collections.Generic;
using System.Linq;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using OpenTKTools;

using ASMCellSim;

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

        private SpriteShader mySpriteShader;
        private Sprite myCellSprite;

        internal Program()
            : base( 800, 600, new GraphicsMode( new ColorFormat( 8, 8, 8, 0 ), 0, 0, 4 ), "ASMCellSim Visualizer" )
        {
            myWorld = new World( 256f, true );
            myCamPos = new Vector2( 128f, 128f );

            myWorld.AddCell( new Vector2( 128f, 128f ) );
            myWorld.AddCell( new Vector2( 129f, 128.5f ) );
            myWorld.AddCell( new Vector2( 132f, 128f ) );

            myCamScale = 16.0f;
        }

        protected override void OnLoad( EventArgs e )
        {
            mySpriteShader = new SpriteShader( Width, Height );
            myCellSprite = new Sprite( new BitmapTexture2D( ASMCellSim.Visualizer.Properties.Resources.cell ), myCamScale / 32.0f );
            myCellSprite.UseCentreAsOrigin = true;
        }

        protected override void OnUpdateFrame( FrameEventArgs e )
        {
            myWorld.Step();
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
                myCellSprite.Render( mySpriteShader );
            }
            mySpriteShader.End();

            SwapBuffers();
        }
    }
}
