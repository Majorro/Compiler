using Compiler.CodeAnalysis.SyntaxAnalysis;

namespace Compiler.CodeAnalysis.Typechecker;

public class Context
{
    public Context? parent_context;
    public Dictionary<String, TypeNode> scope = new Dictionary<string, TypeNode>();

    public Context(Context? context)
    {
        this.parent_context = context;
    }

    public void add(String name, TypeNode type)
    {
        scope[name] = type;
    }

    public TypeNode? get(String name)
    {
        var type = scope[name];
        if (type != null) return type;
        if (parent_context != null)
        {
            type = parent_context.get(name);    
        }
        return type;
    }

}