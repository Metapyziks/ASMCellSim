using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASMCellSim
{
    public class Processor
    {
        public const int StackSize = 256;

        private byte[] myStackMemory;

        private byte myPI;
        private byte myPC;
        private byte mySP;
        private byte mySM;

        private bool myPCOverflow;

        public byte[][] Memory { get; private set; }

        public byte[] CurrentProgram
        {
            get { return Memory[ myPI ]; }
        }

        public bool EndOfProgram
        {
            get { return CurrentProgram == null || myPCOverflow; }
        }

        public Processor()
        {
            Memory = new byte[ 256 ][];

            myPI = 0;
            myPC = 0;

            myPCOverflow = true;

            myStackMemory = new byte[ StackSize ];
            mySP = 0;
            mySM = 0;
        }

        public void LoadCode( byte[][] code )
        {
            Memory = code;
            myPI = 0;
            myPC = 0;
            myPCOverflow = code[ 0 ] == null;
        }

        public byte ReadByte()
        {
            if ( !EndOfProgram )
            {
                if ( myPC == 255 )
                    myPCOverflow = true;

                return CurrentProgram[ myPC++ ];
            }

            return 0x00;
        }

        public void Jump( byte index )
        {
            myPC = index;
            myPCOverflow = false;
        }

        public void Call( byte programIndex )
        {
            Push( myPC );
            Push( myPI );
            Push( mySM );
            mySM = mySP;

            myPI = programIndex;
            Jump( 0 );
        }

        public void Return()
        {
            if ( mySM == 0 )
                myPCOverflow = true;
            else
            {
                mySP = mySM;
                mySM = myStackMemory[ --mySP ];
                myPI = Pop();
                Jump( Pop() );
            }
        }

        public void Push( byte value )
        {
            myStackMemory[ mySP++ ] = value;
        }

        public byte Pop()
        {
            --mySP;
            if ( mySP == 255 || mySP < mySM )
            {
                mySP = mySM;
                return 0x00;
            }

            return myStackMemory[ mySP ];
        }

        public byte Peek( int offset )
        {
            if ( mySP > offset )
                return myStackMemory[ mySP - offset - 1 ];
            else
                return 0x00;
        }

        public void LocalStore( byte index, byte value )
        {
            RemoteStore( myPI, index, value );
        }

        public byte LocalLoad( byte index )
        {
            return RemoteLoad( myPI, index );
        }

        public void RemoteStore( byte pindex, byte index, byte value )
        {
            if ( Memory[ pindex ] == null )
                Memory[ pindex ] = new byte[ 256 ];

            Memory[ pindex ][ index ] = value;
        }

        public byte RemoteLoad( byte pindex, byte index )
        {
            if ( Memory[ pindex ] == null )
                return 0x00;

            return Memory[ pindex ][ index ];
        }

        private static byte[] stArgs = new byte[ 4 ];
        public void Step( Cell cell )
        {
            if ( !EndOfProgram )
            {
                byte instID = ReadByte();
                Instruction inst = Instruction.Get( instID );
                if ( inst != null )
                {
                    byte argFlags = (byte) ( instID - inst.InstructionID );
                    for ( int i = 0; i < inst.ArgCount; ++i )
                    {
                        if ( ( ( argFlags >> i ) & 0x1 ) != 0 )
                            stArgs[ i ] = ReadByte();
                        else
                            stArgs[ i ] = Pop();
                    }

                    inst.Action( cell, stArgs );
                }

                if ( EndOfProgram )
                    Return();
            }
        }
    }
}
