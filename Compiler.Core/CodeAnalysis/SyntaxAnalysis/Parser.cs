using Compiler.Core.CodeAnalysis.LexicalAnalysis;
using QUT.Gppg;

namespace Compiler.Core.CodeAnalysis.SyntaxAnalysis;

public partial class Parser
{
    public Tree Tree { get; private set; }
    public Lexer Lexer => (Lexer)Scanner;

    private void SaveTree(ProgramNode root)
    {
        Tree = new Tree(root);
    }

    public Parser(AbstractScanner<Node, LexLocation> scanner) : base(scanner)
    { }
}