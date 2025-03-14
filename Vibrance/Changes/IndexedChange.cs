using Vibrance.Changes.Factories;

namespace Vibrance.Changes;

public interface IndexedChange<out T> : Change<T>
{
	IndexedChangeFactory Factory { get; }

	int OldIndex { get; }
	int NewIndex { get; }
}