using System.Collections;
using Vibrance.Changes;
using Vibrance.Utilities;

namespace Vibrance;

internal sealed class ChangesSourceListAdapter<T> : ReadOnlySourceList<T>, InnerListProvider<T>, IDisposable
{
	public int Count => _items.Count;

	public T this[int index] => _items[index];

	IReadOnlyList<T> InnerListProvider<T>.Inner => _items;

	public ChangesSourceListAdapter(IDisposable disposable)
	{
		_disposable = disposable;
	}

	public IDisposable Subscribe(IObserver<IndexedChange<T>> observer)
	{
		if (_items.Count != 0)
		{
			Insertion<T> initialInsertion = new()
			{
				Index = 0,
				Items = _items.ToList()
			};
			observer.OnNext(initialInsertion);
		}
		_observers.Add(observer);
		return new ActionDisposable(() => _observers.Remove(observer));
	}

	public IEnumerator<T> GetEnumerator()
	{
		return _items.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Dispose()
	{
		_disposable.Dispose();
		foreach (var observer in _observers)
			observer.OnCompleted();
		_observers.Clear();
	}

	internal void ApplyChange(IndexedChange<T> change)
	{
		change.ApplyToList(_items);
		NotifyObservers(change);
	}

	private readonly List<T> _items = new();
	private readonly IDisposable _disposable;
	private readonly List<IObserver<IndexedChange<T>>> _observers = new();

	private void NotifyObservers(IndexedChange<T> change)
	{
		foreach (var observer in _observers)
			observer.OnNext(change);
	}
}