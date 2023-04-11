using Compiler.CodeAnalysis.LexerTokens;
using Compiler.CodeAnalysis.SyntaxAnalysis;
using Compiler.Utils;

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

        // Body
        BodyVisitor(decl.Body, childContext, true);
        
        return null;
    }

    public void DeclParamVisitor(ParameterDeclarationNode param, Context context)
    {
        var name = param.Identifier.Token as IdentifierTk;
        var type = param.Type.Kind.GetDescription();
        context.add(name.value, type);
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

    public void VariableDeclVisitor(VariableDeclarationNode variable, Context context)
    {
        var identifier = variable.Identifier.Token as IdentifierTk;
        String? type = null;
        switch (variable.Expression)
        {
            case LiteralNode node:
                type = LiteralNodeVisitor(node, context);
                break;
            case FactorNode node:
                type = FactorNodeVisitor(node, context);
                break;
            case ModifiablePrimaryNode node:
                type = ModifiablePrimaryNodeVisitor(node, context);
                break;
        }

        if (type is null) throw new Exception("error");
        context.add(identifier.value, type);
        return;
    }
    
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

    public String? LiteralNodeVisitor(LiteralNode expr, Context context)
    {
        var type = expr.Kind;
        switch (type)
        {
            case null:
                context.addError($"Type is null at {expr.Token.Span!.StartColumn} {expr.Token.Span.StartLine} end at {expr.Token.Span.EndColumn} {expr.Token.Span.EndLine}");
                break;
        }
        return expr.Kind.GetDescription() ?? throw new InvalidOperationException();
    }

    public String? FactorNodeVisitor(FactorNode expr, Context context)
    {
        String? type = null;

        object? lhs = expr.Lhs switch
        {
            FactorNode node => FactorNodeVisitor(node, context),
            LiteralNode node => LiteralNodeVisitor(node, context),
            ModifiablePrimaryNode node => ModifiablePrimaryNodeVisitor(node, context),
            _ => null
        };

        String? operator_ = expr.Operator switch
        {
            OperatorNode node => node.Kind.GetDescription().Split(" ")[0],
            _ => null
        };
        
        object? rhs = expr.Rhs switch
        {
            FactorNode node => FactorNodeVisitor(node, context),
            LiteralNode node => LiteralNodeVisitor(node, context),
            ModifiablePrimaryNode node => ModifiablePrimaryNodeVisitor(node, context),
            _ => null
        };

        if (rhs == null && operator_ == null)
        {
            return (string?)lhs;
        }

        if (((string)lhs! == (string)rhs!) && operator_ is "+" or "-")
        {
            return (string)lhs!;
        }
        context.addError($"Incorrect type at factor node");
        return null;
    }

    public String? ModifiablePrimaryNodeVisitor(ModifiablePrimaryNode expr, Context context)
    {
        switch (expr.Identifier.Token)
        {
            case IdentifierTk ident:
                return context.get(ident.value);
        }
        context.addError($"Variable doesnt exist at {expr.Identifier.Token.Span!.StartLine} {expr.Identifier.Token.Span!.StartColumn} {expr.Identifier.Token.Span!.EndLine} {expr.Identifier.Token.Span!.EndColumn}");
        return null;
    }

    public void ExpressionNodeVisitor()
    {
        return;
    }
}