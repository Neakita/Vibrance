using Vibrance.Changes;

namespace Vibrance.Middlewares;

internal sealed class Filter<T> : IndexedChangesMiddleware<T, T>
{
	public Filter(Func<T, bool> predicate)
	{
		_predicate = predicate;
	}

	internal IReadOnlyList<int> SourceToFilteredIndexLookup => _sourceToFilteredIndexLookup;

	protected override void HandleChange(IndexedChange<T> change)
	{
		var oldItems = HandleOldItems(change.OldItemsAsIndexed());
		var newItems = HandleNewItems(change.NewItemsAsIndexed());
		if (change.Factory.TryCreateChange(oldItems, newItems, out var filteredChange))
			DestinationObserver.OnNext(filteredChange);
	}

	private readonly Func<T, bool> _predicate;
	private readonly List<int> _sourceToFilteredIndexLookup = new();

	private IndexedItems<T> HandleOldItems(IndexedItems<T> items)
	{
		if (items.List.Count == 0)
			return IndexedItems<T>.Empty;
		var filteredItems = CreateFilteredItemsList(items);
		RemoveLookupIndexes(items, filteredItems.Count);
		return filteredItems;
	}

	private void RemoveLookupIndexes(IndexedItems<T> sourceItems, int filteredItemsCount)
	{
		_sourceToFilteredIndexLookup.RemoveRange(sourceItems.Index, sourceItems.Count);
		ShiftLookupIndexes(sourceItems.Index, -filteredItemsCount);
	}

	private IndexedItems<T> HandleNewItems(IndexedItems<T> items)
	{
		if (items.Count == 0)
			return IndexedItems<T>.Empty;
		AppendLookup(items);
		return CreateFilteredItemsList(items);
	}

	private void AppendLookup(IndexedItems<T> items)
	{
		var lookupIndexes = BuildLookup(items, out var filteredItemsCount);
		InsertLookupIndexes(items.Index, filteredItemsCount, lookupIndexes);
	}

	private List<int> BuildLookup(IndexedItems<T> items, out int passedItemsCount)
	{
		passedItemsCount = 0;
		List<int> lookup = new(items.Count);
		int filteredStartIndex;
		// weird edge case when adding items at the end
		if (items.Index == _sourceToFilteredIndexLookup.Count && items.Index != 0)
			filteredStartIndex = GetFilteredIndex(items.Index - 1) + 1;
		else
			filteredStartIndex = GetFilteredIndex(items.Index);
		var filteredIndex = filteredStartIndex;
		foreach (var item in items.List)
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

	private IndexedItems<T> CreateFilteredItemsList(IndexedItems<T> sourceItems)
	{
		List<T> filteredItems = new();
		for (var i = 0; i < sourceItems.Count; i++)
		{
			var sourceIndex = sourceItems.Index + i;
			if (IsPassedFilterAt(sourceIndex))
				filteredItems.Add(sourceItems.List[i]);
		}
		if (filteredItems.Count == 0)
			return IndexedItems<T>.Empty;
		var filteredStartIndex = GetFilteredIndex(sourceItems.Index);
		return new IndexedItems<T>(filteredStartIndex, filteredItems);
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