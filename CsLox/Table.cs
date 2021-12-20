global using static CsLox.TableStatics;

using System.Diagnostics.CodeAnalysis;

namespace CsLox;

internal class Table: Dictionary<ObjString,Value>
{
	public Table()
	{
	}
}

static class TableStatics
{
	public static bool tableSet(Table table, ObjString key, Value value)
	{
		int count = table.Count;
		table[key] = value;
		return count != table.Count;
	}

	public static void tableAddAll(Table from, Table to)
	{
		foreach (var item in from)
		{
			to[item.Key] = item.Value;
		}
	}

	public static bool tableGet(Table table, ObjString key, ref Value value) 
		=> table.TryGetValue(key, out value);

	public static bool tableDelete(Table table, ObjString key) 
		=> table.Remove(key);

}
