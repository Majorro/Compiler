using Compiler.CodeAnalysis.LexerTokens;
using Compiler.CodeAnalysis.SyntaxAnalysis;

namespace Compiler.CodeAnalysis.Typechecker;

public class Visitskel
{
    public void ProgramVisitor(ProgramNode program, Context context)
    {
        foreach (var routine in program.DeclarationList)
        {
            switch (routine)
            {
                case RoutineDeclarationNode node:
                    DeclVisitor(node, context);
                    break;
            }
        }
        return;
    }

    public object DeclVisitor(RoutineDeclarationNode decl, Context context)
    {
        var childContext = new Context(context);
        foreach (var param in decl.Parameters)
        {
            DeclParamVisitor(param, childContext);
        }

        object? returnType;
        
        if (decl.ReturnType != null)
        {
            returnType = decl.ReturnType.Kind;
        }
        else
        {
            returnType = null;
        }
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