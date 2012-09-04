using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASMCellSim
{
    internal class Processor
    {
        internal const int StackSize = 256;

        private byte[] myStackMemory;

        private byte myPI;
        private byte myPC;
        private byte mySP;

        private bool myPCOverflow;

        internal byte[][] Memory { get; private set; }

        internal byte[] CurrentProgram
        {
            get { return Memory[ myPI ]; }
        }

        internal bool EndOfProgram
        {
            get { return CurrentProgram == null || myPCOverflow; }
        }

        internal Processor()
        {
            Memory = new byte[ 256 ][];

            myPI = 0;
            myPC = 0;

            myPCOverflow = true;

            mySP = 0;
        }

        internal byte ReadByte()
        {
            if ( !EndOfProgram )
            {
                if ( myPC == 255 )
                    myPCOverflow = true;

                return CurrentProgram[ myPC++ ];
            }

            return 0x00;
        }

        internal void Jump( byte index )
        {
            myPC = index;
            myPCOverflow = false;
        }

        internal void Call( byte programIndex )
        {
            Push( myPC );
            Push( myPI );

            myPI = programIndex;
            Jump( 0 );
        }

        internal void Return()
        {
            myPI = Pop();
            Jump( Pop() );
        }

        internal void Push( byte value )
        {
            myStackMemory[ mySP++ ] = value;
        }

        internal byte Pop()
        {
            --mySP;
            if ( mySP == 255 )
            {
                mySP = 0;
                return 0x00;
            }

            return myStackMemory[ mySP ];
        }

        internal byte Peek( int offset )
        {
            if ( mySP > ( offset + 1 ) )
                return myStackMemory[ mySP - ( offset + 1 ) ];
            else
                return 0x00;
        }

        internal void LocalStore( byte index, byte value )
        {
            RemoteStore( myPI, index, value );
        }

        internal byte LocalLoad( byte index )
        {
            return RemoteLoad( myPI, index );
        }

        internal void RemoteStore( byte pindex, byte index, byte value )
        {
            if ( Memory[ pindex ] == null )
                Memory[ pindex ] = new byte[ 256 ];

            Memory[ pindex ][ index ] = value;
        }

        internal byte RemoteLoad( byte pindex, byte index )
        {
            if ( Memory[ pindex ] == null )
                return 0x00;

            return Memory[ pindex ][ index ];
        }

        internal void Step( Cell cell )
        {
            if ( !EndOfProgram )
            {
                byte instID = ReadByte();
                Instruction inst = Instruction.Get( instID );
                if ( inst != null )
                    inst.Action( cell );

                if ( EndOfProgram )
                    Return();
            }
        }
    }
}
