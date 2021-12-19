global using Value = System.Double;

namespace CsLox;


class ValueArray : List<Value>
{
	public void write(Value value) => Add(value);

}
