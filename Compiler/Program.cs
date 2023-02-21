// See https://aka.ms/new-console-template for more information

using Compiler.SyntaxAnalyser;
using Newtonsoft.Json;

var program = @"routine gcd (a: integer, b: integer): integer is
if a > b then
    var small := b;
else
var small := a;
end;
for i in 1 .. (small + 1) loop is
if (a % i = 0) and (b % i = 0) then
    var ans := i;
end;
return ans;";

// program = @"for i in 1 .. (small + 1) loop is";


foreach (var tok in SyntaxAnalyser.FinalStateAutomata(program))
    Console.WriteLine($"{tok.TokenId} \t {JsonConvert.SerializeObject(tok)}");