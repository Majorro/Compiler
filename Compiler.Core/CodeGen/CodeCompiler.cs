using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Compiler.Core.CodeAnalysis.LexicalAnalysis.LexerTokens;
using Compiler.Core.CodeAnalysis.SyntaxAnalysis;
using Compiler.Core.CodeAnalysis.Typechecker;
using Compiler.Core.CodeAnalysis.Typechecker.VisitskellDirectory;
using Compiler.Core.Utils;

namespace Compiler.Core.CodeGen;

public partial class CodeCompiler
{
    protected static readonly Guid SGuid = new("87D4DBE1-1143-4FAD-AAB3-1001F92068E6");
    protected static readonly BlobContentId SContentId = new(SGuid, 0x04030201);

    protected static readonly Dictionary<string, Action<SignatureTypeEncoder>> TypeMapper = new()
    {
        ["integer"] = type => type.Int32(),
        ["real"] = type => type.Single(),
        ["boolean"] = type => type.Boolean()
    };

    public Action<SignatureTypeEncoder> RecordFieldTypeMap(string typeRecord, int fieldI)
    {
        // fieldI should be 1,2,3 ... 8
        if (fieldI < 1) throw new ArgumentException("field enumeration starts with 1");
        var typeArgs = typeRecord.Split(" ");
        return TypeMap(typeArgs[fieldI]);
    }

    public Action<SignatureTypeEncoder> TypeMap(string type)
    {
        if (TypeMapper.TryGetValue(type, out var action)) return action;
        if (type.StartsWith("record"))
        {
            action = typeEncoder =>
            {
                var typeArgs = type.Split(" ").Skip(1).ToArray();
                var res = typeEncoder.GenericInstantiation(
                    SystemValueTypeRefs[typeArgs.Length], typeArgs.Length, true);
                foreach (var typeArg in typeArgs)
                {
                    TypeMap(typeArg)(res.AddArgument());
                }
            };
            return action;
        }

        throw new ArgumentException($"Unknown type: {type}");
    }

    protected readonly MetadataBuilder Metadata = new();
    protected readonly BlobBuilder BB = new();
    protected readonly BlobBuilder CodeBuilder = new();
    protected readonly ControlFlowBuilder FlowBuilder = new();
    protected readonly BlobBuilder IlBuilder = new();
    protected readonly MethodBodyStreamEncoder MethodBodyStream;
    protected readonly InstructionEncoder IE;

    protected readonly Dictionary<string, Action> OperatorMapper;
    protected readonly Dictionary<string, MethodDefinitionHandle> NameToRoutineHandler = new();
    protected readonly Dictionary<Node, Context> NodeTypecheckContext;
    protected readonly Dictionary<Node, string?> NodeType;

    public readonly List<string> RoutineLocalVariables = new();
    // protected int ParameterIndex = 1;

    protected readonly string ProgramName;
    public string FileName => $"{ProgramName}.dll";
    protected readonly Parser Program;

    protected BlobHandle ParameterlessCtorBlobIndex;
    protected MemberReferenceHandle ObjectCtorMemberRef, ArgumentExceptionMemberRef;

    public CodeCompiler(string programName, Parser program, Visitskel typecheckVisitor)
    {
        ProgramName = programName;
        Program = program;
        NodeTypecheckContext = typecheckVisitor.NodeContext;
        NodeType = typecheckVisitor.NodeType;

        MethodBodyStream = new MethodBodyStreamEncoder(IlBuilder);
        IE = new InstructionEncoder(CodeBuilder, FlowBuilder);
        OperatorMapper = new Dictionary<string, Action>
        {
            ["-"] = () => IE.OpCode(ILOpCode.Sub),
            ["+"] = () => IE.OpCode(ILOpCode.Add),
            ["*"] = () => IE.OpCode(ILOpCode.Mul),
            ["/"] = () => IE.OpCode(ILOpCode.Div),
            ["%"] = () => IE.OpCode(ILOpCode.Rem),
            ["and"] = () => IE.OpCode(ILOpCode.And),
            ["or"] = () => IE.OpCode(ILOpCode.Or),
            ["xor"] = () => IE.OpCode(ILOpCode.Xor),
            [">"] = () => IE.OpCode(ILOpCode.Cgt),
            ["<"] = () => IE.OpCode(ILOpCode.Clt),
            ["="] = () => IE.OpCode(ILOpCode.Ceq),
            ["/="] = () =>
            {
                IE.OpCode(ILOpCode.Ceq);
                IE.LoadConstantI4(0);
                IE.OpCode(ILOpCode.Ceq);
            },
            [">="] = () =>
            {
                IE.OpCode(ILOpCode.Clt);
                IE.LoadConstantI4(0);
                IE.OpCode(ILOpCode.Ceq);
            },
            ["<="] = () =>
            {
                IE.OpCode(ILOpCode.Cgt);
                IE.LoadConstantI4(0);
                IE.OpCode(ILOpCode.Ceq);
            }
        };

        InitMetadataModuleAsm();
        InitTypeRefs();
        InitMemberRef();
    }

