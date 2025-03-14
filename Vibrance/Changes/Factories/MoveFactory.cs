namespace Vibrance.Changes.Factories;

internal sealed class MoveFactory : IndexedChangeFactory
{
	public static MoveFactory Instance { get; } = new();

	public bool CanCreateChange<T>(IndexedItems<T> oldItems, IndexedItems<T> newItems)
	{
		return oldItems.Index != newItems.Index &&
		       oldItems.Count > 0 &&
		       (ReferenceEquals(oldItems.List, newItems.List) || oldItems.List.SequenceEqual(newItems.List));
	}

	public IndexedChange<T> CreateChange<T>(IndexedItems<T> oldItems, IndexedItems<T> newItems)
	{
		return new Move<T>
		{
			OldIndex = oldItems.Index,
			NewIndex = newItems.Index,
			Items = newItems.List
		};
	}
}