using Vibrance.Changes;

namespace Vibrance.Middlewares.Sorting;

internal sealed class Sorter<T> : IndexedChangesMiddleware<T, T>
{
	public Sorter(IComparer<T>? comparer)
	{
		_oldItemsHandler = new SorterOldItemsHandler<T>(_sourceToSortedIndexLookup, SortedItems);
		comparer ??= Comparer<T>.Default;
		_newItemsHandler = new SorterNewItemsHandler<T>(_sourceToSortedIndexLookup, SortedItems, comparer);
	}

	internal List<T> SortedItems { get; } = new();
	internal IReadOnlyList<int> SourceToSortedIndexLookup => _sourceToSortedIndexLookup;

	protected override void HandleChange(IndexedChange<T> change)
	{
		var removedItems = _oldItemsHandler.HandleOldItems(change.OldItemsAsIndexed());
		var addedItems = _newItemsHandler.HandleNewItems(change.NewItemsAsIndexed());
		if (change is Move<T>)
			return;
		var addedItemsList = addedItems.ToList();
		if (TryNotifySingleChange(change, removedItems, addedItemsList))
			return;
		foreach (var removal in removedItems.Select(ItemsToRemoval))
			DestinationObserver.OnNext(removal);
		foreach (var insertion in addedItemsList.Select(ItemsToInsertion))
			DestinationObserver.OnNext(insertion);
	}

	private readonly List<int> _sourceToSortedIndexLookup = new();
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

	private static IndexedRemoval<T> ItemsToRemoval(IndexedItems<T> items)
	{
		return new IndexedRemoval<T>
		{
			Index = items.Index,
			Items = items.List
		};
	}

	private static Insertion<T> ItemsToInsertion(IndexedItems<T> items)
	{
		return new Insertion<T>
		{
			Index = items.Index,
			Items = items.List
		};
	}
}