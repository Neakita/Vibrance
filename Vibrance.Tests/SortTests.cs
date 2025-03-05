using FluentAssertions;
using Vibrance.Sort;
using Vibrance.Tests.Utilities;

namespace Vibrance.Tests;

public sealed class SortTests
{
	[Fact]
	public void ShouldObserveInitiallySortedItems()
	{
		SourceList<int> list = [1, 2, 3];
		using var observer = list.Sort().ObserveChanges();
		observer.LastObservedValue.NewItems.Should().ContainInOrder(1, 2, 3);
		CheckDataIntegrity(observer.Subscription, list);
	}

	[Fact]
	public void ShouldObserveInOrderInitiallyUnOrderedItems()
	{
		SourceList<int> list = [2, 3, 1];
		using var observer = list.Sort().ObserveChanges();
		observer.LastObservedValue.NewItems.Should().ContainInOrder(1, 2, 3);
		CheckDataIntegrity(observer.Subscription, list);
	}

	[Fact]
	public void ShouldObserveNewItemInProperIndex()
	{
		SourceList<int> list = [1, 2, 4];
		using var observer = list.Sort().ObserveChanges();
		list.Add(3);
		observer.LastObservedValue.NewItemsStartIndex.Should().Be(2);
		CheckDataIntegrity(observer.Subscription, list);
	}

	[Fact]
	public void ShouldRemoveItem()
	{
		SourceList<int> list = [1, 2, 3];
		using var observer = list.Sort().ObserveChanges();
		list.Remove(2);
		observer.LastObservedValue.OldItems.Should().Contain(2);
		observer.LastObservedValue.OldItemsStartIndex.Should().Be(1);
		CheckDataIntegrity(observer.Subscription, list);
	}

	[Fact]
	public void ShouldRemoveItems()
	{
		SourceList<int> list = [1, 2, 3, 4, 5];
		using var observer = list.Sort().ObserveChanges();
		list.RemoveRange(1, 2);
		observer.LastObservedValue.OldItems.Should().Contain([2, 3]);
		observer.LastObservedValue.OldItemsStartIndex.Should().Be(1);
		CheckDataIntegrity(observer.Subscription, list);
	}

	[Fact]
	public void ShouldRemoveItemWhenUnordered()
	{
		SourceList<int> list = [5, 1, 2, 4, 3];
		using var observer = list.Sort().ObserveChanges();
		list.RemoveRange(1, 2);
		Change<int> expectedChange = new()
		{
			OldItems = [1, 2],
			OldItemsStartIndex = 0
		};
		observer.LastObservedValue.Should().BeEquivalentTo(expectedChange);
		CheckDataIntegrity(observer.Subscription, list);
	}

	[Fact]
	public void ShouldNotObserveMove()
	{
		SourceList<int> list = [1, 2, 3];
		using var observer = list.Sort().ObserveChanges();
		var initialChange = observer.LastObservedValue;
		list.Move(0, 2);
		observer.LastObservedValue.Should().Be(initialChange);
		CheckDataIntegrity(observer.Subscription, list);
	}

	[Fact]
	public void ShouldNotObserveMoveRange()
	{
		SourceList<int> list = [1, 2, 3, 4, 5];
		using var observer = list.Sort().ObserveChanges();
		var initialChange = observer.LastObservedValue;
		list.MoveRange(0, 2, 3);
		observer.LastObservedValue.Should().Be(initialChange);
		CheckDataIntegrity(observer.Subscription, list);
	}

	[Fact]
	public void ShouldAddUnorderedItems()
	{
		SourceList<int> list = [1, 2, 3];
		using var observer = list.Sort().ObserveChanges();
		list.AddRange([6, 4, 5]);
		observer.LastObservedValue.NewItems.Should().ContainInOrder(4, 5, 6);
		CheckDataIntegrity(observer.Subscription, list);
	}

	private static void CheckDataIntegrity<T>(IDisposable subscription, IReadOnlyList<T> source)
	{
		CheckInnerListIntegrity<T>(subscription);
		CheckLookupIntegrity(subscription, source);
	}

	private static void CheckInnerListIntegrity<T>(IDisposable subscription)
	{
		var sorted = ((InnerListProvider<T>)subscription).Inner;
		sorted.Should().ContainInOrder(sorted.Order());
	}

	private static void CheckLookupIntegrity<T>(IDisposable subscription, IReadOnlyList<T> source)
	{
		var sortSubscription = (SortSubscription<T>)subscription;
		var sorted = ((InnerListProvider<T>)subscription).Inner;
		var lookup = sortSubscription.SourceToSortedIndexLookup;
		for (var i = 0; i < source.Count; i++)
		{
			var sortedIndex = lookup[i];
			var actual = sorted[sortedIndex];
			var expected = source[i];
			actual.Should().Be(expected);
		}
	}
}