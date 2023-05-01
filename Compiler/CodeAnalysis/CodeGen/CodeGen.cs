using Compiler.CodeAnalysis.SyntaxAnalysis;
using Compiler.CodeAnalysis.Typechecker;


namespace Compiler.CodeAnalysis.CodeGen;

public abstract class CodeGen
{
    public static CodeCompiler Generate(
        string programName, Parser program, Visitskel typecheckVisitor, string? path = null)
    {
        var compiler = new CodeCompiler(programName, program, typecheckVisitor);
        compiler.CompileToFile(path);
        return compiler;
    }
}