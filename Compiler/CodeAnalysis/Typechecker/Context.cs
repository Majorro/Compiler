using Compiler.CodeAnalysis.SyntaxAnalysis;

namespace Compiler.CodeAnalysis.Typechecker;

public class Context
{
    public Context? parent_context;
    public Dictionary<String, String> scope = new Dictionary<string, string>();
    public List<String> errors;
    public Dictionary<String, String> routines = new Dictionary<string, string>();

    public Context(Context? context)
    {
        this.parent_context = context;
        if (parent_context == null)
        {
            errors = new List<String>();
        }
    }

    public void addRoutine(String name, String parameters)
    {
        routines[name] = parameters;
    }

    public String? getParameters(String name)
    {
        return routines.TryGetValue(name, out var value) ? value : parent_context?.getParameters(name);
    }

    public void add(String name, String type)
    {
        scope[name] = type;
    }

    public String? get(String name)
    {
        return scope.TryGetValue(name, out var value) ? value : parent_context?.get(name);
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