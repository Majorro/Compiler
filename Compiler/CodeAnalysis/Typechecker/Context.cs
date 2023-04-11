using Compiler.CodeAnalysis.SyntaxAnalysis;

namespace Compiler.CodeAnalysis.Typechecker;

public class Context
{
    public Context? parent_context;
    public Dictionary<String, String> scope = new Dictionary<string, string>();
    public List<String> errors;

    public Context(Context? context)
    {
        this.parent_context = context;
        if (parent_context == null)
        {
            errors = new List<String>();
        }
    }

    public void add(String name, String type)
    {
        scope[name] = type;
    }

    public String? get(String name)
    {
        var type = scope[name];
        if (type != null) return type;
        if (parent_context != null)
        {
            type = parent_context.get(name);    
        }
        return type;
    }

    public void addError(String error)
    {
        if (parent_context == null)
        {
            this.errors.Add(error);
        }
        else
        {
            this.parent_context.addError(error);
        }
    }

    public List<String> getErrors()
    {
        if (parent_context == null)
        {
            return this.errors;
        }
        else
        {
            return this.parent_context.getErrors();
        }
    }

}