using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ASMCellSim
{
    public class Program
    {
        public static void Main( string[] args )
        {
            if ( args.Length > 0 )
            {
                byte[][] code = Assembler.Assemble( File.ReadAllText( args[ 0 ] ) );

                for ( int i = 0; i < code.Length; ++i )
                    if ( code[ i ] != null )
                        File.WriteAllBytes( Path.GetFileNameWithoutExtension( args[ 0 ] ) + "." + i + ".cellprg", code[ i ] );

                World world = new World( 256.0f, true );

                Console.ReadKey();
            }
        }
    }
}
