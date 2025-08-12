using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using Vibrance.Changes;
using Vibrance.Utilities;

namespace Vibrance;

public sealed class ReadOnlyNotifyingList<T> :
	IReadOnlyList<T>,
	IList,
	INotifyCollectionChanged,
	INotifyPropertyChanged
{
	public event NotifyCollectionChangedEventHandler? CollectionChanged
	{
		add
		{
			InternalCollectionChanged += value;
			UpdateSubscription();
		}
		remove
		{
			InternalCollectionChanged -= value;
			UpdateSubscription();
		}
	}

	public event PropertyChangedEventHandler? PropertyChanged
	{
		add
		{
			InternalPropertyChanged += value;
			UpdateSubscription();
		}
		remove
		{
			InternalPropertyChanged -= value;
			UpdateSubscription();
		}
	}

	public int Count => _observableList.Count;

	public T this[int index] => _observableList[index];

	public bool IsFixedSize => ((IList)_observableList).IsFixedSize;

	public bool IsReadOnly => ((IList)_observableList).IsReadOnly;

	object? IList.this[int index]
	{
		get => _observableList[index];
		set => ((IList)_observableList)[index] = value;
	}

	public bool IsSynchronized => ((ICollection)_observableList).IsSynchronized;

	public object SyncRoot => ((ICollection)_observableList).SyncRoot;

	public ReadOnlyNotifyingList(ReadOnlyObservableList<T> observableList)
	{
		_observableList = observableList;
	}

	public IEnumerator<T> GetEnumerator()
	{
		return _observableList.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)_observableList).GetEnumerator();
	}

	public void CopyTo(Array array, int index)
	{
		((ICollection)_observableList).CopyTo(array, index);
	}
	
	public int Add(object? value)
	{
		return ((IList)_observableList).Add(value);
	}

	public void Clear()
	{
		((IList)_observableList).Clear();
	}

	public bool Contains(object? value)
	{
		return ((IList)_observableList).Contains(value);
	}

	public int IndexOf(object? value)
	{
		for (var i = 0; i < _observableList.Count; i++)
			if (Equals(_observableList[i], value))
				return i;
		return -1;
	}

	public void Insert(int index, object? value)
	{
		((IList)_observableList).Insert(index, value);
	}

	public void Remove(object? value)
	{
		((IList)_observableList).Remove(value);
	}

	public void RemoveAt(int index)
	{
		((IList)_observableList).RemoveAt(index);
	}

	private event NotifyCollectionChangedEventHandler? InternalCollectionChanged;
	private event PropertyChangedEventHandler? InternalPropertyChanged;

	private readonly ReadOnlyObservableList<T> _observableList;
	private IDisposable? _disposable;
	private bool _ignoreChanges;

	private void UpdateSubscription()
	{
		var hasHandlers = InternalCollectionChanged != null || InternalPropertyChanged != null;
		if (hasHandlers)
			Subscribe();
		else
			Unsubscribe();
	}

	private void Unsubscribe()
	{
		if (_disposable == null)
			return;
		_disposable.Dispose();
		_disposable = null;
	}

	private void Subscribe()
	{
		if (_disposable != null)
			return;
		_ignoreChanges = true;
		_disposable = _observableList.Subscribe(OnChange);
		_ignoreChanges = false;
	}

	private void OnChange(IndexedChange<T> change)
	{
		if (_ignoreChanges)
			return;
		NotifyCollectionChanged(change);
		NotifyCountChanged(change);
		NotifyIndexerChanged(change);
	}

	private void NotifyCollectionChanged(IndexedChange<T> change)
	{
		if (InternalCollectionChanged == null)
			return;
		var notification = change.ToNotifyCollectionChangedEventArgs();
		InternalCollectionChanged.Invoke(this, notification);
	}

	private void NotifyCountChanged(IndexedChange<T> change)
	{
		if (change.NewItems.Count != change.OldItems.Count)
			InternalPropertyChanged?.Invoke(this, KnownPropertyChangedEventArgs.CountChangedEventArgs);
	}

	private void NotifyIndexerChanged(IndexedChange<T> change)
	{
		if (change.NewItems.Count > 0 || change.OldItems.Count > 0)
			InternalPropertyChanged?.Invoke(this, KnownPropertyChangedEventArgs.IndexerChangedEventArgs);
	}
}