using NSubstitute;

namespace Vibrance.Tests;

public sealed class TransformTests
{
	[Fact]
	public void ShouldTransformInitialItems()
	{
		SourceList<int> list = [1, 2, 3];
		var transformedChanges = list.Transform(number => number.ToString());
		using var subscription = transformedChanges.ObserveChanges(out var observer);
		IEnumerable<string> expectedItems = ["1", "2", "3"];
		observer.Received().OnNext(Arg.Is<Change<string>>(change => change.NewItems.SequenceEqual(expectedItems)));
	}

	[Fact]
	public void ShouldTranslateReset()
	{
		SourceList<int> list = [1, 2, 3];
		var transformedChanges = list.Transform(number => number.ToString());
		using var subscription = transformedChanges.ObserveChanges(out var observer);
		list.Clear();
		observer.Received().OnNext(Arg.Is<Change<string>>(change => change.Reset));
	}

	[Fact]
	public void ShouldTranslateRemovedItems()
	{
		SourceList<int> list = [1, 2, 3];
		var transformedChanges = list.Transform(number => number.ToString());
		using var subscription = transformedChanges.ObserveChanges(out var observer);
		list.RemoveRange(0, 2);
		IEnumerable<string> expectedRemovedItems = ["1", "2"];
		observer.Received().OnNext(Arg.Is<Change<string>>(change => change.OldItems.SequenceEqual(expectedRemovedItems)));
	}
}