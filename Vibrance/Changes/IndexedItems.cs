using System.Collections.ObjectModel;

namespace Vibrance.Changes;

public readonly struct IndexedItems<T>
{
	public static IndexedItems<T> Empty => new(-1, ReadOnlyCollection<T>.Empty);

	public int Index { get; }
	public IReadOnlyList<T> List { get; }
	public int Count => List.Count;

	public IndexedItems(int index, IReadOnlyList<T> list)
	{
		Index = index;
		List = list;
	}
}