global using static CsLox.ObjType;

namespace CsLox;

enum ObjType
{
	OBJ_STRING,
}


internal class Obj
{
	public ObjType type;

}

internal class ObjString : Obj
{
	public string chars = "";

}

static class ObjStatics
{
	public static ObjType OBJ_TYPE(Value value) => AS_OBJ(value).type;
	public static bool IS_STRING(Value value) => isObjType(value, OBJ_STRING);
	public static ObjString AS_STRING(Value value) => ((ObjString)AS_OBJ(value));
	public static string AS_CSTRING(Value value) => (((ObjString)AS_OBJ(value)).chars);

	static bool isObjType(Value value, ObjType type)
	{
		return IS_OBJ(value) && AS_OBJ(value).type == type;
	}

}