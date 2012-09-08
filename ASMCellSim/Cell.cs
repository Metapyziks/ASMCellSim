using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASMCellSim
{
    public class Cell
    {
        public const float Radius = 1.0f;
        public const float Diam2 = ( Radius * 2 ) * ( Radius * 2 );
        public const float RepulsionMultiplier = 0.5f;

        public const ushort StartingEnergy = 4096;
        public const ushort ReproduceEnergy = 8192;

        public Vector2 Position { get; private set; }
        public Vector2 Velocity { get; private set; }
        public Vector2 Acceleration { get; private set; }

        public ushort Energy { get; private set; }

        public Processor Processor { get; private set; }

        public Cell( Vector2 pos )
        {
            Position = pos;
            Velocity = new Vector2();
            Acceleration = new Vector2();

            Energy = StartingEnergy;

            Processor = new Processor();
        }

        public void UseEnergy( ushort amount )
        {
            if ( Energy > amount )
                Energy -= amount;
            else
                Energy = 0;
        }

        public void StepPhysics( World world )
        {
            Position += Velocity;
            Velocity += Acceleration;
            Acceleration = new Vector2();
        }

        public void Step( World world )
        {
            World.NearbyCellEnumerator iter = new World.NearbyCellEnumerator( world, Position, Radius * 2.0f );
            while ( iter.MoveNext() )
            {
                Cell cur = iter.Current;
                if ( cur != this )
                {
                    Vector2 diff = world.Difference( Position, cur.Position );
                    float dist2 = world.Difference( Position, cur.Position ).Length2;
                    if ( dist2 < Diam2 )
                        Acceleration -= diff * RepulsionMultiplier / dist2;
                }
            }

            Processor.Step( this );
        }
    }
}
