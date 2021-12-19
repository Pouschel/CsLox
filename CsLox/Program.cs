global using System;
global using System.Collections.Generic;
global using CsLox;
global using static CsLox.OpCode;
global using static Globals;
using System.Globalization;

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
		DumpTokens(source);
		InterpretResult result = interpret(source, path);
		if (result == INTERPRET_COMPILE_ERROR) Environment.Exit(65);
		if (result == INTERPRET_RUNTIME_ERROR) Environment.Exit(70);
	}

	static InterpretResult interpret(string source, string fileName)
	{
		var compiler = new Compiler(source, fileName);
		if (!compiler.compile())
			return INTERPRET_COMPILE_ERROR;
		VM vm = new VM(compiler.CompiledChunk);
		return vm.interpret();
	}

	static void DumpTokens(string source)
	{
		new Compiler(source).DumpTokens();
	}
}

class Globals
{

	public static void printf(string fmt, params object[] args) =>
		Console.Write(string.Format(CultureInfo.InvariantCulture, fmt, args));
}