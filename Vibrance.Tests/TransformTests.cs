using FluentAssertions;
using Vibrance.Changes;
using Vibrance.Tests.Utilities;

namespace Vibrance.Tests;

public sealed class TransformTests
{
	[Fact]
	public void ShouldObserveInitialItems()
	{
		SourceList<int> source = [1, 2, 3];
		using var observer = source.Transform(number => number.ToString()).ObserveChanges();
		observer.LastObservedValue.NewItems.Should().ContainInOrder("1", "2", "3");
		VerifyDataIntegrity(source, NumberToString, observer);
	}

	[Fact]
	public void ShouldObserveReset()
	{
		SourceList<int> source = [1, 2, 3];
		using var observer = source.Transform(number => number.ToString()).ObserveChanges();
		source.ReplaceAll(4, 5, 6);
		observer.LastObservedValue.Reset.Should().BeTrue();
		observer.LastObservedValue.NewItems.Should().ContainInOrder("4", "5", "6");
		observer.LastObservedValue.OldItems.Should().BeEmpty();
		VerifyDataIntegrity(source, NumberToString, observer);
	}

	[Fact]
	public void ShouldObserveOldItems()
	{
		SourceList<int> source = [1, 2, 3];
		using var observer = source.Transform(number => number.ToString()).ObserveChanges();
		source.RemoveRange(0, 2);
		observer.LastObservedValue.OldItems.Should().ContainInOrder("1", "2");
		VerifyDataIntegrity(source, NumberToString, observer);
	}

	[Fact]
	public void ShouldObserveNewItems()
	{
		SourceList<int> source = [1, 2, 3];
		using var observer = source.Transform(number => number.ToString()).ObserveChanges();
		source.InsertRange(1, 4, 5, 6);
		observer.LastObservedValue.NewItems.Should().ContainInOrder("4", "5", "6");
		observer.LastObservedValue.NewItems.StartIndex.Should().Be(1);
		VerifyDataIntegrity(source, NumberToString, observer);
	}

	[Fact]
	public void ShouldObserveMove()
	{
		SourceList<int> source = [1, 2, 3];
		using var observer = source.Transform(NumberToString).ObserveChanges();
		source.MoveRange(0, 2, 1);
		var observation = observer.LastObservedValue;
		observation.OldItems.StartIndex.Should().Be(0);
		observation.OldItems.Should().ContainInOrder("1", "2");
		observation.NewItems.StartIndex.Should().Be(1);
		observation.NewItems.Should().ContainInOrder("1", "2");
		VerifyDataIntegrity(source, NumberToString, observer);
	}

	private static string NumberToString(int number)
	{
		return number.ToString();
	}

	private static void VerifyDataIntegrity<TSource, TDestination>(
		IReadOnlyList<TSource> source,
		Func<TSource, TDestination> selector,
		RecordingObserver<Change<TDestination>> observer)
	{
		var transformed = ((InnerListProvider<TDestination>)observer.Subscription).Inner;
		transformed.Should().ContainInOrder(source.Select(selector));
	}
}