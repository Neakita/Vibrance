using Vibrance.Changes;

namespace Vibrance.Middlewares;

internal sealed class SorterNewItemsHandler<T>
{
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

	public SorterNewItemsHandler(List<int> sourceToSortedIndexLookup, List<T> sortedItems, IComparer<T> comparer)
	{
		_sourceToSortedIndexLookup = sourceToSortedIndexLookup;
		_sortedItems = sortedItems;
		_comparer = comparer;
	}

	public IEnumerable<IndexedItems<T>> HandleNewItems(IndexedItems<T> items)
	{
		if (items.IsEmpty)
			return Enumerable.Empty<IndexedItems<T>>();
		List<InsertionGroup> insertionGroups = CreateInsertionGroups(items);
		UpdateLookup(items, insertionGroups);
		InsertItems(insertionGroups);
		return insertionGroups.Select(GroupToItems);
	}

	private readonly List<int> _sourceToSortedIndexLookup;
	private readonly List<T> _sortedItems;
	private readonly IComparer<T> _comparer;

	private List<InsertionGroup> CreateInsertionGroups(IndexedItems<T> sourceItems)
	{
		List<InsertionGroup> insertionGroups = sourceItems.List
			.Select(CreateIndexedItem)
			.GroupBy(item => FindInsertionIndex(item.Item), CreateInsertionGroup)
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
			var items = group.Items;
			for (var i = 0; i < items.Count; i++)
				insertion[items[i].Index] = group.SortedIndex + i;
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

	private int FindInsertionIndex(T item)
	{
		var index = _sortedItems.BinarySearch(item, _comparer);
		if (index < 0)
			index = ~index;
		return index;
	}

	private void ShiftIndexesLookup(int startSortedIndex, int delta)
	{
		if (_sourceToSortedIndexLookup.Count == 0)
			return;
		for (var i = 0; i < _sourceToSortedIndexLookup.Count; i++)
			if (_sourceToSortedIndexLookup[i] >= startSortedIndex)
				_sourceToSortedIndexLookup[i] += delta;
	}

	private void InsertItems(List<InsertionGroup> insertionGroups)
	{
		foreach (var group in insertionGroups)
		{
			var sortedIndex = group.SortedIndex;
			var sortedItems = group.Items;
			var itemsList = sortedItems.Select(tuple => tuple.Item).ToList();
			_sortedItems.InsertRange(sortedIndex, itemsList);
		}
	}

	private IndexedItems<T> GroupToItems(InsertionGroup group)
	{
		var items = _sortedItems.GetRange(group.SortedIndex, group.Items.Count);
		return new IndexedItems<T>(group.SortedIndex, items);
	}
}