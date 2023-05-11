using System.Globalization;
using System.Reflection;

namespace Compiler.Core;

public static class Entrypoint
{
    private const string ProgramText = @"
routine CastingAndRecord(
    r: record {
        var ifield : integer;
        var rfield : real;
        var bfield : boolean;
    } end
): record {
       var ifield : integer;
       var rfield : real;
       var bfield : boolean;
   } end
is
    r.bfield := false;
    return r;
end;
";
    public class Program
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

    public static void Main(string[] args)
    {
        Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US", false);
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US", false);

        var (methodName, programText, methodArgs) = ParseArgs(args);
        // Console.WriteLine(methodName);
        // Console.WriteLine(programText);
        // Console.WriteLine(methodArgs);
        var compiler = ImperativeCompiler.Compile(programText, methodName, verbose: false);
        var blob = compiler.CompileToBlob();
        var assembly = Assembly.Load(blob.ToArray());
        var theType = assembly.GetType($"ProjectI.Program{methodName}");
        var method = theType!.GetMethod(methodName);
        var program = new Program(theType, method!);
        
        var res = program.Call<object>(methodArgs);
        Console.Write(res);
        // ImperativeCompiler.Compile(ProgramText, "BabylonianSqrt", "../BabylonianSqrt.dll");
    }

    private static (string methodName, string programText, object[] methodArgs) ParseArgs(string[] args)
    {
        if (args.Length == 0)
            throw new ArgumentException(
                "Arguments required. Provide path to program text file and arguments");
        var path = Path.GetFullPath(args[0]);
        var methodName = Path.GetFileNameWithoutExtension(path);
        var programText = File.ReadAllText(args[0]);
        var parsedArgs = new List<object>();
        for (var i = 1; i < args.Length; i++)
        {
            parsedArgs.Add(ParseArg(args[i]));
        }

        return (methodName, programText, parsedArgs.ToArray());
    }

    private static object ParseArg(string arg)
    {
        arg = arg.Trim();
        if (int.TryParse(arg, out var resInt)) return resInt;
        if (float.TryParse(arg, out var resFloat)) return resFloat;
        if (bool.TryParse(arg, out var resBool)) return resBool;
        if (arg.StartsWith("("))
        {
            var recordArgs = arg.Trim('(', ')').Split(",").Select(ParseArg).ToArray();
            return new ValueTuple<int, float, bool>((int) recordArgs[0], (float) recordArgs[1], (bool) recordArgs[2]);
        }
        // if (arg.StartsWith("("))
        // {
        //     var recordArgs = arg.Trim('(', ')').Split(",").Select(ParseArg).ToArray();
        //     var recordArgsTypes = recordArgs.Select(a => a.GetType()).ToArray();
        //     var record = recordArgs.Length switch
        //     {
        //         0 => new ValueTuple(),
        //         1 => CreateValueTuple(typeof(ValueTuple<>), recordArgsTypes, recordArgs),
        //         2 => CreateValueTuple(typeof(ValueTuple<,>), recordArgsTypes, recordArgs),
        //         3 => CreateValueTuple(typeof(ValueTuple<,,>), recordArgsTypes, recordArgs),
        //         // 2 => new ValueTuple<object, object>(recordArgs[0], recordArgs[1]),
        //         // 3 => new ValueTuple<object, object, object>(recordArgs[0], recordArgs[1], recordArgs[2]),
        //         _ => null
        //     };
        //     if (record is not null) return record;
        // }
        throw new ArgumentException($"Cannot parse argument {arg}");
    }

    // private static object? CreateValueTuple(Type type, Type[] recordArgsTypes, object[] recordArgs)
    // {
    //     var genericType = type.MakeGenericType(recordArgsTypes);
    //     Console.WriteLine("2123");
    //     return genericType.GetMethod("Create")?.Invoke(genericType, recordArgs);
    // }
}