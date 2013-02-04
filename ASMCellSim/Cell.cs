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

        public const double Radius = 1.0;
        public const double RepulsionMultiplier = 1.0 / 64.0;
        public const double BondMultiplier = 1.0 / 2.0;

        private const double stDiam = Radius * 2;
        private const double stDiam2 = stDiam * stDiam;

        public const ushort StartingEnergy = 4096;
        public const ushort ReproduceEnergy = 8192;

        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public Vector2 TargVel { get; set; }

        public double Rotation { get; set; }
        public double RotSpeed { get; set; }

        public ushort Energy { get; private set; }

        public Cell[] Attachments { get; private set; }
        public AttachmentState[] AttStates { get; private set; }

        internal Processor Processor { get; private set; }

        internal Cell( Vector2 pos )
        {
            Position = pos;
            Velocity = new Vector2();

            Rotation = 0.0;
            RotSpeed = 0.0;

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

        private double GetHectantAngle( Hectant hect )
        {
            return Rotation + HectantToIndex( hect ) * Math.PI / 3.0;
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

        internal void StepPhysics( World world, double dt )
        {
            Velocity += ( TargVel - Velocity ) * world.Friction;
            Position += Velocity;
            TargVel = new Vector2();
        }

        internal void Step( World world, double dt )
        {
            for ( int i = 0; i < 6; ++i )
            {
                if ( Attachments[ i ] != null )
                {
                    Cell cell = Attachments[ i ];
                    Vector2 delta = world.Difference( Position, cell.Position );
                    if ( delta.Length2 > stDiam2 )
                    {
                        delta *= stDiam2 / ( delta.Length2 + stDiam2 ) - 0.5;
                        TargVel -= delta;
                    }
                }
            }

            World.NearbyCellEnumerator iter = new World.NearbyCellEnumerator( world, Position, stDiam );
            while ( iter.MoveNext() )
            {
                Cell cell = iter.Current;
                Vector2 delta = world.Difference( Position, cell.Position );
                double len2 = delta.Length2;
                if ( len2 < stDiam2 )
                {
                    delta *= stDiam2 / ( delta.Length2 + stDiam2 ) - 0.5;
                    TargVel -= delta;
                }
            }

            Processor.Step( this );
        }
    }
}
