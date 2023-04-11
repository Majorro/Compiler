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
        
        // Parameters
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

        // Body
        BodyVisitor(decl.Body, childContext, true);
        
        return null;
    }

    public void DeclParamVisitor(ParameterDeclarationNode param, Context context)
    {
        var name = param.Identifier.Token as IdentifierTk;
        var type = param.Type.Kind;
        context.add(name.value, param.Type);
    }

    public void BodyVisitor(BodyNode body, Context context, Boolean reversedIteration = false)
    {
        var items = body.Items;
        if (reversedIteration)
        {
            items.Reverse();
        }
        
        foreach (var bodyStatement in items)
        {
            switch (bodyStatement)
            {
                case ReturnNode node:
                    ReturnVisitor(node, context);
                    break;
                
                case VariableDeclarationNode node:
                    VariableDeclVisitor(node, context);
                    break;
                
                case IfNode node:
                    IfVisitor(node, context);
                    break;
                    
                case ForLoopNode node:
                    ForLoopVisitor(node, context);
                    break;
                
                case WhileLoopNode node:
                    WhileLoopVisitor(node, context);
                    break;
                
                case AssignmentNode node:
                    AssigmentVisitor(node, context);
                    break;
                
                // Doesn't work
                case RoutineCallNode node:
                    RoutineCallVisitor(node, context);
                    break;
            }
        }
    }
    
    public void ReturnVisitor(ReturnNode returnStatement, Context context) {}
    
    public void VariableDeclVisitor(VariableDeclarationNode variable, Context context) {}
    
    public void AssigmentVisitor(AssignmentNode assigment, Context context) {}

    public void IfVisitor(IfNode ifStatement, Context context)
    {
        var childContext = new Context(context); 
        // Add handling condition;
         
        BodyVisitor(ifStatement.ThenBody, childContext);

        if (ifStatement.ElseBody != null) BodyVisitor(ifStatement.ElseBody, childContext);
    }

    public void ForLoopVisitor(ForLoopNode loop, Context context)
    {
        var childContext = new Context(context);
        
        //Add handling Range and identifier
        
        BodyVisitor(loop.Body, childContext);
    }

    public void WhileLoopVisitor(WhileLoopNode loop, Context context) {}
    
    public void RoutineCallVisitor(RoutineCallNode call, Context context) {}
}