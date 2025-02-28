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

	private readonly INotifyCollectionChanged _collection;
	private readonly IObserver<Change<T>> _observer;
	private bool _disposed;

	private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
	{
		var change = ArgsToChange(args);
		_observer.OnNext(change);
	}

	private Change<T> ArgsToChange(NotifyCollectionChangedEventArgs args) => args switch
	{
		{ Action: NotifyCollectionChangedAction.Add, NewItems: [T item] } => new AddItemChange<T>(item, args.NewStartingIndex),
		{ Action: NotifyCollectionChangedAction.Add, NewItems.Count: > 1} => new AddRangeChange<T>(args.NewItems.Cast<T>(), args.NewStartingIndex, args.NewItems.Count),
		{ Action: NotifyCollectionChangedAction.Remove, OldItems: [T item] } => new RemoveItemChange<T>(item, args.OldStartingIndex),
		{ Action: NotifyCollectionChangedAction.Remove, OldItems.Count: > 1} => new RemoveRangeChange<T>(args.OldItems.Cast<T>(), args.OldStartingIndex, args.OldItems.Count),
		{ Action: NotifyCollectionChangedAction.Replace, NewItems: [T oldItem], OldItems: [T newItem] } => new ReplaceItemChange<T>(newItem, oldItem, args.NewStartingIndex),
		{ Action: NotifyCollectionChangedAction.Move, NewItems: [T item] } => new MoveItemChange<T>(item, args.OldStartingIndex, args.NewStartingIndex),
		{ Action: NotifyCollectionChangedAction.Move, NewItems.Count : > 1 } => new MoveRangeChange<T>(args.NewItems.Cast<T>(), args.OldStartingIndex, args.NewStartingIndex, args.NewItems.Count),
		{ Action: NotifyCollectionChangedAction.Reset } when _collection is IEnumerable<T> enumerable => new ResetChange<T>(enumerable),
		_ => throw new ArgumentOutOfRangeException(nameof(args))
	};

	public void Dispose()
	{
		if (_disposed)
			return;
		_collection.CollectionChanged -= OnCollectionChanged;
		_disposed = true;
	}
}