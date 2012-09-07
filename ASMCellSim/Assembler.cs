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

            internal abstract void ResolveValue( List<Token> program, ref int index, Dictionary<String, byte> constants );
        }

        private class LiteralToken : Token
        {
            private String myConstName;

            internal override bool IsLiteral { get { return true; } }

            internal LiteralToken( byte value )
            {
                Value = value;
            }

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

            internal override void ResolveValue( List<Token> program, ref int index, Dictionary<string, byte> constants )
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

            public override string ToString()
            {
                if ( myConstName != null )
                    return myConstName;
                else
                    return "0x" + Value.ToString( "X2" );
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

            internal override void ResolveValue( List<Token> program, ref int index, Dictionary<string, byte> constants )
            {
                Value = Instruction.InstructionID;

                byte variant = 0;
                int i = index - 1;
                int lits = 0;
                for ( int a = 0; a < ArgumentCount; ++a )
                {
                    if ( i < 0 )
                        break;

                    if ( program[ i ].IsLiteral )
                    {
                        variant |= (byte) ( 1 << a );
                        Token tok = program[ i ];
                        program.RemoveAt( i-- );
                        program.Insert( index, tok );
                        tok.ResolveValue( program, ref index, constants );
                        ++lits;
                    }
                    else
                    {
                        int s = 1;
                        while ( s-- > 0 && i >= 0 )
                            s += program[ i-- ].ArgumentCount;
                    }
                }

                index -= lits;
                Value += variant;
            }

            public override string ToString()
            {
                return Instruction.Mnemonic;
            }
        }

        internal static byte[][] Assemble( String asm )
        {
            String[] lines = asm.Split( '\n' );

            Dictionary<String, byte> constants = new Dictionary<string, byte>();

            List<Token>[] programs = new List<Token>[ 256 ];
            Dictionary<String, byte>[] labels = new Dictionary<string, byte>[ 256 ];
            byte progIndex = 0x00;
            List<Token> curProgram = null;
            Dictionary<String, byte> curLabels = null;

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

                            programs[ progIndex ] = curProgram;
                            labels[ progIndex ] = curLabels;

                            curProgram = new List<Token>();
                            curLabels = new Dictionary<string, byte>();

                            progIndex = new LiteralToken( split[ 1 ] ).Value;

                            if ( split.Length > 2 )
                                constants.Add( split[ 2 ], progIndex );
                        }
                        else if ( split[ 0 ] == ".dat" )
                        {
                            for ( int i = 1; i < split.Length; ++i )
                                curProgram.Add( new LiteralToken( split[ i ] ) );
                        }
                        else if ( split[ 0 ] == ".org" )
                        {
                            int index = new LiteralToken( split[ 1 ] ).Value;

                            while ( curProgram.Count < index )
                                curProgram.Add( new LiteralToken( 0x00 ) );
                        }
                        else if ( split[ 0 ] == ".str" )
                        {
                            int start = line.IndexOf( '"' ) + 1;
                            int end = line.IndexOf( '"', start );

                            String str = line.Substring( start, end - start );
                            foreach ( char ch in str )
                                curProgram.Add( new LiteralToken( (byte) ch ) );

                            curProgram.Add( new LiteralToken( (byte) 0x00 ) );
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

            programs[ progIndex ] = curProgram;
            labels[ progIndex ] = curLabels;

            byte[][] bytes = new byte[ 256 ][];

            for( int p = 0; p < 256; ++ p )
            {
                List<Token> program = programs[ p ];
                if ( program != null )
                {
                    foreach ( KeyValuePair<String, byte> constant in constants )
                        labels[ p ].Add( constant.Key, constant.Value );

                    for ( int t = program.Count - 1; t >= 0; --t )
                        program[ t ].ResolveValue( program, ref t, labels[ p ] );

                    bytes[ p ] = new byte[ 256 ];
                    for ( int t = 0; t < program.Count; ++t )
                        bytes[ p ][ t ] = program[ t ].Value;
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
