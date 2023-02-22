namespace Compiler.Tokens;

public abstract class Token
{
    public Span span;
    public abstract TokenCONST TokenId { get; }
}

public class UnknownTk : Token
{
    public string value;

    public UnknownTk(string value)
    {
        this.value = value;
    }

    public override TokenCONST TokenId => TokenCONST.TkUnknown;
}

public class IdentifierTk : Token
{
    public string value;

    public IdentifierTk(string value)
    {
        this.value = value;
    }

    public override TokenCONST TokenId => TokenCONST.TkIdentifier;
}

/// <summary>
///     This generic class is recommended to use for Keyword, Type, Operator, Punctuator, and comparator tokens
/// </summary>
/// <typeparam name="T"></typeparam>
public class EnumeratedTk<T> : Token where T : Enum
{
    private readonly int _tokenId;

    public EnumeratedTk(TokenCONST value)
    {
        if (Enum.IsDefined(typeof(T), (int)value)) _tokenId = (int)value;
        else throw new ArgumentOutOfRangeException($"Enum {typeof(T)} desn't have value {value}");
    }

    public override TokenCONST TokenId => (TokenCONST)_tokenId;
}

#region Constant Tokens

public class IntTk : Token
{
    public long value;

    public IntTk(long value)
    {
        this.value = value;
    }

    public override TokenCONST TokenId => TokenCONST.TkInt;
}

public class RealTk : Token
{
    public double value;

    public RealTk(double value)
    {
        this.value = value;
    }

    public override TokenCONST TokenId => TokenCONST.TkReal;
}

public class BoolTk : Token
{
    public bool value;

    public BoolTk(bool value)
    {
        this.value = value;
    }

    public override TokenCONST TokenId => TokenCONST.TkBool;
}

#endregion

#region LiteralTokens

public class CharTk : Token
{
    public char value;

    public CharTk(char value)
    {
        this.value = value;
    }

    public override TokenCONST TokenId => TokenCONST.TkChar;
}

public class StringTk : Token
{
    public string value;

    public StringTk(string value)
    {
        this.value = value;
    }

    public override TokenCONST TokenId => TokenCONST.TkString;
}

#endregion

public class Span
{
    public long LineNumber;
    public int StartPos, EndPost;

    public Span(long lineNumber, int startPos, int endPost)
    {
        LineNumber = lineNumber;
        StartPos = startPos;
        EndPost = endPost;
    }
}