using System.Reflection;
using CIL;

namespace UnitTests;

public class Program
{
    public Type Type;
    public MethodInfo Entrypoint;

    public Program(Type type, MethodInfo entrypoint)
    {
        Type = type;
        Entrypoint = entrypoint;
    }

    public TReturn Call<TReturn>(params object[] args)
    {
        return (TReturn)Entrypoint.Invoke(Type, args)!;
    }
}

public class Tests
{
    public Program LoadProgram(string name, string? compileToPath = null)
    {
        var code = File.ReadAllText($"../../../Tests/{name}.txt");

        var compiler = Compiler.Program.Compile(code, name, compileToPath);

        var dllFile = new FileInfo(compileToPath ?? name + ".dll");
        var dll = Assembly.LoadFile(dllFile.FullName);
        var theType = dll.GetType($"ProjectI.Program{name}");
        Assert.IsNotNull(theType, "theType is null");
        var method = theType!.GetMethod(name);
        Assert.IsNotNull(method, "method is null");

        var il = new ILReader(method!);
        while (il.MoveNext())
        {
            Console.WriteLine($"{il.Current}");
        }

        Console.WriteLine("-- Local variable names:");
        Console.WriteLine(string.Join("\n", compiler.RoutineLocalVariables.ToArray()));

        return new Program(theType, method!);
    }

    public static string GetDllName(string name, object[] args)
    {
        return $"{name}_{string.Join(' ', args)}.dll";
    }

    [TestCaseSource(nameof(DivideCases))]
    public void Test<TReturn>(string name, object[] args, TReturn result)
    {
        var dllName = GetDllName(name, args);
        var res = LoadProgram(name, compileToPath: dllName).Call<TReturn>(args);
        Assert.That(res, Is.EqualTo(result));
    }
    
    [Test]
    public void TestIntToBoolException()
    {
        var name = "IntToBool"; 
        object[] args = {"2"}; 
        var dllName = GetDllName(name, args);
        Assert.Throws<ArgumentException>(() => LoadProgram(name, compileToPath: dllName).Call<bool>(args));
    }

    public static (int a, float b, bool c) RecordTest = (3, 2f, true);

    public static object[] DivideCases =
    {
        new object[] { "PlusOne", new object[] { 41 }, 42 },
        new object[] { "PlusOneReal", new object[] { 41f }, 42f },
        new object[] { "IntToReal", new object[] { 42 }, 42f },
        new object[] { "IntToBool", new object[] { 0 }, false },
        new object[] { "IntToBool", new object[] { 1 }, true },
        new object[] { "SumRealToInt", new object[] { 1.1f, 2.2f }, 3 },
        new object[] { "SumRealToInt", new object[] { 1.1f, 2.6f }, 4 },
        new object[] { "Equal", new object[] { 5, 5 }, true },
        new object[] { "Equal", new object[] { 5, 6 }, false },
        new object[] { "Equal", new object[] { 6, 5 }, false },
        new object[] { "SqrWithWhileLoop", new object[] { 5, 0 }, 1 },
        new object[] { "SqrWithWhileLoop", new object[] { 5, 1 }, 5 },
        new object[] { "SqrWithWhileLoop", new object[] { 5, 3 }, 125 },
        new object[] { "BabylonianSqrt", new object[] { 5f }, (float)Math.Sqrt(5) },
        new object[] { "Logic1010", new object[] { true, true }, true },
        new object[] { "Logic1010", new object[] { true, false }, false },
        new object[] { "Logic1010", new object[] { false, true }, true },
        new object[] { "Logic1010", new object[] { false, false }, false },
        // new object[] { "Pythagoras", new object[] { 3f, 4f }, 5f },
        // new object[] { "CastingAndRecord", new object[] { RecordTest }, RecordTest },
    };
}