using Vibrance.Changes;
using Vibrance.Utilities;
using Range = Vibrance.Utilities.Range;

namespace Vibrance.Middlewares;

internal sealed class TransformMany<TSource, TTarget> : IDisposable
{
	public TransformMany(IObservable<IndexedChange<TSource>> source, Func<TSource, IObservable<IndexedChange<TTarget>>> selector, IObserver<IndexedChange<TTarget>> observer)
	{
		_selector = selector;
		_observer = observer;
		_disposable = source.Subscribe(HandleChange);
	}

	public void Dispose()
	{
		_disposable.Dispose();
		foreach (var listInfo in _listsInfo)
			listInfo.Disposable.Dispose();
		_listsInfo.Clear();
	}

	private sealed class ListInfo
	{
		public int Index { get; set; }
		public int Offset { get; set; }
		public int Count => Items.Count;
		public List<TTarget> Items { get; } = new();
		public IDisposable Disposable { get; set; } = EmptyDisposable.Instance;
	}

	private readonly Func<TSource, IObservable<IndexedChange<TTarget>>> _selector;
	private readonly IObserver<IndexedChange<TTarget>> _observer;
	private readonly IDisposable _disposable;
	private readonly List<ListInfo> _listsInfo = new();

	private void HandleChange(IndexedChange<TSource> change)
	{
		var oldItems = HandleOldItems(change.OldItemsAsIndexed());
		var newItems = HandleNewItems(change.NewItemsAsIndexed());
		if (change.Factory.TryCreateChange(oldItems, newItems, out var targetChange))
			_observer.OnNext(targetChange);
	}

	private IndexedItems<TTarget> HandleOldItems(IndexedItems<TSource> items)
	{
		if (items.IsEmpty)
			return IndexedItems<TTarget>.Empty;
		var listsRange = Range.FromCount(items.Index, items.Count);
		var targetItems = GetItemsFromLists(listsRange);
		var itemsOffset = _listsInfo[items.Index].Offset;
		RemoveListsInfo(listsRange);
		if (targetItems.Length == 0)
			return IndexedItems<TTarget>.Empty;
		return new IndexedItems<TTarget>(itemsOffset, targetItems);
	}

	private void RemoveListsInfo(Range listsRange)
	{
		DisposeListsSubscriptions(listsRange);
		var itemsCount = CountItemsInLists(listsRange);
		_listsInfo.RemoveRange(listsRange.Start, listsRange.Count);
		ShiftLists(listsRange.Start, -listsRange.Count, -itemsCount);
	}

	private void DisposeListsSubscriptions(Range range)
	{
		for (var i = 0; i < range.Count; i++)
		{
			var listInfo = _listsInfo[range.Start + i];
			listInfo.Disposable.Dispose();
		}
	}

	private TTarget[] GetItemsFromLists(Range range)
	{
		var itemsCount = CountItemsInLists(range);
		var items = new TTarget[itemsCount];
		var itemsCounter = 0;
		for (int i = 0; i < range.Count; i++)
		{
			var listInfo = _listsInfo[range.Start + i];
			listInfo.Items.CopyTo(items, itemsCounter);
			itemsCounter += listInfo.Count;
		}
		return items;
	}

	private int CountItemsInLists(Range range)
	{
		int itemsCount = 0;
		for (var i = 0; i < range.Count; i++)
		{
			var listInfo = _listsInfo[range.Start + i];
			itemsCount += listInfo.Count;
		}
		return itemsCount;
	}

	private void ShiftLists(int listsIndex, int listsCount, int itemsInListsCount)
	{
		foreach (var listInfo in _listsInfo.Skip(listsIndex))
		{
			listInfo.Index += listsCount;
			listInfo.Offset += itemsInListsCount;
		}
	}

