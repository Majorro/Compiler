using Compiler.CodeAnalysis.SyntaxAnalysis;

namespace Compiler.CodeAnalysis.Typechecker;

public static class Typecheck
{
    public static Visitskel typecheckProgram(Parser program)
    {
        if (program.Tree == null)
        {
            throw new Exception("AST tree in null");
        }

        var visit = new Visitskel();
        var context = new Context(null);
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