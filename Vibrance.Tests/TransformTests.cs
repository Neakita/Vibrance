using FluentAssertions;
using Vibrance.Changes;
using Vibrance.Middlewares;
using Vibrance.Tests.Utilities;

namespace Vibrance.Tests;

public sealed class TransformTests
{
	[Fact]
	public void ShouldObserveInitialItems()
	{
		ObservableList<int> source = [1, 2, 3];
		using var observer = source.Transform(number => number.ToString()).ObserveChanges();
		observer.LastObservedValue.NewItems.Should().ContainInOrder("1", "2", "3");
		VerifyDataIntegrity(source, NumberToString, observer);
	}

	[Fact]
	public void ShouldObserveReset()
	{
		ObservableList<int> source = [1, 2, 3];
		using var observer = source.Transform(number => number.ToString()).ObserveChanges();
		source.ReplaceAll(4, 5, 6);
		observer.LastObservedValue.Should().BeOfType<Reset<string>>();
		observer.LastObservedValue.NewItems.Should().ContainInOrder("4", "5", "6");
		VerifyDataIntegrity(source, NumberToString, observer);
	}

	[Fact]
	public void ShouldObserveOldItems()
	{
		ObservableList<int> source = [1, 2, 3];
		using var observer = source.Transform(number => number.ToString()).ObserveChanges();
		source.RemoveRange(0, 2);
		observer.LastObservedValue.OldItems.Should().ContainInOrder("1", "2");
		VerifyDataIntegrity(source, NumberToString, observer);
	}

	[Fact]
	public void ShouldObserveNewItems()
	{
		ObservableList<int> source = [1, 2, 3];
		using var observer = source.Transform(number => number.ToString()).ObserveChanges();
		source.InsertRange(1, 4, 5, 6);
		observer.LastObservedValue.NewItems.Should().ContainInOrder("4", "5", "6");
		observer.LastObservedValue.NewIndex.Should().Be(1);
		VerifyDataIntegrity(source, NumberToString, observer);
	}

	[Fact]
	public void ShouldObserveMove()
	{
		ObservableList<int> source = [1, 2, 3];
		using var observer = source.Transform(NumberToString).ObserveChanges();
		source.MoveRange(0, 2, 1);
		var observation = observer.LastObservedValue;
		observation.OldIndex.Should().Be(0);
		observation.OldItems.Should().ContainInOrder("1", "2");
		observation.NewIndex.Should().Be(1);
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
		RecordingObserver<IndexedChange<TDestination>> observer)
	{
		var transformed = ((Transformer<TSource, TDestination>)observer.Subscription).TransformedItems;
		transformed.Should().ContainInOrder(source.Select(selector));
	}
}