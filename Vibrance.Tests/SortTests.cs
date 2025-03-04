using FluentAssertions;
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
	}

	[Fact]
	public void ShouldObserveInOrderInitiallyUnOrderedItems()
	{
		SourceList<int> list = [2, 3, 1];
		using var observer = list.Sort().ObserveChanges();
		observer.LastObservedValue.NewItems.Should().ContainInOrder(1, 2, 3);
	}

	[Fact]
	public void ShouldObserveNewItemInProperIndex()
	{
		SourceList<int> list = [1, 2, 4];
		using var observer = list.Sort().ObserveChanges();
		list.Add(3);
		observer.LastObservedValue.NewItemsStartIndex.Should().Be(2);
	}

	[Fact]
	public void ShouldRemoveItem()
	{
		SourceList<int> list = [1, 2, 3];
		using var observer = list.Sort().ObserveChanges();
		list.Remove(2);
		observer.LastObservedValue.OldItems.Should().Contain(2);
		observer.LastObservedValue.OldItemsStartIndex.Should().Be(1);
	}

	[Fact]
	public void ShouldRemoveItems()
	{
		SourceList<int> list = [1, 2, 3, 4, 5];
		using var observer = list.Sort().ObserveChanges();
		list.RemoveRange(1, 2);
		observer.LastObservedValue.OldItems.Should().Contain([2, 3]);
		observer.LastObservedValue.OldItemsStartIndex.Should().Be(1);
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
	}
}