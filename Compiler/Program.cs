// See https://aka.ms/new-console-template for more information

using Newtonsoft.Json;

using Compiler.SyntaxAnalyser;

string program = "int 'f'  4.20;";

foreach (var tok in SyntaxAnalyser.FinalStateAutomata(program))
{
    Console.WriteLine(JsonConvert.SerializeObject(tok));
    
}
