using Compiler.Core.CodeAnalysis.LexicalAnalysis.LexerTokens;
using Compiler.Core.CodeAnalysis.SyntaxAnalysis;
using Compiler.Core.Utils;

namespace Compiler.Core.CodeAnalysis.Typechecker.VisitskellDirectory;

public class Visitskel
{
    public readonly Dictionary<Node, Context> NodeContext = new();
    public readonly Dictionary<Node, string?> NodeType = new();

    public void ProgramVisitor(ProgramNode program, Context context)
    {
        NodeContext[program] = context;
        foreach (var routine in program.DeclarationList)
        {
            DeclarationNodeVisitor(routine, context);
        }
    }

    public object RoutineDeclarationNodeVisitor(RoutineDeclarationNode decl, Context context)
    {
        var childContext = new Context(context);
        NodeContext[decl] = childContext;

        // Routine name
        var ident = IdentifierNodeVisitor(decl.Identifier, childContext);

        var parameters = new List<string>();
        // Parameters
        foreach (var param in decl.Parameters)
        {
            var temp = ParameterDeclarationNodeVisitor(param, childContext);
            if (temp != null)
                parameters.Add(temp);
        }

        // Return type
        string? returnType = null;
        if (decl.ReturnType != null)
        {
            returnType = TypeNodeVisitor(decl.ReturnType, childContext);
            NodeType[decl.ReturnType] = returnType;
        }
        
        context.AddRoutine(ident, string.Join(" ", parameters), returnType);

        // Body
        var bodyType = BodyVisitor(decl.Body, childContext, true);
        if (bodyType != returnType)
        {
            context.AddError($"Return type not same as declared in routine {ident}");
            throw new Exception();
        }

        return null;
    }

    public string? ParameterDeclarationNodeVisitor(ParameterDeclarationNode param, Context context)
    {
        var name = param.Identifier.Token as IdentifierTk;
        var type = TypeNodeVisitor(param.Type, context);
        context.Add(name.Value, type);
        return type;
    }

    public string? BodyVisitor(BodyNode body, Context context, bool reversedIteration = false)
    {
        var results = new List<string>();
        var items = reversedIteration ? body.Items.ReverseList().ToList() : body.Items;
        foreach (var bodyStatement in items)
        {
            switch (bodyStatement)
            {
                case IStatementNode node:
                    var result = StatementNodeVisitor((Node)node, context);
                    if (result != null) results.Add(result);
                    break;
                case DeclarationNode node:
                    DeclarationNodeVisitor(node, context);
                    break;
            }
        }

        return items.Last() is ReturnNode ? results.Last() : null;
    }

    public string? ReturnNodeVisitor(ReturnNode returnStatement, Context context)
    {
        NodeContext[returnStatement] = context;
        var type = ExpressionNodeVisitor(returnStatement.Expression!, context);
        NodeType[returnStatement] = type;
        return type;
    }

    public string? VariableDeclVisitor(VariableDeclarationNode variable, Context context)
    {
        var varName = variable.Identifier.Name;

        var type = variable.Type?.Kind.GetDescription() ?? ExpressionNodeVisitor(variable.Expression, context);
        if (type is null)
        {
            context.AddError($"variable {varName} has no type");
            return null;
        }
        context.Add(varName, type);
        return type;
    }

    public string? AssignmentNodeVisitor(AssignmentNode assigment, Context context)
    {
        var type = ExpressionNodeVisitor(assigment.Expression, context);
        var mainType = ModifiablePrimaryNodeVisitor(assigment.Identifier, context);

        if (mainType == type) return null;
        if (type != null && (type == "integer" || type == "boolean" || type == "real")) return null;
        context.AddError("Type of expression is not convertable!");
        throw new Exception();
    }

    public string? IfNodeVisitor(IfNode ifStatement, Context context)
    {
        var childContext = new Context(context);
        NodeContext[ifStatement] = childContext;

        // Add handling condition;

        var condition = ExpressionNodeVisitor(ifStatement.Condition, context);
        if (condition != "boolean")
        {
            context.AddError("Condition is not boolean!");
            throw new Exception();
        }

        var returnType = BodyVisitor(ifStatement.ThenBody, childContext, true);

        if (ifStatement.ElseBody == null) return returnType;
        var elseType = BodyVisitor(ifStatement.ElseBody, childContext);

        if (returnType != null && elseType != null)
        {
            if (returnType == elseType) return returnType;
            context.AddError("Return types at ifloop not same!");
            throw new Exception();
        }
        if (returnType == null && elseType != null)
        {
            return elseType;
        }
        if (returnType != null && elseType == null)
        {
            return returnType;
        }

        return null;
    }

