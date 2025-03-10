using System.Collections;
using Vibrance.Changes;
using Vibrance.Utilities;

namespace Vibrance;

public sealed class SourceList<T> : IList<T>, IReadOnlyList<T>, IObservable<Change<T>>, InnerListProvider<T>
{
	public int Count => _items.Count;

	public IDisposable Subscribe(IObserver<Change<T>> observer)
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
			Change<T> change = new()
			{
				OldItems = new PositionalReadOnlyList<T>([oldItem], index),
				NewItems = new PositionalReadOnlyList<T>([value], index)
			};
			NotifyObservers(change);
		}
	}

	IReadOnlyList<T> InnerListProvider<T>.Inner => _items;

	public SourceList()
	{
		_items = new List<T>();
	}

	public SourceList(int capacity)
	{
		_items = new List<T>(capacity);
	}

	public SourceList(IEnumerable<T> collection)
	{
		_items = new List<T>(collection);
	}

	public void Add(T item)
	{
		var index = _items.Count;
		Insert(index, item);
	}

	public void AddRange(IEnumerable<T> items)
	{
		var index = _items.Count;
		InsertRange(index, items);
	}

	public void Clear()
	{
		_items.Clear();
		Change<T> change = new()
		{
			Reset = true
		};
		NotifyObservers(change);
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
		Change<T> change = new()
		{
			NewItems = new PositionalReadOnlyList<T>([item], index)
		};
		NotifyObservers(change);
	}

	public void InsertRange(int index, IEnumerable<T> items)
	{
		var itemsList = items.ToList();
		_items.InsertRange(index, itemsList);
		Change<T> change = new()
		{
			NewItems = new PositionalReadOnlyList<T>(itemsList, index)
		};
		NotifyObservers(change);
	}

	public void RemoveAt(int index)
	{
		var item = _items[index];
		_items.RemoveAt(index);
		Change<T> change = new()
		{
			OldItems = new PositionalReadOnlyList<T>([item], index)
		};
		NotifyObservers(change);
	}

	public void RemoveRange(int index, int count)
	{
		if (count == 0)
			return;
		var items = _items.GetRange(index, count);
		_items.RemoveRange(index, count);
		Change<T> change = new()
		{
			OldItems = new PositionalReadOnlyList<T>(items, index)
		};
		NotifyObservers(change);
	}

	public void Move(int oldIndex, int newIndex)
	{
		var item = _items[oldIndex];
		_items.RemoveAt(oldIndex);
		_items.Insert(newIndex, item);
		IReadOnlyList<T> itemAsCollection = [item];
		Change<T> change = new()
		{
			OldItems = new PositionalReadOnlyList<T>(itemAsCollection, oldIndex),
			NewItems = new PositionalReadOnlyList<T>(itemAsCollection, newIndex)
		};
		NotifyObservers(change);
	}

	public void MoveRange(int oldIndex, int count, int newIndex)
	{
		var movedItems = _items.MoveRange(oldIndex, count, newIndex);
		Change<T> change = new()
		{
			OldItems = new PositionalReadOnlyList<T>(movedItems, oldIndex),
			NewItems = new PositionalReadOnlyList<T>(movedItems, newIndex)
		};
		NotifyObservers(change);
	}

	public void ReplaceAll(IEnumerable<T> items)
	{
		_items.Clear();
		var itemsList = items.ToList();
		_items.AddRange(itemsList);
		Change<T> change = new()
		{
			Reset = true,
			NewItems = new PositionalReadOnlyList<T>(itemsList, 0)
		};
		NotifyObservers(change);
	}

	private readonly List<T> _items;
	private readonly List<IObserver<Change<T>>> _observers = new();

	private void SendInitialItems(IObserver<Change<T>> observer)
	{
		if (_items.Count == 0)
			return;
		Change<T> initialChange = new()
		{
			NewItems = new PositionalReadOnlyList<T>(_items.ToList(), 0)
		};
		observer.OnNext(initialChange);
	}

	private void NotifyObservers(Change<T> change)
	{
		foreach (var observer in _observers)
			observer.OnNext(change);
	}
}