    public void CompileToFile(string? path = null)
    {
        ProgramNode(Program.Tree.Root, new CodegenContext());
        WriteMetadataToFile(path: path ?? FileName);
    }

    public BlobBuilder CompileToBlob()
    {
        ProgramNode(Program.Tree.Root, new CodegenContext());
        return WriteMetadataToBlob();
    }

    protected void ProgramNode(ProgramNode program, CodegenContext context)
    {
        List<MethodDefinitionHandle> methods = new();
        foreach (var declarationNode in program.DeclarationList)
        {
            switch (declarationNode)
            {
                // case SimpleDeclarationNode node:
                //     SimpleDeclarationNodeVisitor(node, context);
                //     break;
                case RoutineDeclarationNode node:
                    var method = RoutineDeclarationNodeVisitor(node, context);
                    methods.Add(method);
                    break;
            }
        }

        FinalizeProgram(methods);
    }

    protected MethodDefinitionHandle RoutineDeclarationNodeVisitor(
        RoutineDeclarationNode node,
        CodegenContext context)
    {
        var typecheckContext = NodeTypecheckContext[node];
        var childContext = new CodegenContext(context);

        var parameterTypeNames = new List<string>();
        var firstParameterHandle = MetadataTokens.ParameterHandle(1);

        for (var index = 0; index < node.Parameters.Count; index++)
        {
            var param = node.Parameters[index];
            var name = param.Identifier.Name;

            // var parameterHandle = Metadata.AddParameter(
            //     ParameterAttributes.None,
            //     Metadata.GetOrAddString(name),
            //     ParameterIndex);
            // ParameterIndex++;
            // if (index == 0) firstParameterHandle = parameterHandle;

            var indexNew = index;
            childContext.AddVariable(name,
                () => IE.LoadArgument(indexNew),
                () => IE.StoreArgument(indexNew)
            );
            childContext.AddLocalVariableIndexes(name, index);

            var temp = typecheckContext.Get(name);
            if (temp != null) parameterTypeNames.Add(temp);
        }

        var returnTypeName = node.ReturnType == null ? null : NodeType[node.ReturnType];

        var routineSig = new BlobBuilder();

        new BlobEncoder(routineSig).MethodSignature().Parameters(
            parameterTypeNames.Count,
            returnType =>
            {
                if (returnTypeName == null) returnType.Void();
                // else if (returnTypeName.StartsWith("record")) 
                else TypeMap(returnTypeName)(returnType.Type());
            },
            parameters =>
            {
                foreach (var typeName in parameterTypeNames)
                {
                    TypeMap(typeName)(parameters.AddParameter().Type());
                }
            });

        RoutineLocalVariables.Clear();
        BodyVisitor(node.Body, childContext, true);

        var localVariablesSignature = Metadata.AddStandaloneSignature(EncodeBlob(e =>
        {
            var localsEncoder = e.LocalVariableSignature(RoutineLocalVariables.Count);
            foreach (var local in RoutineLocalVariables)
            {
                var typeEncoder = localsEncoder.AddVariable().Type();
                TypeMap(
                    typecheckContext.Get(local, fromChildren: true) ??
                    throw new InvalidOperationException($"{local} has undefined type")
                )(typeEncoder);
            }
        }));

        var routineBodyOffset = MethodBodyStream.AddMethodBody(
            instructionEncoder: IE,
            localVariablesSignature: localVariablesSignature);

        var routineName = node.Identifier.Name;
        var routineMethodDef = Metadata.AddMethodDefinition(
            MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig,
            MethodImplAttributes.IL,
            Metadata.GetOrAddString(routineName),
            Metadata.GetOrAddBlob(routineSig),
            routineBodyOffset,
            firstParameterHandle);

        BB.Clear();
        CodeBuilder.Clear();
        FlowBuilder.Clear();

        NameToRoutineHandler[routineName] = routineMethodDef;

        return routineMethodDef;
    }

