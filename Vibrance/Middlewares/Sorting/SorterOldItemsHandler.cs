using System.Collections.ObjectModel;
using Vibrance.Changes;
using Vibrance.Utilities;
using Range = Vibrance.Utilities.Range;

namespace Vibrance.Middlewares.Sorting;

internal sealed class SorterOldItemsHandler<T>
{
	public SorterOldItemsHandler(List<int> sourceToSortedIndexLookup, List<T> sortedItems)
	{
		_sourceToSortedIndexLookup = sourceToSortedIndexLookup;
		_sortedItems = sortedItems;
	}

	public IReadOnlyList<IndexedItems<T>> HandleOldItems(IndexedItems<T> items)
	{
		if (items.IsEmpty)
			return ReadOnlyCollection<IndexedItems<T>>.Empty;
		var sortedItems = ExistingItemsToOrdered(items);
		RemoveSortedItems(sortedItems);
		RemoveIndexesFromLookup(items);
		return sortedItems;
	}

	private readonly List<int> _sourceToSortedIndexLookup;
	private readonly List<T> _sortedItems;

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
		return new IndexedItems<T>(range.Start, _sortedItems.GetRange(range));
	}

	private void RemoveSortedItems(List<IndexedItems<T>> sortedItems)
	{
		foreach (var items in sortedItems)
			_sortedItems.RemoveRange(items.Index, items.Count);
	}

	private void ShiftIndexesLookup(int startSortedIndex, int delta)
	{
		if (_sourceToSortedIndexLookup.Count == 0)
			return;
		for (var i = 0; i < _sourceToSortedIndexLookup.Count; i++)
			if (_sourceToSortedIndexLookup[i] >= startSortedIndex)
				_sourceToSortedIndexLookup[i] += delta;
	}
}