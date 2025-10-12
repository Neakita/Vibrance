using System.Collections.ObjectModel;
using Vibrance.Changes;
using Vibrance.Utilities;

namespace Vibrance.Middlewares;

internal sealed class Concat<T> : IDisposable
{
	public Concat(IObservable<IndexedChange<T>> firstSource, IObservable<IndexedChange<T>> secondSource, IObserver<IndexedChange<T>> observer)
	{
		_firstSourceSubscription = firstSource.SubscribeAndGetInitialValue(HandleFirstSourceChange, out var firstSourceInitialItems);
		_secondSourceSubscription = secondSource.SubscribeAndGetInitialValue(HandleSecondSourceChange, out var secondSourceInitialItems);
		_observer = observer;
		HandleInitialChanges(firstSourceInitialItems, secondSourceInitialItems);
	}

	public void Dispose()
	{
		_firstSourceSubscription.Dispose();
		_secondSourceSubscription.Dispose();
	}

	internal int SecondSourceItemsOffset { get; private set; }

	private readonly IDisposable _firstSourceSubscription;
	private readonly IDisposable _secondSourceSubscription;
	private readonly IObserver<IndexedChange<T>> _observer;

	private void HandleInitialChanges(IndexedChange<T>? firstSourceChange, IndexedChange<T>? secondSourceChange)
	{
		var firstSourceItems = firstSourceChange?.NewItems ?? ReadOnlyCollection<T>.Empty;
		var secondSourceItems = secondSourceChange?.NewItems ?? ReadOnlyCollection<T>.Empty;
		if (firstSourceItems.Count + secondSourceItems.Count == 0)
			return;
		SecondSourceItemsOffset = firstSourceItems.Count;
		Insertion<T> concatenatedChange = new()
		{
			Index = 0,
			Items = ConcatItems(firstSourceItems, secondSourceItems)
		};
		_observer.OnNext(concatenatedChange);
	}

	private static IReadOnlyList<T> ConcatItems(IReadOnlyList<T> firstList, IReadOnlyList<T> secondList)
	{
		if (firstList.Count > 0 && secondList.Count > 0)
			return firstList.Concat(secondList).ToList();
		if (firstList.Count > 0)
			return firstList;
		if (secondList.Count > 0)
			return secondList;
		return ReadOnlyCollection<T>.Empty;
	}

	private void HandleFirstSourceChange(IndexedChange<T> change)
	{
		if (change is Reset<T> reset)
			change = reset.AsRemovalOrReplacement;
		SecondSourceItemsOffset -= change.OldItems.Count;
		SecondSourceItemsOffset += change.NewItems.Count;
		_observer.OnNext(change);
	}

	private void HandleSecondSourceChange(IndexedChange<T> change)
	{
		if (change is Reset<T> reset)
			change = reset.AsRemovalOrReplacement;
		IndexedItems<T> offsetOldItems = OffsetItems(change.OldItemsAsIndexed());
		IndexedItems<T> offsetNewItems = OffsetItems(change.NewItemsAsIndexed());
		var offsetChange = change.Factory.CreateChange(offsetOldItems, offsetNewItems);
		_observer.OnNext(offsetChange);
	}

	private IndexedItems<T> OffsetItems(IndexedItems<T> items)
	{
		if (items.IsEmpty)
			return items;
		return new IndexedItems<T>(items.Index + SecondSourceItemsOffset, items.List);
	}
}