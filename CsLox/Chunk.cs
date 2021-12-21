using System.Globalization;
namespace CsLox;

class Chunk
{
	internal byte[] code;
	int capacity;
	public int count;
	internal ValueArray constants;
	internal List<int> lines;

	public string FileName = "";

	public Chunk()
	{
		capacity = 8;
		code = new byte[capacity];
		constants = new ValueArray();
		lines = new();
	}

	public void write(byte by, int line = 0)
	{
		if (capacity < count + 1)
		{
			int oldCapacity = capacity;
			capacity *= 2;
			var oldCode = code;
			code = new byte[capacity];
			Array.Copy(oldCode, code, oldCapacity);
		}
		code[count++] = by;
		lines.Add(line);
	}

	public void write(OpCode oc, int line = 0) => write((byte)oc, line);

	public void writeConstant(Value val)
	{
		write(OP_CONSTANT);
		var cons = addConstant(val);
		write((byte)cons);
	}

	public int addConstant(Value value)
	{
		constants.write(value);
		return constants.Count - 1;
	}

	public void disassemble(string name, TextWriter? tw = null)
	{
		tw ??= Console.Out;
		tw.WriteLine($"== {name} ==");

		for (int offset = 0; offset < count;)
		{
			offset = disassembleInstruction(offset, tw);
		}
	}
	internal int disassembleInstruction(int offset, TextWriter tw)
	{
		tw.Write($"{offset:0000} ");
		if (offset>=lines.Count || offset > 0 && lines[offset] == lines[offset - 1])
			tw.Write("   | ");
		else
			tw.Write("{0,4} ", lines[offset]);

		var instruction = (OpCode)code[offset];
		var instructionString = $"{instruction.ToString()[3..],-16}";
		switch (instruction)
		{
			case OP_RETURN:
			case OP_NEGATE:
			case OP_ADD:
			case OP_SUBTRACT:
			case OP_MULTIPLY:
			case OP_DIVIDE:
			case OP_NIL:
			case OP_TRUE:
			case OP_FALSE:
			case OP_NOT:
			case OP_POP:
			case OP_EQUAL:
			case OP_GREATER:
			case OP_LESS:
			case OP_PRINT:
				tw.WriteLine(instructionString);
				return offset + 1;
			case OP_CONSTANT:
			case OP_DEFINE_GLOBAL:
			case OP_GET_GLOBAL:
			case OP_SET_GLOBAL:
				{
					var constant = code[offset + 1];
					tw.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0} {1,4} '{2}'",
						instructionString, constant, constants[constant]));
					return offset + 2;
				}
			case OP_GET_LOCAL:
			case OP_SET_LOCAL:
			case OP_CALL:
				var slot = code[offset + 1];
				tw.WriteLine($"{instructionString} {slot}");
				return offset + 2;
			case OP_CLOSURE:
				{
					offset++;
					var constant = code[offset++];
					tw.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0} {1,4} '{2}'",
						instructionString, constant, constants[constant]));
					return offset;
				}
			case OP_JUMP:
			case OP_JUMP_IF_FALSE:
				return jumpInstruction(1);
			case OP_LOOP:return jumpInstruction(-1);
			default:
				tw.WriteLine($"Unknown opcode {instruction}");
				return offset + 1;
		}

		int jumpInstruction(int sign)
		{
			ushort jump = (ushort)(code[offset + 1] << 8);
			jump |= code[offset + 2];
			tw.WriteLine($"{instructionString} {offset} -> {offset + 3 + sign * jump}");
			return offset + 3;
		}

	}
}
