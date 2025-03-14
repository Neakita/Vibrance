namespace Vibrance.Changes;

public interface Change<out T>
{
	IReadOnlyList<T> OldItems { get; }
	IReadOnlyList<T> NewItems { get; }
}