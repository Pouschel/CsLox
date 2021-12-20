﻿
using System.Globalization;

namespace CsLox;


enum OpCode : byte
{
	OP_CONSTANT,
	OP_NIL,
	OP_TRUE,
	OP_FALSE,
	OP_EQUAL,
	OP_GREATER,
	OP_LESS,
	OP_ADD,
	OP_SUBTRACT,
	OP_MULTIPLY,
	OP_DIVIDE,
	OP_NOT,
	OP_NEGATE,
	OP_RETURN,
}

class Chunk
{
	internal byte[] code;
	int capacity;
	int count;
	internal ValueArray constants;
	internal List<int> lines;

	public string FileName="";

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
		if (offset > 0 && lines[offset] == lines[offset - 1])
			tw.Write("   | ");
		else
			tw.Write("{0,4} ", lines[offset]);

		var instruction = (OpCode)code[offset];
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
			case OP_EQUAL:
			case OP_GREATER:
			case OP_LESS:
				tw.WriteLine(instruction);
				return offset + 1;
			case OP_CONSTANT:
				var constant = code[offset + 1];
				tw.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0} {1,4} '{2}'", 
					instruction, constant, constants[constant]));
				return offset + 2;
			default:
				tw.WriteLine($"Unknown opcode {instruction}");
				return offset + 1;
		}
	}
}
