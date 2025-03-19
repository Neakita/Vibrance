using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using Vibrance.Changes;
using Vibrance.Utilities;

namespace Vibrance;

internal sealed class ChangeableObservableList<T> : ReadOnlyObservableList<T>
{
	public event NotifyCollectionChangedEventHandler? CollectionChanged;
	public event PropertyChangedEventHandler? PropertyChanged;

	public int Count => _items.Count;

	public T this[int index] => _items[index];

	public ChangeableObservableList(IObservable<IndexedChange<T>> changes)
	{
		_subscription = changes.Subscribe(HandleChange);
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

	private void HandleChange(IndexedChange<T> change)
	{
		change.ApplyToList(_items);
		Notify(change);
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