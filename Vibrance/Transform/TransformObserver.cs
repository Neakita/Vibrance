using System.Collections.ObjectModel;

namespace Vibrance.Transform;

internal sealed class TransformObserver<TSource, TDestination> : IObserver<Change<TSource>>, InnerListProvider<TDestination>, IDisposable
{
	IReadOnlyList<TDestination> InnerListProvider<TDestination>.Inner => _transformedItems;

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

	private void HandleChange(Change<TSource> change)
	{
		var transformedChange = Transform(change);
		transformedChange.ApplyToList(_transformedItems);
		_observer.OnNext(transformedChange);
	}

	private Change<TDestination> Transform(Change<TSource> change) => new()
	{
		OldItems = GetExistingItems(change.OldItemsStartIndex, change.OldItems.Count),
		OldItemsStartIndex = change.OldItemsStartIndex,
		NewItems = Transform(change.NewItems),
		NewItemsStartIndex = change.NewItemsStartIndex,
		Reset = change.Reset
	};

	private IReadOnlyCollection<TDestination> GetExistingItems(int index, int count)
	{
		if (count == 0)
			return ReadOnlyCollection<TDestination>.Empty;
		return _transformedItems.GetRange(index, count);
	}

	private IReadOnlyCollection<TDestination> Transform(IReadOnlyCollection<TSource> items)
	{
		return items.Select(Transform).ToList();
	}

	private TDestination Transform(TSource item)
	{
		return _selector(item);
	}
}