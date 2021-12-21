﻿global using static CsLox.ObjType;
global using static CsLox.ObjStatics;

namespace CsLox;

enum ObjType
{
	OBJ_CLASS, 
	OBJ_CLOSURE,
	OBJ_FUNCTION,
	OBJ_NATIVE,
	OBJ_STRING,
	OBJ_UPVALUE,
}

internal class Obj
{
	public ObjType type;
}

internal class ObjFunction : Obj
{
	public int arity;
	public readonly Chunk chunk;
	public int upvalueCount;
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

delegate Value NativeFn(Value[] args);

internal class ObjNative : Obj
{
	public readonly NativeFn function;

	public ObjNative(NativeFn function)
	{
		this.type = OBJ_NATIVE;
		this.function = function;
	}

	public override string ToString() => "<native fn>";

}

internal class ObjString : Obj, IEquatable<ObjString>
{
	public readonly string chars;
	public readonly int hash;

	public ObjString(string v)
	{
		this.type = OBJ_STRING;
		this.chars = v;
		this.hash = v.GetHashCode();
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

class ObjUpvalue : Obj
{
	public int slotIndex; // -1 when closed
	public Value closed;	// value when closed
	public ObjUpvalue? next;

	public ObjUpvalue(int local)
	{
		this.type = OBJ_UPVALUE;
		this.slotIndex = local;
		this.closed = NIL_VAL;
		this.next = null;
	}

	public override string ToString() => "upvalue";
}


class ObjClosure : Obj
{
	public ObjFunction function;
	public ObjUpvalue[] upvalues;
	public int upvalueCount;
	public ObjClosure(ObjFunction function)
	{
		this.type = OBJ_CLOSURE;
		this.function = function;
		this.upvalueCount = function.upvalueCount; 
		upvalues = new ObjUpvalue[function.upvalueCount];
	}

	public override string ToString() => function.ToString();

}

class ObjClass: Obj
{
	public ObjString name;

	public ObjClass(ObjString name)
	{
		this.type = OBJ_CLASS;
		this.name = name;
	}

	public override string ToString() => name.chars;

} 
static class ObjStatics
{
	public static ObjType OBJ_TYPE(Value value) => AS_OBJ(value).type;
	public static bool IS_CLASS(Value value) => isObjType(value, OBJ_CLASS);
	public static bool IS_CLOSURE(Value value) => isObjType(value, OBJ_CLOSURE);
	public static bool IS_FUNCTION(Value value) => isObjType(value, OBJ_FUNCTION);
	public static bool IS_NATIVE(Value value) => isObjType(value, OBJ_NATIVE);
	public static bool IS_STRING(Value value) => isObjType(value, OBJ_STRING);
	public static ObjString AS_STRING(Value value) => ((ObjString)AS_OBJ(value));
	public static ObjClosure AS_CLOSURE(Value value) => ((ObjClosure)AS_OBJ(value));
	public static ObjClass AS_CLASS(Value value) => ((ObjClass)AS_OBJ(value));
	public static ObjFunction AS_FUNCTION(Value value) => ((ObjFunction)AS_OBJ(value));
	public static NativeFn AS_NATIVE(Value value) => ((ObjNative)AS_OBJ(value)).function;
	public static string AS_CSTRING(Value value) => (((ObjString)AS_OBJ(value)).chars);

	static bool isObjType(Value value, ObjType type)
	{
		return IS_OBJ(value) && AS_OBJ(value).type == type;
	}

}