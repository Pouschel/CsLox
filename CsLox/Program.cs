global using System;
global using System.Collections.Generic;
global using CsLox;
global using static CsLox.OpCode;


class Program
{

	static void Main()
	{

		Chunk chunk = new Chunk();
		var vm = new VM(chunk);
		chunk.writeConstant(1.2);
		chunk.writeConstant(3.4);
		chunk.write(OP_ADD);
		chunk.writeConstant(5.6);
		chunk.write(OP_DIVIDE);
		chunk.write(OP_NEGATE);
		chunk.write(OP_RETURN, 123);
		chunk.disassemble("test");
		Console.WriteLine("--- Running");
		vm.interpret();

		Console.ReadLine();
	}

}