    protected void BodyVisitor(BodyNode body, CodegenContext context, bool reversedIteration = false)
    {
        var items = reversedIteration ? body.Items.ReverseList() : body.Items;

        foreach (var bodyStatement in items)
        {
            switch (bodyStatement)
            {
                case IStatementNode node:
                    StatementNodeVisitor(node, context);
                    break;
                case DeclarationNode node:
                    DeclarationNodeVisitor(node, context);
                    break;
            }
        }
    }

    protected void DeclarationNodeVisitor(DeclarationNode declarationNode, CodegenContext context)
    {
        var simpleDeclarationNode = (declarationNode as SimpleDeclarationNode)!;
        switch (simpleDeclarationNode)
        {
            case VariableDeclarationNode node:
                VariableDeclVisitor(node, context);
                return;
            // case TypeDeclarationNode node:
            //     TypeDeclarationNodeVisitor(node, context);
            //     return;
        }
    }

    protected void VariableDeclVisitor(VariableDeclarationNode variable, CodegenContext context)
    {
        var variableName = variable.Identifier.Name;
        ExpressionNodeVisitor(variable.Expression, context);
        StoreLocalVariable(variableName, context);
    }

    protected int StoreLocalVariable(
        string variableName,
        CodegenContext context,
        Action? loadAction = null,
        Action? storeAction = null
    )
    {
        if (RoutineLocalVariables.Contains(variableName))
            throw new ArgumentException($"variable {variableName} already exists");
        RoutineLocalVariables.Add(variableName);
        var index = RoutineLocalVariables.IndexOf(variableName);
        context.AddVariable(variableName,
            loadAction ?? (() => IE.LoadLocal(index)),
            storeAction ?? (() => IE.StoreLocal(index))
        );
        IE.StoreLocal(index);
        return index;
    }

    protected void StatementNodeVisitor(IStatementNode statementNode, CodegenContext context)
    {
        var childContext = new CodegenContext(context);

        switch (statementNode)
        {
            case AssignmentNode node:
                AssignmentNodeVisitor(node, childContext);
                break;
            case RoutineCallNode node:
                RoutineCallVisitor(node, childContext);
                break;
            case WhileLoopNode node:
                WhileLoopVisitor(node, childContext);
                break;
            case ForLoopNode node:
                ForLoopVisitor(node, childContext);
                break;
            case IfNode node:
                IfNodeVisitor(node, childContext);
                break;
            case ReturnNode node:
                ReturnNodeVisitor(node, childContext);
                break;
        }
    }

    protected void AssignmentNodeVisitor(AssignmentNode assigment, CodegenContext context)
    {
        if (assigment.Identifier.Prev is not null)
        {
            // If record, load address of record
            IE.LoadArgumentAddress(context.GetLocalVariableIndexes(assigment.Identifier.Prev.Identifier!.Name));
        }

        var typecheckContext = NodeTypecheckContext[assigment];
        ExpressionNodeVisitor(assigment.Expression, context);
        ModifiablePrimaryNodeVisitor(assigment.Identifier, context, load: false);
        var name = assigment.Identifier.Identifier!.Name;
        var expectedType = typecheckContext.Get(name);
        var actualType = NodeType[assigment.Expression];

        Cast(expectedType!, actualType!);
        if (assigment.Identifier.Prev is null)
        {
            context.StoreVariable(name);
            return;
        }

        var recordName = assigment.Identifier.Prev.Identifier!.Name;
        var fieldName = name;

        var recordType = typecheckContext.Get(recordName);
        var typeNames = recordType!.Split(" ").ToList();
        var fieldType = typecheckContext.Get(fieldName);
        if (fieldType is null) throw new ArgumentException($"No such field: {recordName}.{fieldName}");
        var fieldIndex = typeNames.IndexOf(fieldType);
        if (fieldIndex == -1) throw new ArgumentException($"No such field: {recordName}.{fieldName}");
        IE.OpCode(ILOpCode.Stfld);
        IE.Token(GetRecordField(
            recordType,
            fieldIndex));
    }

