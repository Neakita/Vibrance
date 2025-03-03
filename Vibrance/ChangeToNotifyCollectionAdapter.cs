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
			var args = change.ToNotifyCollectionArgs();
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