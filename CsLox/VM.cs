#define DEBUG_TRACE_EXECUTION

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsLox;

public enum InterpretResult
{
	INTERPRET_OK,
	INTERPRET_COMPILE_ERROR,
	INTERPRET_RUNTIME_ERROR
}


public class VM
{
	Chunk chunk;
	int ip;

	internal VM(Chunk chunk)
	{
		this.chunk = chunk;
	}

	private byte ReadByte() => chunk.code[ip++];

	private Value ReadConstant() => chunk.constants[ReadByte()];

	public InterpretResult interpret()
	{
		this.ip = 0;
		while (true)
		{
#if DEBUG_TRACE_EXECUTION
			chunk.disassembleInstruction(ip, Console.Out);
#endif
			var instruction = (OpCode)ReadByte();
			switch (instruction)
			{
				case OP_RETURN: return InterpretResult.INTERPRET_OK;
				case OP_CONSTANT:
					Value constant = ReadConstant();
					Console.WriteLine(constant);
					break;
			}
		}
	}
}

