using Microsoft.Win32.SafeHandles;
using QUT.Gppg;

namespace Compiler.CodeAnalysis.SyntaxAnalysis;

public partial class Parser
{
    public Tree Tree { get; private set; }
    public Lexer Lexer => (Lexer)Scanner;

    protected void SaveTree(ProgramNode root)
    {
        Tree = new Tree(root);
    }

    public Parser(AbstractScanner<Node, LexLocation> scanner) : base(scanner)
    {
    }
}