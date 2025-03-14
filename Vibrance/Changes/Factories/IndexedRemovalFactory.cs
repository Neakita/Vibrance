namespace Vibrance.Changes.Factories;

internal sealed class IndexedRemovalFactory : IndexedChangeFactory
{
	public static IndexedRemovalFactory Instance { get; } = new();

	public bool CanCreateChange<T>(IndexedItems<T> oldItems, IndexedItems<T> newItems)
	{
		return oldItems.Count != 0 && newItems.Count == 0;
	}

	public IndexedChange<T> CreateChange<T>(IndexedItems<T> oldItems, IndexedItems<T> newItems)
	{
		return new IndexedRemoval<T>
		{
			Index = oldItems.Index,
			Items = oldItems.List
		};
	}
}