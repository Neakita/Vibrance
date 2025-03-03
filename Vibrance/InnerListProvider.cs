namespace Vibrance;

public interface InnerListProvider<out T>
{
	IReadOnlyList<T> Inner { get; }
}