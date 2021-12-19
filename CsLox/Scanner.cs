using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsLox
{
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
	}
}