    protected void RoutineCallVisitor(RoutineCallNode node, CodegenContext context)
    {
        var routineName = node.RoutineIdentifier.Name;
        foreach (var arg in node.Arguments)
        {
            ExpressionNodeVisitor(arg, context);
        }

        IE.Call(NameToRoutineHandler[routineName]);
    }

    protected void WhileLoopVisitor(WhileLoopNode loop, CodegenContext context)
    {
        var endWhileLabel = IE.DefineLabel();
        var startWhileBlockLabel = IE.DefineLabel();

        IE.MarkLabel(startWhileBlockLabel);
        ExpressionNodeVisitor(loop.Condition, context);
        IE.Branch(ILOpCode.Brfalse_s, endWhileLabel); // If false, jump over the body

        BodyVisitor(loop.Body, context, reversedIteration: true); // If true, execute while body
        IE.Branch(ILOpCode.Br_s, startWhileBlockLabel); // Jump back and check condition again

        IE.MarkLabel(endWhileLabel);
    }

    protected void ForLoopVisitor(ForLoopNode loop, CodegenContext context)
    {
        var iName = loop.VariableIdentifier.Name;

        var conditionCheckLabel = IE.DefineLabel();
        var bodyLabel = IE.DefineLabel();

        void StoreAction()
        {
            throw new TargetException(
                "for loop variable should not be assigned to"
            );
        }

        ExpressionNodeVisitor(loop.Range.From, context);
        var iIndex = StoreLocalVariable(iName, context, storeAction: StoreAction);
        IE.Branch(ILOpCode.Br_s, conditionCheckLabel);

        IE.MarkLabel(bodyLabel);
        BodyVisitor(loop.Body, context, reversedIteration: true);
        IE.LoadLocal(iIndex);
        IE.LoadConstantI4(1);
        IE.OpCode(ILOpCode.Add);
        IE.StoreLocal(iIndex);

        IE.MarkLabel(conditionCheckLabel);
        IE.LoadLocal(iIndex);
        IE.LoadConstantI4(1);
        ExpressionNodeVisitor(loop.Range.To, context);
        IE.OpCode(ILOpCode.Add);
        IE.Branch(ILOpCode.Blt_s, bodyLabel);
    }

    protected void IfNodeVisitor(IfNode ifStatement, CodegenContext context)
    {
        var childContext = new CodegenContext(context);

        var elseLabel = IE.DefineLabel();
        var endLabel = IE.DefineLabel();

        ExpressionNodeVisitor(ifStatement.Condition, context);
        IE.Branch(ILOpCode.Brfalse_s, elseLabel); // If false jump to else block

        BodyVisitor(ifStatement.ThenBody, childContext); // If true, execute if block
        if (ifStatement.ElseBody != null) IE.Branch(ILOpCode.Br_s, endLabel); // Jump over else block

        IE.MarkLabel(elseLabel);
        if (ifStatement.ElseBody != null) BodyVisitor(ifStatement.ElseBody, childContext);
        if (ifStatement.ElseBody != null) IE.MarkLabel(endLabel);
    }

    protected void ExpressionNodeVisitor(ExpressionNode expressionNode, CodegenContext context)
    {
        switch (expressionNode)
        {
            // case RelationNode node:
            //     RelationNodeVisitor(node, context);
            //     break;
            // case SimpleNode node:
            //     SimpleNodeVisitor(node, context);
            //     break;
            // case FactorNode node:
            //     FactorNodeVisitor(node, context);
            //     break;
            case RoutineCallNode node:
                RoutineCallVisitor(node, context);
                break;
            case PrimaryNode node:
                PrimaryNodeVisitor(node, context);
                break;
            default:
                ExpressionNodeVisitor(expressionNode.Lhs!, context);
                ExpressionNodeVisitor(expressionNode.Rhs!, context);
                OperatorNodeVisitor(expressionNode.Operator!, context);
                break;
        }

        // var lhs = ExpressionNodeVisitor(expressionNode.Lhs, context);
        // var rhs = ExpressionNodeVisitor(expressionNode.Rhs, context);
        // var operator_ = OperatorNodeVisitor(expressionNode.Operator, context);
        //
        // if (rhs != "boolean" || lhs != "boolean") return null;
        // return operator_ is "and" or "xor" or "or" ? "boolean" : null;
    }

    protected void OperatorNodeVisitor(OperatorNode node, CodegenContext context)
    {
        var type = NodeType[node]!;
        OperatorMapper.TryGetValue(type, out var applyOperator);
        if (applyOperator == null) throw new ArithmeticException($"unknown operator: {type}");
        applyOperator();
    }

