using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using Vibrance.Utilities;

namespace Vibrance;

internal sealed class ObservableList<T> : ReadOnlyObservableList<T>, ChangesHandler<T>
{
	public event NotifyCollectionChangedEventHandler? CollectionChanged;
	public event PropertyChangedEventHandler? PropertyChanged;

	public int Count => _items.Count;

	public List<T>.Enumerator GetEnumerator() => _items.GetEnumerator();
	IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public T this[int index] => _items[index];

	public void HandleChange(Change<T> change)
	{
		int count = _items.Count;
		change.ApplyToList(_items);
		NotifyCollectionChanged(change);
		if (count != _items.Count)
			NotifyCountChanged();
		NotifyIndexerChanged();
	}

	private readonly List<T> _items = new();

	private void NotifyCollectionChanged(Change<T> change)
	{
		if (CollectionChanged == null)
			return;
		var args = change.ToNotifyCollectionArgs();
		CollectionChanged.Invoke(this, args);
	}

	private void NotifyCountChanged()
	{
		PropertyChanged?.Invoke(this, KnownPropertyChangedEventArgs.CountChangedEventArgs);
	}

	private void NotifyIndexerChanged()
	{
		PropertyChanged?.Invoke(this, KnownPropertyChangedEventArgs.IndexerChangedEventArgs);
	}
}