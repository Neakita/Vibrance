using System.Collections.ObjectModel;
using Vibrance.Changes;
using Vibrance.Utilities;
using Range = Vibrance.Utilities.Range;

namespace Vibrance.Middlewares;

internal sealed class Sorter<T> : IndexedChangesMiddleware<T, T>, InnerListProvider<T>
{
	IReadOnlyList<T> InnerListProvider<T>.Inner => _sorted;

	public Sorter(IComparer<T>? comparer)
	{
		_comparer = comparer ?? Comparer<T>.Default;
	}

	internal IReadOnlyList<int> SourceToSortedIndexLookup => _sourceToSortedIndexLookup;

	protected override void HandleChange(IndexedChange<T> change)
	{
		var removals = HandleOldItems(change.OldItemsAsIndexed());
		var insertions = HandleNewItems(change.NewItemsAsIndexed());
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

	private IEnumerable<IndexedItems<T>> HandleNewItems(IndexedItems<T> items)
	{
		if (items.IsEmpty)
			return Enumerable.Empty<IndexedItems<T>>();
		List<InsertionGroup> insertionGroups = CreateInsertionGroups(items);
		UpdateLookup(items, insertionGroups);
		InsertItems(insertionGroups);
		return insertionGroups.Select(group =>
			new IndexedItems<T>(group.SortedIndex, _sorted.GetRange(group.SortedIndex, group.Items.Count)));
	}

	private sealed class IndexedItem
	{
		public int Index { get; }
		public T Item { get; }

		public IndexedItem(int index, T item)
		{
			Index = index;
			Item = item;
		}
	}

	private sealed class InsertionGroup
	{
		public int SortedIndex { get; set; }
		public List<IndexedItem> Items { get; }

		public InsertionGroup(int sortedIndex, List<IndexedItem> items)
		{
			SortedIndex = sortedIndex;
			Items = items;
		}
	}

	private void InsertItems(List<InsertionGroup> insertionGroups)
	{
		foreach (var group in insertionGroups)
		{
			var sortedIndex = group.SortedIndex;
			var sortedItems = group.Items;
			var itemsList = sortedItems.Select(tuple => tuple.Item).ToList();
			_sorted.InsertRange(sortedIndex, itemsList);
		}
	}

	private List<InsertionGroup> CreateInsertionGroups(IndexedItems<T> sourceItems)
	{
		List<InsertionGroup> insertionGroups = sourceItems.List
			.Select(CreateIndexedItem)
			.GroupBy(item => FindIndexToInsert(item.Item), CreateInsertionGroup)
			.OrderBy(group => group.SortedIndex)
			.ToList();
		int offset = 0;
		foreach (var group in insertionGroups)
		{
			group.SortedIndex += offset;
			offset += group.Items.Count;
		}
		return insertionGroups;
	}

	private void UpdateLookup(IndexedItems<T> sourceItems, List<InsertionGroup> insertionGroups)
	{
		ShiftIndexesLookupForInsertions(insertionGroups);
		var insertion = CreateLookupInsertion(sourceItems.Count, insertionGroups);
		_sourceToSortedIndexLookup.InsertRange(sourceItems.Index, insertion);
	}

	private void ShiftIndexesLookupForInsertions(List<InsertionGroup> insertionGroups)
	{
		foreach (var group in insertionGroups)
		{
			var sortedIndex = group.SortedIndex;
			var sortedItems = group.Items;
			ShiftIndexesLookup(sortedIndex, sortedItems.Count);
		}
	}

	private static int[] CreateLookupInsertion(int itemsCount, List<InsertionGroup> insertionGroups)
	{
		int[] insertion = new int[itemsCount];
		foreach (var group in insertionGroups)
		{
			var sortedItems = group.Items;
			var sortedIndex = group.SortedIndex;
			for (var i = 0; i < sortedItems.Count; i++)
				insertion[sortedItems[i].Index] = sortedIndex + i;
		}
		return insertion;
	}

	private static IndexedItem CreateIndexedItem(T item, int sourceIndex)
	{
		return new IndexedItem(sourceIndex, item);
	}

	private InsertionGroup CreateInsertionGroup(int sortedIndex, IEnumerable<IndexedItem> items)
	{
		return new InsertionGroup(sortedIndex, items.OrderBy(item => item.Item, _comparer).ToList());
	}

	private int FindIndexToInsert(T item)
	{
		var index = _sorted.BinarySearch(item, _comparer);
		if (index < 0)
			index = ~index;
		return index;
	}

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