//#define DEBUG_TRACE_EXECUTION
global using static CsLox.InterpretResult;
using System.Diagnostics;
using static CsLox.NativeFunctions;
namespace CsLox;

public enum InterpretResult
{
	INTERPRET_OK,
	INTERPRET_COMPILE_ERROR,
	INTERPRET_RUNTIME_ERROR
}

class CallFrame
{
	public ObjClosure? closure;
	public int ip;
	public int slotIndex;
};


public class VM
{
	public int FRAMES_MAX = 1000;
	public bool DumpStackOnError = true;

	List<Value> stack;
	int stackTop;
	List<CallFrame> frames;
	int frameCount;
	Table globals;
	TextWriter tw;

	internal VM(TextWriter tw)
	{
		stack = new(); stackTop = 0;
		globals = new();
		frames = new();
		this.tw = tw;
		defineNative("clock", clock);
	}

	private void push(Value val)
	{
		if (stackTop == stack.Count)
			stack.Add(val);
		else
			stack[stackTop] = val;
		stackTop++;
	}

	private Value pop()
	{
		var ret = stack[--stackTop];
		return ret;
	}
	Value peek(int distance)
	{
		return stack[stackTop - 1 - distance];
	}

	CallFrame CreateFrame(ObjClosure closure, int argCount)
	{
		CallFrame? frame = null;
		if (frameCount < frames.Count)
			frame = frames[frameCount];
		if (frame == null)
			frame = new CallFrame();
		if (frameCount++ >= frames.Count)
			frames.Add(frame);
		frame.closure = closure;
		frame.ip = 0;
		frame.slotIndex = stackTop - argCount;
		return frame;
	}
	internal InterpretResult interpret(ObjFunction function)
	{
		var closure = new ObjClosure(function);
		push(OBJ_VAL(closure));
		call(closure, 0);
		return run();
	}

	public InterpretResult run()
	{

		CallFrame frame = frames[frameCount - 1];
		Chunk chunk() => frame.closure!.function!.chunk;

		byte READ_BYTE() => chunk().code[frame.ip++];
		ushort READ_SHORT()
		{
			frame.ip += 2;
			return (ushort)((chunk().code[frame.ip - 2] << 8) | chunk().code[frame.ip - 1]);
		}
		Value READ_CONSTANT() => chunk().constants[READ_BYTE()];
		ObjString READ_STRING() => AS_STRING(READ_CONSTANT());

		InterpretResult iresult = INTERPRET_OK;
		while (true)
		{
#if DEBUG_TRACE_EXECUTION
			Console.Write("          ");
			for (int i = 0; i < stackTop; i++)
			{
				var slot = stack[i];
				if (i == frame.slotIndex)
					Console.Write(" | ");
				Console.Write($"[{slot}]");
			}
			Console.WriteLine();
			frame.closure!.function.chunk.disassembleInstruction(frame.ip, Console.Out);
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
						frame.ip += offset;
						break;
					}
				case OP_JUMP_IF_FALSE:
					{
						ushort offset = READ_SHORT();
						if (isFalsey(peek(0))) frame.ip += offset;
						break;
					}
				case OP_LOOP:
					{
						ushort offset = READ_SHORT();
						frame.ip -= offset;
						break;
					}

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
						push(stack[frame.slotIndex + slot]);
						break;
					}
				case OP_SET_LOCAL:
					{
						byte slot = READ_BYTE();
						stack[frame.slotIndex + slot] = peek(0);
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
				case OP_GREATER: iresult = PopAndOp((a, b) => BOOL_VAL(a > b)); break;
				case OP_LESS: iresult = PopAndOp((a, b) => BOOL_VAL(a < b)); break;
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
				case OP_SUBTRACT: iresult = PopAndOp((a, b) => NUMBER_VAL(a - b)); break;
				case OP_MULTIPLY: iresult = PopAndOp((a, b) => NUMBER_VAL(a * b)); break;
				case OP_DIVIDE: iresult = PopAndOp((a, b) => NUMBER_VAL(a / b)); break;
				case OP_CALL:
					{
						int argCount = READ_BYTE();
						if (!callValue(peek(argCount), argCount))
						{
							return INTERPRET_RUNTIME_ERROR;
						}
						frame = frames[frameCount - 1];
						break;
					}
				case OP_CLOSURE:
					{
						ObjFunction function = AS_FUNCTION(READ_CONSTANT());
						ObjClosure closure = new ObjClosure(function);
						push(OBJ_VAL(closure));
						break;
					}
				case OP_RETURN:
					{
						Value result = pop();
						frameCount--;
						if (frameCount == 0)
						{
							pop();
							return INTERPRET_OK;
						}
						stackTop = frame.slotIndex;
						push(result);
						frame = frames[frameCount - 1];
						break;
					}
			}
			if (iresult != INTERPRET_OK) return iresult;
		}

	}

	bool callValue(Value callee, int argCount)
	{
		if (IS_OBJ(callee))
		{
			switch (OBJ_TYPE(callee))
			{
				case OBJ_CLOSURE:
					return call(AS_CLOSURE(callee), argCount);
				case OBJ_NATIVE:
					{
						NativeFn native = AS_NATIVE(callee);
						Value[] args = new Value[argCount];
						for (int i = 0; i < argCount; i++)
						{
							args[i] = stack[stackTop - argCount + i];
						}
						Value result = native(args);
						stackTop -= argCount + 1;
						push(result);
						return true;
					}
				default:
					break; // Non-callable object type.
			}
		}
		runtimeError("Can only call functions and classes.");
		return false;
	}
	void defineNative(string name, NativeFn function)
	{
		var oname = new ObjString(name);
		var ofun = OBJ_VAL(new ObjNative(function));
		tableSet(globals, oname, ofun);
	}
	bool call(ObjClosure closure, int argCount)
	{
		var function = closure.function;
		if (argCount != function.arity)
		{
			runtimeError($"Expected {function.arity} arguments but got {argCount}.");
			return false;
		}
		if (frameCount >= FRAMES_MAX)
		{
			runtimeError("Stack overflow.");
			return false;
		}
		CreateFrame(closure, argCount + 1);
		return true;
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
		CallFrame frame = frames[frameCount - 1];
		var chunk = frame.closure!.function.chunk;
		int instruction = frame.ip - 1;
		int line = chunk.lines[instruction];
		var text = string.IsNullOrEmpty(chunk.FileName) ? msg : $"{chunk.FileName}({line}): {msg}";
		tw.WriteLine(text);
		Trace.WriteLine(text);
		if (DumpStackOnError) dumpStack();
		resetStack();
	}

	void dumpStack()
	{
		for (int i = frameCount - 1; i >= 0; i--)
		{
			CallFrame frame = frames[i];
			ObjFunction function = frame.closure!.function;
			int instruction = frame.ip - 1;
			tw.WriteLine($"[line {function.chunk.lines[instruction]}] in {function.NameOrScript}");
		}
	}

	void resetStack()
	{
		stack.Clear();
	}
}


