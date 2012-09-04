using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASMCellSim
{
    internal class Processor
    {
        internal const int MemorySize = 256;

        internal List<byte[]> Code { get; private set; }
        internal byte[] Memory { get; private set; }

        internal byte PI { get; private set; }
        internal byte PC { get; private set; }
        internal byte SP { get; private set; }

        internal byte[] CurrentProgram
        {
            get { return Code[ PI ]; }
        }

        internal bool EndOfProgram
        {
            get { return PI >= Code.Count || PC >= CurrentProgram.Length; }
        }

        internal Processor()
        {
            Code = new List<byte[]>();
            Memory = new byte[ MemorySize ];

            PI = 0;
            PC = 0;

            SP = MemorySize - 1;
        }

        internal byte ReadByte()
        {
            if ( !EndOfProgram )
                return CurrentProgram[ PC++ ];

            return 0x00;
        }

        internal void Jump( byte index )
        {
            PC = index;
        }

        internal void Call( byte programIndex )
        {
            Push( PC );
            Push( PI );

            PI = programIndex;
            PC = 0;
        }

        internal void Return()
        {
            PI = Pop();
            PC = Pop();
        }

        internal void Push( byte value )
        {
            Memory[ SP-- ] = value;
        }

        internal byte Pop()
        {
            return Memory[ ++SP ];
        }

        internal byte Peek()
        {
            return Memory[ SP + 1 ];
        }

        internal void Store( byte index, byte value )
        {
            Memory[ index % MemorySize ] = value;
        }

        internal byte Load( byte index )
        {
            return Memory[ index % MemorySize ];
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
