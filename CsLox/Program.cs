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
		//DumpTokens(source);
		InterpretResult result = interpret(source, path, Console.Out);
		if (result == INTERPRET_COMPILE_ERROR) Environment.Exit(65);
		if (result == INTERPRET_RUNTIME_ERROR) Environment.Exit(70);
	}


	static void DumpTokens(string source, TextWriter tw)
	{
		new Compiler(source,"",tw).DumpTokens();
	}
}

public class Globals
{

	internal static bool identifiersEqual(in Token a, in Token b) 
		=> a.StringValue == b.StringValue;

	internal static InterpretResult interpret(string source, string fileName, TextWriter tw
		, bool debugPrintCode = false)
	{
		var compiler = new Compiler(source, fileName, tw)
		{
			DEBUG_PRINT_CODE = debugPrintCode
		};
		if (!compiler.compile())
			return INTERPRET_COMPILE_ERROR;
		VM vm = new VM(compiler.CompiledChunk, tw);
		return vm.interpret();
	}

	public static bool RunFile(string path, TextWriter tw)
	{
		string source = File.ReadAllText(path);
		InterpretResult result = interpret(source, path, tw);
		return result != INTERPRET_OK;
	}

	public static bool RunCode(string source, TextWriter tw)
	{
		InterpretResult result = interpret(source, "", tw);
		return result != INTERPRET_OK;
	}


}