%namespace Compiler.Core.CodeAnalysis.SyntaxAnalysis
%output=Parser.cs
%partial
%start Program

%YYSTYPE Node

%token TkUnknown
%token TkIdentifier

/* Types */
%token TkBool
%token TkInt
%token TkReal

/* Keywords */
%token TkType
%token TkIs
%token TkEnd
%token TkReturn
%token TkVar
%token TkRoutine
%token TkFor
%token TkWhile
%token TkLoop
%token TkIn
%token TkReverse
%token TkIf
%token TkThen
%token TkElse
%token TkArray
%token TkRecord

/* Punctuators */
%token TkRoundOpen
%token TkRoundClose
%token TkCurlyOpen
%token TkCurlyClose
%token TkSquareOpen
%token TkSquareClose
%token TkSemicolon
%token TkColon
%token TkComma

/* Operators */
%token TkAssign
%token TkDot
%token TkMinus
%token TkPlus
%token TkMultiply
%token TkDivide
%token TkPercent
%token TkAnd
%token TkOr
%token TkXor
%token TkRange

/* Comparators */
%token TkLeq
%token TkGeq
%token TkLess
%token TkGreater
%token TkEqual
%token TkNotEqual

/* Literals */
%token TkIntLiteral
%token TkRealLiteral
%token TkBoolLiteral


%left TkOr
%left TkXor
%left TkAnd
%left TkEqual TkNotEqual
%left TkLess TkLeq TkGreater TkGeq
%left TkPlus TkMinus
%left TkMultiply TkDivide TkPercent

%nonassoc TkIs TkColon TkAssign TkDot TkSquareOpen TkRoundOpen TkRange TkSemicolon TkComma

%%

/* ProgramNode(ListNode<DeclarationNode> declarationList) */
Program
: /* empty */ { var node = new ProgramNode(); $$ = node; SaveTree(node); }
| DeclarationList { var node = new ProgramNode((ListNode<DeclarationNode>)$1); $$ = node; SaveTree(node); }
;

/* ListNode<T>(T parameterDeclaration, ListNode<T>? parameterDeclarations) */
DeclarationList
: Declaration DeclarationList { $$ = new ListNode<DeclarationNode>((DeclarationNode)$1, (ListNode<DeclarationNode>?)$2); }
| Declaration { $$ = new ListNode<DeclarationNode>((DeclarationNode)$1, null); }
;

Declaration
: SimpleDeclaration
| RoutineDeclaration TkSemicolon
;

SimpleDeclaration
: VariableDeclaration
| TypeDeclaration TkSemicolon
;

/* VariableDeclarationNode(IdentifierNode identifier, TypeNode type, ExpressionNode expression) */
VariableDeclaration
: TkVar Identifier TkColon Type TkIs Expression TkSemicolon { $$ = new VariableDeclarationNode((IdentifierNode)$2, (TypeNode)$4, (ExpressionNode)$6); }
| TkVar Identifier TkColon Type TkSemicolon { $$ = new VariableDeclarationNode((IdentifierNode)$2, (TypeNode)$4, null); }
| TkVar Identifier TkIs Expression TkSemicolon { $$ = new VariableDeclarationNode((IdentifierNode)$2, null, (ExpressionNode)$4); }
;

VariableDeclarations
: VariableDeclaration VariableDeclarations { $$ = new ListNode<VariableDeclarationNode>((VariableDeclarationNode)$1, (ListNode<VariableDeclarationNode>?)$2); }
| VariableDeclaration { $$ = new ListNode<VariableDeclarationNode>((VariableDeclarationNode)$1, null); }
;

/* TypeDeclarationNode(IdentifierNode identifier, TypeNode type) */
TypeDeclaration
: TkType Identifier TkIs Type { $$ = new TypeDeclarationNode((IdentifierNode)$2, (TypeNode)$4); }
;

