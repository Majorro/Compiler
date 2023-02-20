namespace Compiler.Tokens;

public enum TokenCONST
{
    TkUnknown,

    TkBool,
    TkInt,
    TkReal,
    TkChar,
    TkString,

    TkType,
    TkIs,
    TkEnd,
    TkReturn,
    TkVar,
    TkRoutine,
    TkRecord,
    TkArray,
    TkFor,
    TkWhile,
    TkLoop,
    TkIn,
    TkReverse,
    TkIf,
    TkThen,
    TkElse,


    TkIdentifier
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
    TkType = TokenCONST.TkType,
    TkIs = TokenCONST.TkIs,
    TkEnd = TokenCONST.TkEnd,
    TkReturn = TokenCONST.TkReturn,
    TkVar = TokenCONST.TkVar,
    TkRoutine = TokenCONST.TkRoutine,
    TkRecord = TokenCONST.TkRecord,
    TkArray = TokenCONST.TkArray,
    TkFor = TokenCONST.TkFor,
    TkWhile = TokenCONST.TkWhile,
    TkLoop = TokenCONST.TkLoop,
    TkIn = TokenCONST.TkIn,
    TkReverse = TokenCONST.TkReverse,
    TkIf = TokenCONST.TkIf,
    TkThen = TokenCONST.TkThen,
    TkElse = TokenCONST.TkElse
}

public enum OperatorTokens
{
    tkInt = TokenCONST.TkInt,
    tkFloat = TokenCONST.TkReal,
    tkChar = TokenCONST.TkChar,
    tkString = TokenCONST.TkString
}