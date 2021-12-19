
using System.Globalization;

namespace CsLox;


enum OpCode : byte
{
	OP_CONSTANT,
	OP_RETURN,
}


class Chunk
{
	byte[] code;
	int capacity;
	int count;
	ValueArray constants;
	List<int> lines;

	public Chunk()
	{
		capacity = 8;
		code = new byte[capacity];
		constants = new ValueArray();
		lines = new List<int>();
	}

	public void write(byte by)
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
	}

	public void write(OpCode oc) => write((byte)oc);

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

	int disassembleInstruction(int offset, TextWriter tw)
	{
		tw.Write($"{offset:0000} ");
		var instruction = (OpCode)code[offset];
		switch (instruction)
		{
			case OP_RETURN:
				tw.WriteLine(instruction);
				return offset + 1;
			case OP_CONSTANT:
				var constant = code[offset + 1];
				tw.WriteLine(string.Format( CultureInfo.InvariantCulture, "{0} {1}", instruction, constants[constant]));
				return offset + 2;
			default:
				tw.WriteLine($"Unknown opcode {instruction}");
				return offset + 1;
		}
	}
}
