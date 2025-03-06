namespace Vibrance.Changes;

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
		var oldItems = HandleOldItems(value.OldItems);
		var newItems = HandleNewItems(value.NewItems);
		if (oldItems.Count == 0 && newItems.Count == 0)
			return;
		Change<T> change = new()
		{
			OldItems = oldItems,
			NewItems = newItems,
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

	private PositionalReadOnlyList<T> HandleOldItems(PositionalReadOnlyList<T> items)
	{
		if (items.Count == 0)
			return PositionalReadOnlyList<T>.Default;
		var filteredItems = CreateFilteredItemsList(items);
		RemoveLookupIndexes(items, filteredItems.Count);
		return filteredItems;
	}

	private void RemoveLookupIndexes(PositionalReadOnlyList<T> sourceItems, int filteredItemsCount)
	{
		_sourceToFilteredIndexLookup.RemoveRange(sourceItems.StartIndex, sourceItems.Count);
		ShiftLookupIndexes(sourceItems.StartIndex, -filteredItemsCount);
	}

	private PositionalReadOnlyList<T> HandleNewItems(PositionalReadOnlyList<T> items)
	{
		if (items.Count == 0)
			return PositionalReadOnlyList<T>.Default;
		AppendLookup(items);
		return CreateFilteredItemsList(items);
	}

	private void AppendLookup(PositionalReadOnlyList<T> items)
	{
		var lookupIndexes = BuildLookup(items, out var filteredItemsCount);
		InsertLookupIndexes(items.StartIndex, filteredItemsCount, lookupIndexes);
	}

	private List<int> BuildLookup(PositionalReadOnlyList<T> items, out int passedItemsCount)
	{
		passedItemsCount = 0;
		List<int> lookup = new(items.Count);
		int filteredStartIndex = GetFilteredIndex(items.StartIndex);
		var filteredIndex = filteredStartIndex;
		foreach (var item in items)
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

	private PositionalReadOnlyList<T> CreateFilteredItemsList(PositionalReadOnlyList<T> sourceItems)
	{
		List<T> filteredItems = new();
		for (var i = 0; i < sourceItems.Count; i++)
		{
			var sourceIndex = sourceItems.StartIndex + i;
			if (IsPassedFilterAt(sourceIndex))
				filteredItems.Add(sourceItems[i]);
		}
		var filteredStartIndex = GetFilteredIndex(sourceItems.StartIndex);
		return new PositionalReadOnlyList<T>(filteredItems, filteredStartIndex);
	}

	private bool IsPassedFilterAt(int sourceIndex)
	{
		var sortedIndex = _sourceToFilteredIndexLookup[sourceIndex];
		return sortedIndex >= 0;
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
}