using Vibrance.Changes;

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
		HandleOldItems(value.OldItems);
		HandleNewItems(value.NewItems);
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

	private void HandleOldItems(PositionalReadOnlyList<T> items)
	{
		if (items.Count == 0)
			return;
		var filteredItems = FilterItems(items);
		RemoveLookupIndexes(items, filteredItems.Count);
		NotifyOldItems(filteredItems);
	}

	private PositionalReadOnlyList<T> FilterItems(PositionalReadOnlyList<T> items)
	{
		List<T> filteredItems = new();
		for (var i = 0; i < items.Count; i++)
		{
			var sourceIndex = items.StartIndex + i;
			var sortedIndex = _sourceToFilteredIndexLookup[sourceIndex];
			if (sortedIndex >= 0)
				filteredItems.Add(items[i]);
		}
		var filteredStartIndex = GetFilteredIndex(items.StartIndex);
		return new PositionalReadOnlyList<T>(filteredItems, filteredStartIndex);
	}

	private void RemoveLookupIndexes(PositionalReadOnlyList<T> sourceItems, int filteredItemsCount)
	{
		_sourceToFilteredIndexLookup.RemoveRange(sourceItems.StartIndex, sourceItems.Count);
		ShiftLookupIndexes(sourceItems.StartIndex, -filteredItemsCount);
	}

	private void NotifyOldItems(PositionalReadOnlyList<T> items)
	{
		Change<T> filteredChange = new()
		{
			OldItems = items
		};
		_observer.OnNext(filteredChange);
	}

	private void HandleNewItems(PositionalReadOnlyList<T> items)
	{
		if (items.Count == 0)
			return;
		int filteredStartIndex = GetFilteredIndex(items.StartIndex);
		var lookupIndexes = BuildLookup(items, filteredStartIndex);
		var filteredItems = CreateNewFilteredItemsList(items, lookupIndexes);
		InsertLookupIndexes(items.StartIndex, filteredItems.Count, lookupIndexes);
		NotifyNewItems(new PositionalReadOnlyList<T>(filteredItems, filteredStartIndex));
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

	private List<int> BuildLookup(PositionalReadOnlyList<T> sourceItems, int filteredStartIndex)
	{
		List<int> lookup = new(sourceItems.Count);
		var filteredIndex = filteredStartIndex;
		foreach (var item in sourceItems)
		{
			if (_predicate(item))
				lookup.Add(filteredIndex++);
			else
				lookup.Add(~filteredIndex);
		}
		return lookup;
	}

	private static IReadOnlyList<T> CreateNewFilteredItemsList(IReadOnlyList<T> sourceItems, List<int> lookupIndexes)
	{
		List<T> filteredItems = new();
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

	private void NotifyNewItems(PositionalReadOnlyList<T> items)
	{
		Change<T> change = new()
		{
			NewItems = items
		};
		_observer.OnNext(change);
	}
}