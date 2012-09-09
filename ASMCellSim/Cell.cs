using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ASMCellSim
{
    public enum Hectant : byte
    {
        Front = 0x00,
        FrontLeft = 0x40,
        FrontRight = 0x60,
        Back = 0x80,
        BackLeft = 0xc0,
        BackRight = 0xe0
    }

    public enum AttachmentState : byte
    {
        None = 0x00,
        ThisToThat = 0x01,
        ThatToThis = 0x02,
        TwoWay = 0x03
    }

    public class Cell
    {
        private static byte HectantToIndex( Hectant hect )
        {
            switch ( hect )
            {
                case Hectant.Back:
                    return 0x03;
                case Hectant.FrontLeft:
                    return 0x05;
                case Hectant.FrontRight:
                    return 0x01;
                case Hectant.BackLeft:
                    return 0x04;
                case Hectant.BackRight:
                    return 0x02;
                default:
                    return 0x00;
            }
        }

        public const float Radius = 1.0f;
        public const float RepulsionMultiplier = 1.0f / 64.0f;
        public const float BondMultiplier = 1.0f / 2.0f;

        private const float stDiam2 = ( Radius * 2 ) * ( Radius * 2 );

        public const ushort StartingEnergy = 4096;
        public const ushort ReproduceEnergy = 8192;

        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }

        private Vector2 myPosJump;

        public float Rotation { get; set; }
        public float RotSpeed { get; set; }

        public ushort Energy { get; private set; }

        public Cell[] Attachments { get; private set; }
        public AttachmentState[] AttStates { get; private set; }

        internal Processor Processor { get; private set; }

        internal Cell( Vector2 pos )
        {
            Position = pos;
            Velocity = new Vector2();

            Rotation = 0f;
            RotSpeed = 0f;

            Energy = StartingEnergy;

            Attachments = new Cell[ 6 ];
            AttStates = new AttachmentState[ 6 ];

            Processor = new Processor();
        }

        private int GetAttachmentIndex( Cell cell )
        {
            for ( int i = 0; i < 6; ++i )
                if ( Attachments[ i ] == cell )
                    return i;

            return -1;
        }

        public Cell GetAttachment( Hectant hect )
        {
            return Attachments[ HectantToIndex( hect ) ];
        }

        public void Attach( Cell cell, Hectant thisHect, Hectant thatHect )
        {
            this.Attach( cell, thisHect );
            cell.Attach( this, thatHect );
        }

        private void Attach( Cell cell, Hectant hect )
        {
            int index = HectantToIndex( hect );

            if ( Attachments[ index ] != null )
                Attachments[ index ].Unattach( this );

            Attachments[ index ] = cell;
            AttStates[ index ] = AttachmentState.TwoWay;
        }

        public void Unattach( Cell cell )
        {
            for ( int i = 0; i < 6; ++i )
            {
                if ( Attachments[ i ] == cell )
                {
                    Attachments[ i ] = null;
                    AttStates[ i ] = AttachmentState.None;
                }
            }
        }

        private float GetHectantAngle( Hectant hect )
        {
            return Rotation + HectantToIndex( hect ) * (float) Math.PI / 3f;
        }

        public void LoadCode( String filePath )
        {
            Processor.LoadCode( Assembler.Assemble( filePath ) );
        }

        public void LoadCode( Stream stream )
        {
            Processor.LoadCode( Assembler.Assemble( stream ) );
        }

        public void LoadCode( byte[][] bytecode )
        {
            Processor.LoadCode( bytecode );
        }

        internal void UseEnergy( ushort amount )
        {
            if ( Energy > amount )
                Energy -= amount;
            else
                Energy = 0;
        }

        internal void StepPhysics( World world )
        {
            Position = world.Wrap( myPosJump + Velocity );
            Velocity *= world.Friction;

            Rotation += RotSpeed;
            if ( Rotation >= Math.PI )
                Rotation -= (float) Math.PI * 2f;
            else if ( Rotation < -Math.PI )
                Rotation += (float) Math.PI * 2f;
            RotSpeed *= world.Friction;
        }

        internal void Step( World world )
        {
            myPosJump = Position;

            for ( int i = 0; i < 6; ++i )
            {
                if ( Attachments[ i ] != null )
                {
                    Cell cell = Attachments[ i ];
                    int index = cell.GetAttachmentIndex( this );
                    float angle = cell.Rotation + index * (float) Math.PI / 3f;
                    Vector2 dest = cell.Position;
                    dest.X += (float) Math.Cos( angle ) * Radius * 2f;
                    dest.Y += (float) Math.Sin( angle ) * Radius * 2f;
                    Velocity += world.Difference( Position, dest ) * BondMultiplier;
                    // myPosJump += world.Difference( Position, dest ) / 2f * world.Friction;
                    Vector2 diff = world.Difference( Position, cell.Position );
                    angle = (float) Math.Atan2( diff.Y, diff.X ) - i * (float) Math.PI / 3f;
                    float angDiff = angle - Rotation;
                    if ( angDiff > Math.PI )
                        angDiff -= (float) ( Math.PI * 2.0 );
                    else if ( angDiff < -Math.PI )
                        angDiff += (float) ( Math.PI * 2.0 );
                    RotSpeed += angDiff * BondMultiplier;
                }
            }

            Vector2 newPos = myPosJump;

            World.NearbyCellEnumerator iter = new World.NearbyCellEnumerator( world, newPos, Radius * 2.0f );
            while ( iter.MoveNext() )
            {
                Cell cur = iter.Current;
                if ( cur != this )
                {
                    Vector2 diff = world.Difference( newPos, cur.Position );
                    float dist2 = diff.Length2;
                    if ( dist2 < stDiam2 && dist2 > 0f )
                    {
                        Vector2 dest = cur.Position - diff.Normal * Radius * 2f;
                        // Velocity += world.Difference( newPos, dest ) * RepulsionMultiplier;
                        myPosJump += world.Difference( newPos, dest ) / 2f * world.Friction;
                    }
                }
            }

            Processor.Step( this );
        }
    }
}
