namespace Compiler.CodeAnalysis.CodeGen;

public class CodegenContext
{
    protected readonly CodegenContext? ParentContext;
    protected readonly Dictionary<string, Action> VariableLoadActions = new();
    protected readonly Dictionary<string, Action> VariableStoreActions = new();

    public CodegenContext(CodegenContext? context = null)
    {
        ParentContext = context;
    }

    public void AddVariable(string name, Action loadAction, Action storeAction)
    {
        VariableLoadActions[name] = loadAction;
        VariableStoreActions[name] = storeAction;
    }

    public void LoadVariable(string name)
    {
        VariableLoadActions.TryGetValue(name, out var value);
        if (value == null)
        {
            if (ParentContext == null) throw new ArgumentException($"{name} does not exist in current scope");
            ParentContext.LoadVariable(name);
        }
        else value.Invoke();
    }
    
    public void StoreVariable(string name)
    {
        VariableStoreActions.TryGetValue(name, out var value);
        if (value == null)
        {
            if (ParentContext == null) throw new ArgumentException($"{name} does not exist in current scope");
            ParentContext.StoreVariable(name);
        }
        else value.Invoke();
    }
}