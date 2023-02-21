// See https://aka.ms/new-console-template for more information

using Compiler.SyntaxAnalyser;
using Newtonsoft.Json;

var program = " 1 := or else";

foreach (var tok in SyntaxAnalyser.FinalStateAutomata(program))
    Console.WriteLine($"{tok.TokenId} \t {JsonConvert.SerializeObject(tok)}");