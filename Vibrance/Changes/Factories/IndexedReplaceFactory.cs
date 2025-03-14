namespace Vibrance.Changes.Factories;

internal sealed class IndexedReplaceFactory : IndexedChangeFactory
{
	public static IndexedReplaceFactory Instance { get; } = new();

	public bool CanCreateChange<T>(IndexedItems<T> oldItems, IndexedItems<T> newItems)
	{
		return oldItems.Index == newItems.Index &&
		       !ReferenceEquals(oldItems.List, newItems.List) &&
		       oldItems.Count > 0 &&
		       newItems.Count > 0;
	}

	public IndexedChange<T> CreateChange<T>(IndexedItems<T> oldItems, IndexedItems<T> newItems)
	{
		return new IndexedReplace<T>
		{
			Index = newItems.Index,
			OldItems = oldItems.List,
			NewItems = newItems.List
		};
	}
}