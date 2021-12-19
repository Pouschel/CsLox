global using static CsLox.TokenType;

namespace CsLox;

enum TokenType
{
	// Single-character tokens.
	TOKEN_LEFT_PAREN, TOKEN_RIGHT_PAREN,
	TOKEN_LEFT_BRACE, TOKEN_RIGHT_BRACE,
	TOKEN_COMMA, TOKEN_DOT, TOKEN_MINUS, TOKEN_PLUS,
	TOKEN_SEMICOLON, TOKEN_SLASH, TOKEN_STAR,
	// One or two character tokens.
	TOKEN_BANG, TOKEN_BANG_EQUAL,
	TOKEN_EQUAL, TOKEN_EQUAL_EQUAL,
	TOKEN_GREATER, TOKEN_GREATER_EQUAL,
	TOKEN_LESS, TOKEN_LESS_EQUAL,
	// Literals.
	TOKEN_IDENTIFIER, TOKEN_STRING, TOKEN_NUMBER,
	// Keywords.
	TOKEN_AND, TOKEN_CLASS, TOKEN_ELSE, TOKEN_FALSE,
	TOKEN_FOR, TOKEN_FUN, TOKEN_IF, TOKEN_NIL, TOKEN_OR,
	TOKEN_PRINT, TOKEN_RETURN, TOKEN_SUPER, TOKEN_THIS,
	TOKEN_TRUE, TOKEN_VAR, TOKEN_WHILE,

	TOKEN_ERROR, TOKEN_EOF
}


internal struct Token
{
	public TokenType type;
	public int start;
	public int end;
	public int line;
	public string source;

	public string StringValue => source[start..end];

}

internal class Scanner
{
	int line;
	int start;
	int current;
	private string source;

	public Scanner(string source)
	{
		this.source = source;
		this.line = 1;
	}

	public Token scanToken()
	{
		skipWhitespace();
		start = current;
		if (isAtEnd()) return makeToken(TOKEN_EOF);
		char c = advance();
		if (isDigit(c)) return number();
		switch (c)
		{
			case '(': return makeToken(TOKEN_LEFT_PAREN);
			case ')': return makeToken(TOKEN_RIGHT_PAREN);
			case '{': return makeToken(TOKEN_LEFT_BRACE);
			case '}': return makeToken(TOKEN_RIGHT_BRACE);
			case ';': return makeToken(TOKEN_SEMICOLON);
			case ',': return makeToken(TOKEN_COMMA);
			case '.': return makeToken(TOKEN_DOT);
			case '-': return makeToken(TOKEN_MINUS);
			case '+': return makeToken(TOKEN_PLUS);
			case '/': return makeToken(TOKEN_SLASH);
			case '*': return makeToken(TOKEN_STAR);
			case '!':
				return makeToken(
						match('=') ? TOKEN_BANG_EQUAL : TOKEN_BANG);
			case '=':
				return makeToken(
						match('=') ? TOKEN_EQUAL_EQUAL : TOKEN_EQUAL);
			case '<':
				return makeToken(
						match('=') ? TOKEN_LESS_EQUAL : TOKEN_LESS);
			case '>':
				return makeToken(
						match('=') ? TOKEN_GREATER_EQUAL : TOKEN_GREATER);
			case '"': return scanString();
		}

		return errorToken("Unexpected character.");
	}

	static bool isDigit(char c) => c >= '0' && c <= '9';

	Token number()
	{
		while (isDigit(peek())) advance();
		// Look for a fractional part.
		if (peek() == '.' && isDigit(peekNext()))
		{
			// Consume the ".".
			advance();
			while (isDigit(peek())) advance();
		}
		return makeToken(TOKEN_NUMBER);
	}

	Token scanString()
	{
		while (peek() != '"' && !isAtEnd())
		{
			if (peek() == '\n') line++;
			advance();
		}

		if (isAtEnd()) return errorToken("Unterminated string.");

		// The closing quote.
		advance();
		return makeToken(TOKEN_STRING);
	}


	void skipWhitespace()
	{
		while (true)
		{
			char c = peek();
			switch (c)
			{
				case ' ':
				case '\r':
				case '\t':
					advance();
					break;
				case '\n':
					line++;
					advance();
					break;
				case '/':
					if (peekNext() == '/')
					{
						// A comment goes until the end of the line.
						while (peek() != '\n' && !isAtEnd()) advance();
					}
					else
					{
						return;
					}
					break;
				default:
					if (char.IsWhiteSpace(c))
						continue;
					return;
			}
		}
	}

	char advance() => source[current++];
	char peek() => source[current];
	char peekNext() => current >= source.Length - 1 ? '\0' : source[current + 1];

	bool isAtEnd()
	{
		return current >= source.Length;
	}

	bool match(char expected)
	{
		if (isAtEnd()) return false;
		if (source[current] != expected) return false;
		current++;
		return true;
	}

	Token makeToken(TokenType type)
	{
		Token token = new();
		token.type = type;
		token.start = start;
		token.end = current;
		token.line = line;
		token.source = source;
		return token;
	}

	Token errorToken(string message)
	{
		Token token;
		token.type = TOKEN_ERROR;
		token.start = 0;
		token.end = message.Length;
		token.line = line;
		token.source = message;
		return token;
	}

}




