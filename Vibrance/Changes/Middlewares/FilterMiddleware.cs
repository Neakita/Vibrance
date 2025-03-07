namespace Vibrance.Changes.Middlewares;

internal sealed class FilterMiddleware<T> : ChangesMiddleware<T, T>
{
	public FilterMiddleware(Func<T, bool> predicate)
	{
		_predicate = predicate;
	}

	internal IReadOnlyList<int> SourceToFilteredIndexLookup => _sourceToFilteredIndexLookup;

	protected override void HandleChange(Change<T> change)
	{
		var oldItems = HandleOldItems(change.OldItems);
		var newItems = HandleNewItems(change.NewItems);
		if (oldItems.Count == 0 && newItems.Count == 0 && !change.Reset)
			return;
		if (oldItems.StartIndex == newItems.StartIndex)
			return;
		Change<T> filteredChange = new()
		{
			OldItems = oldItems,
			NewItems = newItems,
			Reset = change.Reset
		};
		DestinationObserver.OnNext(filteredChange);
	}

	private readonly Func<T, bool> _predicate;
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
		int filteredStartIndex;
		// weird edge case when adding items at the end
		if (items.StartIndex == _sourceToFilteredIndexLookup.Count && items.StartIndex != 0)
			filteredStartIndex = GetFilteredIndex(items.StartIndex - 1) + 1;
		else
			filteredStartIndex = GetFilteredIndex(items.StartIndex);
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
			_sourceToFilteredIndexLookup[i] += index >= 0 ? delta : -delta;
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