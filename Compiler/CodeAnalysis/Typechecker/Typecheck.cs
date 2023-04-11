using Compiler.CodeAnalysis.SyntaxAnalysis;

namespace Compiler.CodeAnalysis.Typechecker;

public static class Typecheck
{
    public static void typecheckProgram(Parser program)
    {
        if (program.Tree == null)
        {
            throw new Exception("AST tree in null");
        }

        var visit = new Visitskel();
        var context = new Context(null);
        visit.ProgramVisitor(program.Tree.Root,context);
    }
}