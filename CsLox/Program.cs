global using System;
global using System.Collections.Generic;
global using CsLox;
global using static CsLox.OpCode;


class Program
{

	static void Main(string[] args)
	{
		Console.WriteLine("CsLox v1");
		if (args.Length > 0)
			runFile(args[0]);

		Console.ReadLine();

	}

	static void runFile(string path)
	{

		string source = File.ReadAllText(path);
		InterpretResult result = interpret(source);
		if (result == INTERPRET_COMPILE_ERROR) Environment.Exit(65);
		if (result == INTERPRET_RUNTIME_ERROR) Environment.Exit(70);
	}

	static InterpretResult interpret(string source)
	{
		new Compiler(source).compile();
		return INTERPRET_OK;
	}

}