/* RoutineDeclarationNode(IdentifierNode identifier, TypeNode returnType, ListNode<ParameterDeclarationNode> parameters, BodyNode body) */
RoutineDeclaration
: TkRoutine Identifier TkRoundOpen Parameters TkRoundClose TkColon Type TkIs Body TkEnd
    {
        $$ = new RoutineDeclarationNode((IdentifierNode)$2, (TypeNode)$7, (ListNode<ParameterDeclarationNode>)$4, (BodyNode)$9);
    }
| TkRoutine Identifier TkRoundOpen Parameters TkRoundClose TkIs Body TkEnd
    {
        $$ = new RoutineDeclarationNode((IdentifierNode)$2, null, (ListNode<ParameterDeclarationNode>)$4, (BodyNode)$7);
    }
;

/* ListNode<T>(T parameterDeclaration, ListNode<T>? parameterDeclarations) */
Parameters
: ParameterDeclaration TkComma Parameters { $$ = new ListNode<ParameterDeclarationNode>((ParameterDeclarationNode)$1, (ListNode<ParameterDeclarationNode>?)$3); }
| ParameterDeclaration { $$ = new ListNode<ParameterDeclarationNode>((ParameterDeclarationNode)$1, null); }
| /* empty */ { $$ = new ListNode<ParameterDeclarationNode>(); }
;

/* ParameterDeclarationNode(IdentifierNode identifier, TypeNode type) */
ParameterDeclaration
: Identifier TkColon Type { $$ = new ParameterDeclarationNode((IdentifierNode)$1, (TypeNode)$3); }
;

/* TypeNode(TypeKind kind, IdentifierNode identifier) */
Type
: PrimitiveType
| RecordType
| ArrayType
| Identifier { $$ = new TypeNode(TypeKind.UserDefined, (IdentifierNode)$1); }
;

PrimitiveType
: TkInt { $$ = new TypeNode(TypeKind.Integer, new IdentifierNode(Lexer.ProgramTokenByLocation[@1])); }
| TkReal { $$ = new TypeNode(TypeKind.Real, new IdentifierNode(Lexer.ProgramTokenByLocation[@1])); }
| TkBool { $$ = new TypeNode(TypeKind.Boolean, new IdentifierNode(Lexer.ProgramTokenByLocation[@1])); }
;

/* RecordTypeNode(IdentifierNode identifier, ListNode<VariableDeclarationNode> member`s) */
RecordType
: TkRecord TkCurlyOpen VariableDeclarations TkCurlyClose TkEnd { $$ = new RecordTypeNode((IdentifierNode)$1, (ListNode<VariableDeclarationNode>)$3); }
;

/* ArrayTypeNode(IdentifierNode identifier, TypeNode elementsType, ExpressionNode sizeExpression) */
ArrayType
: TkArray TkSquareOpen Expression TkSquareClose Type { $$ = new ArrayTypeNode((IdentifierNode)$1, (TypeNode)$5, (ExpressionNode)$3); }
| TkArray TkSquareOpen TkSquareClose Type { $$ = new ArrayTypeNode((IdentifierNode)$1, (TypeNode)$4, null); }
;

/* BodyNode(SimpleDeclarationNode declaration, BodyNode? remaining) */
/* BodyNode(Node statement, BodyNode? remaining) */
Body
: /* empty */ { $$ = new BodyNode(); }
| Body SimpleDeclaration { $$ = new BodyNode((SimpleDeclarationNode)$2, (BodyNode)$1); }
| Body Statement TkSemicolon { $$ = new BodyNode($2, (BodyNode)$1); }
| SimpleDeclaration { $$ = new BodyNode((SimpleDeclarationNode)$1, null); }
| Statement TkSemicolon { $$ = new BodyNode($1, null); }
;

Statement
: Assignment
| RoutineCall
| WhileLoop
| ForLoop
| IfStatement
| ReturnStatement
;

/* AssignmentNode(ModifiablePrimaryNode identifier, ExpressionNode expression) */
Assignment
: ModifiablePrimary TkAssign Expression { $$ = new AssignmentNode((ModifiablePrimaryNode)$1, (ExpressionNode)$3); }
;

