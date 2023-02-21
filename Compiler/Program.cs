// See https://aka.ms/new-console-template for more information

using Compiler.SyntaxAnalyser;
using Newtonsoft.Json;

var program = @"routine bubbleSort(array: integer[]): integer[] is
   var size := array.Length;
   var swapped := false;

    for i in 0 .. (size - 1) loop is
        for j in 0 .. (size - i - 1) loop is
            if array[j] > array[j+1] then
               var swapped := true;
               var temp := array[j];
                array[j] := array[j + 1];
                array[j + 1] := temp;
        if  swapped = false then
            return array;";

// program = @"for i in 1 .. (small + 1) loop is";


foreach (var tok in SyntaxAnalyser.FinalStateAutomata(program))
    Console.WriteLine($"{tok.TokenId} \t {JsonConvert.SerializeObject(tok)}");