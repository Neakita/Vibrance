using System.Collections.ObjectModel;
using Vibrance.Changes;
using Vibrance.Utilities;
using Range = Vibrance.Utilities.Range;

namespace Vibrance.Middlewares;

internal sealed class SorterMiddleware<T> : IndexedChangesMiddleware<T, T>, InnerListProvider<T>
{
	IReadOnlyList<T> InnerListProvider<T>.Inner => _sorted;

	public SorterMiddleware(IComparer<T>? comparer)
	{
		_comparer = comparer ?? Comparer<T>.Default;
	}

	internal IReadOnlyList<int> SourceToSortedIndexLookup => _sourceToSortedIndexLookup;

	protected override void HandleChange(IndexedChange<T> change)
	{
		var removals = HandleOldItems(change.OldItemsAsIndexed());
		var insertionRanges = HandleNewItems(change.NewItemsAsIndexed());
		if (change is Move<T>)
			return;
		if (TryNotifySingleChange(change, removals, insertionRanges))
			return;
		foreach (var removal in removals)
			DestinationObserver.OnNext(new IndexedRemoval<T>
			{
				Index = removal.Index,
				Items = removal.List
			});
		foreach (var range in insertionRanges)
		{
			var items = _sorted.GetRange(range);
			Insertion<T> insertion = new()
			{
				Index = range.Start,
				Items = items
			};
			DestinationObserver.OnNext(insertion);
		}
	}

	private bool TryNotifySingleChange(IndexedChange<T> change, IReadOnlyList<IndexedItems<T>> removals, IReadOnlyList<Range> insertionRanges)
	{
		if (removals.Count > 1 || insertionRanges.Count > 1)
			return false;
		var singleRemoval = removals.SingleOrDefault(IndexedItems<T>.Empty);
		var singleInsertion = insertionRanges is [var singleInsertionRange]
			? new IndexedItems<T>(singleInsertionRange.Start, _sorted.GetRange(singleInsertionRange))
			: IndexedItems<T>.Empty;
		var sortedChange = change.Factory.CreateChange(singleRemoval, singleInsertion);
		DestinationObserver.OnNext(sortedChange);
		return true;
	}

	private readonly IComparer<T> _comparer;
	private readonly List<int> _sourceToSortedIndexLookup = new();
	private readonly List<T> _sorted = new();

	private IReadOnlyList<IndexedItems<T>> HandleOldItems(IndexedItems<T> items)
	{
		if (items.IsEmpty)
			return ReadOnlyCollection<IndexedItems<T>>.Empty;
		var sortedItems = ExistingItemsToOrdered(items);
		RemoveSortedItems(sortedItems);
		RemoveIndexesFromLookup(items);
		return sortedItems;
	}

	private void RemoveIndexesFromLookup(IndexedItems<T> items)
	{
		var startSortedIndex = _sourceToSortedIndexLookup[items.Index];
		_sourceToSortedIndexLookup.RemoveRange(items.Index, items.Count);
		ShiftIndexesLookup(startSortedIndex, -items.Count);
	}

	private List<IndexedItems<T>> ExistingItemsToOrdered(IndexedItems<T> items)
	{
		return items.List
			.Select((_, index) => _sourceToSortedIndexLookup[index + items.Index])
			.Order()
			.ToRanges()
			.Reverse()
			.Select(ToIndexedItems)
			.ToList();
	}

	private IndexedItems<T> ToIndexedItems(Range range)
	{
		return new IndexedItems<T>(range.Start, _sorted.GetRange(range));
	}

	private void RemoveSortedItems(List<IndexedItems<T>> sortedItems)
	{
		foreach (var items in sortedItems)
			_sorted.RemoveRange(items.Index, items.Count);
	}

	private void ShiftIndexesLookup(int startSortedIndex, int delta)
	{
		if (_sourceToSortedIndexLookup.Count == 0)
			return;
		for (var i = 0; i < _sourceToSortedIndexLookup.Count; i++)
			if (_sourceToSortedIndexLookup[i] >= startSortedIndex)
				_sourceToSortedIndexLookup[i] += delta;
	}

	private IReadOnlyList<Range> HandleNewItems(IndexedItems<T> items)
	{
		if (items.IsEmpty)
			return ReadOnlyCollection<Range>.Empty;
		return InsertItemsInOrder(items);
	}

	private List<Range> InsertItemsInOrder(IndexedItems<T> sourceItems)
	{
		List<(int sortedIndex, List<(T item, int sourceLocalIndex)> items)> sortedGroups = sourceItems.List
			.Select((item, sourceIndex) => (item, sourceIndex))
			.GroupBy(tuple => FindIndexToInsert(tuple.item), (sortedIndex, items) => (sortedIndex, items: items.OrderBy(tuple => tuple.item, _comparer).ToList()))
			.OrderBy(tuple => tuple.sortedIndex)
			.ToList();
		int offset = 0;
		int[] lookup = new int[sourceItems.Count];
		List<Range> result = new(sortedGroups.Count);
		foreach (var (sortedIndex, sortedItems) in sortedGroups)
		{
			var itemsList = sortedItems.Select(tuple => tuple.item).ToList();
			_sorted.InsertRange(sortedIndex + offset, itemsList);
			for (var i = 0; i < sortedItems.Count; i++)
			{
				lookup[sortedItems[i].sourceLocalIndex] = sortedIndex + i + offset;
				ShiftIndexesLookup(sortedIndex + i + offset, 1);
			}
			result.Add(Range.FromCount(sortedIndex + offset, sortedItems.Count));
			offset += sortedItems.Count;
		}
		_sourceToSortedIndexLookup.InsertRange(sourceItems.Index, lookup);
		return result;
	}

	private int FindIndexToInsert(T item)
	{
		var index = _sorted.BinarySearch(item, _comparer);
		if (index >= 0)
			throw new ArgumentException("Item already existing");
		return ~index;
	}
}