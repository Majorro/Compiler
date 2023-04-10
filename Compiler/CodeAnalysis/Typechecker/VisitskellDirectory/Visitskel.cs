using Compiler.CodeAnalysis.LexerTokens;
using Compiler.CodeAnalysis.SyntaxAnalysis;

namespace Compiler.CodeAnalysis.Typechecker;

public class Visitskel
{
    public void ProgramVisitor(ProgramNode program, Context context)
    {
        foreach (var routine in program.DeclarationList)
        {
            var routine_ = routine as RoutineDeclarationNode;
            DeclVisitor(routine_, context);
        }
        return;
    }

    public object DeclVisitor(RoutineDeclarationNode decl, Context context)
    {
        var child_context = new Context(context);
        foreach (var param in decl.Parameters)
        {
            DeclParamVisitor(param, child_context);
        }

        var returnType = decl.ReturnType.Kind;
        Console.WriteLine(returnType);
        return null;
    }

    public void DeclParamVisitor(ParameterDeclarationNode param, Context context)
    {
        var name = param.Identifier.Token as IdentifierTk;
        var type = param.Type.Kind;
        context.addStelladient(name.value, param.Type);
    }
}