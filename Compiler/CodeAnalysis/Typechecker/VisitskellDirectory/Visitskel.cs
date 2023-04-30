using System.Xml;
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
            DeclarationNodeVisitor(routine, context);
        }

        var t = context;
    }

    public object RoutineDeclarationNodeVisitor(RoutineDeclarationNode decl, Context context)
    {
        var childContext = new Context(context);
        var parameters = new List<String>();
        // Parameters
        foreach (var param in decl.Parameters)
        {
            var temp = ParameterDeclarationNodeVisitor(param, childContext);
            if (temp != null)
            {
                parameters.Add(temp);
            };
        }

        // Return type
        String? returnType = null;
        if (decl.ReturnType != null)
        {
            returnType = TypeNodeVisitor(decl.ReturnType, childContext);
        }

        // Body
        var bodyType = BodyVisitor(decl.Body, childContext, true);
        var ident = IdentifierNodeVisitor(decl.Identifier, childContext);
        if (bodyType != returnType)
        {
            context.addError($"Return type not same as declared in routine {ident}");
            throw new Exception();
        }
        context.addRoutine(ident, String.Join(" ",parameters));
        return null;
    }

    public String? ParameterDeclarationNodeVisitor(ParameterDeclarationNode param, Context context)
    {
        var name = param.Identifier.Token as IdentifierTk;
        var type = TypeNodeVisitor(param.Type, context);
        context.add(name.value, type);
        return type;
    }

    public String? BodyVisitor(BodyNode body, Context context, Boolean reversedIteration = false)
    {
        var items = body.Items;
        var results = new List<String>();
        if (reversedIteration)
        {
            items.Reverse();
        }
        
        foreach (var bodyStatement in items)
        {

            switch (bodyStatement)
            {
                case IStatementNode node:
                    var result = StatementNodeVisitor(node, context);
                    if(result != null) results.Add(result);
                    break;
                case DeclarationNode node:
                    DeclarationNodeVisitor(node, context);
                    break;
            }
            
        }

        return results.Distinct().Count() == 1 ? results.First() : null;
    }

    public String? ReturnNodeVisitor(ReturnNode returnStatement, Context context)
    {
        return ExpressionNodeVisitor(returnStatement.Expression, context);
        
    }

    public void VariableDeclVisitor(VariableDeclarationNode variable, Context context)
    {
        var identifier = variable.Identifier.Token as IdentifierTk;
        String? type = ExpressionNodeVisitor(variable.Expression, context);
        if (type is null) throw new Exception("error");
        context.add(identifier.value, type);
    }

    public String? AssignmentNodeVisitor(AssignmentNode assigment, Context context)
    {
        String? type = ExpressionNodeVisitor(assigment.Expression, context);
        var mainType = ModifiablePrimaryNodeVisitor(assigment.Identifier, context);

        if (mainType == type) return null;
        if (type != null && (type == "integer" || type == "boolean" || type == "real")) return null;
        context.addError($"Type of expression is not convertable!");
        throw new Exception();

    }

    public String? IfNodeVisitor(IfNode ifStatement, Context context)
    {
        var childContext = new Context(context); 
        // Add handling condition;

        var condition = ExpressionNodeVisitor(ifStatement.Condition, context);
        if (condition != "boolean")
        {
            context.addError($"Condition is not boolean!");
            throw new Exception();
        }
        
        var returnType = BodyVisitor(ifStatement.ThenBody, childContext);

        if (ifStatement.ElseBody == null) return returnType;
        var elseType = BodyVisitor(ifStatement.ElseBody, childContext);

        if (returnType != null && elseType != null)
        {
            if (returnType == elseType) return returnType;
            context.addError($"Return types at ifloop not same!");
            throw new Exception();

        }else if (returnType == null && elseType != null)
        {
            return elseType;
                
        }else if (returnType != null && elseType == null)
        {
            return returnType;
        }
        return null;
    }

    public String? ForLoopVisitor(ForLoopNode loop, Context context)
    {
        //Add handling Range and identifier
        var ident = ((loop.VariableIdentifier.Token as IdentifierTk)!).value;
        var rangeType = RangeNodeVisitor(loop.Range, context);
        context.add(ident, rangeType!);
        var bodyType = BodyVisitor(loop.Body, context);
        return null;
    }

    public String? WhileLoopVisitor(WhileLoopNode loop, Context context)
    {
        return null;
        
    }

    public string? RoutineCallVisitor(RoutineCallNode call, Context context)
    {
        var paramList = new List<String>();
        var ident = IdentifierNodeVisitor(call.RoutineIdentifier, context);
        foreach (var expr in call.Arguments)
        {
            paramList.Add(ExpressionNodeVisitor(expr, context));
        }

        if (String.Join(" ", paramList) == context.getParameters(ident)) return null;
        
        context.addError($"Incorrect routine call {ident}");
        throw new Exception();
    }

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
        var lhs = ExpressionNodeVisitor(expr.Lhs, context);

        String? operator_ = OperatorNodeVisitor(expr.Operator, context);

        var rhs = ExpressionNodeVisitor(expr.Rhs, context);

        if (rhs == null && operator_ == null)
        {
            return lhs;
        }

        if ((lhs! == rhs!) && operator_ is "+" or "-")
        {
            return lhs!;
        }
        context.addError($"Incorrect type at factor node");
        return null;
    }

    public String? ModifiablePrimaryNodeVisitor(ModifiablePrimaryNode expr, Context context)
    {
        String? type = null;
        String? operator_ = null;

        if (expr.Prev == null)
        {
            type = IdentifierNodeVisitor(expr.Identifier!, context);
            if (type == null)
            {
                context.addError($"Variable doesnt exist at {expr.Identifier!.Token.Span!.StartLine} {expr.Identifier.Token.Span!.StartColumn} {expr.Identifier.Token.Span!.EndLine} {expr.Identifier.Token.Span!.EndColumn}");
                return null;
            }
            return context.get(type);
        }

        type = ModifiablePrimaryNodeVisitor(expr.Prev, context);
        if (type?.Split(" ")[0] == "array")
        {
            var temp = IdentifierNodeVisitor(expr.Identifier!, context);
            if (temp == "Length") return type.Split(" ")[1];
        }
        
        return null;
    }

    public String? RangeNodeVisitor(RangeNode node, Context context)
    {
        var from = ExpressionNodeVisitor(node.From, context);
        var to = ExpressionNodeVisitor(node.To, context);

        if (from == to)
        {
            return from;
        }
        
        context.addError($"Incorrect loop range: types are not same");
        throw new Exception();
    }

    public String? ExpressionNodeVisitor(ExpressionNode expressionNode, Context context)
    {
        switch (expressionNode)
        {
            case PrimaryNode node:
                return PrimaryNodeVisitor(node, context);
            case FactorNode node:
                return FactorNodeVisitor(node, context);
            case RelationNode node:
                return RelationNodeVisitor(node, context);
            case SimpleNode node:
                return SimpleNodeVisitor(node, context);
            default:
            {
                var lhs = ExpressionNodeVisitor(expressionNode.Lhs, context);
                var rhs = ExpressionNodeVisitor(expressionNode.Rhs, context);
                var operator_ = OperatorNodeVisitor(expressionNode.Operator, context);
                
                if (rhs != "boolean" || lhs != "boolean") return null;
                return operator_ is "and" or "xor" or "or" ? "boolean" : null;
                
            }
        }
    }

    public String? PrimaryNodeVisitor(PrimaryNode primaryNode, Context context)
    {
        return primaryNode switch
        {
            ModifiablePrimaryNode node => ModifiablePrimaryNodeVisitor(node, context),
            LiteralNode node => LiteralNodeVisitor(node, context),
            IdentifierNode node => IdentifierNodeVisitor(node, context),
            _ => null
        };
    }

    public String? IdentifierNodeVisitor(IdentifierNode identifierNode, Context context)
    {
        var ident = identifierNode.Token as IdentifierTk;
        return ident?.value;
    }


    public void TypeDeclarationNodeVisitor(TypeDeclarationNode typeDeclarationNode, Context context)
    {
        
    }
    
    public void DeclarationNodeVisitor(DeclarationNode declarationNode, Context context)
    {
        switch (declarationNode)
        {
            case SimpleDeclarationNode node:
                SimpleDeclarationNodeVisitor(node, context);
                return;
            case RoutineDeclarationNode node:
                RoutineDeclarationNodeVisitor(node, context);
                return;
        }
        
    }


    public void SimpleDeclarationNodeVisitor(SimpleDeclarationNode simpleDeclarationNode, Context context)
    {
        switch (simpleDeclarationNode)
        {
            case VariableDeclarationNode node:
                 VariableDeclVisitor(node, context);
                 return;
            case TypeDeclarationNode node:
                 TypeDeclarationNodeVisitor(node, context);
                 return;
        }
    }

    public String? StatementNodeVisitor(IStatementNode statementNode, Context context)
    {
        var child_context = new Context(context);

        switch (statementNode)
        {
            case AssignmentNode node:
                return AssignmentNodeVisitor(node, child_context);
            case RoutineCallNode node:
                return RoutineCallVisitor(node, context);
            case WhileLoopNode node:
                return WhileLoopVisitor(node, child_context);
            case ForLoopNode node:
                return ForLoopVisitor(node, child_context);
            case IfNode node:
                return IfNodeVisitor(node, child_context);
            case ReturnNode node:
                return ReturnNodeVisitor(node, child_context);
        }
        
        return null;
    }

    public String? TypeNodeVisitor(TypeNode typeNode, Context context)
    {
        switch (typeNode)
        {
            case ArrayTypeNode node:
                return ArrayTypeNodeVisitor(node, context);
            case RecordTypeNode node:
                return RecordTypeNodeVisitor(node, context);
            default:
                return typeNode.Kind.GetDescription();
        }
    }

    public String? ArrayTypeNodeVisitor(ArrayTypeNode arrayTypeNode, Context context)
    {
        var elementsType = TypeNodeVisitor(arrayTypeNode.ElementsType, context);
        var kind = arrayTypeNode.Kind.GetDescription();
        return $"{kind} {elementsType}";
    }

    public String? RecordTypeNodeVisitor(RecordTypeNode recordTypeNode, Context context)
    {
        return null;
    }

    public String? RelationNodeVisitor(RelationNode node, Context context)
    {
        var lhs = ExpressionNodeVisitor(node.Lhs, context);
        var rhs = ExpressionNodeVisitor(node.Rhs, context);
        var operator_ = OperatorNodeVisitor(node.Operator, context);

        if (rhs == null && operator_ == null)
        {
            return lhs;
        }

        if ((rhs != "integer" && rhs != "real") || (lhs != "integer" && lhs != "real")) return null;
        return operator_ is ">" or ">=" or "<" or "<=" or "=" or "/=" ? "boolean" : null;
    }

    public String? SimpleNodeVisitor(SimpleNode node, Context context)
    {
        var lhs = ExpressionNodeVisitor(node.Lhs, context);
        var rhs = ExpressionNodeVisitor(node.Rhs, context);
        var operator_ = OperatorNodeVisitor(node.Operator, context);
        if ((rhs != "integer" && rhs != "real") || (lhs != "integer" && lhs != "real")) return null;
        return operator_ switch
        {
            "*" => "real",
            "/" => "real",
            "%" => "integer",
            _ => null
        };
    }

    public String? OperatorNodeVisitor(OperatorNode operatorNode, Context context)
    {
        return operatorNode.Kind.GetDescription().Split(" ")[0];
    }
    
}