using System.Text.RegularExpressions;

namespace Compiler.Tokens;

public static class TokenRegexes
{
    // https://regex101.com/
    public static readonly Regex Numbers = new(@"^[-]?(\d+\.?\d*|\d*\.\d+)");
    public static readonly Regex Letters = new(@"[_a-zA-Z]");
    public static readonly Regex Identifiers = new(@"^[_a-zA-Z]+[_a-zA-Z1-9]*");
    public static readonly Regex Strings = new("\"[^\"]*\"");
    public static readonly Regex Chars = new("'.'");
    public static readonly Regex Comments = new(@"^((\/\*[\S\s]*\*\/)|(\/\/.*\r\n))");
    public static readonly Regex Whitespaces = new(@"^\s");
    public static readonly Regex Comparators = new(@"^(<=|>=|<|>|=|/=)$");
    public static readonly Regex Puncuators = new(@"^[\(\)\{\}\[\]\;\:\,]$");
    public static readonly Regex Operators = new(@"^(:=|-|\+|\*|\/|%|\.\.|\.)|((and|xor|or))$");
}