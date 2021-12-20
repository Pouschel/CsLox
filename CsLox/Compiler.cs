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

class ParseRule
{
	public Action? prefix;
	public Action? infix;
	public Precedence precedence;
}


internal class Compiler
{
	Scanner scanner;
	Chunk rootChunk;
	Chunk compilingChunk;
	public Chunk CompiledChunk => rootChunk;
	Parser parser;
	string fileName;
	ParseRule[] rules;
	public bool DEBUG_PRINT_CODE { get; set; }

	void InitTable()
	{
		SetRule(TOKEN_LEFT_PAREN, grouping, null);
		SetRule(TOKEN_RIGHT_PAREN, null, null, PREC_NONE);
		SetRule(TOKEN_LEFT_BRACE, null, null, PREC_NONE);
		SetRule(TOKEN_RIGHT_BRACE, null, null, PREC_NONE);
		SetRule(TOKEN_COMMA, null, null, PREC_NONE);
		SetRule(TOKEN_DOT, null, null, PREC_NONE);
		SetRule(TOKEN_MINUS, unary, binary, PREC_TERM);
		SetRule(TOKEN_PLUS, null, binary, PREC_TERM);
		SetRule(TOKEN_SEMICOLON, null, null, PREC_NONE);
		SetRule(TOKEN_SLASH, null, binary, PREC_FACTOR);
		SetRule(TOKEN_STAR, null, binary, PREC_FACTOR);
		SetRule(TOKEN_BANG, null, null, PREC_NONE);
		SetRule(TOKEN_BANG_EQUAL, null, null, PREC_NONE);
		SetRule(TOKEN_EQUAL, null, null, PREC_NONE);
		SetRule(TOKEN_EQUAL_EQUAL, null, null, PREC_NONE);
		SetRule(TOKEN_GREATER, null, null, PREC_NONE);
		SetRule(TOKEN_GREATER_EQUAL, null, null, PREC_NONE);
		SetRule(TOKEN_LESS, null, null, PREC_NONE);
		SetRule(TOKEN_LESS_EQUAL, null, null, PREC_NONE);
		SetRule(TOKEN_IDENTIFIER, null, null, PREC_NONE);
		SetRule(TOKEN_STRING, null, null, PREC_NONE);
		SetRule(TOKEN_NUMBER, number, null, PREC_NONE);
		SetRule(TOKEN_AND, null, null, PREC_NONE);
		SetRule(TOKEN_CLASS, null, null, PREC_NONE);
		SetRule(TOKEN_ELSE, null, null, PREC_NONE);
		SetRule(TOKEN_FALSE, null, null, PREC_NONE);
		SetRule(TOKEN_FOR, null, null, PREC_NONE);
		SetRule(TOKEN_FUN, null, null, PREC_NONE);
		SetRule(TOKEN_IF, null, null, PREC_NONE);
		SetRule(TOKEN_NIL, null, null, PREC_NONE);
		SetRule(TOKEN_OR, null, null, PREC_NONE);
		SetRule(TOKEN_PRINT, null, null, PREC_NONE);
		SetRule(TOKEN_RETURN, null, null, PREC_NONE);
		SetRule(TOKEN_SUPER, null, null, PREC_NONE);
		SetRule(TOKEN_THIS, null, null, PREC_NONE);
		SetRule(TOKEN_TRUE, null, null, PREC_NONE);
		SetRule(TOKEN_VAR, null, null, PREC_NONE);
		SetRule(TOKEN_WHILE, null, null, PREC_NONE);
		SetRule(TOKEN_ERROR, null, null, PREC_NONE);
		SetRule(TOKEN_EOF, null, null, PREC_NONE);

		void SetRule(TokenType tt, Action? prefix, Action? infix, Precedence prec = PREC_NONE)
		{
			var rule = new ParseRule
			{
				prefix = prefix,
				infix = infix,
				precedence = prec
			};
			rules[(int)tt] = rule;
		}
	}

	public Compiler(string source, string fileName = "")
	{
		rules = new ParseRule[(int)TOKEN_EOF + 1];
		InitTable();
		this.fileName = fileName;
		scanner = new Scanner(source);
		compilingChunk = rootChunk = new Chunk();
		compilingChunk.FileName = fileName;
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
		advance();
		var prefixRule = getRule(parser.previous.type).prefix;
		if (prefixRule == null)
		{
			error("Expect expression.");
			return;
		}
		prefixRule();
		while (precedence <= getRule(parser.current.type).precedence)
		{
			advance();
			var infixRule = getRule(parser.previous.type).infix;
			infixRule!();
		}
	}

	ParseRule getRule(TokenType type) => rules[(int)type];

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
		emitConstant(NUMBER_VAL(value));
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

	void binary()
	{
		TokenType operatorType = parser.previous.type;
		ParseRule rule = getRule(operatorType);
		parsePrecedence(rule.precedence + 1);

		switch (operatorType)
		{
			case TOKEN_PLUS: emitByte(OP_ADD); break;
			case TOKEN_MINUS: emitByte(OP_SUBTRACT); break;
			case TOKEN_STAR: emitByte(OP_MULTIPLY); break;
			case TOKEN_SLASH: emitByte(OP_DIVIDE); break;
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

	void endCompiler()
	{
		emitReturn();
		if (DEBUG_PRINT_CODE)
		{
			if (!parser.hadError)
			{
				currentChunk().disassemble("code");
			}
		}
	}

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

