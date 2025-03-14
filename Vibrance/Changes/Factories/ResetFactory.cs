namespace Vibrance.Changes.Factories;

internal sealed class ResetFactory : IndexedChangeFactory
{
	public static ResetFactory Instance { get; } = new();

	public bool CanCreateChange<T>(IndexedItems<T> oldItems, IndexedItems<T> newItems)
	{
		return oldItems is { Count: > 0, Index: 0 } && newItems.Index <= 0;
	}

	public IndexedChange<T> CreateChange<T>(IndexedItems<T> oldItems, IndexedItems<T> newItems)
	{
		return new Reset<T>
		{
			OldItems = oldItems.List,
			NewItems = newItems.List
		};
	}
}