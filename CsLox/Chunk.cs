namespace CsLox;


enum OpCode : byte
{
	OP_RETURN
}

class Chunk : List<byte>
{
	public void write(OpCode oc)
	{
		this.Add((byte)oc);
	}

	public void disassemble(string name, TextWriter? tw = null)
	{
		tw ??= Console.Out;
		tw.WriteLine($"== {name} ==");

		for (int offset = 0; offset < Count;)
		{
			offset = disassembleInstruction(offset, tw);
		}
	}

	int disassembleInstruction(int offset, TextWriter tw)
	{
		tw.Write($"{offset:0000} ");
		var instruction = (OpCode)this[offset];
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
