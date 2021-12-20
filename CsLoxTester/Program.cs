
using System.Diagnostics;

class Program
{
	int nTests, nSuccess, nFail, nSkipped;


	void TestFile(string fileName)
	{
		string? msg = null;
		try
		{
			var baseName = Path.GetFileName(fileName);
			if (baseName[0] == '-')
			{
				Interlocked.Increment(ref nSkipped);
				return;
			}
			Interlocked.Increment(ref nTests);
			var source = File.ReadAllText(fileName);
			var sw = new StringWriter();
			Globals.RunCode(source, sw);
			var res = sw.ToString().ReplaceLineEndings();
			var expected = GetSourceOutput(source).ReplaceLineEndings();
			if (res == expected)
			{
				Interlocked.Increment(ref nSuccess);
				return;
			}
			msg = $@"..........got.......
{res}
.......expected.........
{expected}";

		}
		catch (Exception ex)
		{
			msg = ex.ToString();
		}
		if (msg == null) return;
		Interlocked.Increment(ref nFail);
		lock (this)
		{
			var col = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Magenta;
			Console.WriteLine($"\r{fileName}              ");
			Console.ForegroundColor = col;
			Console.WriteLine(msg);
		}
	}

	static string GetSourceOutput(string source)
	{
		const string search = "// expect: ";
		const string searchRtErr = "// expect runtime error: ";

		var sw = new StringWriter();
		var lines = source.Split('\n');
		foreach (var line in lines)
		{
			int idx = line.IndexOf(search);
			if (idx >= 0)
			{
				var resString = line[(idx + search.Length)..].TrimEnd();
				sw.WriteLine(resString);
			}
			idx = line.IndexOf(searchRtErr);
			if (idx >= 0)
			{
				sw.WriteLine(line.Substring(idx + searchRtErr.Length).TrimEnd());
			}
			idx = line.IndexOf("//");
			if (idx < 0) continue;
			idx = line.IndexOf("Error at", idx);
			if (idx < 0) continue;
			idx = line.IndexOf(": ", idx);
			if (idx < 0) continue;
			sw.WriteLine(line.Substring(idx + 2).TrimEnd());
		}
		return sw.ToString();
	}

	void RunTestsInDir(string dir)
	{
		var files = Directory.GetFiles(dir, "*.lox");
		foreach (var file in files)
		{
			lock (this) Console.Write($"\r{file}           ");
			TestFile(file);
		}
		foreach (var idir in Directory.GetDirectories(dir))
		{
			RunTestsInDir(idir);
		}
	}

	void RunTestsInDirParallel(string dir)
	{
		Parallel.ForEach(Directory.GetDirectories(dir), idir =>
			RunTestsInDirParallel(idir));

		var files = Directory.GetFiles(dir, "*.lox");
		Parallel.ForEach(files, file =>
		{
			lock (this) Console.Write($"\r{file}           ");
			TestFile(file);
		});
	}

	void RunTests(string dir)
	{
		Console.WriteLine($"Start Testing dir: {dir}");
		var watch = Stopwatch.StartNew();
		RunTestsInDir(dir);

		Console.WriteLine();
		Console.WriteLine("---- Results ---");
		Console.WriteLine($"Skipped: {nSkipped,4}");
		Console.WriteLine($"Tests  : {nTests,4} in {watch.ElapsedMilliseconds} ms");
		Console.WriteLine($"Success: {nSuccess,4}");
		if (nFail > 0)
			Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine($"Fail   : {nFail,4}");
	}


	public static void Main(string[] args)
	{
		Console.WriteLine("CsLox Tester v1");
		if (args.Length < 1) return;
		if (args.Length >= 2)
		{
			Console.WriteLine($"Running file: {args[1]}");
			Globals.RunFile(args[1], Console.Out, true);
		}
		var prog = new Program();
		prog.RunTests(args[0]);
		if (Debugger.IsAttached) Console.ReadLine();
	}
}

