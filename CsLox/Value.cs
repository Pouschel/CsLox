global using static CsLox.ValueType;
global using static CsLox.ValueStatics;

namespace CsLox;

enum ValueType
{
	VAL_BOOL,
	VAL_NIL,
	VAL_NUMBER,
}


struct Value
{
	public ValueType type;
	public double dValue;
	public object? oValue;

	public Value(ValueType type, double dValue, object? oValue = null)
	{
		this.type = type;
		this.dValue = dValue;
		this.oValue = oValue;
	}

	public override string ToString()
	{
		switch (type)
		{
			case ValueType.VAL_NIL: return "nil";
			case ValueType.VAL_BOOL: return dValue != 0 ? "true" : "false";
			case ValueType.VAL_NUMBER: return dValue.ToString();
			default: return $"invalid value type {type}";
		}
	}
}

static class ValueStatics
{
	public static Value BOOL_VAL(bool value) => new(VAL_BOOL, value ? 1 : 0);

	public static readonly Value NIL_VAL = new(VAL_NIL, 0);

	public static Value NUMBER_VAL(double value) => new(ValueType.VAL_NUMBER, value);

	public static bool AS_BOOL( Value value) => value.dValue != 0;
	public static double AS_NUMBER( Value value) => value.dValue;

	public static bool IS_BOOL( Value value) => value.type == VAL_BOOL;
	public static bool IS_NIL( Value value) => value.type == VAL_NIL;
	public static bool IS_NUMBER(Value value) => value.type == VAL_NUMBER;

}

class ValueArray : List<Value>
{
	public void write(Value value) => Add(value);

}
