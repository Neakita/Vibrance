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
		if (change is Move<T> move)
			HandleMove(move);
		else if (change is Reset<T> reset)
			HandleReset(reset);
		else
		{
			HandleOldItems(change.OldItemsAsIndexed());
			HandleNewItems(change.NewItemsAsIndexed());
		}
	}

	private readonly IComparer<T> _comparer;
	private readonly List<int> _sourceToSortedIndexLookup = new();
	private List<T> _sorted = new();

	private void HandleReset(Reset<T> items)
	{
		var oldSortedItems = _sorted;
		_sorted = new List<T>();
		_sourceToSortedIndexLookup.Clear();
		InsertItemsInOrder(items.NewItemsAsIndexed());
		Reset<T> change = new()
		{
			OldItems = oldSortedItems,
			NewItems = _sorted.ToList()
		};
		DestinationObserver.OnNext(change);
	}

	private void HandleMove(Move<T> value)
	{
		var oldIndex = value.OldIndex;
		var newIndex = value.NewIndex;
		_sourceToSortedIndexLookup.MoveRange(oldIndex, value.Items.Count, newIndex);
	}

	private void HandleOldItems(IndexedItems<T> items)
	{
		if (items.List.Count == 0)
			return;
		var changes = PrepareDeletionChanges(items);
		RemoveOrderedItems(items);
		NotifyObserver(changes);
	}

	private IReadOnlyList<IndexedChange<T>> PrepareDeletionChanges(IndexedItems<T> items)
	{
		return items.List
			.Select((_, index) => _sourceToSortedIndexLookup[index + items.Index])
			.Order()
			.ToRanges()
			.Select(ToChange)
			.ToList();
	}

	private IndexedRemoval<T> ToChange(Range range)
	{
		return new IndexedRemoval<T>
		{
			Index = range.Start,
			Items = _sorted.GetRange(range)
		};
	}

	private void RemoveOrderedItems(IndexedItems<T> items)
	{
		for (var i = items.List.Count - 1; i >= 0; i--)
		{
			var sourceIndex = items.Index + i;
			var sortedIndex = _sourceToSortedIndexLookup[sourceIndex];
			_sorted.RemoveAt(sortedIndex);
			RemoveIndexFromLookup(sourceIndex);
		}
	}

	private void RemoveIndexFromLookup(int sourceIndex)
	{
		var sortedIndex = _sourceToSortedIndexLookup[sourceIndex];
		_sourceToSortedIndexLookup.RemoveAt(sourceIndex);
		ShiftIndexesLookupForRemoval(sortedIndex);
	}

	private void ShiftIndexesLookupForRemoval(int sortedIndex)
	{
		for (var i = 0; i < _sourceToSortedIndexLookup.Count; i++)
			if (_sourceToSortedIndexLookup[i] >= sortedIndex)
				_sourceToSortedIndexLookup[i]--;
	}

	private void NotifyObserver(IEnumerable<IndexedChange<T>> changes)
	{
		foreach (var change in changes)
			DestinationObserver.OnNext(change);
	}

	private void HandleNewItems(IndexedItems<T> items)
	{
		if (items.List.Count == 0)
			return;
		InsertItemsInOrder(items);
		NotifyNewSortedItems(items);
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

	private void NotifyNewSortedItems(IndexedItems<T> items)
	{
		var sortedRanges = items.List
			.Select((_, index) => _sourceToSortedIndexLookup[index + items.Index])
			.Order()
			.ToRanges();
		foreach (var range in sortedRanges)
		{
			var sortedRange = _sorted.GetRange(range);
			Insertion<T> change = new()
			{
				Index = range.Start,
				Items = sortedRange
			};
			DestinationObserver.OnNext(change);
		}
	}
}