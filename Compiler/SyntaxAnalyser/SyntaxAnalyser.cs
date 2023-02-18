using Compiler.Tokens;
namespace Compiler.SyntaxAnalyser;
using System.Text.RegularExpressions;


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
        List<Token> tokens = new List<Token>(); 
        string buffer = "";
        
        foreach (var symbol in text+' ')
        {
            var type = CheckForType(buffer);
            
            if ( type != TokenCONST.tkUnknown && ! char.IsLetter(symbol))
            {
                tokens.Add(new TypeTk(){TokenConst = type});
                buffer = "";
            }
            else if (Numbers.IsMatch(buffer) && ! (char.IsNumber(symbol) || symbol == '.'))
            {
                if (Int64.TryParse(buffer, out long integer))
                {
                    tokens.Add(new IntTk(integer));
                    buffer = "";
                }
                else if (Double.TryParse(buffer, out double real))
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
            else if (Identifiers.IsMatch(buffer) && !(char.IsLetterOrDigit(symbol) || symbol == '_') )
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
            case "int": return TokenCONST.tkInt;
            case "float": return TokenCONST.tkFloat;
            case "char": return TokenCONST.tkChar;
            case "string": return TokenCONST.tkString;
            default: return TokenCONST.tkUnknown;
        }
    }
}

