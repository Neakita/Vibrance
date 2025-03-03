using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Vibrance;

internal sealed class ChangeToNotifyCollectionAdapter<T> : ReadOnlyObservableList<T>, ChangesHandler<T>
{
	public event NotifyCollectionChangedEventHandler? CollectionChanged;
	public event PropertyChangedEventHandler? PropertyChanged;

	public ChangeToNotifyCollectionAdapter(IReadOnlyList<T> list)
	{
		_list = list;
	}

	public void HandleChange(Change<T> change)
	{
		if (CollectionChanged != null)
		{
			var args = TranslateChangeToArgs(change);
			CollectionChanged.Invoke(this, args);
		}
		if (change.OldItems.Count != change.NewItems.Count)
			PropertyChanged?.Invoke(this, KnownPropertyChangedEventArgs.CountChangedEventArgs);
		PropertyChanged?.Invoke(this, KnownPropertyChangedEventArgs.IndexerChangedEventArgs);
	}

	public IEnumerator<T> GetEnumerator()
	{
		return _list.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public int Count => _list.Count;

	public T this[int index] => _list[index];

	private readonly IReadOnlyList<T> _list;

	private static NotifyCollectionChangedEventArgs TranslateChangeToArgs(Change<T> change)
	{
		return change switch
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
	}
}