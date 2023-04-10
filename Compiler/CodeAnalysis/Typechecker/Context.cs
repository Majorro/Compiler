using Compiler.CodeAnalysis.SyntaxAnalysis;

namespace Compiler.CodeAnalysis.Typechecker;

public class Context
{
    public Context? parent_context;
    public Dictionary<String, TypeNode> stelladients = new Dictionary<string, TypeNode>();

    public Context(Context? context)
    {
        this.parent_context = context;
    }

    public void addStelladient(String name, TypeNode type)
    {
        stelladients[name] = type;
    }

    public TypeNode? getStelladient(String name)
    {
        var type = stelladients[name];
        if (type != null) return type;
        if (parent_context != null)
        {
            type = parent_context.getStelladient(name);    
        }
        return type;
    }

}