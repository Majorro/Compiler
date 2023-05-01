using System.Globalization;
using Compiler.CodeAnalysis;
using Compiler.CodeAnalysis.CodeGen;
using Compiler.CodeAnalysis.SyntaxAnalysis;
using Compiler.CodeAnalysis.Typechecker;
using Newtonsoft.Json;

namespace Compiler;

// Doesn't work:
// function calls
// const string program = @"
// routine example(a: integer, b: integer): integer is
//     var d is 1;
//      
//     for i in 1 .. a + 1 loop
//         if a % i = 0 and b % i = 0 then
//             d := i;
//         else
//             d := d + 1;
//         end;
//     end;
//
//     return d;
// end;
//
// routine main(a: integer): integer is
//     var d is a + 1 * 10 / 100;
//     return d;
// end;
// ";

// const string program = @"
// var a is 1+2;
// ";

// const string program = @"
// routine SumOfArray ( arr : array[] integer ) : integer is
//     var sum is 0;
//     var check is 0+2+2;
//     var pup is sum;
//     var pup1 is sum+1;
//     for i in 0 .. arr.Length loop
//         sum := i + 2;
//     end;
//
//     return sum;
// end;
// ";

// const string program = @"
// routine SumFunction (a: integer, b: integer): integer is
//     return a + b;
// end;
//
// routine SumOfArray (a: integer, b: integer ): integer is
//     SumFunction(a,b);
//     return 1;
// end;
// ";

// const string program = @"
// print(42);
// ";

// const string program = @"
// var a: integer is 1;
// var b: real is 2.1;
// var c: boolean is true;
// var d: array[3] boolean;
// var e: record
// {
//     var f: integer;
// }
// end;
// ";

// program = @"2a int";


// const string program = @"
// routine factorial(n: integer): bool is
//     if n /= 1 then
//         var a is n * factorial(n - 1);
//         return a;
//     else
//         return 1;
//     end;
// end;
//
// routine main(): integer is
//     factorial(5);
//     return 1;
// end;";

public class Program
{
    public static void Main()
    {
        const string program = @"
        routine PlusOne(a: integer): integer is
            var res is a + 1;
            return res;
        end;
        ";
        Compile(program, "PlusOne");
    }

    public static CodeCompiler Compile(string code, string name, string? path = null)
    {
        // Magic for double.TryParse function to work properly
        Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US", false); 
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US", false);
        
        var lexer = new Lexer(code);
        Console.WriteLine("Tokenization done!");
        PrintTokens(lexer);
        
        var parser = new Parser(lexer);
        parser.Parse();
        Console.WriteLine("Parsing done!");
        PrintTree(parser);

        var typeCheckContext = Typecheck.typecheckProgram(parser);
        Console.WriteLine("Type checking done!");
        
        var compiler = CodeGen.Generate(name, parser, typeCheckContext, path);
        Console.WriteLine("Code generation done!");
        return compiler;
    }
    
    public static void PrintTokens(Lexer lexer)
    {
        foreach (var tok in lexer.ProgramTokens)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{tok.GetType().Name}\t");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{tok.TokenId}\t");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"{JsonConvert.SerializeObject(tok)}");
        }
    }
    
    public static void PrintTree(Parser parser)
    {
        Console.WriteLine(parser.Tree);
    }
}