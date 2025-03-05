using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Vibrance.Changes;

namespace Vibrance.NotifyCollection;

internal sealed class NotifyCollectionSubscription<T> : IDisposable
{
	public NotifyCollectionSubscription(INotifyCollectionChanged collection, IObserver<Change<T>> observer)
	{
		collection.CollectionChanged += OnCollectionChanged;
		_collection = collection;
		_observer = observer;
	}

	public void Dispose()
	{
		if (_disposed)
			return;
		_collection.CollectionChanged -= OnCollectionChanged;
		_disposed = true;
	}

	private readonly INotifyCollectionChanged _collection;
	private readonly IObserver<Change<T>> _observer;
	private bool _disposed;

	private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
	{
		var change = ArgsToChange(args);
		_observer.OnNext(change);
	}

	private static Change<T> ArgsToChange(NotifyCollectionChangedEventArgs args)
	{
		var oldItems = GetItems(args.OldItems);
		var newItems = GetItems(args.NewItems);
		return new Change<T>
		{
			OldItems = oldItems,
			OldItemsStartIndex = args.OldStartingIndex,
			NewItems = newItems,
			NewItemsStartIndex = args.NewStartingIndex,
			Reset = args.Action == NotifyCollectionChangedAction.Reset
		};
	}

	private static IReadOnlyList<T> GetItems(IList? list)
	{
		if (list == null)
			return ReadOnlyCollection<T>.Empty;
		return list.Cast<T>().ToList();
	}
}