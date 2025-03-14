namespace Vibrance.Changes.Factories;

internal sealed class InsertionFactory : IndexedChangeFactory
{
	public static InsertionFactory Instance { get; } = new();

	public bool CanCreateChange<T>(IndexedItems<T> oldItems, IndexedItems<T> newItems)
	{
		return oldItems.Count == 0 && newItems.Count > 0;
	}

	public IndexedChange<T> CreateChange<T>(IndexedItems<T> oldItems, IndexedItems<T> newItems)
	{
		return new Insertion<T>
		{
			Index = newItems.Index,
			Items = newItems.List
		};
	}
}