using Vibrance.Changes;
using Vibrance.Utilities;
using Range = Vibrance.Utilities.Range;

namespace Vibrance.Sort;

internal sealed class SortSubscription<T> : IObserver<Change<T>>, InnerListProvider<T>, IDisposable
{
	IReadOnlyList<T> InnerListProvider<T>.Inner => _sorted;

	public SortSubscription(IObservable<Change<T>> source, IComparer<T> comparer, IObserver<Change<T>> observer)
	{
		_comparer = comparer;
		_observer = observer;
		_subscription = source.Subscribe(this);
	}

	public void OnNext(Change<T> value)
	{
		if (value.IsMove())
			HandleMove(value);
		else
		{
			HandleOldItems(value.OldItems);
			HandleNewItems(value.NewItems);
		}
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

	internal IReadOnlyList<int> SourceToSortedIndexLookup => _sourceToSortedIndexLookup;

	private readonly IComparer<T> _comparer;
	private readonly IObserver<Change<T>> _observer;
	private readonly List<T> _sorted = new();
	private readonly List<int> _sourceToSortedIndexLookup = new();
	private readonly IDisposable _subscription;

	private void HandleMove(Change<T> value)
	{
		var oldIndex = value.OldItems.StartIndex;
		var newIndex = value.NewItems.StartIndex;
		_sourceToSortedIndexLookup.MoveRange(oldIndex, value.NewItems.Count, newIndex);
	}

	private void HandleOldItems(PositionalReadOnlyList<T> items)
	{
		if (items.Count == 0)
			return;
		var changes = PrepareDeletionChanges(items);
		RemoveOrderedItems(items);
		NotifyDeletions(changes);
	}

	private List<Change<T>> PrepareDeletionChanges(PositionalReadOnlyList<T> items)
	{
		return items
			.Select((_, index) => _sourceToSortedIndexLookup[index + items.StartIndex])
			.Order()
			.ToRanges()
			.Select(ToChange)
			.ToList();
	}

	private Change<T> ToChange(Range range)
	{
		return new Change<T> { OldItems = new PositionalReadOnlyList<T>(_sorted.GetRange(range), range.Start)};
	}

	private void RemoveOrderedItems(PositionalReadOnlyList<T> items)
	{
		for (var i = items.Count - 1; i >= 0; i--)
		{
			var sourceIndex = items.StartIndex + i;
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

	private void HandleNewItems(PositionalReadOnlyList<T> items)
	{
		if (items.Count == 0)
			return;
		InsertItemsInOrder(items);
		NotifyNewSortedItems(items);
	}

	private void InsertItemsInOrder(PositionalReadOnlyList<T> items)
	{
		for (var i = 0; i < items.Count; i++)
		{
			var item = items[i];
			var sourceIndex = items.StartIndex + i;
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

	private void NotifyNewSortedItems(PositionalReadOnlyList<T> items)
	{
		var sortedRanges = items
			.Select((_, index) => _sourceToSortedIndexLookup[index + items.StartIndex])
			.Order()
			.ToRanges();
		foreach (var range in sortedRanges)
		{
			var sortedRange = _sorted.GetRange(range);
			Change<T> change = new()
			{
				NewItems = new PositionalReadOnlyList<T>(sortedRange, range.Start)
			};
			_observer.OnNext(change);
		}
	}
}