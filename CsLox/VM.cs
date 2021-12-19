#define DEBUG_TRACE_EXECUTION
global using static CsLox.InterpretResult;
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
	List<Value> stack;
	
	int ip;

	internal VM(Chunk chunk)
	{
		this.chunk = chunk;
		stack = new();
	}

	private byte ReadByte() => chunk.code[ip++];

	private Value ReadConstant() => chunk.constants[ReadByte()];

	private void push(Value val) => stack.Add(val);

	private Value pop()
	{
		var ret = stack[stack.Count - 1];
		stack.RemoveAt(stack.Count - 1);
		return ret;
	}
	public InterpretResult interpret()
	{
		this.ip = 0;
		stack = new();
		while (true)
		{
#if DEBUG_TRACE_EXECUTION
			Console.Write("          ");
			foreach (var slot in stack)
			{
				Console.Write($"[{slot}]");
			}
			Console.WriteLine();
			chunk.disassembleInstruction(ip, Console.Out);
#endif
			var instruction = (OpCode)ReadByte();
			switch (instruction)
			{
				case OP_ADD: PopAndOp((a, b) => a + b); break;
				case OP_SUBTRACT: PopAndOp((a, b) => a - b);  break;
				case OP_MULTIPLY: PopAndOp((a, b) => a * b); break;
				case OP_DIVIDE: PopAndOp((a, b) => a / b); break;
				case OP_NEGATE: push(-pop()); break;
				case OP_RETURN: return InterpretResult.INTERPRET_OK;
				case OP_CONSTANT:
					Value constant = ReadConstant();
					push(constant);
					break;
			}
		}

		void PopAndOp(Func<Value,Value,Value> func)
		{
			var b = pop();
			var a = pop();
			push(func(a, b));
		}
	}
}

