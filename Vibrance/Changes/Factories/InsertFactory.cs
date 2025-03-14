namespace Vibrance.Changes.Factories;

internal sealed class InsertFactory : IndexedChangeFactory
{
	public static InsertFactory Instance { get; } = new();

	public bool CanCreateChange<T>(IndexedItems<T> oldItems, IndexedItems<T> newItems)
	{
		return oldItems.Count == 0 && newItems.Count > 0;
	}

	public IndexedChange<T> CreateChange<T>(IndexedItems<T> oldItems, IndexedItems<T> newItems)
	{
		return new Insert<T>
		{
			Index = newItems.Index,
			Items = newItems.List
		};
	}
}