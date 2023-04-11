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
                    Console.WriteLine("New routine!");
                    Console.WriteLine(routine);
                    DeclVisitor(node, context);
                    break;
            }
        }
        return;
    }

    public object DeclVisitor(RoutineDeclarationNode decl, Context context)
    {
        // Parameters
        var childContext = new Context(context);
        foreach (var param in decl.Parameters)
        {
            DeclParamVisitor(param, childContext);
        }

        // Return type
        object? returnType;
        
        if (decl.ReturnType != null)
        {
            returnType = decl.ReturnType.Kind;
        }
        else
        {
            returnType = null;
        }
        Console.Write("Return type: ");
        Console.WriteLine(returnType);

        // Body - reversed iterating
        for (int i = decl.Body.Items.Count - 1; i >= 0; i--)
        {
            switch (decl.Body.Items[i])
            {
                case ReturnNode node:
                    //Console.WriteLine("New ReturnNode");
                    //Console.WriteLine(node);
                    break;
                
                case VariableDeclarationNode node:
                    //Console.WriteLine("New VariableDeclarationNode");
                    //Console.WriteLine(node);
                    break;
                
                case IfNode node:
                    //Console.WriteLine("New IfNode");
                    //Console.WriteLine(node);
                    break;
                    
                case ForLoopNode node:
                    //Console.WriteLine("New ForLoopNode");
                    //Console.WriteLine(node);
                    break;
                
                case WhileLoopNode node:
                    break;
                
                case AssignmentNode node:
                    break;
                
                // Doesn't work
                case RoutineCallNode node:
                    break;
            }
        }
        
        return null;
    }

    public void DeclParamVisitor(ParameterDeclarationNode param, Context context)
    {
        var name = param.Identifier.Token as IdentifierTk;
        var type = param.Type.Kind;
        context.addStelladient(name.value, param.Type);
    }
}