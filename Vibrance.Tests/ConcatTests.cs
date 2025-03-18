using FluentAssertions;
using Vibrance.Changes;
using Vibrance.Middlewares;
using Vibrance.Tests.Utilities;

namespace Vibrance.Tests;

public sealed class ConcatTests
{
	[Fact]
	public void ShouldObserveInitialItems()
	{
		SourceList<int> firstSource = [1, 2, 3];
		SourceList<int> secondSource = [4, 5, 6];
		using var observer = firstSource.Concatenate(secondSource).ObserveChanges();
		observer.LastObservedValue.NewIndex.Should().Be(0);
		observer.LastObservedValue.NewItems.Should().ContainInOrder(1, 2, 3, 4, 5, 6);
		ValidateSecondSourceItemsOffset(observer, firstSource);
	}

	[Fact]
	public void ShouldObserveInsertionWhenAddingToFirstSource()
	{
		SourceList<int> firstSource = [1, 2, 3];
		SourceList<int> secondSource = [4, 5, 6];
		using var observer = firstSource.Concatenate(secondSource).ObserveChanges();
		firstSource.AddRange(7, 8);
		observer.LastObservedValue.NewIndex.Should().Be(3);
		observer.LastObservedValue.NewItems.Should().ContainInOrder(7, 8);
		ValidateSecondSourceItemsOffset(observer, firstSource);
	}

	[Fact]
	public void ShouldObserveClearAsRemoval()
	{
		SourceList<int> firstSource = [1, 2, 3];
		SourceList<int> secondSource = [4, 5, 6];
		using var observer = firstSource.Concatenate(secondSource).ObserveChanges();
		firstSource.Clear();
		observer.LastObservedValue.Should().BeOfType<IndexedRemoval<int>>();
		observer.LastObservedValue.OldItems.Should().ContainInOrder(1, 2, 3);
		observer.LastObservedValue.OldIndex.Should().Be(0);
		observer.LastObservedValue.NewItems.Should().BeEmpty();
		ValidateSecondSourceItemsOffset(observer, firstSource);
	}

	private static void ValidateSecondSourceItemsOffset(RecordingObserver<IndexedChange<int>> observer, SourceList<int> firstSource)
	{
		var middleware = (Concat<int>)observer.Subscription;
		middleware.SecondSourceItemsOffset.Should().Be(firstSource.Count);
	}
}