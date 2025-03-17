using System.Collections.ObjectModel;
using Vibrance.Changes;

namespace Vibrance.Middlewares;

internal sealed class Transformer<TSource, TDestination> : IndexedChangesMiddleware<TSource, TDestination>, InnerListProvider<TDestination>
{
	IReadOnlyList<TDestination> InnerListProvider<TDestination>.Inner => _transformedItems;

	public Transformer(Func<TSource, TDestination> selector)
	{
		_selector = selector;
	}

	private readonly Func<TSource, TDestination> _selector;
	private readonly List<TDestination> _transformedItems = new();

	protected override void HandleChange(IndexedChange<TSource> change)
	{
		var transformedChange = Transform(change);
		transformedChange.ApplyToList(_transformedItems);
		DestinationObserver.OnNext(transformedChange);
	}

	private IndexedChange<TDestination> Transform(IndexedChange<TSource> change)
	{
		var oldItems = GetExistingItems(change.OldIndex, change.OldItems.Count);
		var newItems = Transform(change.NewItems);
		return change.Factory.CreateChange(
			new IndexedItems<TDestination>(change.OldIndex, oldItems),
			new IndexedItems<TDestination>(change.NewIndex, newItems));
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