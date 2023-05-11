using System.Globalization;
using Compiler.Core.CodeAnalysis.LexicalAnalysis;
using Compiler.Core.CodeAnalysis.SyntaxAnalysis;
using Compiler.Core.CodeAnalysis.Typechecker;
using Compiler.Core.CodeGen;
using Newtonsoft.Json;

namespace Compiler.Core;

public static class ImperativeCompiler
{
    public static CodeCompiler Compile(string code, string name, string? path = null, bool verbose = true)
    {
        // Magic for double.TryParse function to work properly
        Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US", false);
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US", false);

        var lexer = new Lexer(code);
        if (verbose)
        {
            Console.WriteLine("Tokenization done!");
            PrintTokens(lexer);
        }

        var parser = new Parser(lexer);
        parser.Parse();
        if (verbose)
        {
            Console.WriteLine("Parsing done!");
            PrintTree(parser);
        }

        var typeCheckContext = Typecheck.TypecheckProgram(parser);
        if (verbose)Console.WriteLine("Type checking done!");

        var compiler = CodeGenerator.Generate(name, parser, typeCheckContext, path);
        if (verbose) Console.WriteLine("Code generation done!");
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

    public static void PrintTree(Parser parser) =>
        Console.WriteLine(parser.Tree);
}