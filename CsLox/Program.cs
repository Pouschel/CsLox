global using System;
global using System.Collections.Generic;
global using CsLox;
global using static CsLox.OpCode;


class Program
{

	static void Main()
	{
		Chunk chunk = new Chunk();
		int constant = chunk.addConstant(1.2);
		chunk.write(OP_CONSTANT);
		chunk.write((byte) constant);
		chunk.write(OP_RETURN);
		chunk.disassemble("test");

		Console.ReadLine();
	}

}