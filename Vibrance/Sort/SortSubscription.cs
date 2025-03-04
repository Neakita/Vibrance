using Vibrance.Utilities;
using Range = Vibrance.Utilities.Range;

namespace Vibrance.Sort;

internal sealed class SortSubscription<T> : IObserver<Change<T>>, IDisposable
{
	public SortSubscription(IObservable<Change<T>> source, IComparer<T> comparer, IObserver<Change<T>> observer)
	{
		_comparer = comparer;
		_observer = observer;
		_subscription = source.Subscribe(this);
	}

	public void OnNext(Change<T> value)
	{
		HandleOldItems(value);
		HandleNewItems(value);
	}

	public void OnCompleted()
	{
		_observer.OnCompleted();
	}

	public void OnError(Exception error)
	{
		_observer.OnError(error);
	}

	public void Dispose()
	{
		_subscription.Dispose();
	}

	private readonly IComparer<T> _comparer;
	private readonly IObserver<Change<T>> _observer;
	private readonly List<T> _sorted = new();
	private readonly List<int> _sourceToSortedIndexLookup = new();
	private readonly IDisposable _subscription;

	private void HandleOldItems(Change<T> value)
	{
		if (value.OldItems.Count == 0)
			return;
		var changes = PrepareDeletionChanges(value);
		RemoveOrderedItems(value);
		NotifyDeletions(changes);
	}

	private List<Change<T>> PrepareDeletionChanges(Change<T> value)
	{
		return value.OldItems
			.Select((_, index) => _sourceToSortedIndexLookup[index + value.OldItemsStartIndex])
			.Order()
			.ToRanges()
			.Select(ToChange)
			.ToList();
	}

	private Change<T> ToChange(Range range)
	{
		return new Change<T> { OldItems = _sorted.GetRange(range), OldItemsStartIndex = range.Start };
	}

	private void RemoveOrderedItems(Change<T> value)
	{
		for (var i = 0; i < value.OldItems.Count; i++)
		{
			var sourceIndex = value.OldItemsStartIndex + i;
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

	private void NotifyDeletions(List<Change<T>> changes)
	{
		foreach (var change in changes)
			_observer.OnNext(change);
	}

	private void HandleNewItems(Change<T> value)
	{
		if (value.NewItems.Count == 0)
			return;
		InsertItemsInOrder(value);
		NotifyNewSortedItems(value);
	}

	private void InsertItemsInOrder(Change<T> value)
	{
		for (var i = 0; i < value.NewItems.Count; i++)
		{
			var item = value.NewItems[i];
			var sourceIndex = value.NewItemsStartIndex + i;
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

	private void NotifyNewSortedItems(Change<T> value)
	{
		var sortedRanges = value.NewItems
			.Select((_, index) => _sourceToSortedIndexLookup[index + value.NewItemsStartIndex])
			.Order()
			.ToRanges();
		foreach (var range in sortedRanges)
		{
			var sortedRange = _sorted.GetRange(range);
			Change<T> change = new()
			{
				NewItems = sortedRange,
				NewItemsStartIndex = range.Start
			};
			_observer.OnNext(change);
		}
	}
}