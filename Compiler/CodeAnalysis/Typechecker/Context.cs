using Compiler.CodeAnalysis.SyntaxAnalysis;

namespace Compiler.CodeAnalysis.Typechecker;

public class Context
{
    public readonly Context? ParentContext;
    public readonly List<Context> ChildrenContexts = new();
    public readonly Dictionary<string, string> Scope = new();
    public readonly List<string>? Errors;
    public readonly Dictionary<string, string> RoutineParams = new();
    public readonly Dictionary<string, string> RoutineReturn = new();
    public static readonly List<string> RoutineNames = new();

    public Context(Context? context = null)
    {
        ParentContext = context;
        if (ParentContext == null)
        {
            Errors = new List<string>();
        }
        else ParentContext.ChildrenContexts.Add(this);
    }

    public void AddRoutine(string name, string paramsType, string returnType)
    {
        RoutineNames.Add(name);
        RoutineParams[name] = paramsType;
        RoutineReturn[name] = returnType;
    }

    public (string ParamsType, string ReturnType)? GetRoutine(string name)
    {
        return RoutineParams.TryGetValue(name, out var paramsType) &&
               RoutineReturn.TryGetValue(name, out var returnType)
            ? (paramsType, returnType)
            : ParentContext?.GetRoutine(name);
    }

    public void Add(string name, string type)
    {
        Scope[name] = type;
    }

    public string? Get(string? name, bool fromChildren = false)
    {
        if (name == null) return null;
        if (Scope.TryGetValue(name, out var value)) return value;
        if (fromChildren)
        {
            return ChildrenContexts
                .Select(context => context.Get(name, fromChildren: true))
                .Where(e => e != null)
                .FirstOrDefault(defaultValue: null);
        }

        return ParentContext?.Get(name);
    }

    public void AddError(string error)
    {
        if (ParentContext == null)
        {
            Errors!.Add(error);
        }
        else
        {
            ParentContext.AddError(error);
        }
    }

    public List<string> GetErrors()
    {
        if (ParentContext == null)
        {
            return Errors;
        }
        else
        {
            return ParentContext.GetErrors();
        }
    }
}