/* RoutineCallNode(IdentifierNode identifier, ListNode<ExpressionNode> arguments) */
RoutineCall
: Identifier TkRoundOpen ExpressionList TkRoundClose { $$ = new RoutineCallNode((IdentifierNode)$1, (ListNode<ExpressionNode>)$3); }
| Identifier TkRoundOpen TkRoundClose { $$ = new RoutineCallNode((IdentifierNode)$1, new ListNode<ExpressionNode>()); }
;

/* ListNode<T>(T item, ListNode<T>? items) */
ExpressionList
: ExpressionList TkComma Expression { $$ = new ListNode<ExpressionNode>((ExpressionNode)$3, (ListNode<ExpressionNode>)$1); }
| Expression { $$ = new ListNode<ExpressionNode>((ExpressionNode)$1, null); }
;

/* WhileLoopNode(ExpressionNode condition, BodyNode body) */
WhileLoop
: TkWhile Expression TkLoop Body TkEnd { $$ = new WhileLoopNode((ExpressionNode)$2, (BodyNode)$4); }
;

/* ForLoopNode(IdentifierNode identifier, RangeNode range, BodyNode body) */
ForLoop
: TkFor Identifier Range TkLoop Body TkEnd { $$ = new ForLoopNode((IdentifierNode)$2, (RangeNode)$3, (BodyNode)$5); }
;

/* RangeNode(ExpressionNode from, ExpressionNode to, bool reverse) */
Range
: TkIn TkReverse Expression TkRange Expression { $$ = new RangeNode((ExpressionNode)$3, (ExpressionNode)$5, true); }
| TkIn Expression TkRange Expression { $$ = new RangeNode((ExpressionNode)$2, (ExpressionNode)$4); }
;

/* IfNode(ExpressionNode condition, BodyNode thenBody, BodyNode? elseBody) */
IfStatement
: TkIf Expression TkThen Body TkElse Body TkEnd { $$ = new IfNode((ExpressionNode)$2, (BodyNode)$4, (BodyNode)$6); }
| TkIf Expression TkThen Body TkEnd { $$ = new IfNode((ExpressionNode)$2, (BodyNode)$4); }
;

/* ReturnNode(ExpressionNode? expression) */
ReturnStatement
: TkReturn Expression { $$ = new ReturnNode((ExpressionNode)$2); }
| TkReturn { $$ = new ReturnNode(); }
;

/* ExpressionNode(ExpressionNode lhs, OperatorNode op, ExpressionNode rhs) */
Expression
: Expression TkAnd Relation { $$ = new ExpressionNode((ExpressionNode)$1, new OperatorNode(Operator.And, Lexer.ProgramTokenByLocation[@2]), (ExpressionNode)$3); }
| Expression TkOr Relation { $$ = new ExpressionNode((ExpressionNode)$1, new OperatorNode(Operator.Or, Lexer.ProgramTokenByLocation[@2]), (ExpressionNode)$3); }
| Expression TkXor Relation { $$ = new ExpressionNode((ExpressionNode)$1, new OperatorNode(Operator.Xor, Lexer.ProgramTokenByLocation[@2]), (ExpressionNode)$3); }
| Relation { $$ = $1; }
;

