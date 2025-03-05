using System.Collections.Specialized;

namespace Vibrance.Changes;

internal static class ChangeExtensions
{
	public static void ApplyToList<T>(this Change<T> change, List<T> list)
	{
		if (change.Reset)
			list.Clear();
		else if (change.OldItems.Count > 0)
			list.RemoveRange(change.OldItems.StartIndex, change.OldItems.Count);
		if (change.NewItems.Count > 0)
			list.InsertRange(change.NewItems.StartIndex, change.NewItems);
	}

	public static NotifyCollectionChangedEventArgs ToNotifyCollectionArgs<T>(this Change<T> change) => change switch
	{
		{ Reset: true } => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset),
		{ OldItems.Count: > 0, NewItems.Count: > 0 } when change.OldItems.StartIndex == change.NewItems.StartIndex =>
			new NotifyCollectionChangedEventArgs(
				NotifyCollectionChangedAction.Replace,
				change.NewItems.AsList(),
				change.OldItems.AsList(),
				change.NewItems.StartIndex),
		{ OldItems.Count: > 0, NewItems.Count: > 0 } when ReferenceEquals(change.OldItems, change.NewItems) || change.OldItems.SequenceEqual(change.NewItems) =>
			new NotifyCollectionChangedEventArgs(
				NotifyCollectionChangedAction.Move,
				change.NewItems.AsList(),
				change.NewItems.StartIndex,
				change.OldItems.StartIndex),
		{ OldItems.Count: > 0 } =>
			new NotifyCollectionChangedEventArgs(
				NotifyCollectionChangedAction.Remove,
				change.OldItems.AsList(),
				change.OldItems.StartIndex),
		{ NewItems.Count: > 0 } =>
			new NotifyCollectionChangedEventArgs(
				NotifyCollectionChangedAction.Add,
				change.NewItems.AsList(),
				change.NewItems.StartIndex),
		_ => throw new ArgumentOutOfRangeException(nameof(change), change, null)
	};

	internal static bool IsMove<T>(this Change<T> change) =>
		change.NewItems.Count > 0 &&
		change.OldItems.Count > 0 &&
		change.OldItems.StartIndex != change.NewItems.StartIndex &&
		ReferenceEquals(change.NewItems, change.OldItems) || change.NewItems.SequenceEqual(change.OldItems);
}