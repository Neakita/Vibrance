using System.Collections.ObjectModel;

namespace Vibrance.Changes;

internal sealed class ChangesTransformer<TSource, TDestination> : IObserver<Change<TSource>>, InnerListProvider<TDestination>, IDisposable
{
	IReadOnlyList<TDestination> InnerListProvider<TDestination>.Inner => _transformedItems;

	public ChangesTransformer(
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

	private void HandleChange(Change<TSource> change)
	{
		var transformedChange = Transform(change);
		transformedChange.ApplyToList(_transformedItems);
		_observer.OnNext(transformedChange);
	}

	private Change<TDestination> Transform(Change<TSource> change)
	{
		var oldItems = GetExistingItems(change.OldItems.StartIndex, change.OldItems.Count);
		var newItems = Transform(change.NewItems);
		return new Change<TDestination>
		{
			OldItems = new PositionalReadOnlyList<TDestination>(oldItems, change.OldItems.StartIndex),
			NewItems = new PositionalReadOnlyList<TDestination>(newItems, change.NewItems.StartIndex),
			Reset = change.Reset
		};
	}

	private IReadOnlyList<TDestination> GetExistingItems(int index, int count)
	{
		if (count == 0)
			return ReadOnlyCollection<TDestination>.Empty;
		return _transformedItems.GetRange(index, count);
	}

	private IReadOnlyList<TDestination> Transform(IReadOnlyCollection<TSource> items)
	{
		if (items.Count == 0)
			return ReadOnlyCollection<TDestination>.Empty;
		return items.Select(Transform).ToList();
	}

	private TDestination Transform(TSource item)
	{
		return _selector(item);
	}
}