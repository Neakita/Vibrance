using System.Collections;
using Vibrance.Changes;
using Vibrance.Utilities;

namespace Vibrance;

internal sealed class ChangesSourceListAdapter<T> : ReadOnlySourceList<T>
{
	public int Count => _items.Count;

	public T this[int index] => _items[index];

	public ChangesSourceListAdapter(IObservable<IndexedChange<T>> changes)
	{
		_subscription = changes.Subscribe(ApplyChange);
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
		_subscription.Dispose();
		foreach (var observer in _observers)
			observer.OnCompleted();
		_observers.Clear();
	}

	private readonly List<T> _items = new();
	private readonly IDisposable _subscription;
	private readonly List<IObserver<IndexedChange<T>>> _observers = new();

	private void ApplyChange(IndexedChange<T> change)
	{
		change.ApplyToList(_items);
		NotifyObservers(change);
	}

	private void NotifyObservers(IndexedChange<T> change)
	{
		foreach (var observer in _observers)
			observer.OnNext(change);
	}
}