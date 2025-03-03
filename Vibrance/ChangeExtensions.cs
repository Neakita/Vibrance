namespace Vibrance;

internal static class ChangeExtensions
{
	public static void ApplyToList<T>(this Change<T> change, List<T> list)
	{
		if (change.Reset)
			list.Clear();
		else if (change.OldItems.Count > 0)
			list.RemoveRange(change.OldItemsStartIndex, change.OldItems.Count);
		if (change.NewItems.Count > 0)
			list.InsertRange(change.NewItemsStartIndex, change.NewItems);
	}
}