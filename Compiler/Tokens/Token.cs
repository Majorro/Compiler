namespace Compiler.Tokens;

public abstract class Token
{
    protected int _tokenId;
    public Span span;
    public abstract TokenCONST TokenId { get; }
}

#region Identidier Token

public class IdentifierTk : Token
{
    public string value;

    public IdentifierTk(string value)
    {
        this.value = value;
    }

    public override TokenCONST TokenId => TokenCONST.TkIdentifier;
}

#endregion

#region Keyword Tokens

/// <summary>
///     This generic class is recommended to use for Keyword, Type, Operator, and Punctuator tokes
/// </summary>
/// <typeparam name="T"></typeparam>
public class EnumeratedTk<T> : Token where T : Enum
{
    public EnumeratedTk(TokenCONST value)
    {
        if (Enum.IsDefined(typeof(T), (int)value)) _tokenId = (int)value;
        else throw new ArgumentOutOfRangeException($"Enum {typeof(T)} desn't have value {value}");
    }

    public override TokenCONST TokenId => (TokenCONST)_tokenId;
}

// public class OperatorTk : Token
// {
//     public  int TokenId
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
//     public  int TokenId
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
//     public  int TokenId
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