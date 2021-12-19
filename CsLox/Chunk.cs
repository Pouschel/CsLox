namespace CsLox;


enum OpCode : byte
{
	OP_RETURN
}

class Chunk
{
	byte[] code;
	int capacity;
	int count;

	public Chunk()
	{
		capacity = 8;
		code = new byte[capacity];
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
			default:
				tw.WriteLine($"Unknown opcode {instruction}");
				return offset + 1;
		}
	}
}
