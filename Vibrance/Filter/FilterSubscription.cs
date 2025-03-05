namespace Vibrance.Filter;

internal sealed class FilterSubscription<T> : IObserver<Change<T>>, IDisposable
{
	public FilterSubscription(IObservable<Change<T>> source, Func<T, bool> predicate, IObserver<Change<T>> observer)
	{
		_predicate = predicate;
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

	internal IReadOnlyList<int> SourceToFilteredIndexLookup => _sourceToFilteredIndexLookup;

	private readonly Func<T, bool> _predicate;
	private readonly IObserver<Change<T>> _observer;
	private readonly IDisposable _subscription;
	private readonly List<int> _sourceToFilteredIndexLookup = new();

	private void HandleOldItems(Change<T> change)
	{
		if (change.OldItems.Count == 0)
			return;
		var filteredItems = CreateOldFilteredItemsList(change);
		var filteredStartIndex = GetFilteredIndex(change.OldItemsStartIndex);
		RemoveLookupIndexes(change, filteredItems);
		NotifyOldItems(filteredItems, filteredStartIndex);
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

	private void NotifyOldItems(List<T> filteredItems, int filteredStartIndex)
	{
		Change<T> filteredChange = new()
		{
			OldItems = filteredItems,
			OldItemsStartIndex = filteredStartIndex
		};
		_observer.OnNext(filteredChange);
	}

	private void HandleNewItems(Change<T> change)
	{
		if (change.NewItems.Count == 0)
			return;
		int filteredStartIndex = GetFilteredIndex(change.NewItemsStartIndex);
		var lookupIndexes = BuildLookup(change, filteredStartIndex, out var passedItemsCount);
		var filteredItems = CreateNewFilteredItemsList(change, passedItemsCount, lookupIndexes);
		InsertLookupIndexes(change.NewItemsStartIndex, filteredItems.Count, lookupIndexes);
		NotifyNewItems(filteredItems, filteredStartIndex);
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

	private void NotifyNewItems(IReadOnlyList<T> filteredItems, int filteredStartIndex)
	{
		Change<T> change = new()
		{
			NewItems = filteredItems,
			NewItemsStartIndex = filteredStartIndex
		};
		_observer.OnNext(change);
	}
}