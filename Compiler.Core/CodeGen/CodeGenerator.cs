using Compiler.Core.CodeAnalysis.SyntaxAnalysis;
using Compiler.Core.CodeAnalysis.Typechecker.VisitskellDirectory;

namespace Compiler.Core.CodeGen;

public static class CodeGenerator
{
    public static CodeCompiler Generate(
        string programName,
        Parser program,
        Visitskel typecheckVisitor,
        string? path = null)
    {
        var compiler = new CodeCompiler(programName, program, typecheckVisitor);
        compiler.CompileToFile(path);
        return compiler;
    }
}