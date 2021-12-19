namespace CsLox;

internal class Compiler
{
	Scanner scanner;

	public Compiler(string source)
	{
		scanner = new Scanner(source);
	}


	public void compile()
	{
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

      if (token.type == TOKEN_EOF || token.type==TOKEN_ERROR) break;
    }
  }
}

