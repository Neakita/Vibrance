using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using Vibrance.Changes;
using Vibrance.Utilities;

namespace Vibrance;

public sealed class ObservableList<T> : IList<T>, ReadOnlyObservableList<T>
{
	public event NotifyCollectionChangedEventHandler? CollectionChanged;
	public event PropertyChangedEventHandler? PropertyChanged;

	public int Count => _items.Count;

	public IDisposable Subscribe(IObserver<IndexedChange<T>> observer)
	{
		SendInitialItems(observer);
		_observers.Add(observer);
		return new ActionDisposable(() => _observers.Remove(observer));
	}

	public List<T>.Enumerator GetEnumerator() => _items.GetEnumerator();
	IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public bool IsReadOnly => false;

	public T this[int index]
	{
		get => _items[index];
		set
		{
			var oldItem = _items[index];
			_items[index] = value;
			IndexedReplacement<T> change = new()
			{
				Index = index,
				OldItems = [oldItem],
				NewItems = [value]
			};
			Notify(change);
		}
	}

	public ObservableList()
	{
		_items = new List<T>();
	}

	public ObservableList(int capacity)
	{
		_items = new List<T>(capacity);
	}

	public ObservableList(IEnumerable<T> collection)
	{
		_items = new List<T>(collection);
	}

	public void Add(T item)
	{
		var index = _items.Count;
		Insert(index, item);
	}

	public void AddRange(params IEnumerable<T> items)
	{
		var index = _items.Count;
		InsertRange(index, items);
	}

	public void Clear()
	{
		if (_items.Count == 0)
			return;
		Reset<T> change = new()
		{
			OldItems = _items
		};
		_items = new List<T>();
		Notify(change);
	}

	public bool Contains(T item)
	{
		return _items.Contains(item);
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		_items.CopyTo(array, arrayIndex);
	}

	public bool Remove(T item)
	{
		var index = IndexOf(item);
		if (index < 0)
			return false;
		RemoveAt(index);
		return true;
	}

	public int IndexOf(T item)
	{
		return _items.IndexOf(item);
	}

	public void Insert(int index, T item)
	{
		_items.Insert(index, item);
		Insertion<T> change = new()
		{
			Index = index,
			Items = [item]
		};
		Notify(change);
	}

	public void InsertRange(int index, params IEnumerable<T> items)
	{
		var itemsList = items.ToList();
		_items.InsertRange(index, itemsList);
		Insertion<T> change = new()
		{
			Index = index,
			Items = itemsList
		};
		Notify(change);
	}

	public void RemoveAt(int index)
	{
		var item = _items[index];
		_items.RemoveAt(index);
		IndexedRemoval<T> change = new()
		{
			Index = index,
			Items = [item]
		};
		Notify(change);
	}

	public void RemoveRange(int index, int count)
	{
		if (count == 0)
			return;
		var items = _items.GetRange(index, count);
		_items.RemoveRange(index, count);
		IndexedRemoval<T> change = new()
		{
			Index = index,
			Items = items
		};
		Notify(change);
	}

	public void Move(int oldIndex, int newIndex)
	{
		var item = _items[oldIndex];
		_items.RemoveAt(oldIndex);
		_items.Insert(newIndex, item);
		Move<T> change = new()
		{
			OldIndex = oldIndex,
			NewIndex = newIndex,
			Items = [item]
		};
		Notify(change);
	}

	public void MoveRange(int oldIndex, int count, int newIndex)
	{
		var movedItems = _items.MoveRange(oldIndex, count, newIndex);
		Move<T> change = new()
		{
			OldIndex = oldIndex,
			NewIndex = newIndex,
			Items = movedItems
		};
		Notify(change);
	}

	public void ReplaceAll(params IEnumerable<T> items)
	{
		var itemsList = items.ToList();
		if (itemsList.Count == 0)
		{
			Clear();
			return;
		}
		Reset<T> change = new()
		{
			OldItems = _items,
			NewItems = itemsList
		};
		_items = new List<T>(itemsList);
		Notify(change);
	}

	public void Dispose()
	{
		foreach (var observer in _observers)
			observer.OnCompleted();
		_observers.Clear();
	}

	private readonly List<IObserver<IndexedChange<T>>> _observers = new();
	private List<T> _items;

	private void SendInitialItems(IObserver<IndexedChange<T>> observer)
	{
		if (_items.Count == 0)
			return;
		Insertion<T> initialChange = new()
		{
			Index = 0,
			Items = _items.ToList()
		};
		observer.OnNext(initialChange);
	}

	private void Notify(IndexedChange<T> change)
	{
		NotifyObservers(change);
		NotifyCollectionChanged(change);
		NotifyCountChanged();
		NotifyIndexerChanged();
	}

	private void NotifyObservers(IndexedChange<T> change)
	{
		foreach (var observer in _observers)
			observer.OnNext(change);
	}

	private void NotifyCollectionChanged(IndexedChange<T> change)
	{
		if (CollectionChanged == null)
			return;
		var args = change.ToNotifyCollectionChangedEventArgs();
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