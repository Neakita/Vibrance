using System.Collections;
using System.Collections.Specialized;

namespace Vibrance.Changes;

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

	public static NotifyCollectionChangedEventArgs ToNotifyCollectionArgs<T>(this Change<T> change) => change switch
	{
		{ Reset: true } => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset),
		{ OldItems.Count: > 0, NewItems.Count: > 0 } when change.OldItemsStartIndex == change.NewItemsStartIndex =>
			new NotifyCollectionChangedEventArgs(
				NotifyCollectionChangedAction.Replace,
				(IList)change.NewItems,
				(IList)change.OldItems,
				change.NewItemsStartIndex),
		{ OldItems.Count: > 0, NewItems.Count: > 0 } when ReferenceEquals(change.OldItems, change.NewItems) || change.OldItems.SequenceEqual(change.NewItems) =>
			new NotifyCollectionChangedEventArgs(
				NotifyCollectionChangedAction.Move,
				(IList)change.NewItems,
				change.NewItemsStartIndex,
				change.OldItemsStartIndex),
		{ OldItems.Count: > 0 } =>
			new NotifyCollectionChangedEventArgs(
				NotifyCollectionChangedAction.Remove,
				(IList)change.OldItems,
				change.OldItemsStartIndex),
		{ NewItems.Count: > 0 } =>
			new NotifyCollectionChangedEventArgs(
				NotifyCollectionChangedAction.Add,
				(IList)change.NewItems,
				change.NewItemsStartIndex),
		_ => throw new ArgumentOutOfRangeException(nameof(change), change, null)
	};

	internal static bool IsMove<T>(this Change<T> change) =>
		change.NewItems.Count > 0 &&
		change.OldItems.Count > 0 &&
		change.OldItemsStartIndex != change.NewItemsStartIndex &&
		ReferenceEquals(change.NewItems, change.OldItems) || change.NewItems.SequenceEqual(change.OldItems);
}