using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace ASMCellSim
{
    internal static class Assembler
    {
        private abstract class Token
        {
            internal byte Value { get; set; }
            internal abstract bool IsLiteral { get; }

            internal virtual byte ArgumentCount { get { return 0; } }

            internal abstract void ResolveValue( Token[] program, int index, Dictionary<String, byte> constants );
        }

        private class LiteralToken : Token
        {
            private String myConstName;

            internal override bool IsLiteral { get { return true; } }

            internal LiteralToken( String literal )
            {
                if ( char.IsNumber( literal[ 0 ] ) )
                {
                    byte val;
                    if ( !byte.TryParse( literal, out val ) )
                        throw new Exception( "Invalid decimal literal: " + literal );
                    
                    Value = val;
                }
                else if ( literal[ 0 ] == '$' )
                {
                    byte val;
                    if ( !byte.TryParse( literal.Substring( 1 ), NumberStyles.HexNumber, null, out val ) )
                        throw new Exception( "Invalid hex literal: " + literal );

                    Value = val;
                }
                else if ( literal[ 0 ] == '@' )
                {
                    myConstName = literal.Substring( 1 ).TrimEnd();

                    if ( myConstName.Length == 0 )
                        throw new Exception( "No identifier for const given" );
                }
                else
                    throw new Exception( "Invalid literal: " + literal );
            }

            internal override void ResolveValue( Token[] program, int index, Dictionary<string, byte> constants )
            {
                if ( myConstName != null )
                {
                    try
                    {
                        Value = constants[ myConstName ];
                    }
                    catch
                    {
                        throw new Exception( "No value given for constant: " + myConstName );
                    }
                }
            }
        }

        private class InstructionToken : Token
        {
            internal override bool IsLiteral { get { return false; } }

            internal Instruction Instruction { get; private set; }
            internal override byte ArgumentCount { get { return Instruction.ArgCount; } }

            internal InstructionToken( String inst )
            {
                try
                {
                    Instruction = Instruction.Get( inst.Trim() );
                }
                catch
                {
                    throw new Exception( "Invalid instruction: " + inst );
                }
            }

            internal override void ResolveValue( Token[] program, int index, Dictionary<string, byte> constants )
            {
                Value = Instruction.InstructionID;

                byte variant = 0;
                int i = index - 1;
                for ( int a = 0; a < ArgumentCount; ++a )
                {
                    if ( i < 0 )
                        break;

                    if ( program[ i ].IsLiteral )
                        variant |= (byte) ( 1 << a );

                    int s = 1;
                    while ( s-- > 0 && i >= 0 )
                        s += program[ i-- ].ArgumentCount;
                }

                Value += variant;
            }
        }

        internal static byte[][] Assemble( String asm )
        {
            String[] lines = asm.Split( '\n' );

            Dictionary<String, byte> constants = new Dictionary<string, byte>();

            Token[][] programs = new Token[ 256 ][];
            Dictionary<String, byte>[] labels = new Dictionary<string, byte>[ 256 ];
            byte progIndex = 0x00;
            List<Token> curProgram = new List<Token>();
            Dictionary<String, byte> curLabels = new Dictionary<string, byte>();

            for( int l = 0; l < lines.Length; ++l )
            {
                String line = lines[ l ].Trim();
                if ( line.Length > 0 )
                {
                    if ( line[ 0 ] == '.' )
                    {
                        String[] split = line.Split( new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries );
                        if ( split[ 0 ] == ".prg" )
                        {
                            if( split.Length < 2 )
                                throw new Exception( "Invalid program declaration: " + line );

                            programs[ progIndex ] = curProgram.ToArray();
                            labels[ progIndex ] = curLabels;

                            curProgram.Clear();
                            curLabels = new Dictionary<string, byte>();

                            progIndex = new LiteralToken( split[ 1 ] ).Value;

                            if ( split.Length > 2 )
                                constants.Add( split[ 2 ], progIndex );
                        }
                    }
                    else if ( line[ 0 ] == ':' )
                    {
                        String label = "";
                        for ( int i = 1; i < line.Length && !char.IsWhiteSpace( line[ i ] ); ++i )
                            label += line[ i ];

                        if ( label.Length > 0 )
                            curLabels.Add( label, (byte) curProgram.Count );
                    }
                    else
                    {
                        try
                        {
                            curProgram.AddRange( TokenizeLine( line ) );
                            if ( curProgram.Count > 256 )
                                throw new Exception( "Program is too long!" );
                        }
                        catch ( Exception e )
                        {
                            throw new Exception( e.GetType().Name + " at line " + ( l + 1 ) + "\n" + e.Message, e );
                        }
                    }
                }
            }

            programs[ progIndex ] = curProgram.ToArray();
            labels[ progIndex ] = curLabels;

            byte[][] bytes = new byte[ 256 ][];

            for( int p = 0; p < 256; ++ p )
            {
                Token[] program = programs[ p ];
                if ( program != null )
                {
                    foreach ( KeyValuePair<String, byte> constant in constants )
                        labels[ p ].Add( constant.Key, constant.Value );

                    bytes[ p ] = new byte[ 256 ];
                    for ( int t = 0; t < program.Length; ++t )
                    {
                        program[ t ].ResolveValue( program, t, labels[ p ] );
                        bytes[ p ][ t ] = program[ t ].Value;
                    }
                }
            }

            return bytes;
        }

        private static Token[] TokenizeLine( String line )
        {
            int index = line.IndexOf( ';' );
            if ( index != -1 )
                line = line.Substring( 0, index );

            String[] split = line.Split( new char[] { '\t', ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries );

            Token[] tokens = new Token[ split.Length ];

            for ( int i = 0; i < split.Length; ++i )
            {
                if ( Instruction.Exists( split[ i ] ) )
                    tokens[ split.Length - 1 - i ] = new InstructionToken( split[ i ] );
                else
                    tokens[ split.Length - 1 - i ] = new LiteralToken( split[ i ] );
            }

            return tokens;
        }
    }
}
