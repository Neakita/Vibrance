using FluentAssertions;
using Vibrance.Changes;
using Vibrance.Tests.Utilities;

namespace Vibrance.Tests;

public sealed class TransformTests
{
	[Fact]
	public void ShouldTransformInitialItems()
	{
		SourceList<int> list = [1, 2, 3];
		var transformedChanges = list.Transform(number => number.ToString());
		using var observer = transformedChanges.ObserveChanges();
		IEnumerable<string> expectedItems = ["1", "2", "3"];
		observer.LastObservedValue.NewItems.Should().BeEquivalentTo(expectedItems);
	}

	[Fact]
	public void ShouldTranslateReset()
	{
		SourceList<int> list = [1, 2, 3];
		var transformedChanges = list.Transform(number => number.ToString());
		using var observer = transformedChanges.ObserveChanges();
		list.Clear();
		observer.LastObservedValue.Reset.Should().BeTrue();
	}

	[Fact]
	public void ShouldTranslateRemovedItems()
	{
		SourceList<int> list = [1, 2, 3];
		var transformedChanges = list.Transform(number => number.ToString());
		using var observer = transformedChanges.ObserveChanges();
		list.RemoveRange(0, 2);
		IEnumerable<string> expectedRemovedItems = ["1", "2"];
		observer.LastObservedValue.OldItems.Should().BeEquivalentTo(expectedRemovedItems);
	}
}