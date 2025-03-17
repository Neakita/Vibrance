using System.Collections.ObjectModel;

namespace Vibrance.Changes;

public readonly struct IndexedItems<T>
{
	public static IndexedItems<T> Empty => new(-1, ReadOnlyCollection<T>.Empty);

	public int Index { get; }
	public IReadOnlyList<T> List { get; }
	public int Count => List.Count;
	public bool IsEmpty => Count == 0;

	public IndexedItems(int index, IReadOnlyList<T> list)
	{
		if ((index == -1) ^ (list.Count == 0))
			throw new ArgumentException($"the {nameof(index)} and the {nameof(list)} should together represent either an emptiness or significance");
		Index = index;
		List = list;
	}
}