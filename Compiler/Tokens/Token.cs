using System.Text.Json;

namespace Compiler.Tokens;

public abstract class Token
{
    public Span span;
}

public class TypeTk : Token
{
    public TokenCONST TokenConst;
}
public class IntTk : Token
{
    public long value;

    public IntTk(long value)
    {
        this.value = value;
    }
}

public class IdentifierTk : Token
{
    public string value;

    public IdentifierTk(string value)
    {
        this.value = value;
    }
}
public class RealTk : Token
{
    public double value;

    public RealTk(double value)
    {
        this.value = value;
    }
}

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
    public StringTk(string value)
    {
        this.value = value;
    }

    public string value;
}

public class BoolTk : Token
{
    public bool value;
}

public class ArrayTk<T> : Token
{
    
}

public class StructTk : Token
{
    
}

public class LinkedListTk : Token
{
    
}
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