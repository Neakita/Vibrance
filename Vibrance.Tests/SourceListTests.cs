using System.Reactive.Concurrency;
using System.Reactive.Linq;
using FluentAssertions;
using Vibrance.Changes;
using Vibrance.Tests.Utilities;

namespace Vibrance.Tests;

public sealed class SourceListTests
{
	[Fact]
	public void ShouldContainAddedItems()
	{
		ObservableList<int> list = new();
		IReadOnlyCollection<int> items = [1, 2, 4];
		list.AddRange(items);
		list.Should().Contain(items);
	}

	[Fact]
	public void ShouldObserveExistingItemsWhenSubscribing()
	{
		ObservableList<int> list = [1, 2, 4];
		using var observer = list.ObserveChanges();
		observer.LastObservedValue.NewItems.Should().Contain([1, 2, 4]);
	}

	[Fact]
	public void ShouldNotObserveAnythingWhenSubscribingOnEmpty()
	{
		ObservableList<int> list = new();
		using var observer = list.ObserveChanges();
		observer.ObservedValues.Should().BeEmpty();
	}

	[Fact]
	public void ShouldObserveNewItem()
	{
		ObservableList<int> list = new();
		using var observer = list.ObserveChanges();
		const int item = 5;
		list.Add(item);
		observer.LastObservedValue.NewItems.Should().Contain(item);
	}

	[Fact]
	public void ShouldReplaceItemViaIndexer()
	{
		ObservableList<int> list = new();
		IReadOnlyCollection<int> items = [1, 2, 4];
		list.AddRange(items);
		const int item = 5;
		list[1] = item;
		list[1].Should().Be(item);
	}

	[Fact]
	public void ShouldReplaceAllItems()
	{
		ObservableList<int> list = new();
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
		ObservableList<int> list = [1, 2, 3];
		list.MoveRange(1, 2, 0);
		list.Should().ContainInOrder(2, 3, 1);
	}

	[Fact]
	public void ShouldMoveItemsForward()
	{
		ObservableList<int> list = [1, 2, 3];
		list.MoveRange(0, 2, 1);
		list.Should().ContainInOrder(3, 1, 2);
	}

	[Fact]
	public void ShouldMoveItem()
	{
		ObservableList<int> list = [1, 2, 3];
		list.Move(0, 1);
		list.Should().ContainInOrder(2, 1, 3);
	}

	[Fact]
	public void ShouldRemoveRange()
	{
		ObservableList<int> list = [1, 2, 3];
		list.RemoveRange(0, 2);
		list.Should().Contain(3);
	}

	[Fact]
	public void ShouldRemoveAtIndex()
	{
		ObservableList<int> list = [1, 2, 3];
		list.RemoveAt(1);
		list.Should().Contain([1, 3]);
	}

	[Fact]
	public void ShouldFindIndex()
	{
		ObservableList<int> list = [1, 2, 3];
		list.IndexOf(2).Should().Be(1);
	}

	[Fact]
	public void ShouldRemoveItem()
	{
		ObservableList<int> list = [1, 2, 3];
		list.Remove(2);
		list.Should().Contain([1, 3]);
	}

	[Fact]
	public void ShouldClearList()
	{
		ObservableList<int> list = [1, 2, 3];
		list.Clear();
		list.Should().BeEmpty();
	}

	[Fact]
	public void ShouldObserveRemovedItems()
	{
		ObservableList<int> list = [1, 2, 3, 4, 5];
		using var observer = list.ObserveChanges();
		list.RemoveRange(2, 2);
		IEnumerable<int> removedItems = [3, 4];
		observer.LastObservedValue.OldItems.Should().Contain(removedItems);
		observer.LastObservedValue.OldIndex.Should().Be(2);
	}

	[Fact]
	public void ShouldObserveInsertedItems()
	{
		ObservableList<int> list = [1, 2, 3];
		using var observer = list.ObserveChanges();
		IReadOnlyCollection<int> newItems = [4, 5];
		list.InsertRange(2, newItems);
		observer.LastObservedValue.NewItems.Should().Contain(newItems);
		observer.LastObservedValue.NewIndex.Should().Be(2);
	}

	[Fact]
	public void ShouldObserverResetWhenClearing()
	{
		ObservableList<int> list = [1, 2, 3];
		using var observer = list.ObserveChanges();
		list.Clear();
		observer.LastObservedValue.Should().BeOfType<Reset<int>>();
	}

	[Fact]
	public void ShouldObserveOnOtherThread()
	{
		ObservableList<int> list = new();
		using var observer = list.ObserveOn(ThreadPoolScheduler.Instance).ObserveChanges();
		list.AddRange([1, 2, 3]);
		Thread.Sleep(1);
		observer.LastObservedValue.NewItems.Should().Contain([1, 2, 3]);
	}
}