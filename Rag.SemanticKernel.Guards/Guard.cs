
namespace Rag.SemanticKernel.Guards;

public class Guard
{
    public static T ThrowIfNull<T>(T o)
    {
        return o ?? throw new ArgumentNullException(nameof(o));
    }

}
