using Vibrance.Changes;

namespace Vibrance.Transform;

internal sealed class TransformObserver<TSource, TDestination> : IObserver<Change<TSource>>, IDisposable
{
	public TransformObserver(
		IObservable<Change<TSource>> source,
		Func<TSource, TDestination> selector,
		IObserver<Change<TDestination>> observer)
	{
		_selector = selector;
		_observer = observer;
		_subscriptionDisposable = source.Subscribe(this);
	}

	public void OnNext(Change<TSource> change)
	{
		HandleChange(change);
	}

	public void OnCompleted()
	{
		_observer.OnCompleted();
	}

	public void OnError(Exception error)
	{
		_observer.OnError(error);
	}

	public void Dispose()
	{
		_subscriptionDisposable.Dispose();
	}

	private readonly IDisposable _subscriptionDisposable;
	private readonly Func<TSource, TDestination> _selector;
	private readonly IObserver<Change<TDestination>> _observer;
	private readonly List<TDestination> _transformedItems = new();

	private Change<TDestination> HandleChange(Change<TSource> change)
	{
		if (change is AddItemChange<TSource> addItemChange)
		{
			var transformed = Transform(addItemChange.Item);
			_transformedItems.Insert(addItemChange.Index, transformed);
			return new AddItemChange<TDestination>(transformed, addItemChange.Index);
		}
		if (change is AddRangeChange<TSource> addRangeChange)
		{
			var transformed = Transform(addRangeChange.Items).ToList();
			_transformedItems.InsertRange(addRangeChange.Count, transformed);
			return new AddRangeChange<TDestination>(transformed, addRangeChange.StartIndex, addRangeChange.Count);
		}
		if (change is AggregateChange<TSource> aggregateChange)
		{
			var transformed = aggregateChange.Changes.Select(HandleChange).ToList();
			return new AggregateChange<TDestination>(transformed);
		}
		if (change is MoveItemChange<TSource> moveItemChange)
		{
			var transformed = _transformedItems[moveItemChange.OldIndex];
			_transformedItems.RemoveAt(moveItemChange.OldIndex);
			_transformedItems.Insert(moveItemChange.NewIndex, transformed);
			return new MoveItemChange<TDestination>(transformed, moveItemChange.OldIndex, moveItemChange.NewIndex);
		}
		if (change is MoveRangeChange<TSource> moveRangeChange)
		{
			var transformed = _transformedItems.GetRange(moveRangeChange.OldStartIndex, moveRangeChange.Count);
			var modifiedNewStartIndex = moveRangeChange.NewStartIndex;
			transformed.RemoveRange(moveRangeChange.OldStartIndex, moveRangeChange.Count);
			if (moveRangeChange.NewStartIndex > moveRangeChange.OldStartIndex)
				modifiedNewStartIndex -= moveRangeChange.Count;
			_transformedItems.InsertRange(modifiedNewStartIndex, transformed);
			return new MoveRangeChange<TDestination>(
				transformed,
				moveRangeChange.OldStartIndex,
				moveRangeChange.NewStartIndex,
				moveRangeChange.Count);
		}
		if (change is RemoveItemChange<TSource> removeItemChange)
		{
			var transformed = _transformedItems[removeItemChange.Index];
			_transformedItems.RemoveAt(removeItemChange.Index);
			return new RemoveItemChange<TDestination>(transformed, removeItemChange.Index);
		}
		if (change is RemoveRangeChange<TSource> removeRangeChange)
		{
			var transformed = _transformedItems.GetRange(removeRangeChange.StartIndex, removeRangeChange.Count);
			_transformedItems.RemoveRange(removeRangeChange.StartIndex, removeRangeChange.Count);
			return new RemoveRangeChange<TDestination>(transformed, removeRangeChange.StartIndex, removeRangeChange.Count);
		}
		if (change is ReplaceItemChange<TSource> replaceItemChange)
		{
			var oldTransformedItem = _transformedItems[replaceItemChange.Index];
			var newTransformedItem = Transform(replaceItemChange.NewItem);
			_transformedItems[replaceItemChange.Index] = newTransformedItem;
			return new ReplaceItemChange<TDestination>(newTransformedItem, oldTransformedItem, replaceItemChange.Index);
		}
		if (change is ResetChange<TSource> resetChange)
		{
			_transformedItems.Clear();
			var transformed = Transform(resetChange.Items).ToList();
			_transformedItems.AddRange(transformed);
			return new ResetChange<TDestination>(transformed);
		}
		throw new ArgumentOutOfRangeException(nameof(change), change, null);
	}

	private IEnumerable<TDestination> Transform(IEnumerable<TSource> items)
	{
		return items.Select(Transform);
	}

	private TDestination Transform(TSource item)
	{
		return _selector(item);
	}
}