    public string? ForLoopVisitor(ForLoopNode loop, Context context)
    {
        //Add handling Range and identifier
        var ident = loop.VariableIdentifier.Name;
        var rangeType = RangeNodeVisitor(loop.Range, context);
        context.Add(ident, rangeType!);
        var bodyType = BodyVisitor(loop.Body, context);
        return null;
    }

    public string? WhileLoopVisitor(WhileLoopNode loop, Context context)
    {
        var conditionType = ExpressionNodeVisitor(loop.Condition, context);
        if (conditionType != "boolean")
            context.AddError($"While condition expected to be boolean, " +
                             $"found {conditionType} at {loop.Condition}");
        return BodyVisitor(loop.Body, context);
    }

    public string? RoutineCallVisitor(RoutineCallNode call, Context context)
    {
        var paramList = new List<string>();
        var ident = IdentifierNodeVisitor(call.RoutineIdentifier, context);
        foreach (var expr in call.Arguments)
        {
            paramList.Add(ExpressionNodeVisitor(expr, context));
        }

        var (paramsType, returnType) = context.GetRoutine(ident)!.Value;
        if (string.Join(" ", paramList) == paramsType) return returnType;
        context.AddError($"Incorrect routine call {ident}");
        throw new Exception();
    }

    public string? LiteralNodeVisitor(LiteralNode expr, Context context)
    {
        switch (expr.Kind)
        {
            case null:
                context.AddError($"Type is null at {expr.Token.Span!.StartColumn} " +
                                 $"{expr.Token.Span.StartLine} end at {expr.Token.Span.EndColumn} " +
                                 $"{expr.Token.Span.EndLine}");
                break;
        }

        var type = expr.Kind?.GetDescription() ?? throw new InvalidOperationException();
        NodeType[expr] = type;
        return type;
    }

    public string? FactorNodeVisitor(FactorNode expr, Context context)
    {
        string? type = null;
        var lhs = ExpressionNodeVisitor(expr.Lhs, context);
        var rhs = ExpressionNodeVisitor(expr.Rhs, context);
        var @operator = OperatorNodeVisitor(expr.Operator, context);

        if (rhs == null && @operator == null)
        {
            return lhs;
        }

        if (lhs! == rhs! && @operator is "+" or "-")
        {
            return lhs!;
        }

        context.AddError("Incorrect type at factor node");
        return null;
    }

    public string? ModifiablePrimaryNodeVisitor(ModifiablePrimaryNode expr, Context context)
    {
        string? type = null;
        string? @operator = null;

        if (expr.Prev == null)
        {
            type = IdentifierNodeVisitor(expr.Identifier!, context);
            if (type == null)
            {
                context.AddError(
                    $"Variable doesnt exist at {expr.Identifier!.Token.Span!.StartLine} {expr.Identifier.Token.Span!.StartColumn} {expr.Identifier.Token.Span!.EndLine} {expr.Identifier.Token.Span!.EndColumn}");
                return null;
            }

            return context.Get(type);
        }

        type = ModifiablePrimaryNodeVisitor(expr.Prev, context);
        if (type?.Split(" ")[0] == "array")
        {
            var temp = IdentifierNodeVisitor(expr.Identifier!, context);
            if (temp == "Length") return type.Split(" ")[1];
        }

        return null;
    }

    public string? RangeNodeVisitor(RangeNode node, Context context)
    {
        var from = ExpressionNodeVisitor(node.From, context);
        var to = ExpressionNodeVisitor(node.To, context);

        if (from == to)
        {
            NodeType[node] = from;
            return from;
        }

        context.AddError("Incorrect loop range: types are not same");
        throw new Exception();
    }

    public string? ExpressionNodeVisitor(ExpressionNode expressionNode, Context context)
    {
        var type = expressionNode switch
        {
            PrimaryNode node => PrimaryNodeVisitor(node, context),
            FactorNode node => FactorNodeVisitor(node, context),
            RelationNode node => RelationNodeVisitor(node, context),
            SimpleNode node => SimpleNodeVisitor(node, context),
            _ => null
        };
        if (type is null)
        {
            var lhs = ExpressionNodeVisitor(expressionNode.Lhs, context);
            var rhs = ExpressionNodeVisitor(expressionNode.Rhs, context);
            var @operator = OperatorNodeVisitor(expressionNode.Operator, context);

            if (rhs != "boolean" || lhs != "boolean") return null;
            type = @operator is "and" or "xor" or "or" ? "boolean" : null;
        }

        NodeType[expressionNode] = type;
        return type;
    }