    // protected void RelationNodeVisitor(RelationNode node, CodegenContext context)
    // {
    //     ExpressionNodeVisitor(node.Lhs!, context);
    //     ExpressionNodeVisitor(node.Rhs!, context);
    //     OperatorNodeVisitor(node.Operator!, context);
    // }
    //
    // protected void SimpleNodeVisitor(SimpleNode node, CodegenContext context)
    // {
    //     ExpressionNodeVisitor(node.Lhs!, context);
    //     ExpressionNodeVisitor(node.Rhs!, context);
    //     OperatorNodeVisitor(node.Operator!, context);
    // }
    //
    //
    // protected void FactorNodeVisitor(FactorNode node, CodegenContext context)
    // {
    //     ExpressionNodeVisitor(node.Lhs!, context);
    //     ExpressionNodeVisitor(node.Rhs!, context);
    //     OperatorNodeVisitor(node.Operator!, context);
    // }

    protected void PrimaryNodeVisitor(PrimaryNode primaryNode, CodegenContext context)
    {
        switch (primaryNode)
        {
            case ModifiablePrimaryNode node:
                ModifiablePrimaryNodeVisitor(node, context);
                break;
            case LiteralNode node:
                LiteralNodeVisitor(node, context);
                break;
            // IdentifierNode node => IdentifierNodeVisitor(node, context),
        }
    }

    protected void LiteralNodeVisitor(LiteralNode node, CodegenContext context)
    {
        var type = NodeType[node];
        switch (type)
        {
            case "integer":
                IE.LoadConstantI4((int)(node.Token as IntTk)!.Value);
                break;
            case "real":
                IE.LoadConstantR4((float)(node.Token as RealTk)!.Value);
                break;
            case "boolean":
                IE.LoadConstantI4((node.Token as BoolTk)!.Value ? 1 : 0);
                break;
            default:
                throw new ArgumentOutOfRangeException($"Unknown type {type}");
        }
    }

    protected void ModifiablePrimaryNodeVisitor(ModifiablePrimaryNode expr, CodegenContext context, bool load = true)
    {
        if (load) context.LoadVariable(expr.Identifier!.Name);
    }

    protected void ReturnNodeVisitor(ReturnNode node, CodegenContext context)
    {
        var typecheckContext = NodeTypecheckContext[node];
        var currRoutineName = Context.RoutineNames.Last();

        ExpressionNodeVisitor(node.Expression!, context);
        var actualType = NodeType[node];
        var expectedType = typecheckContext.GetRoutine(currRoutineName)!.Value.ReturnType;


        Cast(expectedType, actualType);
        IE.OpCode(ILOpCode.Ret);
    }

    protected void Cast(string? expectedType, string? actualType)
    {
        var typesTuple = (expectedType, actualType);

        if (typesTuple is ("integer", "real"))
        {
            // Rounding
            IE.LoadConstantR4(0.5f);
            IE.OpCode(ILOpCode.Add);
            IE.OpCode(ILOpCode.Conv_i4);
        }
        else if (typesTuple is ("integer", "boolean"))
            ;
        else if (typesTuple is ("real", "integer"))
            IE.OpCode(ILOpCode.Conv_r4);
        else if (typesTuple is ("real", "boolean"))
            IE.OpCode(ILOpCode.Conv_r4);
        else if (typesTuple is ("boolean", "integer"))
        {
            var okLabel = IE.DefineLabel();

            IE.OpCode(ILOpCode.Dup);
            IE.LoadConstantI4(0);
            IE.Branch(ILOpCode.Beq, okLabel);
            IE.OpCode(ILOpCode.Dup);
            IE.LoadConstantI4(1);
            IE.Branch(ILOpCode.Beq, okLabel);

            IE.LoadString(Metadata.GetOrAddUserString(
                "error while converting integer to boolean. must be 0 or 1"));
            // newobj instance void [System.Runtime]System.ArgumentException::.ctor(string)
            IE.OpCode(ILOpCode.Newobj);
            IE.Token(ArgumentExceptionMemberRef);
            IE.OpCode(ILOpCode.Throw);

            IE.MarkLabel(okLabel);
        }
        else if (typesTuple is ("boolean", "real"))
            throw new ArgumentException("cannot convert real to boolean");
    }
}