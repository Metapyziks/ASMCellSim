using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASMCellSim
{
    internal delegate void InstructionAction( Cell cell );

    internal class Instruction
    {
        private static Instruction[] stDictionary = new Instruction[ 256 ];

        static Instruction()
        {
            byte i = 1;

            // PUSH {literal}
            Register( "PUSH", i, i++, cell =>
            {
                cell.Processor.Push( cell.Processor.ReadByte() );
            } );

            // POP
            Register( "POP", i, i++, cell =>
            {
                cell.Processor.Pop();
            } );

            // PEEK
            Register( "PEEK", i, i++, cell =>
            {
                cell.Processor.Push( cell.Processor.Peek( 0x00 ) );
            } );

            // COPY offset
            Register( "COPY", i, i++, cell =>
            {
                cell.Processor.Push( cell.Processor.Peek( cell.Processor.Pop() ) );
            } );

            // LLOD index
            Register( "LLOD", i, i++, cell =>
            {
                cell.Processor.Push( cell.Processor.LocalLoad( cell.Processor.Pop() ) );
            } );

            // LSTO index val
            Register( "LSTO", i, i++, cell =>
            {
                cell.Processor.LocalStore( cell.Processor.Pop(), cell.Processor.Pop() );
            } );

            // RLOD pindex index
            Register( "RLOD", i, i++, cell =>
            {
                cell.Processor.Push( cell.Processor.RemoteLoad( cell.Processor.Pop(), cell.Processor.Pop() ) );
            } );

            // RSTO pindex index val
            Register( "RSTO", i, i++, cell =>
            {
                cell.Processor.RemoteStore( cell.Processor.Pop(), cell.Processor.Pop(), cell.Processor.Pop() );
            } );

            // JUMP index
            Register( "JUMP", i, i++, cell =>
            {
                cell.Processor.Jump( cell.Processor.Pop() );
            } );

            // JPIF val index
            Register( "JPIF", i, i++, cell =>
            {
                if ( cell.Processor.Pop() != 0x00 )
                    cell.Processor.Jump( cell.Processor.Pop() );
                else
                    cell.Processor.Pop();
            } );

            // CALL pindex
            Register( "CALL", i, i++, cell =>
            {
                cell.Processor.Call( cell.Processor.Pop() );
            } );

            // RTN
            Register( "RTN", i, i++, cell =>
            {
                cell.Processor.Return();
            } );

            // SLP ticks
            Register( "SLP", i, i++, cell =>
            {
                // not implemented
            } );

            // INC val
            Register( "INC", i, i++, cell =>
            {
                cell.Processor.Push( (byte) ( cell.Processor.Pop() + 1 ) );
            } );

            // DEC val
            Register( "DEC", i, i++, cell =>
            {
                cell.Processor.Push( (byte) ( cell.Processor.Pop() - 1 ) );
            } );

            // AND a b
            Register( "AND", i, i++, cell =>
            {
                cell.Processor.Push( (byte) ( cell.Processor.Pop() & cell.Processor.Pop() ) );
            } );

            // OR a b
            Register( "OR", i, i++, cell =>
            {
                cell.Processor.Push( (byte) ( cell.Processor.Pop() | cell.Processor.Pop() ) );
            } );

            // XOR a b
            Register( "XOR", i, i++, cell =>
            {
                cell.Processor.Push( (byte) ( cell.Processor.Pop() ^ cell.Processor.Pop() ) );
            } );

            // NOT val
            Register( "NOT", i, i++, cell =>
            {
                cell.Processor.Push( (byte) ~cell.Processor.Pop() );
            } );

            // ADD a b
            Register( "ADD", i, i++, cell =>
            {
                cell.Processor.Push( (byte) ( cell.Processor.Pop() + cell.Processor.Pop() ) );
            } );

            // SUB a b
            Register( "SUB", i, i++, cell =>
            {
                cell.Processor.Push( (byte) ( cell.Processor.Pop() - cell.Processor.Pop() ) );
            } );

            // MUL a b
            Register( "MUL", i, i++, cell =>
            {
                cell.Processor.Push( (byte) ( cell.Processor.Pop() * cell.Processor.Pop() ) );
            } );

            // DIV a b
            Register( "DIV", i, i++, cell =>
            {
                cell.Processor.Push( (byte) ( cell.Processor.Pop() / cell.Processor.Pop() ) );
            } );

            // LSFT val n
            Register( "LSFT", i, i++, cell =>
            {
                cell.Processor.Push( (byte) ( cell.Processor.Pop() << ( cell.Processor.Pop() & 0x7 ) ) );
            } );

            // LLOP val n
            Register( "LLOP", i, i++, cell =>
            {
                byte val = cell.Processor.Pop();
                byte n = cell.Processor.Pop();
                cell.Processor.Push( (byte) ( val << ( n & 0x7 ) | val >> ( 8 - ( n & 0x7 ) ) ) );
            } );

            // RSFT val n
            Register( "RSFT", i, i++, cell =>
            {
                cell.Processor.Push( (byte) ( cell.Processor.Pop() >> ( cell.Processor.Pop() & 0x7 ) ) );
            } );

            // RLOP val n
            Register( "RLOP", i, i++, cell =>
            {
                byte val = cell.Processor.Pop();
                byte n = cell.Processor.Pop();
                cell.Processor.Push( (byte) ( val >> ( n & 0x7 ) | val << ( 8 - ( n & 0x7 ) ) ) );
            } );

            // EQUL a b
            Register( "EQUL", i, i++, cell =>
            {
                cell.Processor.Push( (byte) ( cell.Processor.Pop() == cell.Processor.Pop() ? 0x01 : 0x00 ) );
            } );

            // GRT a b
            Register( "GRT", i, i++, cell =>
            {
                cell.Processor.Push( (byte) ( cell.Processor.Pop() > cell.Processor.Pop() ? 0x01 : 0x00 ) );
            } );

            // LST a b
            Register( "LST", i, i++, cell =>
            {
                cell.Processor.Push( (byte) ( cell.Processor.Pop() < cell.Processor.Pop() ? 0x01 : 0x00 ) );
            } );

            // ECHK
            Register( "ECHK", i, i++, cell =>
            {
                cell.Processor.Push( (byte) ( cell.Energy >> 8 ) );
            } );

            // EGIV hectant amount
            Register( "EGIV", i, i++, cell =>
            {
                // not implemented
            } );

            // SCAN hectant
            Register( "SCAN", i, i++, cell =>
            {
                // not implemented
            } );

            // JET hectant power
            Register( "JET", i, i++, cell =>
            {
                // not implemented
            } );

            // LCHK hectant
            Register( "LCHK", i, i++, cell =>
            {
                // not implemented
            } );

            // LINK hectant
            Register( "LINK", i, i++, cell =>
            {
                // not implemented
            } );

            // MCHK hectant
            Register( "MCHK", i, i++, cell =>
            {
                // not implemented
            } );

            // MGET hectant
            Register( "MGET", i, i++, cell =>
            {
                // not implemented
            } );

            // MSND hectant message
            Register( "MSND", i, i++, cell =>
            {
                // not implemented
            } );

            // DUP hectant
            Register( "DUP", i, i++, cell =>
            {
                // not implemented
            } );
        }

        internal static void Register( String mnemonic, byte rangeStart, byte rangeEnd,
            InstructionAction action )
        {
            Instruction inst = new Instruction( mnemonic, action );

            for ( int i = rangeStart; i <= rangeEnd; ++i )
                stDictionary[ i ] = inst;
        }

        internal static Instruction Get( byte id )
        {
            return stDictionary[ id ];
        }

        internal readonly String Mnemonic;
        internal readonly InstructionAction Action;

        private Instruction( String mnemonic, InstructionAction action )
        {
            Mnemonic = mnemonic;
            Action = action;
        }
    }
}
