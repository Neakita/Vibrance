using Vibrance.Changes;

namespace Vibrance.Middlewares;

internal sealed class Sorter<T> : IndexedChangesMiddleware<T, T>, InnerListProvider<T>
{
	IReadOnlyList<T> InnerListProvider<T>.Inner => _sorted;

	public Sorter(IComparer<T>? comparer)
	{
		_oldItemsHandler = new SorterOldItemsHandler<T>(_sourceToSortedIndexLookup, _sorted);
		comparer ??= Comparer<T>.Default;
		_newItemsHandler = new SorterNewItemsHandler<T>(_sourceToSortedIndexLookup, _sorted, comparer);
	}

	internal IReadOnlyList<int> SourceToSortedIndexLookup => _sourceToSortedIndexLookup;

	protected override void HandleChange(IndexedChange<T> change)
	{
		var removals = _oldItemsHandler.HandleOldItems(change.OldItemsAsIndexed());
		var insertions = _newItemsHandler.HandleNewItems(change.NewItemsAsIndexed());
		if (change is Move<T>)
			return;
		var insertionsList = insertions.ToList();
		if (TryNotifySingleChange(change, removals, insertionsList))
			return;
		foreach (var items in removals)
		{
			IndexedRemoval<T> removal = new()
			{
				Index = items.Index,
				Items = items.List
			};
			DestinationObserver.OnNext(removal);
		}

		foreach (var items in insertionsList)
		{
			Insertion<T> insertion = new()
			{
				Index = items.Index,
				Items = items.List
			};
			DestinationObserver.OnNext(insertion);
		}
	}

	private readonly List<int> _sourceToSortedIndexLookup = new();
	private readonly List<T> _sorted = new();
	private readonly SorterOldItemsHandler<T> _oldItemsHandler;
	private readonly SorterNewItemsHandler<T> _newItemsHandler;

	private bool TryNotifySingleChange(IndexedChange<T> change, IReadOnlyList<IndexedItems<T>> removals, IReadOnlyList<IndexedItems<T>> insertions)
	{
		if (removals.Count > 1 || insertions.Count > 1)
			return false;
		var singleRemoval = removals.SingleOrDefault(IndexedItems<T>.Empty);
		var singleInsertion = insertions.SingleOrDefault(IndexedItems<T>.Empty);
		var sortedChange = change.Factory.CreateChange(singleRemoval, singleInsertion);
		DestinationObserver.OnNext(sortedChange);
		return true;
	}
}