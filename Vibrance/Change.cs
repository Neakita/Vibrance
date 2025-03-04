using System.Collections.ObjectModel;

namespace Vibrance;

public sealed class Change<T>
{
	public IReadOnlyList<T> OldItems { get; init; } = ReadOnlyCollection<T>.Empty;
	public int OldItemsStartIndex { get; init; } = -1;
	public IReadOnlyList<T> NewItems { get; init; } = ReadOnlyCollection<T>.Empty;
	public int NewItemsStartIndex { get; init; } = -1;
	public bool Reset { get; init; }
}