global using static CsLox.ObjType;
global using static CsLox.ObjStatics;

namespace CsLox;

enum ObjType
{
	OBJ_FUNCTION, 
	OBJ_STRING,
}


internal class Obj
{
	public ObjType type;

}

internal class ObjFunction : Obj
{
	public int arity;
	public Chunk chunk;
	public ObjString name;

	public ObjFunction()
	{
		this.type = OBJ_FUNCTION;
		this.arity = 0;
		this.name = new ObjString("");
		this.chunk = new Chunk();
	}

	public override string ToString() => $"<fn {NameOrScript}>";

	public string NameOrScript
	{
		get
		{
			var s = name.chars;
			if (string.IsNullOrEmpty(s)) return "<script>";
			return s;
		}
	}

}
internal class ObjString : Obj, IEquatable<ObjString>
{
	public readonly string chars;
	public readonly int hash;

	public ObjString(string v)
	{
		type = OBJ_STRING;
		chars = v;
		hash = v.GetHashCode();
	}

	public bool Equals(ObjString? other)
	{
		if (other is null) return false;
		if (hash != other.hash) return false;
		return chars == other.chars;
	}

	public override int GetHashCode() => hash;
	public override bool Equals(object? obj) => Equals(obj as ObjString);

	public override string ToString() => chars;
}

static class ObjStatics
{
	public static ObjType OBJ_TYPE(Value value) => AS_OBJ(value).type;
	public static bool IS_FUNCTION(Value value) => isObjType(value, OBJ_FUNCTION);
	public static bool IS_STRING(Value value) => isObjType(value, OBJ_STRING);
	public static ObjString AS_STRING(Value value) => ((ObjString)AS_OBJ(value));
	public static ObjFunction AS_FUNCTION(Value value) => ((ObjFunction)AS_OBJ(value));
	public static string AS_CSTRING(Value value) => (((ObjString)AS_OBJ(value)).chars);

	static bool isObjType(Value value, ObjType type)
	{
		return IS_OBJ(value) && AS_OBJ(value).type == type;
	}

}