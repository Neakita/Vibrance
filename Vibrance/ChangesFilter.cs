using System.Collections.ObjectModel;

namespace Vibrance;

internal sealed class ChangesFilter<T> : IObserver<Change<T>>, IDisposable
{
	public ChangesFilter(IObservable<Change<T>> source, Func<T, bool> predicate, IObserver<Change<T>> observer)
	{
		_predicate = predicate;
		_observer = observer;
		_subscription = source.Subscribe(this);
	}

	public void OnNext(Change<T> value)
	{
		var (oldStartIndex, oldItems) = HandleOldItems(value);
		var (newStartIndex, newItems) = HandleNewItems(value);
		if (oldItems.Count == 0 && newItems.Count == 0)
			return;
		Change<T> change = new()
		{
			OldItems = oldItems,
			OldItemsStartIndex = oldStartIndex,
			NewItems = newItems,
			NewItemsStartIndex = newStartIndex,
			Reset = value.Reset
		};
		_observer.OnNext(change);
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

	internal IReadOnlyList<int> SourceToFilteredIndexLookup => _sourceToFilteredIndexLookup;

	private readonly Func<T, bool> _predicate;
	private readonly IObserver<Change<T>> _observer;
	private readonly IDisposable _subscription;
	private readonly List<int> _sourceToFilteredIndexLookup = new();

	private (int startIndex, IReadOnlyList<T> items) HandleOldItems(Change<T> change)
	{
		if (change.OldItems.Count == 0)
			return (-1, ReadOnlyCollection<T>.Empty);
		var filteredItems = CreateOldFilteredItemsList(change);
		var filteredStartIndex = GetFilteredIndex(change.OldItemsStartIndex);
		RemoveLookupIndexes(change, filteredItems);
		return (filteredStartIndex, filteredItems);
	}

	private List<T> CreateOldFilteredItemsList(Change<T> change)
	{
		List<T> filteredItems = new();
		for (var i = 0; i < change.OldItems.Count; i++)
		{
			var sourceIndex = change.OldItemsStartIndex + i;
			var sortedIndex = _sourceToFilteredIndexLookup[sourceIndex];
			if (sortedIndex >= 0)
				filteredItems.Add(change.OldItems[i]);
		}

		return filteredItems;
	}

	private void RemoveLookupIndexes(Change<T> change, List<T> filteredItems)
	{
		_sourceToFilteredIndexLookup.RemoveRange(change.OldItemsStartIndex, change.OldItems.Count);
		ShiftLookupIndexes(change.OldItemsStartIndex, -filteredItems.Count);
	}

	private (int startIndex, IReadOnlyList<T> items) HandleNewItems(Change<T> change)
	{
		if (change.NewItems.Count == 0)
			return (-1, ReadOnlyCollection<T>.Empty);
		int filteredStartIndex = GetFilteredIndex(change.NewItemsStartIndex);
		var lookupIndexes = BuildLookup(change, filteredStartIndex, out var passedItemsCount);
		var filteredItems = CreateNewFilteredItemsList(change, passedItemsCount, lookupIndexes);
		InsertLookupIndexes(change.NewItemsStartIndex, filteredItems.Count, lookupIndexes);
		return (filteredStartIndex, filteredItems);
	}

	private int GetFilteredIndex(int sourceIndex)
	{
		if (_sourceToFilteredIndexLookup.Count == 0)
			return 0;
		int filteredIndex = _sourceToFilteredIndexLookup[sourceIndex];
		if (filteredIndex < 0)
			filteredIndex = ~filteredIndex;
		return filteredIndex;
	}

	private List<int> BuildLookup(Change<T> change, int filteredStartIndex, out int passedItemsCount)
	{
		passedItemsCount = 0;
		List<int> lookup = new(change.NewItems.Count);
		var filteredIndex = filteredStartIndex;
		foreach (var item in change.NewItems)
		{
			if (_predicate(item))
			{
				lookup.Add(filteredIndex++);
				passedItemsCount++;
			}
			else
			{
				lookup.Add(~filteredIndex);
			}
		}
		return lookup;
	}

	private static IReadOnlyList<T> CreateNewFilteredItemsList(Change<T> change, int passedItemsCount, List<int> lookupIndexes)
	{
		if (change.NewItems.Count == passedItemsCount)
			return change.NewItems;
		List<T> items = new(passedItemsCount);
		for (int sourceIndex = 0; sourceIndex < change.NewItems.Count; sourceIndex++)
		{
			var sortedIndex = lookupIndexes[sourceIndex];
			if (sortedIndex < 0)
				continue;
			var item = change.NewItems[sourceIndex];
			items.Add(item);
		}
		return items;
	}

	private void InsertLookupIndexes(int sourceStartIndex, int filteredItemsCount, List<int> lookupIndexes)
	{
		ShiftLookupIndexes(sourceStartIndex, filteredItemsCount);
		_sourceToFilteredIndexLookup.InsertRange(sourceStartIndex, lookupIndexes);
	}

	private void ShiftLookupIndexes(int sourceStartIndex, int delta)
	{
		for (var i = sourceStartIndex; i < _sourceToFilteredIndexLookup.Count; i++)
		{
			var index = _sourceToFilteredIndexLookup[i];
			_sourceToFilteredIndexLookup[i] += index > 0 ? delta : -delta;
		}
	}
}