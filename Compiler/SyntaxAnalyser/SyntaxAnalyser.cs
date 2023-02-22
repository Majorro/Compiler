using System.Text.RegularExpressions;
using Compiler.Tokens;

namespace Compiler.SyntaxAnalyser;

public static class SyntaxAnalyser
{
    public static List<Token> FinalStateAutomata(string text)
    {
        var tokens = new List<Token>();
        var buffer = "";
        var lineNumber = 0;
        var columnNumber = 0;
        text += ' ';
        foreach (var symbol in text + ' ')
        {
            var preset = CheckForEnum(buffer);
            Token token = null;

            if (TokenRegexes.Comments.IsMatch(buffer))
            {
                lineNumber += Regex.Matches(buffer, "\r").Count;
                buffer = "";
            }
            else if (preset != TokenCONST.TkUnknown)
            {
                if (Enum.IsDefined(typeof(TypeTokens), (int)preset) && !char.IsLetter(symbol))
                    token = new EnumeratedTk<TypeTokens>(preset);

                else if (Enum.IsDefined(typeof(KeywordTokens), (int)preset) && !char.IsLetter(symbol))
                    token = new EnumeratedTk<KeywordTokens>(preset);

                else if (TokenRegexes.Operators.IsMatch(buffer) && buffer + symbol is not (".." or "//" or "/*"))
                    token = new EnumeratedTk<OperatorTokens>(preset);

                else if (TokenRegexes.Puncuators.IsMatch(buffer) &&
                         !TokenRegexes.Comparators.IsMatch(symbol.ToString()))
                    token = new EnumeratedTk<PunctuatorTokens>(preset);

                else if (TokenRegexes.Comparators.IsMatch(buffer))
                    token = new EnumeratedTk<Comparators>(preset);
            }
            // Makes Const tokens of types integer and real
            else if (TokenRegexes.Numbers.IsMatch(buffer) &&
                     !(char.IsNumber(symbol) || char.IsLetter(symbol) || symbol == '.'))
            {
                if (long.TryParse(buffer, out var integer)) token = new IntTk(integer);

                else if (double.TryParse(buffer, out var real)) token = new RealTk(real);
            }
            else if (bool.TryParse(buffer, out var boolean))
            {
                token = new BoolTk(boolean);
            }
            else if (TokenRegexes.Strings.IsMatch(buffer))
            {
                token = new StringTk(buffer.Split('"')[1]);
            }
            else if (TokenRegexes.Chars.IsMatch(buffer))
            {
                token = new CharTk(buffer[1]);
            }
            else if (TokenRegexes.Identifiers.IsMatch(buffer) && !(char.IsLetterOrDigit(symbol) || symbol == '_'))
            {
                token = new IdentifierTk(buffer);
            }
            else if (TokenRegexes.Whitespaces.IsMatch(buffer))
            {
                if (buffer.Contains('\n'))
                {
                    lineNumber++;
                    columnNumber = 0;
                }

                buffer = "";
            }
            else if (TokenRegexes.Unknown.IsMatch(buffer) && TokenRegexes.Whitespaces.IsMatch(symbol.ToString()))
            {
                token = new UnknownTk(buffer);
            }

            if (token is not null)
            {
                token.span = new Span(lineNumber, columnNumber - buffer.Length, columnNumber - 1);
                tokens.Add(token);
                buffer = "";
            }

            buffer += symbol;
            columnNumber++;
        }

        return tokens;
    }


    private static TokenCONST CheckForEnum(string inputWord)
    {
        switch (inputWord)
        {
            //Types:
            case "boolean": return TokenCONST.TkBool;
            case "integer": return TokenCONST.TkInt;
            case "float": return TokenCONST.TkReal;
            case "char": return TokenCONST.TkChar;
            case "string": return TokenCONST.TkString;

            //KeyWords:
            case "type": return TokenCONST.TkType;
            case "is": return TokenCONST.TkIs;
            case "end": return TokenCONST.TkEnd;
            case "return": return TokenCONST.TkReturn;
            case "var": return TokenCONST.TkVar;
            case "routine": return TokenCONST.TkRoutine;
            case "for": return TokenCONST.TkFor;
            case "while": return TokenCONST.TkWhile;
            case "loop": return TokenCONST.TkLoop;
            case "in": return TokenCONST.TkIn;
            case "reverse": return TokenCONST.TkReverse;
            case "if": return TokenCONST.TkIf;
            case "then": return TokenCONST.TkThen;
            case "else": return TokenCONST.TkElse;

            //Punctuators:
            case "(": return TokenCONST.TkRoundOpen;
            case ")": return TokenCONST.TkRoundClose;
            case "{": return TokenCONST.TkCurlyOpen;
            case "}": return TokenCONST.TkCurlyClose;
            case "[": return TokenCONST.TkSquareOpen;
            case "]": return TokenCONST.TkSquareClose;
            case ";": return TokenCONST.TkSemicolon;
            case ":": return TokenCONST.TkColon;
            case ",": return TokenCONST.TkComma;

            //Operators:
            case ":=": return TokenCONST.TkAssign;
            case ".": return TokenCONST.TkDot;
            case "-": return TokenCONST.TkMinus;
            case "+": return TokenCONST.TkPlus;
            case "*": return TokenCONST.TkMultiply;
            case "/": return TokenCONST.TkDivide;
            case "%": return TokenCONST.TkPercent;
            case "and": return TokenCONST.TkAnd;
            case "or": return TokenCONST.TkOr;
            case "xor": return TokenCONST.TkXor;
            case "..": return TokenCONST.TkRange;

            // Comparators:
            case "<=": return TokenCONST.TkLeq;
            case ">=": return TokenCONST.TkGeq;
            case "<": return TokenCONST.TkLess;
            case ">": return TokenCONST.TkGreater;
            case "=": return TokenCONST.TkEqual;
            case "\\=": return TokenCONST.TkNotEqual;


            default: return TokenCONST.TkUnknown;
        }
    }
}