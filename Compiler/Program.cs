using System.Security.Cryptography;
using Compiler.CodeAnalysis;
using Compiler.CodeAnalysis.LexerTokens;
using Compiler.CodeAnalysis.SyntaxAnalysis;
using Compiler.CodeAnalysis.Typechecker;
using Newtonsoft.Json;

// Doesn't work:
// function calls
const string program = @"
routine example(a: integer, b: integer): integer is
    var d is 1;

    for i in 1 .. a + 1 loop
        if a % i = 0 and b % i = 0 then
            d := i;
        else
            d := d + 1;
        end;
    end;

    return d;
end;

routine main(a: integer): integer is
    var d is a + 1 * 10 / 100;
    return c;
end;
";



// const string program = @"
// var a is 1+2;
// ";
// const string program = @"
// routine SumOfArray ( arr : array[] integer ) : integer is
//     var sum is 0;
//
//     for i in 0 .. arr.Length loop
//         sum := i + 1;
//     end;
//
//     return sum;
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

// PrintTokens(program);
PrintAst(program);

void PrintTokens(string prog)
{
    var lexer = new Lexer(prog);

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

void PrintAst(string prog)
{
    var lexer = new Lexer(prog);

    var parser = new Parser(lexer);

    parser.Parse();
    
    Console.WriteLine(parser.Tree);
    var tree = parser.Tree;
    
    Typecheck.typecheckProgram(parser);
    
    Console.WriteLine("Type checking done!");
}