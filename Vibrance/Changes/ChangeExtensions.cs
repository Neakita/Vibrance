namespace Vibrance.Changes;

internal static class ChangeExtensions
{
	public static void ApplyToList<T>(this IndexedChange<T> change, List<T> list)
	{
		if (change.OldItems.Count > 0)
			list.RemoveRange(change.OldIndex, change.OldItems.Count);
		if (change.NewItems.Count > 0)
			list.InsertRange(change.NewIndex, change.NewItems);
	}

	public static IndexedItems<T> OldItemsAsIndexed<T>(this IndexedChange<T> change)
	{
		return new IndexedItems<T>(change.OldIndex, change.OldItems);
	}

	public static IndexedItems<T> NewItemsAsIndexed<T>(this IndexedChange<T> change)
	{
		return new IndexedItems<T>(change.NewIndex, change.NewItems);
	}
}