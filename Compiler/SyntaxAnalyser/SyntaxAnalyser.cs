using System.Text.RegularExpressions;
using Compiler.Tokens;

namespace Compiler.SyntaxAnalyser;

public static class SyntaxAnalyser
{
    // https://regex101.com/
    private static readonly Regex Numbers = new(@"[-]?(\d+\.?\d*|\d*\.\d+)");
    private static readonly Regex Letters = new(@"[_a-zA-Z]");
    private static readonly Regex Identifiers = new(@"^[_a-zA-Z]+[_a-zA-Z1-9]*");
    private static readonly Regex Strings = new("\"[^\"]*\"");
    private static readonly Regex Chars = new("'.'");
    private static readonly Regex Comments = new(@"(\/\*[\S\s]*\*\/)|(\/\/.*)");
    private static readonly Regex Whitespaces = new(@"\s");
    private static readonly Regex Comparisons = new(@"<=|>=|<|>|==|!=");

    public static List<Token> FinalStateAutomata(string text)
    {
        var tokens = new List<Token>();
        var buffer = "";

        foreach (var symbol in text + ' ')
        {
            var type = CheckForType(buffer);

            if (type != TokenCONST.TkUnknown && !char.IsLetter(symbol))
            {
                tokens.Add(new KeywordTk<TypeTokens> { TokenId = type });
                buffer = "";
            }
            // Makes Const tokens of types integer and real
            else if (Numbers.IsMatch(buffer) && !(char.IsNumber(symbol) || symbol == '.'))
            {
                if (long.TryParse(buffer, out var integer))
                {
                    tokens.Add(new IntTk(integer));
                    buffer = "";
                }
                else if (double.TryParse(buffer, out var real))
                {
                    tokens.Add(new RealTk(real));
                    buffer = "";
                }
            }
            else if (Strings.IsMatch(buffer))
            {
                tokens.Add(new StringTk(buffer.Split('"')[1]));
                buffer = "";
            }
            else if (Chars.IsMatch(buffer))
            {
                tokens.Add(new CharTk(buffer[1]));
                buffer = "";
            }
            else if (Identifiers.IsMatch(buffer) && !(char.IsLetterOrDigit(symbol) || symbol == '_'))
            {
                tokens.Add(new IdentifierTk(buffer));
                buffer = "";
            }
            else if (buffer == " ")
            {
                buffer = "";
            }

            buffer += symbol;
        }

        return tokens;
    }

    private static TokenCONST CheckForType(string inputWord)
    {
        switch (inputWord)
        {
            case "int": return TokenCONST.TkInt;
            case "float": return TokenCONST.TkReal;
            case "char": return TokenCONST.TkChar;
            case "string": return TokenCONST.TkString;
            default: return TokenCONST.TkUnknown;
        }
    }
}