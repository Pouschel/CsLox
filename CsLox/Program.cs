
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Text;
global using System.Threading.Tasks;
global using CsLox;
global using static CsLox.OpCode;
global using Value = System.Double;

using System.Runtime.InteropServices;

class Program
{

	static void Main()
	{
		Chunk chunk = new Chunk();
		chunk.write(OP_RETURN);
		chunk.disassemble("test");

		Console.ReadLine();
	}

}