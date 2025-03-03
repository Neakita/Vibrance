using FluentAssertions;
using NSubstitute;

namespace Vibrance.Tests;

public sealed class SourceListTests
{
	[Fact]
	public void ShouldContainAddedItems()
	{
		SourceList<int> list = new();
		IReadOnlyCollection<int> items = [1, 2, 4];
		list.AddRange(items);
		list.Should().Contain(items);
	}

	[Fact]
	public void ShouldObserveExistingItemsWhenSubscribing()
	{
		SourceList<int> list = new();
		IReadOnlyCollection<int> items = [1, 2, 4];
		list.AddRange(items);
		using var subscription = list.ObserveChanges(out var observer);
		observer.Received().OnNext(Arg.Is<Change<int>>(change => change.NewItems.SequenceEqual(items)));
	}

	[Fact]
	public void ShouldNotObserveAnythingWhenSubscribingOnEmpty()
	{
		SourceList<int> list = new();
		using var subscription = list.ObserveChanges(out var observer);
		observer.DidNotReceive().OnNext(Arg.Any<Change<int>>());
	}

	[Fact]
	public void ShouldObserveNewItem()
	{
		SourceList<int> list = new();
		using var subscription = list.ObserveChanges(out var observer);
		const int item = 5;
		list.Add(item);
		observer.Received().OnNext(Arg.Is<Change<int>>(change => change.NewItems.Single() == item));
	}

	[Fact]
	public void ShouldReplaceItemViaIndexer()
	{
		SourceList<int> list = new();
		IReadOnlyCollection<int> items = [1, 2, 4];
		list.AddRange(items);
		const int item = 5;
		list[1] = item;
		list[1].Should().Be(item);
	}

	[Fact]
	public void ShouldReplaceAllItems()
	{
		SourceList<int> list = new();
		IReadOnlyCollection<int> items1 = [1, 2, 3];
		list.AddRange(items1);
		IReadOnlyCollection<int> items2 = [1, 10, 100];
		list.ReplaceAll(items2);
		list.Should().Contain(items2);
		list.Should().NotContain(items1.Except(items2));
	}

	[Fact]
	public void ShouldMoveItemsBackward()
	{
		SourceList<int> list = [1, 2, 3];
		list.MoveRange(1, 2, 0);
		list.Should().ContainInOrder(2, 3, 1);
	}

	[Fact]
	public void ShouldMoveItemsForward()
	{
		SourceList<int> list = [1, 2, 3];
		list.MoveRange(0, 2, 3);
		list.Should().ContainInOrder(3, 1, 2);
	}

	[Fact]
	public void ShouldMoveItem()
	{
		SourceList<int> list = [1, 2, 3];
		list.Move(0, 1);
		list.Should().ContainInOrder(2, 1, 3);
	}

	[Fact]
	public void ShouldRemoveRange()
	{
		SourceList<int> list = [1, 2, 3];
		list.RemoveRange(0, 2);
		list.Should().Contain(3);
	}

	[Fact]
	public void ShouldRemoveAtIndex()
	{
		SourceList<int> list = [1, 2, 3];
		list.RemoveAt(1);
		list.Should().Contain([1, 3]);
	}

	[Fact]
	public void ShouldFindIndex()
	{
		SourceList<int> list = [1, 2, 3];
		list.IndexOf(2).Should().Be(1);
	}

	[Fact]
	public void ShouldRemoveItem()
	{
		SourceList<int> list = [1, 2, 3];
		list.Remove(2);
		list.Should().Contain([1, 3]);
	}

	[Fact]
	public void ShouldClearList()
	{
		SourceList<int> list = [1, 2, 3];
		list.Clear();
		list.Should().BeEmpty();
	}

	[Fact]
	public void ShouldObserveRemovedItems()
	{
		SourceList<int> list = [1, 2, 3, 4, 5];
		using var subscription = list.ObserveChanges(out var observer);
		list.RemoveRange(2, 2);
		IEnumerable<int> removedItems = [3, 4];
		observer.Received().OnNext(Arg.Is<Change<int>>(change => change.OldItems.SequenceEqual(removedItems) && change.OldItemsStartIndex == 2));
	}

	[Fact]
	public void ShouldObserveInsertedItems()
	{
		SourceList<int> list = [1, 2, 3];
		using var subscription = list.ObserveChanges(out var observer);
		IReadOnlyCollection<int> newItems = [4, 5];
		list.InsertRange(2, newItems);
		observer.Received().OnNext(Arg.Is<Change<int>>(change => change.NewItems.SequenceEqual(newItems) && change.NewItemsStartIndex == 2));
	}

	[Fact]
	public void ShouldObserverResetWhenClearing()
	{
		SourceList<int> list = [1, 2, 3];
		using var subscription = list.ObserveChanges(out var observer);
		list.Clear();
		observer.Received().OnNext(Arg.Is<Change<int>>(change => change.Reset));
	}
}