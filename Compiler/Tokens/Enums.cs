namespace Compiler.Tokens;

public enum TokenCONST
{
    TkUnknown,

//Types
    TkBool,
    TkInt,
    TkReal,
    TkChar,
    TkString,

// Keywords
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

// Punctuators
    TkRoundOpen,
    TkRoundClose,
    TkCurlyOpen,
    TkCurlyClose,
    TkSquareOpen,
    TkSquareClose,
    TkSemicolon,
    TkColon,
    TkDot,
    TkComma,

    // Operators:
    TkAssign,
    TkEqual,
    TkMinus,
    TkPlus,
    TkMultiply,
    TkDivide,
    TkPercent,
    TkAnd,
    TkOr,
    TkXor,

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
    TkRoundOpen = TokenCONST.TkRoundOpen,
    TkRoundClose = TokenCONST.TkRoundClose,
    TkCurlyOpen = TokenCONST.TkCurlyOpen,
    TkCurlyClose = TokenCONST.TkCurlyClose,
    TkSquareOpen = TokenCONST.TkSquareOpen,
    TkSquareClose = TokenCONST.TkSquareClose,
    TkSemicolon = TokenCONST.TkSemicolon,
    TkColon = TokenCONST.TkColon,
    TkDot = TokenCONST.TkDot,
    TkComma = TokenCONST.TkComma
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
    TkAssign = TokenCONST.TkAssign,
    TkEqual = TokenCONST.TkEqual,
    TkMinus = TokenCONST.TkMinus,
    TkPlus = TokenCONST.TkPlus,
    TkMultiply = TokenCONST.TkMultiply,
    TkDivide = TokenCONST.TkDivide,
    TkPercent = TokenCONST.TkPercent,
    TkAnd = TokenCONST.TkAnd,
    TkOr = TokenCONST.TkOr,
    TkXor = TokenCONST.TkXor
}