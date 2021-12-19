using static CsLox.Precedence;
namespace CsLox;

enum Precedence
{
	PREC_NONE,
	PREC_ASSIGNMENT,  // =
	PREC_OR,          // or
	PREC_AND,         // and
	PREC_EQUALITY,    // == !=
	PREC_COMPARISON,  // < > <= >=
	PREC_TERM,        // + -
	PREC_FACTOR,      // * /
	PREC_UNARY,       // ! -
	PREC_CALL,        // . ()
	PREC_PRIMARY
}


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
	Chunk rootChunk;
	Chunk compilingChunk;
	public Chunk CompiledChunk => rootChunk;
	Parser parser;
	string fileName;

	public Compiler(string source, string fileName = "")
	{
		this.fileName = fileName;
		scanner = new Scanner(source);
		compilingChunk = rootChunk = new Chunk();
		parser = new Parser();
	}


	public bool compile()
	{
		scanner.Reset();
		advance();
		expression();
		consume(TOKEN_EOF, "Expect end of expression.");
		endCompiler();
		return !parser.hadError;
	}

	void expression()
	{
		parsePrecedence(PREC_ASSIGNMENT);
	}
	void parsePrecedence(Precedence precedence)
	{
		// What goes here?
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

	void number()
	{
		double value = double.Parse(parser.previous.StringValue);
		emitConstant(value);
	}

	void grouping()
	{
		expression();
		consume(TOKEN_RIGHT_PAREN, "Expect ')' after expression.");
	}

	void unary()
	{
		TokenType operatorType = parser.previous.type;

		// Compile the operand.
		parsePrecedence(PREC_UNARY);

		// Emit the operator instruction.
		switch (operatorType)
		{
			case TOKEN_MINUS: emitByte(OP_NEGATE); break;
			default: return; // Unreachable.
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

	void error(string message) => errorAt(parser.previous, message);
	void errorAtCurrent(string message) => errorAt(parser.current, message);

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

	Chunk currentChunk() => compilingChunk;

	void endCompiler() => emitReturn();
	void emitReturn() => emitByte(OP_RETURN);

	void emitByte(byte by) => currentChunk().write(by, parser.previous.line);
	void emitByte(OpCode op) => currentChunk().write(op, parser.previous.line);

	void emitBytes(OpCode byte1, byte byte2)
	{
		emitByte(byte1);
		emitByte(byte2);
	}

	void emitConstant(Value value)
	{
		emitBytes(OP_CONSTANT, makeConstant(value));
	}

	byte makeConstant(Value value)
	{
		int constant = currentChunk().addConstant(value);
		if (constant > byte.MaxValue)
		{
			error("Too many constants in one chunk.");
			return 0;
		}
		return (byte)constant;
	}
}

