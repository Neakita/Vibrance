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

	private void HandleNewItems(Change<T> change)
	{
		if (change.NewItems.Count == 0)
			return;
		int filteredStartIndex = GetIndexForInsertion(change.NewItemsStartIndex);
		var lookupIndexes = BuildLookup(change, filteredStartIndex, out var passedItemsCount);
		var filteredItems = CreateFilteredItemsList(change, passedItemsCount, lookupIndexes);
		InsertLookupIndexes(change.NewItemsStartIndex, filteredItems.Count, lookupIndexes);
		NotifyNewItems(filteredItems, filteredStartIndex);
	}

	private int GetIndexForInsertion(int sourceIndex)
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

	private IReadOnlyList<T> CreateFilteredItemsList(Change<T> change, int passedItemsCount, List<int> lookupIndexes)
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
		ShiftLookupIndexesForInsertion(sourceStartIndex, filteredItemsCount);
		_sourceToFilteredIndexLookup.InsertRange(sourceStartIndex, lookupIndexes);
	}

	private void ShiftLookupIndexesForInsertion(int sourceStartIndex, int filteredItemsCount)
	{
		for (var i = sourceStartIndex; i < _sourceToFilteredIndexLookup.Count; i++)
		{
			var index = _sourceToFilteredIndexLookup[i];
			var delta = index > 0 ? filteredItemsCount : -filteredItemsCount;
			_sourceToFilteredIndexLookup[i] += delta;
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