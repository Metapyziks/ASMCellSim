using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASMCellSim
{
    internal delegate void InstructionAction( Cell cell, byte[] args );

    internal class Instruction
    {
        private static List<Instruction> stToAdd = new List<Instruction>();
        private static Instruction[] stDictionary = new Instruction[ 256 ];

        static Instruction()
        {
            // PUSH value
            Register( "PUSH", 1, ( cell, args ) =>
            {
                cell.Processor.Push( args[ 0 ] );
            } );

            // POP
            Register( "POP", 0, ( cell, args ) =>
            {
                cell.Processor.Pop();
            } );

            // PEEK
            Register( "PEEK", 0, ( cell, args ) =>
            {
                cell.Processor.Push( cell.Processor.Peek( 0x00 ) );
            } );

            // COPY offset
            Register( "COPY", 1, ( cell, args ) =>
            {
                cell.Processor.Push( cell.Processor.Peek( args[ 0 ] ) );
            } );

            // LLOD index
            Register( "LLOD", 1, ( cell, args ) =>
            {
                cell.Processor.Push( cell.Processor.LocalLoad( args[ 0 ] ) );
            } );

            // LSTO index val
            Register( "LSTO", 2, ( cell, args ) =>
            {
                cell.Processor.LocalStore( args[ 0 ], args[ 1 ] );
            } );

            // RLOD pindex index
            Register( "RLOD", 2, ( cell, args ) =>
            {
                cell.Processor.Push( cell.Processor.RemoteLoad( args[ 0 ], args[ 1 ] ) );
            } );

            // RSTO pindex index val
            Register( "RSTO", 3, ( cell, args ) =>
            {
                cell.Processor.RemoteStore( args[ 0 ], args[ 1 ], args[ 2 ] );
            } );

            // JUMP index
            Register( "JUMP", 1, ( cell, args ) =>
            {
                cell.Processor.Jump( args[ 0 ] );
            } );

            // JPIF val index
            Register( "JPIF", 2, ( cell, args ) =>
            {
                if ( args[ 0 ] != 0x00 )
                    cell.Processor.Jump( args[ 1 ] );
            } );

            // CALL pindex
            Register( "CALL", 1, ( cell, args ) =>
            {
                cell.Processor.Call( args[ 0 ] );
            } );

            // RTN
            Register( "RTN", 0, ( cell, args ) =>
            {
                cell.Processor.Return();
            } );

            // SLP ticks
            Register( "SLP", 1, ( cell, args ) =>
            {
                // not implemented
            } );

            // INC val
            Register( "INC", 1, ( cell, args ) =>
            {
                cell.Processor.Push( (byte) ( args[ 0 ] + 1 ) );
            } );

            // DEC val
            Register( "DEC", 1, ( cell, args ) =>
            {
                cell.Processor.Push( (byte) ( args[ 0 ] - 1 ) );
            } );

            // AND a b
            Register( "AND", 2, ( cell, args ) =>
            {
                cell.Processor.Push( (byte) ( args[ 0 ] & args[ 1 ] ) );
            } );

            // OR a b
            Register( "OR", 2, ( cell, args ) =>
            {
                cell.Processor.Push( (byte) ( args[ 0 ] | args[ 1 ] ) );
            } );

            // XOR a b
            Register( "XOR", 2, ( cell, args ) =>
            {
                cell.Processor.Push( (byte) ( args[ 0 ] ^ args[ 1 ] ) );
            } );

            // NOT val
            Register( "NOT", 2, ( cell, args ) =>
            {
                cell.Processor.Push( (byte) ~args[ 0 ] );
            } );

            // ADD a b
            Register( "ADD", 2, ( cell, args ) =>
            {
                cell.Processor.Push( (byte) ( args[ 0 ] + args[ 1 ] ) );
            } );

            // SUB a b
            Register( "SUB", 2, ( cell, args ) =>
            {
                cell.Processor.Push( (byte) ( args[ 0 ] - args[ 1 ] ) );
            } );

            // MUL a b
            Register( "MUL", 2, ( cell, args ) =>
            {
                cell.Processor.Push( (byte) ( args[ 0 ] * args[ 1 ] ) );
            } );

            // DIV a b
            Register( "DIV", 2, ( cell, args ) =>
            {
                cell.Processor.Push( (byte) ( args[ 0 ] / args[ 1 ] ) );
            } );

            // LSFT val n
            Register( "LSFT", 2, ( cell, args ) =>
            {
                cell.Processor.Push( (byte) ( args[ 0 ] << ( args[ 1 ] & 0x7 ) ) );
            } );

            // LLOP val n
            Register( "LLOP", 2, ( cell, args ) =>
            {
                cell.Processor.Push( (byte) ( args[ 0 ] << ( args[ 1 ] & 0x7 ) | args[ 0 ] >> ( 8 - ( args[ 1 ] & 0x7 ) ) ) );
            } );

            // RSFT val n
            Register( "RSFT", 2, ( cell, args ) =>
            {
                cell.Processor.Push( (byte) ( args[ 0 ] >> ( args[ 1 ] & 0x7 ) ) );
            } );

            // RLOP val n
            Register( "RLOP", 2, ( cell, args ) =>
            {
                cell.Processor.Push( (byte) ( args[ 0 ] >> ( args[ 1 ] & 0x7 ) | args[ 0 ] << ( 8 - ( args[ 1 ] & 0x7 ) ) ) );
            } );

            // EQUL a b
            Register( "EQUL", 2, ( cell, args ) =>
            {
                cell.Processor.Push( (byte) ( args[ 0 ] == args[ 1 ] ? 0x01 : 0x00 ) );
            } );

            // GRT a b
            Register( "GRT", 2, ( cell, args ) =>
            {
                cell.Processor.Push( (byte) ( args[ 0 ] > args[ 1 ] ? 0x01 : 0x00 ) );
            } );

            // LST a b
            Register( "LST", 2, ( cell, args ) =>
            {
                cell.Processor.Push( (byte) ( args[ 0 ] < args[ 1 ] ? 0x01 : 0x00 ) );
            } );

            // ECHK
            Register( "ECHK", 0, ( cell, args ) =>
            {
                cell.Processor.Push( (byte) ( cell.Energy >> 8 ) );
            } );

            // EGIV hectant amount
            Register( "EGIV", 2, ( cell, args ) =>
            {
                // not implemented
            } );

            // SCAN hectant
            Register( "SCAN", 1, ( cell, args ) =>
            {
                // not implemented
            } );

            // JET hectant power
            Register( "JET", 2, ( cell, args ) =>
            {
                // not implemented
            } );

            // LCHK hectant
            Register( "LCHK", 1, ( cell, args ) =>
            {
                // not implemented
            } );

            // LINK hectant
            Register( "LINK", 1, ( cell, args ) =>
            {
                // not implemented
            } );

            // MCHK hectant
            Register( "MCHK", 1, ( cell, args ) =>
            {
                // not implemented
            } );

            // MGET hectant
            Register( "MGET", 1, ( cell, args ) =>
            {
                // not implemented
            } );

            // MSND hectant message
            Register( "MSND", 2, ( cell, args ) =>
            {
                // not implemented
            } );

            // DUP hectant
            Register( "DUP", 1, ( cell, args ) =>
            {
                // not implemented
            } );

            OrganiseInstructions();
        }

        private static void Register( String mnemonic, byte argCount,
            InstructionAction action )
        {
            stToAdd.Add( new Instruction( mnemonic, argCount, action ) );
        }

        private static void OrganiseInstructions()
        {
            int tot = 0;
            foreach ( Instruction inst in stToAdd )
            {
                inst.InstructionID = (byte) tot;
                tot += 1 << inst.ArgCount;
            }

            if ( tot > 256 )
                throw new Exception( "Too many instructions!" );
            
            int i = 0;
            foreach ( Instruction inst in stToAdd )
            {
                for ( int j = 0; j < 1 << inst.ArgCount; ++j, ++i )
                    stDictionary[ i ] = inst;
            }

            stToAdd.Clear();
        }

        internal static Instruction Get( byte id )
        {
            return stDictionary[ id ];
        }

        internal readonly String Mnemonic;
        internal readonly byte ArgCount;
        internal readonly InstructionAction Action;

        internal byte InstructionID { get; private set; }

        private Instruction( String mnemonic, byte argCount, InstructionAction action )
        {
            Mnemonic = mnemonic;
            ArgCount = argCount;
            Action = action;

            InstructionID = 0x00;
        }
    }
}
