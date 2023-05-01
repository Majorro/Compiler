using Compiler.Core;

const string program = @"
routine factorial(n: integer): bool is
    if n /= 1 then
        var a is n * factorial(n - 1);
        return a;
    else
        return 1;
    end;
end;

routine main(): integer is
    factorial(5);
    return 1;
end;";

ImperativeCompiler.Compile(program, "PlusOne");