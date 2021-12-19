namespace CsLox;


struct Parser
{
	public Token current;
	public Token previous;
	public bool hadError;
	public bool panicMode;
}

internal class Compiler
{
	Scanner scanner;
	Chunk chunk;
	public Chunk CompiledChunk => chunk;
	Parser parser;
	string fileName;

	public Compiler(string source, string fileName = "")
	{
		this.fileName = fileName;
		scanner = new Scanner(source);
		chunk = new Chunk();
		parser = new Parser();
	}


	public bool compile()
	{
		scanner.Reset();
		advance();
		//expression();
		consume(TOKEN_EOF, "Expect end of expression.");
		return !parser.hadError;
	}

	void advance()
	{
		parser.previous = parser.current;
		for (; ; )
		{
			parser.current = scanner.scanToken();
			if (parser.current.type != TOKEN_ERROR) break;

			errorAtCurrent(parser.current.StringValue);
		}
	}

	void consume(TokenType type, string message)
	{
		if (parser.current.type == type)
		{
			advance();
			return;
		}
		errorAtCurrent(message);
	}

	void errorAtCurrent(string message)
	{
		errorAt(parser.current, message);
	}

	void errorAt(in Token token, string message)
	{
		if (parser.panicMode) return;
		parser.panicMode = true;
		var msg = $"{fileName}({token.line}): {message}";
		Console.WriteLine(msg);
		System.Diagnostics.Trace.WriteLine(msg);
		parser.hadError = true;
	}

	public void DumpTokens()
	{
		scanner.Reset();
		int line = -1;
		for (; ; )
		{
			Token token = scanner.scanToken();
			if (token.line != line)
			{
				printf("{0,4} ", token.line);
				line = token.line;
			}
			else
			{
				printf("   | ");
			}
			printf("{0,2} '{1}'\n", token.type, token.StringValue);

			if (token.type == TOKEN_EOF || token.type == TOKEN_ERROR) break;
		}
	}
}

