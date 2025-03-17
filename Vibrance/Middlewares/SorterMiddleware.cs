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
		if (removals is [var singleRemoval] && insertionRanges is [var singleInsertionRange])
		{
			var sortedChange = change.Factory.CreateChange(singleRemoval,
				new IndexedItems<T>(singleInsertionRange.Start, _sorted.GetRange(singleInsertionRange)));
			DestinationObserver.OnNext(sortedChange);
			return;
		}
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
		if (items.List.Count == 0)
			return ReadOnlyCollection<Range>.Empty;
		InsertItemsInOrder(items);
		return ExistingItemsToSortedRanges(items);
	}

	private void InsertItemsInOrder(IndexedItems<T> items)
	{
		for (var i = 0; i < items.List.Count; i++)
		{
			var item = items.List[i];
			var sourceIndex = items.Index + i;
			var sortedIndex = InsertItemInOrder(item);
			InsertIndexIntoLookup(sourceIndex, sortedIndex);
		}
	}

	private int InsertItemInOrder(T item)
	{
		var index = FindIndexToInsert(item);
		_sorted.Insert(index, item);
		return index;
	}

	private int FindIndexToInsert(T item)
	{
		var index = _sorted.BinarySearch(item, _comparer);
		if (index >= 0)
			throw new ArgumentException("Item already existing");
		return ~index;
	}

	private void InsertIndexIntoLookup(int sourceIndex, int sortedIndex)
	{
		ShiftIndexesLookupForInsertion(sortedIndex);
		_sourceToSortedIndexLookup.Insert(sourceIndex, sortedIndex);
	}

	private void ShiftIndexesLookupForInsertion(int sortedIndex)
	{
		for (var i = 0; i < _sourceToSortedIndexLookup.Count; i++)
			if (_sourceToSortedIndexLookup[i] >= sortedIndex)
				_sourceToSortedIndexLookup[i]++;
	}

	private List<Range> ExistingItemsToSortedRanges(IndexedItems<T> items)
	{
		var sortedRanges = items.List
			.Select((_, index) => _sourceToSortedIndexLookup[index + items.Index])
			.Order()
			.ToRanges()
			.ToList();
		return sortedRanges;
	}
}