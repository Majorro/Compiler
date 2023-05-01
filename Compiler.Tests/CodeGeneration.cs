using System.Reflection;
using CIL;
using Compiler.Core;

namespace Compiler.Tests;

[TestFixture]
public class CodeGeneration
{
    [TestCaseSource(nameof(_divideCases))]
    public void Case_Successful<TReturn>(string name, object[] args, TReturn result)
    {
        var dllName = GetDllName(name, args);
        var res = LoadProgram(name, dllName).Call<TReturn>(args);
        Assert.That(res, Is.EqualTo(result));
    }
    
    [TestCase("IntToBool", new object[] { 2 })]
    [TestCase("IntToBool2Functions", new object[] { 2 })]
    public void Case_Throws_ArgumentException(string name, object[] args)
    {
        var dllName = GetDllName(name, args);
        Assert.Throws<TargetInvocationException>(() => LoadProgram(name, compileToPath: dllName).Call<bool>(args));
    }

    private Program LoadProgram(string name, string? compileToPath = null)
    {
        var code = File.ReadAllText($"../../../Data/{name}.txt");

        var compiler = ImperativeCompiler.Compile(code, name, compileToPath);

        var dllFile = new FileInfo(compileToPath ?? name + ".dll");
        var dll = Assembly.LoadFile(dllFile.FullName);
        var theType = dll.GetType($"ProjectI.Program{name}");
        Assert.That(theType, Is.Not.Null, "theType is null");
        var method = theType!.GetMethod(name);
        Assert.That(method, Is.Not.Null, "method is null");

        var il = new ILReader(method!);
        while (il.MoveNext())
        {
            Console.WriteLine($"{il.Current}");
        }

        Console.WriteLine("-- Local variable names:");
        Console.WriteLine(string.Join("\n", compiler.RoutineLocalVariables.ToArray()));

        return new Program(theType, method!);
    }

    private static string GetDllName(string name, object[] args) =>
        $"{name}_{string.Join(' ', args)}.dll";

    private static (int a, float b, bool c) _recordTest = (3, 2f, true);

    private static object[] _divideCases =
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
        new object[] { "Pythagoras", new object[] { 3f, 4f }, 5f }
        // new object[] { "CastingAndRecord", new object[] { RecordTest }, RecordTest },
    };

    private class Program
    {
        public readonly Type Type;
        public readonly MethodInfo Entrypoint;

        public Program(Type type, MethodInfo entrypoint)
        {
            Type = type;
            Entrypoint = entrypoint;
        }

        public TReturn Call<TReturn>(params object[] args) =>
            (TReturn)Entrypoint.Invoke(Type, args)!;
    }
}