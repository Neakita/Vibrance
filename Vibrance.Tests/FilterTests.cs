using FluentAssertions;
using Vibrance.Changes;
using Vibrance.Changes.Middlewares;
using Vibrance.Tests.Utilities;

namespace Vibrance.Tests;

public sealed class FilterTests
{
	[Fact]
	public void ShouldObserveAllInitialItems()
	{
		SourceList<int> source = [1, 2, 3];
		using var observer = source.Filter(Predicate).ObserveChanges();
		observer.LastObservedValue.NewItems.Should().Contain([1, 2, 3]);
		observer.LastObservedValue.NewItems.StartIndex.Should().Be(0);
		CheckLookupIntegrity(observer.Subscription, source, Predicate);
		return;
		bool Predicate(int _) => true;
	}

	[Fact]
	public void ShouldObserveFilteredInitialItems()
	{
		SourceList<int> source = [1, 2, 3, 4];
		using var observer = source.Filter(Predicate).ObserveChanges();
		observer.LastObservedValue.NewItems.Should().Contain([2, 4]);
		observer.LastObservedValue.NewItems.StartIndex.Should().Be(0);
		CheckLookupIntegrity(observer.Subscription, source, Predicate);
		return;
		bool Predicate(int value) => value % 2 == 0;
	}

	[Fact]
	public void ShouldObserveNewFilteredItems()
	{
		SourceList<int> source = [1, 2, 3, 4];
		using var observer = source.Filter(Predicate).ObserveChanges();
		source.InsertRange(2, [5, 6, 7, 8]);
		observer.LastObservedValue.NewItems.Should().Contain([6, 8]);
		observer.LastObservedValue.NewItems.StartIndex.Should().Be(1);
		CheckLookupIntegrity(observer.Subscription, source, Predicate);
		return;
		bool Predicate(int value) => value % 2 == 0;
	}

	[Fact]
	public void ShouldObserveRangeRemove()
	{
		SourceList<int> source = [1, 2, 3, 4, 5];
		using var observer = source.Filter(Predicate).ObserveChanges();
		source.RemoveRange(1, 2);
		observer.LastObservedValue.OldItems.Should().Contain([2, 3]);
		observer.LastObservedValue.OldItems.StartIndex.Should().Be(1);
		CheckLookupIntegrity(observer.Subscription, source, Predicate);
		return;
		bool Predicate(int _) => true;
	}

	[Fact]
	public void ShouldObserveFilteredRangeRemove()
	{
		SourceList<int> source = [1, 2, 3, 4, 5, 6];
		using var observer = source.Filter(Predicate).ObserveChanges();
		source.RemoveRange(1, 3);
		observer.LastObservedValue.OldItems.Should().Contain([2, 4]);
		observer.LastObservedValue.OldItems.StartIndex.Should().Be(0);
		CheckLookupIntegrity(observer.Subscription, source, Predicate);
		return;
		bool Predicate(int value) => value % 2 == 0;
	}

	[Fact]
	public void ShouldNotObserveFilteredItemMove()
	{
		SourceList<int> source = [1, 2, 3, 4];
		using var observer = source.Filter(Predicate).ObserveChanges();
		var initialChange = observer.LastObservedValue;
		source.Move(0, 1);
		observer.LastObservedValue.Should().Be(initialChange);
		CheckLookupIntegrity(observer.Subscription, source, Predicate);
		return;
		bool Predicate(int value) => value % 2 == 0;
	}

	[Fact]
	public void ShouldObserveReset()
	{
		SourceList<int> source = [1, 2, 3, 4];
		using var observer = source.Filter(Predicate).ObserveChanges();
		source.ReplaceAll(5, 6, 7, 8);
		observer.LastObservedValue.Reset.Should().BeTrue();
		observer.LastObservedValue.NewItems.Should().ContainInOrder(6, 8);
		CheckLookupIntegrity(observer.Subscription, source, Predicate);
		return;
		bool Predicate(int value) => value % 2 == 0;
	}

	private static void CheckLookupIntegrity<T>(IDisposable subscription, IReadOnlyList<T> source, Func<T, bool> predicate)
	{
		var sortSubscription = (FilterMiddleware<T>)subscription;
		var lookup = sortSubscription.SourceToFilteredIndexLookup;
		var filteredIndex = 0;
		List<int> expectedLookup = new(source.Count);
		foreach (var item in source)
			expectedLookup.Add(predicate(item) ? filteredIndex++ : ~filteredIndex);
		lookup.Should().ContainInOrder(expectedLookup);
	}
}