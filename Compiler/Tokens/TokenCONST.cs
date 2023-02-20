namespace Compiler.Tokens;

public enum TokenCONST
{
    TkUnknown,
    TkBool,
    TkInt,
    TkReal,
    TkChar,
    TkString
}

public enum TypeTokens
{
    TkBool = TokenCONST.TkBool,
    TkInt = TokenCONST.TkInt,
    TkReal = TokenCONST.TkReal,
    TkChar = TokenCONST.TkChar,
    TkString = TokenCONST.TkString
}

public enum PunctuatorTokens
{
    tkInt = TokenCONST.TkInt,
    tkFloat = TokenCONST.TkReal,
    tkChar = TokenCONST.TkChar,
    tkString = TokenCONST.TkString
}

public enum KeywordTokens
{
    tkInt = TokenCONST.TkInt,
    tkFloat = TokenCONST.TkReal,
    tkChar = TokenCONST.TkChar,
    tkString = TokenCONST.TkString
}

public enum OperatorTokens
{
    tkInt = TokenCONST.TkInt,
    tkFloat = TokenCONST.TkReal,
    tkChar = TokenCONST.TkChar,
    tkString = TokenCONST.TkString
}