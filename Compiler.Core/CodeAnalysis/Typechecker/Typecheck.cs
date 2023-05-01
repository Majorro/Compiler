using Compiler.Core.CodeAnalysis.SyntaxAnalysis;
using Compiler.Core.CodeAnalysis.Typechecker.VisitskellDirectory;

namespace Compiler.Core.CodeAnalysis.Typechecker;

public static class Typecheck
{
    public static Visitskel TypecheckProgram(Parser program)
    {
        if (program.Tree == null)
            throw new Exception("AST tree in null");

        var visit = new Visitskel();
        var context = new Context();
        try
        {
            visit.ProgramVisitor(program.Tree.Root, context);
        }
        catch (Exception exception)
        {
            Console.WriteLine(string.Join("\n", context.GetErrors()));
        }

        return visit;
    }
}