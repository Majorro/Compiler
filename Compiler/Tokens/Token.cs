namespace Compiler.Tokens;

public abstract class Token
{
    protected int _tokenId;
    public Span span;
    public TokenCONST TokenId { get; set; }
}

#region Identidier Token

public class IdentifierTk : Token
{
    public string value;

    public IdentifierTk(string value)
    {
        this.value = value;
    }
}

#endregion

#region Keyword Tokens

/// <summary>
///     This generic class is recommended to use for Keyword, Type, Operator, and Punctuator tokes
/// </summary>
/// <typeparam name="T"></typeparam>
public class KeywordTk<T> : Token where T : Enum
{
    public new TokenCONST TokenId
    {
        get => (TokenCONST)_tokenId;
        set
        {
            if (Enum.IsDefined(typeof(T), (int)value)) _tokenId = (int)value;
        }
    }
}

// public class OperatorTk : Token
// {
//     public new int TokenId
//     {
//         get { return _tokenId; }
//         set
//         {
//             if (Enum.IsDefined(typeof(OperatorTokens), value))
//             {
//                 _tokenId = value;
//             }
//         }
//     }
// }
//
// public class PunctuatorTk : Token
// {
//     public new int TokenId
//     {
//         get { return _tokenId; }
//         set
//         {
//             if (Enum.IsDefined(typeof(PunctuatorTokens), value))
//             {
//                 _tokenId = value;
//             }
//         }
//     }
// }
//
// public class TypeTk : Token
// {
//     public new int TokenId
//     {
//         get { return _tokenId; }
//         set
//         {
//             if (Enum.IsDefined(typeof(TypeTokens), value))
//             {
//                 _tokenId = value;
//             }
//         }
//     }
// }

#endregion

#region Constant Tokens

public class IntTk : Token
{
    public long value;

    public IntTk(long value)
    {
        this.value = value;
    }

    public new TokenCONST TokenId => TokenCONST.TkInt;
}

public class RealTk : Token
{
    public double value;

    public RealTk(double value)
    {
        this.value = value;
    }

    public new TokenCONST TokenId => TokenCONST.TkReal;
}

public class BoolTk : Token
{
    public bool value;
    public new TokenCONST TokenId => TokenCONST.TkBool;
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
}

public class StringTk : Token
{
    public string value;

    public StringTk(string value)
    {
        this.value = value;
    }
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