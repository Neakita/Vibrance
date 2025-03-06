using Vibrance.Changes;

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
		var filteredItems = CreateOldFilteredItemsList(items);
		var filteredStartIndex = GetFilteredIndex(items.StartIndex);
		RemoveLookupIndexes(items, filteredItems);
		return new PositionalReadOnlyList<T>(filteredItems, filteredStartIndex);
	}

	private List<T> CreateOldFilteredItemsList(PositionalReadOnlyList<T> items)
	{
		List<T> filteredItems = new();
		for (var i = 0; i < items.Count; i++)
		{
			var sourceIndex = items.StartIndex + i;
			var sortedIndex = _sourceToFilteredIndexLookup[sourceIndex];
			if (sortedIndex >= 0)
				filteredItems.Add(items[i]);
		}

		return filteredItems;
	}

	private void RemoveLookupIndexes(PositionalReadOnlyList<T> sourceItems, List<T> filteredItems)
	{
		_sourceToFilteredIndexLookup.RemoveRange(sourceItems.StartIndex, sourceItems.Count);
		ShiftLookupIndexes(sourceItems.StartIndex, -filteredItems.Count);
	}

	private PositionalReadOnlyList<T> HandleNewItems(PositionalReadOnlyList<T> items)
	{
		if (items.Count == 0)
			return PositionalReadOnlyList<T>.Default;
		int filteredStartIndex = GetFilteredIndex(items.StartIndex);
		var lookupIndexes = BuildLookup(items, filteredStartIndex, out var passedItemsCount);
		var filteredItems = CreateNewFilteredItemsList(items, passedItemsCount, lookupIndexes);
		InsertLookupIndexes(items.StartIndex, filteredItems.Count, lookupIndexes);
		return new PositionalReadOnlyList<T>(filteredItems, filteredStartIndex);
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

	private List<int> BuildLookup(PositionalReadOnlyList<T> items, int filteredStartIndex, out int passedItemsCount)
	{
		passedItemsCount = 0;
		List<int> lookup = new(items.Count);
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

	private static IReadOnlyList<T> CreateNewFilteredItemsList(PositionalReadOnlyList<T> sourceItems, int passedItemsCount, List<int> lookupIndexes)
	{
		if (sourceItems.Count == passedItemsCount)
			return sourceItems;
		List<T> filteredItems = new(passedItemsCount);
		for (int sourceIndex = 0; sourceIndex < sourceItems.Count; sourceIndex++)
		{
			var sortedIndex = lookupIndexes[sourceIndex];
			if (sortedIndex < 0)
				continue;
			var item = sourceItems[sourceIndex];
			filteredItems.Add(item);
		}
		return filteredItems;
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