/* RelationNode(ExpressionNode lhs, OperatorNode op, ExpressionNode rhs) */
Relation
: Simple TkLess Simple { $$ = new RelationNode((ExpressionNode)$1, new OperatorNode(Operator.Less, Lexer.ProgramTokenByLocation[@2]), (ExpressionNode)$3); }
| Simple TkLeq Simple { $$ = new RelationNode((ExpressionNode)$1, new OperatorNode(Operator.LessOrEqual, Lexer.ProgramTokenByLocation[@2]), (ExpressionNode)$3); }
| Simple TkGreater Simple { $$ = new RelationNode((ExpressionNode)$1, new OperatorNode(Operator.Greater, Lexer.ProgramTokenByLocation[@2]), (ExpressionNode)$3); }
| Simple TkGeq Simple { $$ = new RelationNode((ExpressionNode)$1, new OperatorNode(Operator.GreaterOrEqual, Lexer.ProgramTokenByLocation[@2]), (ExpressionNode)$3); }
| Simple TkEqual Simple { $$ = new RelationNode((ExpressionNode)$1, new OperatorNode(Operator.Equal, Lexer.ProgramTokenByLocation[@2]), (ExpressionNode)$3); }
| Simple TkNotEqual Simple { $$ = new RelationNode((ExpressionNode)$1, new OperatorNode(Operator.NotEqual, Lexer.ProgramTokenByLocation[@2]), (ExpressionNode)$3); }
| Simple { $$ = $1; }
;

/* SimpleNode(ExpressionNode lhs, OperatorNode op, ExpressionNode rhs) */
Simple
: Simple TkMultiply Factor { $$ = new SimpleNode((ExpressionNode)$1, new OperatorNode(Operator.Multiply, Lexer.ProgramTokenByLocation[@2]), (ExpressionNode)$3); }
| Simple TkDivide Factor { $$ = new SimpleNode((ExpressionNode)$1, new OperatorNode(Operator.Divide, Lexer.ProgramTokenByLocation[@2]), (ExpressionNode)$3); }
| Simple TkPercent Factor { $$ = new SimpleNode((ExpressionNode)$1, new OperatorNode(Operator.Modulo, Lexer.ProgramTokenByLocation[@2]), (ExpressionNode)$3); }
| Factor { $$ = $1; }
;

/* FactorNode(ExpressionNode lhs, OperatorNode op, ExpressionNode rhs) */
Factor
: Factor Sign Summand { $$ = new FactorNode((ExpressionNode)$1, (OperatorNode)$2, (ExpressionNode)$3); }
| Summand { $$ = $1; }
;

Summand
: Primary { $$ = $1; }
| TkRoundOpen Expression TkRoundClose { $$ = $2; }
;

Primary
: Unary
| TkIntLiteral { $$ = new LiteralNode(LiteralKind.Integer, Lexer.ProgramTokenByLocation[@1]); }
| TkRealLiteral { $$ = new LiteralNode(LiteralKind.Real, Lexer.ProgramTokenByLocation[@1]); }
| TkBoolLiteral { $$ = new LiteralNode(LiteralKind.Boolean, Lexer.ProgramTokenByLocation[@1]); }
| RoutineCall
| ModifiablePrimary
;

Sign
: TkPlus { $$ = new OperatorNode(Operator.Plus, Lexer.ProgramTokenByLocation[@1]); }
| TkMinus { $$ = new OperatorNode(Operator.Minus, Lexer.ProgramTokenByLocation[@1]); }
;

Unary
: Sign TkIntLiteral { $$ = new UnaryNode((OperatorNode)$1, new LiteralNode(LiteralKind.Integer, Lexer.ProgramTokenByLocation[@2])); }
| Sign TkRealLiteral { $$ = new UnaryNode((OperatorNode)$1, new LiteralNode(LiteralKind.Real, Lexer.ProgramTokenByLocation[@2])); }
;

/* ModifiablePrimaryNode(IdentifierNode identifier, ModifiablePrimaryNode? prev, ExpressionNode? index) */
ModifiablePrimary
: ModifiablePrimary TkDot Identifier { $$ = new ModifiablePrimaryNode((IdentifierNode)$3, (ModifiablePrimaryNode)$1); }
| ModifiablePrimary TkSquareOpen Expression TkSquareClose { $$ = new ModifiablePrimaryNode(null, (ModifiablePrimaryNode)$1, (ExpressionNode)$3); }
| Identifier { $$ = new ModifiablePrimaryNode((IdentifierNode)$1); }
;

Identifier
: TkIdentifier { $$ = new IdentifierNode(Lexer.ProgramTokenByLocation[@1]); }
;

%%