	private IndexedItems<TTarget> HandleNewItems(IndexedItems<TSource> items)
	{
		if (items.IsEmpty)
			return IndexedItems<TTarget>.Empty;
		var listsInfo = CreateListsInfo(items);
		var targetItems = GetItemsFromLists(listsInfo);
		ShiftLists(items.Index, items.Count, targetItems.Length);
		_listsInfo.InsertRange(items.Index, listsInfo);
		if (targetItems.Length == 0)
			return IndexedItems<TTarget>.Empty;
		return new IndexedItems<TTarget>(listsInfo[0].Offset, targetItems);
	}

	private ListInfo[] CreateListsInfo(IndexedItems<TSource> items)
	{
		var listsInfo = new ListInfo[items.Count];
		var nextListOffset = GetNextListOffset(items.Index);
		for (int i = 0; i < items.Count; i++)
		{
			var item = items.List[i];
			var listInfo = CreateListInfo(item, items.Index + i, nextListOffset);
			nextListOffset = GetNextListOffset(listInfo);
			listsInfo[i] = listInfo;
		}
		return listsInfo;
	}

	private ListInfo CreateListInfo(TSource item, int index, int offset)
	{
		var listInfo = new ListInfo
		{
			Index = index,
			Offset = offset
		};
		var observable = _selector(item);
		listInfo.Disposable = observable.SubscribeAndGetInitialValue(change => HandleChange(listInfo, change), out var initialChange);
		ProcessInitialChange(listInfo, initialChange);
		return listInfo;
	}

	private static void ProcessInitialChange(ListInfo listInfo, IndexedChange<TTarget>? change)
	{
		if (change != null)
			listInfo.Items.AddRange(change.NewItems);
	}

	private int GetNextListOffset(int listIndex)
	{
		if (listIndex == 0 || _listsInfo.Count <= listIndex - 1)
			return 0;
		var listInfo = _listsInfo[listIndex - 1];
		return GetNextListOffset(listInfo);
	}

	private static int GetNextListOffset(ListInfo listInfo)
	{
		return listInfo.Offset + listInfo.Count;
	}

	private TTarget[] GetItemsFromLists(ListInfo[] listsInfo)
	{
		var itemsCount = CountItemsInLists(listsInfo);
		var items = new TTarget[itemsCount];
		var itemsCounter = 0;
		foreach (var listInfo in listsInfo)
		{
			listInfo.Items.CopyTo(items, itemsCounter);
			itemsCounter += listInfo.Count;
		}
		return items;
	}

	private static int CountItemsInLists(ListInfo[] listsInfo)
	{
		return listsInfo.Sum(listInfo => listInfo.Count);
	}

	private void HandleChange(ListInfo listInfo, IndexedChange<TTarget> change)
	{
		var oldItems = HandleOldItems(listInfo, change.OldItemsAsIndexed());
		var newItems = HandleNewItems(listInfo, change.NewItemsAsIndexed());
		var offsetChange = change.Factory.CreateChange(oldItems, newItems);
		_observer.OnNext(offsetChange);
	}

	private IndexedItems<TTarget> HandleOldItems(ListInfo listInfo, IndexedItems<TTarget> items)
	{
		if (items.IsEmpty)
			return IndexedItems<TTarget>.Empty;
		listInfo.Items.RemoveRange(items.Index, items.Count);
		foreach (var info in _listsInfo.Skip(listInfo.Index + 1))
			info.Offset -= items.Count;
		var offset = _listsInfo.Take(listInfo.Index).Sum(info => info.Items.Count);
		return new IndexedItems<TTarget>(items.Index + offset, items.List);
	}

	private IndexedItems<TTarget> HandleNewItems(ListInfo listInfo, IndexedItems<TTarget> items)
	{
		if (items.IsEmpty)
			return IndexedItems<TTarget>.Empty;
		listInfo.Items.InsertRange(items.Index, items.List);
		foreach (var info in _listsInfo.Skip(listInfo.Index + 1))
			info.Offset += items.Count;
		var offset = _listsInfo.Take(listInfo.Index).Sum(info => info.Items.Count);
		return new IndexedItems<TTarget>(items.Index + offset, items.List);
	}
}