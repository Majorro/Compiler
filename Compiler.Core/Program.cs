using Compiler.Core;

const string program = @"
routine SumRealToInt(a: real, b: real): integer is
    var res is 99.9;
    res := a + b;
    return res;
end;";

ImperativeCompiler.Compile(program, "PlusOne");