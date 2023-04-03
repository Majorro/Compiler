using Compiler.CodeAnalysis.SyntaxAnalysis;
using QUT.Gppg;

namespace Compiler.CodeAnalysis.LexerTokens;

public abstract class Token
{
    public LexLocation? Span { get; set; }
    public abstract Tokens TokenId { get; }
}

public class UnknownTk : Token
{
    public string value;

    public UnknownTk(string value)
    {
        this.value = value;
    }

    public override Tokens TokenId => Tokens.TkUnknown;
}

public class IdentifierTk : Token
{
    public string value;

    public IdentifierTk(string value)
    {
        this.value = value;
    }

    public override Tokens TokenId => Tokens.TkIdentifier;
}

/// <summary>
///     This generic class is recommended to use for Keyword, Type, Operator, Punctuator, and comparator tokens
/// </summary>
/// <typeparam name="T"></typeparam>
public class EnumeratedTk<T> : Token where T : Enum
{
    private readonly int _tokenId;

    public EnumeratedTk(Tokens value)
    {
        if (Enum.IsDefined(typeof(T), (int)value)) _tokenId = (int)value;
        else throw new ArgumentOutOfRangeException($"Enum {typeof(T)} doesn't have value {value}");
    }

    public override Tokens TokenId => (Tokens)_tokenId;
}

#region LiteralTokens

public class IntTk : Token
{
    public long value;

    public IntTk(long value)
    {
        this.value = value;
    }

    public override Tokens TokenId => Tokens.TkIntLiteral;
}

public class RealTk : Token
{
    public double value;

    public RealTk(double value)
    {
        this.value = value;
    }

    public override Tokens TokenId => Tokens.TkRealLiteral;
}

public class BoolTk : Token
{
    public bool value;

    public BoolTk(bool value)
    {
        this.value = value;
    }

    public override Tokens TokenId => Tokens.TkBoolLiteral;
}

#endregion