using System.Collections.ObjectModel;

namespace Vibrance.Changes;

public sealed class Change<T>
{
	public IReadOnlyCollection<T> OldItems { get; init; } = ReadOnlyCollection<T>.Empty;
	public int OldItemsStartIndex { get; init; } = -1;
	public IReadOnlyCollection<T> NewItems { get; init; } = ReadOnlyCollection<T>.Empty;
	public int NewItemsStartIndex { get; init; } = -1;
	public bool Reset { get; init; }
}