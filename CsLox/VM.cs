//#define DEBUG_TRACE_EXECUTION
global using static CsLox.InterpretResult;
using System.Diagnostics;

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
	Value peek(int distance)
	{
		return stack[stack.Count - 1 - distance];
	}

	public InterpretResult interpret()
	{
		this.ip = 0;
		stack = new();
		InterpretResult result = INTERPRET_OK;
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
				case OP_NOT:
					push(BOOL_VAL(isFalsey(pop())));
					break;
				case OP_NEGATE:
					if (!IS_NUMBER(peek(0)))
					{
						runtimeError("Operand must be a number.");
						return INTERPRET_RUNTIME_ERROR;
					}
					push(NUMBER_VAL(-AS_NUMBER(pop())));
					break;
				case OP_RETURN: return INTERPRET_OK;
				case OP_PRINT:
						Console.WriteLine(pop());
						break;
				case OP_CONSTANT:
					Value constant = ReadConstant();
					push(constant);
					break;
				case OP_NIL: push(NIL_VAL); break;
				case OP_TRUE: push(BOOL_VAL(true)); break;
				case OP_FALSE: push(BOOL_VAL(false)); break;
				case OP_EQUAL:
					{
						Value b = pop();
						Value a = pop();
						push(BOOL_VAL(valuesEqual(a, b)));
						break;
					}
				case OP_GREATER: result = PopAndOp((a, b) => BOOL_VAL(a > b)); break;
				case OP_LESS: result = PopAndOp((a, b) => BOOL_VAL(a < b)); break;
				case OP_ADD:
					{
						if (IS_STRING(peek(0)) && IS_STRING(peek(1)))
						{
							concatenate();
						}
						else if (IS_NUMBER(peek(0)) && IS_NUMBER(peek(1)))
						{
							double b = AS_NUMBER(pop());
							double a = AS_NUMBER(pop());
							push(NUMBER_VAL(a + b));
						}
						else
						{
							runtimeError("Operands must be two numbers or two strings.");
							return INTERPRET_RUNTIME_ERROR;
						}
						break;
					}
				case OP_SUBTRACT: result = PopAndOp((a, b) => NUMBER_VAL(a - b)); break;
				case OP_MULTIPLY: result = PopAndOp((a, b) => NUMBER_VAL(a * b)); break;
				case OP_DIVIDE: result = PopAndOp((a, b) => NUMBER_VAL(a / b)); break;
			}
			if (result != INTERPRET_OK) return result;
		}

		void concatenate()
		{
			ObjString b = AS_STRING(pop());
			ObjString a = AS_STRING(pop());
			var result = new ObjString(a.chars + b.chars);
			push(OBJ_VAL(result));
		}

		InterpretResult PopAndOp(Func<double, double, Value> func)
		{
			if (!IS_NUMBER(peek(0)) || !IS_NUMBER(peek(1)))
			{
				runtimeError("Operands must be numbers.");
				return INTERPRET_RUNTIME_ERROR;
			}
			double b = AS_NUMBER(pop());
			double a = AS_NUMBER(pop());
			push(func(a, b));
			return INTERPRET_OK;
		}

		void runtimeError(string msg)
		{
			int instruction = ip - 1;
			int line = chunk.lines[instruction];
			var text = $"{chunk.FileName}({line}): {msg}";
			Console.WriteLine(text);
			Trace.WriteLine(text);
			resetStack();
		}

		void resetStack()
		{
			stack.Clear();
		}
	}
}

