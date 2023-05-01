namespace Compiler.Utils;

public static class ListExtensions
{
    public static IEnumerable<T> ReverseList<T>(this List<T> items)
    {
        for (int i = items.Count-1; i >= 0; i--) {
            yield return items[i];
        }
    }
}