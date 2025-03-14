using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using Vibrance.Utilities;

namespace Vibrance.Changes;

internal sealed class IndexedChangeToNotifyCollectionAdapter<T> : ReadOnlyObservableList<T>, IndexedChangesHandler<T>
{
	public event NotifyCollectionChangedEventHandler? CollectionChanged;
	public event PropertyChangedEventHandler? PropertyChanged;

	public IndexedChangeToNotifyCollectionAdapter(IReadOnlyList<T> list)
	{
		_list = list;
	}

	public void HandleChange(IndexedChange<T> change)
	{
		if (CollectionChanged != null)
		{
			var args = change.ToNotifyCollectionChangedEventArgs();
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
}