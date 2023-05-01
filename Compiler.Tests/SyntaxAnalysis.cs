using Compiler.Core.CodeAnalysis.LexicalAnalysis;
using Compiler.Core.CodeAnalysis.SyntaxAnalysis;

namespace Compiler.Tests;

[TestFixture]
public static class SyntaxAnalysis
{
    [Test]
    public static void Parser_Should_ParseRoutineCallAsStatement()
    {
        const string program = @"
        routine main() is
            factorial(5);
        end;";
        var parser = Setup(program);

        var success = parser.Parse();
        Assert.That(success, Is.True);

        var tree = parser.Tree;
        Assert.That(tree.Root.HasChildrenNodeWithParent<RoutineCallNode, BodyNode>(), Is.True);
    }

    [Test]
    public static void Parser_Should_ParseRoutineCallAsExpression()
    {
        const string program = @"
        routine main() is
            var a is 1 + factorial(5);
        end;";
        var parser = Setup(program);

        var success = parser.Parse();
        Assert.That(success, Is.True);

        var tree = parser.Tree;
        Assert.That(tree.Root.HasChildrenNodeWithParent<RoutineCallNode, ExpressionNode>(), Is.True);
    }

    [Test]
    public static void Parser_Should_ParseUnarySignWithLiteral()
    {
        const string program = @"
        routine main() is
            var a is -1;
        end;";
        var parser = Setup(program);

        var success = parser.Parse();
        Assert.That(success, Is.True);

        var tree = parser.Tree;
        Assert.That(tree.Root.HasChildrenNodeWithParent<OperatorNode, UnaryNode>(), Is.True);
    }

    [Test]
    public static void Parser_Should_ParseEmptyBody()
    {
        const string program = @"
        routine main() is
        end;";
        var parser = Setup(program);

        var success = parser.Parse();
        Assert.That(success, Is.True);

        var tree = parser.Tree;
        Assert.That(tree.Root.HasChildrenNodeWithParent<BodyNode, RoutineDeclarationNode>(), Is.True);
        Assert.DoesNotThrow(() => Assert.That(
            tree.Root.GetChildren<RoutineDeclarationNode>().First()
                .GetChildren<BodyNode>().First()
                .GetChildren(),
            Is.Empty));
    }

    [Test]
    [TestCase("a := 1;", TestName = "Assignment statement")]
    [TestCase("factorial(5);", TestName = "Routine call statement")]
    [TestCase("while 2 > 1 loop end;", TestName = "While statement")]
    [TestCase("for i in 1 .. 2 loop end;", TestName = "For statement")]
    [TestCase("if a = 1 then end;", TestName = "If statement")]
    public static void Parser_Should_NotParseStatementsInRoot(string program)
    {
        var parser = Setup(program);

        var success = parser.Parse();
        Assert.That(success, Is.False);
    }

    private static Parser Setup(string program)
    {
        var lexer = new Lexer(program);
        return new Parser(lexer);
    }

    private static bool HasChildrenNode<T>(this Node node)
        where T : Node =>
        node is T ||
        node.GetChildren().Any(HasChildrenNode<T>);

    private static bool HasChildrenNodeWithParent<TChild, TParent>(this Node node)
        where TChild : Node
        where TParent : Node =>
        node is TParent parent && parent.GetChildren().Any(HasChildrenNode<TChild>) ||
        node.GetChildren().Any(HasChildrenNodeWithParent<TChild, TParent>);
}