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

            Register( "PUSH", i, i++, cell =>
            {
                byte val = cell.Processor.ReadByte();
                cell.Processor.Push( val );
                cell.UseEnergy( 1 );
            } );

            Register( "POP", i, i++, cell =>
            {
                cell.Processor.Pop();
            } );

            Register( "COPY", i, i++, cell =>
            {
                cell.Processor.Push( cell.Processor.Peek() );
            } );

            Register( "LSTORE", i, i++, cell =>
            {
                byte index = cell.Processor.Pop();
                cell.Processor.LocalStore( index, cell.Processor.Pop() );
                cell.UseEnergy( 1 );
            } );

            Register( "LLOAD", i, i++, cell =>
            {
                byte index = cell.Processor.Pop();
                cell.Processor.Push( cell.Processor.LocalLoad( index ) );
                cell.UseEnergy( 1 );
            } );

            Register( "RSTORE", i, i++, cell =>
            {
                byte index = cell.Processor.Pop();
                cell.Processor.LocalStore( index, cell.Processor.Pop() );
                cell.UseEnergy( 1 );
            } );

            Register( "RLOAD", i, i++, cell =>
            {
                byte index = cell.Processor.Pop();
                cell.Processor.Push( cell.Processor.LocalLoad( index ) );
                cell.UseEnergy( 1 );
            } );

            Register( "JUMP", i, i++, cell =>
            {
                byte index = cell.Processor.Pop();
                cell.Processor.Jump( index );
                cell.UseEnergy( 1 );
            } );

            Register( "JIF", i, i++, cell =>
            {
                byte index = cell.Processor.Pop();
                byte val = cell.Processor.Pop();
                if ( val != 0x00 )
                {
                    cell.Processor.Jump( index );
                    cell.UseEnergy( 2 );
                }
                else
                    cell.UseEnergy( 1 );
            } );

            Register( "CALL", i, i++, cell =>
            {
                byte index = cell.Processor.Pop();
                cell.Processor.Call( index );
                cell.UseEnergy( 2 );
            } );

            Register( "RET", i, i++, cell =>
            {
                cell.Processor.Return();
                cell.UseEnergy( 2 );
            } );

            Register( "AND", i, i++, cell =>
            {
                byte valB = cell.Processor.Pop();
                byte valA = cell.Processor.Pop();
                cell.Processor.Push( (byte) ( valA & valB ) );
                cell.UseEnergy( 1 );
            } );

            Register( "OR", i, i++, cell =>
            {
                byte valB = cell.Processor.Pop();
                byte valA = cell.Processor.Pop();
                cell.Processor.Push( (byte) ( valA | valB ) );
                cell.UseEnergy( 1 );
            } );

            Register( "XOR", i, i++, cell =>
            {
                byte valB = cell.Processor.Pop();
                byte valA = cell.Processor.Pop();
                cell.Processor.Push( (byte) ( valA ^ valB ) );
                cell.UseEnergy( 1 );
            } );

            Register( "NOT", i, i++, cell =>
            {
                byte val = cell.Processor.Pop();
                cell.Processor.Push( (byte) ( ~val ) );
                cell.UseEnergy( 1 );
            } );

            Register( "ADD", i, i++, cell =>
            {
                byte valB = cell.Processor.Pop();
                byte valA = cell.Processor.Pop();
                cell.Processor.Push( (byte) ( valA + valB ) );
                cell.UseEnergy( 2 );
            } );

            Register( "SUB", i, i++, cell =>
            {
                byte valB = cell.Processor.Pop();
                byte valA = cell.Processor.Pop();
                cell.Processor.Push( (byte) ( valA - valB ) );
                cell.UseEnergy( 2 );
            } );

            Register( "MUL", i, i++, cell =>
            {
                byte valB = cell.Processor.Pop();
                byte valA = cell.Processor.Pop();
                cell.Processor.Push( (byte) ( valA * valB ) );
                cell.UseEnergy( 4 );
            } );

            Register( "DIV", i, i++, cell =>
            {
                byte valB = cell.Processor.Pop();
                byte valA = cell.Processor.Pop();
                cell.Processor.Push( (byte) ( valA / valB ) );
                cell.UseEnergy( 4 );
            } );

            Register( "SHIFTL", i, i++, cell =>
            {
                byte valB = cell.Processor.Pop();
                byte valA = cell.Processor.Pop();
                cell.Processor.Push( (byte) ( valA << valB ) );
                cell.UseEnergy( 1 );
            } );

            Register( "LOOPL", i, i++, cell =>
            {
                byte valB = cell.Processor.Pop();
                byte valA = cell.Processor.Pop();
                cell.Processor.Push( (byte) ( valA << ( valB & 0x7 ) | valA >> ( 8 - ( valB & 0x7 ) ) ) );
                cell.UseEnergy( 2 );
            } );

            Register( "SHIFTR", i, i++, cell =>
            {
                byte valB = cell.Processor.Pop();
                byte valA = cell.Processor.Pop();
                cell.Processor.Push( (byte) ( valA >> valB ) );
                cell.UseEnergy( 1 );
            } );

            Register( "LOOPR", i, i++, cell =>
            {
                byte valB = cell.Processor.Pop();
                byte valA = cell.Processor.Pop();
                cell.Processor.Push( (byte) ( valA >> ( valB & 0x7 ) | valA << ( 8 - ( valB & 0x7 ) ) ) );
                cell.UseEnergy( 2 );
            } );

            Register( "EQUAL", i, i++, cell =>
            {
                byte valB = cell.Processor.Pop();
                byte valA = cell.Processor.Pop();
                cell.Processor.Push( (byte) ( valA == valB ? 0x01 : 0x00 ) );
                cell.UseEnergy( 1 );
            } );

            Register( "GREATER", i, i++, cell =>
            {
                byte valB = cell.Processor.Pop();
                byte valA = cell.Processor.Pop();
                cell.Processor.Push( (byte) ( valA > valB ? 0x01 : 0x00 ) );
                cell.UseEnergy( 1 );
            } );

            Register( "GORE", i, i++, cell =>
            {
                byte valB = cell.Processor.Pop();
                byte valA = cell.Processor.Pop();
                cell.Processor.Push( (byte) ( valA >= valB ? 0x01 : 0x00 ) );
                cell.UseEnergy( 1 );
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
