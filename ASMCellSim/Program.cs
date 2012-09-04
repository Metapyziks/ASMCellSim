using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASMCellSim
{
    internal class Program
    {
        internal static void Main( string[] args )
        {
            byte num = 0;
            --num;
            Console.WriteLine( "-1: " + num );
            Console.ReadKey();
        }
    }
}
