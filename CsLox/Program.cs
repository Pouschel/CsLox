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
		chunk.write(OP_CONSTANT, 123);
		chunk.write((byte) constant, 123);
		chunk.write(OP_RETURN, 123);
		chunk.disassemble("test");

		Console.ReadLine();
	}

}