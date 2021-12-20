//#define DEBUG_TRACE_EXECUTION
global using static CsLox.InterpretResult;
using System.Diagnostics;
using System.Globalization;

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
	Table globals;
	TextWriter tw;

	internal VM(Chunk chunk, TextWriter tw)
	{
		this.chunk = chunk;
		stack = new();
		globals = new();
		this.tw = tw;
	}

	private byte READ_BYTE() => chunk.code[ip++];
	private ushort READ_SHORT()
	{
		ip += 2;
		return (ushort)((chunk.code[ip - 2] << 8) | chunk.code[ip - 1]);
	}
	private Value READ_CONSTANT() => chunk.constants[READ_BYTE()];
	private ObjString READ_STRING() => AS_STRING(READ_CONSTANT());

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
			var instruction = (OpCode)READ_BYTE();
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
				case OP_JUMP:
					{
						ushort offset = READ_SHORT();
						ip += offset;
						break;
					}
				case OP_JUMP_IF_FALSE:
					{
						ushort offset = READ_SHORT();
						if (isFalsey(peek(0))) ip += offset;
						break;
					}
				case OP_RETURN: return INTERPRET_OK;
				case OP_PRINT:
					tw.WriteLine(pop());
					break;
				case OP_CONSTANT:
					Value constant = READ_CONSTANT();
					push(constant);
					break;
				case OP_NIL: push(NIL_VAL); break;
				case OP_TRUE: push(BOOL_VAL(true)); break;
				case OP_FALSE: push(BOOL_VAL(false)); break;
				case OP_POP: pop(); break;
				case OP_GET_LOCAL:
					{
						byte slot = READ_BYTE();
						push(stack[slot]);
						break;
					}
				case OP_SET_LOCAL:
					{
						byte slot = READ_BYTE();
						stack[slot] = peek(0);
						break;
					}
				case OP_GET_GLOBAL:
					{
						ObjString name = READ_STRING();
						Value value;
						if (!tableGet(globals, name, out value))
						{
							runtimeError($"Undefined variable '{ name.chars}'.");
							return INTERPRET_RUNTIME_ERROR;
						}
						push(value);
						break;
					}
				case OP_DEFINE_GLOBAL:
					{
						ObjString name = READ_STRING();
						tableSet(globals, name, peek(0));
						pop();
						break;
					}
				case OP_SET_GLOBAL:
					{
						ObjString name = READ_STRING();
						if (tableSet(globals, name, peek(0)))
						{
							tableDelete(globals, name);
							runtimeError($"Undefined variable '{name.chars}'.");
							return INTERPRET_RUNTIME_ERROR;
						}
						break;
					}
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
		var text = string.IsNullOrEmpty(chunk.FileName) ? msg : $"{chunk.FileName}({line}): {msg}";
		tw.WriteLine(text);
		Trace.WriteLine(text);
		resetStack();
	}

	void resetStack()
	{
		stack.Clear();
	}
}


