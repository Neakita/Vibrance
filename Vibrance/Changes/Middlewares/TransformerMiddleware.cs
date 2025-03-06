using System.Collections.ObjectModel;

namespace Vibrance.Changes.Middlewares;

internal sealed class TransformerMiddleware<TSource, TDestination> : ChangesMiddleware<TSource, TDestination>, InnerListProvider<TDestination>
{
	IReadOnlyList<TDestination> InnerListProvider<TDestination>.Inner => _transformedItems;

	public TransformerMiddleware(Func<TSource, TDestination> selector)
	{
		_selector = selector;
	}

	private readonly Func<TSource, TDestination> _selector;
	private readonly List<TDestination> _transformedItems = new();

	protected override void HandleChange(Change<TSource> change)
	{
		var transformedChange = Transform(change);
		transformedChange.ApplyToList(_transformedItems);
		DestinationObserver.OnNext(transformedChange);
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