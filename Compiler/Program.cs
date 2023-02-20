// See https://aka.ms/new-console-template for more information

using Compiler.SyntaxAnalyser;
using Newtonsoft.Json;

var program = "int gagaD12 if 'f' else  4.20; for";

foreach (var tok in SyntaxAnalyser.FinalStateAutomata(program))
    Console.WriteLine($"{tok.TokenId} \t {JsonConvert.SerializeObject(tok)}");