    public string? PrimaryNodeVisitor(PrimaryNode primaryNode, Context context)
    {
        return primaryNode switch
        {
            ModifiablePrimaryNode node => ModifiablePrimaryNodeVisitor(node, context),
            LiteralNode node => LiteralNodeVisitor(node, context),
            IdentifierNode node => IdentifierNodeVisitor(node, context),
            RoutineCallNode node => RoutineCallVisitor(node, context),
            _ => null
        };
    }

    public string IdentifierNodeVisitor(IdentifierNode identifierNode, Context context) =>
        identifierNode.Name;


    public void TypeDeclarationNodeVisitor(TypeDeclarationNode typeDeclarationNode, Context context)
    { }
    
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

    public string? StatementNodeVisitor(Node statementNode, Context context)
    {
        var childContext = new Context(context);
        NodeContext[statementNode] = childContext;

        var type = statementNode switch
        {
            AssignmentNode node => AssignmentNodeVisitor(node, childContext),
            RoutineCallNode node => RoutineCallVisitor(node, context),
            WhileLoopNode node => WhileLoopVisitor(node, childContext),
            ForLoopNode node => ForLoopVisitor(node, childContext),
            IfNode node => IfNodeVisitor(node, childContext),
            ReturnNode node => ReturnNodeVisitor(node, childContext),
            _ => null
        };
        NodeType[statementNode] = type;
        return type;
    }

    public string? TypeNodeVisitor(TypeNode typeNode, Context context)
    {
        var type = typeNode switch
        {
            ArrayTypeNode node => ArrayTypeNodeVisitor(node, context),
            RecordTypeNode node => RecordTypeNodeVisitor(node, context),
            _ => typeNode.Kind.GetDescription()
        };
        NodeType[typeNode] = type;
        return type;
    }

    public string? ArrayTypeNodeVisitor(ArrayTypeNode arrayTypeNode, Context context)
    {
        var elementsType = TypeNodeVisitor(arrayTypeNode.ElementsType, context);
        var kind = arrayTypeNode.Kind.GetDescription();
        return $"{kind} {elementsType}";
    }

    public string? RecordTypeNodeVisitor(RecordTypeNode recordTypeNode, Context context)
    {
        var childContext = new Context(context);
        NodeContext[recordTypeNode] = childContext;

        var type = "record ";
        foreach (var recordVarDeclNode in recordTypeNode.Members)
        {
            var varType = VariableDeclVisitor(recordVarDeclNode, childContext);
            type += $"{varType} ";
        }

        type = type.TrimEnd();

        NodeType[recordTypeNode] = type;
        return type;
    }

    public string? RelationNodeVisitor(RelationNode node, Context context)
    {
        var lhs = ExpressionNodeVisitor(node.Lhs, context);
        var rhs = ExpressionNodeVisitor(node.Rhs, context);
        var @operator = OperatorNodeVisitor(node.Operator, context);

        if (rhs == null && @operator == null)
        {
            return lhs;
        }

        if (rhs != "integer" && rhs != "real" || lhs != "integer" && lhs != "real") return null;
        return @operator is ">" or ">=" or "<" or "<=" or "=" or "/=" ? "boolean" : null;
    }

    public string? SimpleNodeVisitor(SimpleNode node, Context context)
    {
        var lhs = ExpressionNodeVisitor(node.Lhs, context);
        var rhs = ExpressionNodeVisitor(node.Rhs, context);
        var @operator = OperatorNodeVisitor(node.Operator, context);
        var typesList = new List<String>() { "integer", "real" };
        if (!typesList.Contains(lhs) || !typesList.Contains(rhs)) return null;

        if (@operator == "*" || @operator == "/")
            return lhs == "real" || rhs == "real" ? "real" : "integer";
        if (@operator == "%")
        {
            if (lhs != "integer" || rhs != "integer")
            {
                context.AddError("% accepts only integers");
                return null;
            }

            return "integer";
        }

        return null;
    }

    public string? OperatorNodeVisitor(OperatorNode operatorNode, Context context)
    {
        var type = operatorNode.Kind.GetDescription().Split(" ")[0];
        NodeType[operatorNode] = type;
        return type;
    }
}