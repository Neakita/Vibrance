using FluentAssertions;
using Vibrance.Changes;
using Vibrance.Middlewares;
using Vibrance.Middlewares.Sorting;
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
		CheckDataIntegrity(observer, list);
	}

	[Fact]
	public void ShouldObserveInOrderInitiallyUnOrderedItems()
	{
		SourceList<int> list = [2, 3, 1];
		using var observer = list.Sort().ObserveChanges();
		observer.LastObservedValue.NewItems.Should().ContainInOrder(1, 2, 3);
		CheckDataIntegrity(observer, list);
	}

	[Fact]
	public void ShouldObserveNewItemInProperIndex()
	{
		SourceList<int> list = [1, 2, 4];
		using var observer = list.Sort().ObserveChanges();
		list.Add(3);
		observer.LastObservedValue.NewIndex.Should().Be(2);
		CheckDataIntegrity(observer, list);
	}

	[Fact]
	public void ShouldRemoveItem()
	{
		SourceList<int> list = [1, 2, 3];
		using var observer = list.Sort().ObserveChanges();
		list.Remove(2);
		observer.LastObservedValue.OldItems.Should().Contain(2);
		observer.LastObservedValue.OldIndex.Should().Be(1);
		CheckDataIntegrity(observer, list);
	}

	[Fact]
	public void ShouldRemoveItems()
	{
		SourceList<int> list = [1, 2, 3, 4, 5];
		using var observer = list.Sort().ObserveChanges();
		list.RemoveRange(1, 2);
		observer.LastObservedValue.OldItems.Should().Contain([2, 3]);
		observer.LastObservedValue.OldIndex.Should().Be(1);
		CheckDataIntegrity(observer, list);
	}

	[Fact]
	public void ShouldRemoveItemWhenUnordered()
	{
		SourceList<int> list = [5, 1, 2, 4, 3];
		using var observer = list.Sort().ObserveChanges();
		list.RemoveRange(1, 2);
		observer.LastObservedValue.OldItems.Should().ContainInOrder(1, 2);
		observer.LastObservedValue.OldIndex.Should().Be(0);
		CheckDataIntegrity(observer, list);
	}

	[Fact]
	public void ShouldNotObserveMove()
	{
		SourceList<int> list = [1, 2, 3];
		using var observer = list.Sort().ObserveChanges();
		var initialChange = observer.LastObservedValue;
		list.Move(0, 2);
		observer.LastObservedValue.Should().Be(initialChange);
		CheckDataIntegrity(observer, list);
	}

	[Fact]
	public void ShouldNotObserveMoveRange()
	{
		SourceList<int> list = [1, 2, 3, 4, 5];
		using var observer = list.Sort().ObserveChanges();
		var initialChange = observer.LastObservedValue;
		list.MoveRange(0, 2, 3);
		observer.LastObservedValue.Should().Be(initialChange);
		CheckDataIntegrity(observer, list);
	}

	[Fact]
	public void ShouldAddUnorderedItems()
	{
		SourceList<int> list = [1, 2, 3];
		using var observer = list.Sort().ObserveChanges();
		list.AddRange(6, 4, 5);
		observer.LastObservedValue.NewItems.Should().ContainInOrder(4, 5, 6);
		CheckDataIntegrity(observer, list);
	}

	[Fact]
	public void ShouldObserveReset()
	{
		SourceList<int> list = [5, 1, 2, 4, 3];
		using var observer = list.Sort().ObserveChanges();
		list.ReplaceAll(2, 1, 3);
		observer.LastObservedValue.OldItems.Should().ContainInOrder(1, 2, 3, 4, 5);
		observer.LastObservedValue.NewItems.Should().ContainInOrder(1, 2, 3);
		observer.LastObservedValue.Should().BeOfType<Reset<int>>();
		CheckDataIntegrity(observer, list);
	}

	[Fact]
	public void ShouldInsertUnorderedItems()
	{
		SourceList<int> list = [1, 3, 6];
		using var observer = list.Sort().ObserveChanges();
		list.AddRange(5, 2, 4);
		CheckDataIntegrity(observer, list);
	}

	[Fact]
	public void ShouldAddDuplicate()
	{
		SourceList<int> list = [1, 2, 3];
		using var observer = list.Sort().ObserveChanges();
		list.Add(1);
		CheckDataIntegrity(observer, list);
	}

	private static void CheckDataIntegrity<T>(RecordingObserver<IndexedChange<T>> observer, IReadOnlyList<T> source)
	{
		var subscription = observer.Subscription;
		var sorted = ((InnerListProvider<T>)subscription).Inner;
		CheckInnerListIntegrity(source, sorted);
		CheckLookupIntegrity(subscription, source);
	}

	private static void CheckInnerListIntegrity<T>(IReadOnlyList<T> source, IReadOnlyList<T> sorted)
	{
		sorted.Should().ContainInOrder(source.Order());
	}

	private static void CheckLookupIntegrity<T>(IDisposable subscription, IReadOnlyList<T> source)
	{
		var sortSubscription = (Sorter<T>)subscription;
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