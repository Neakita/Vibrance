using FluentAssertions;
using Vibrance.Changes;
using Vibrance.Tests.Utilities;

namespace Vibrance.Tests;

public sealed class TransformTests
{
	[Fact]
	public void ShouldObserveInitialItems()
	{
		SourceList<int> list = [1, 2, 3];
		using var observer = list.Transform(number => number.ToString()).ObserveChanges();
		observer.LastObservedValue.NewItems.Should().ContainInOrder("1", "2", "3");
	}

	[Fact]
	public void ShouldObserveReset()
	{
		SourceList<int> list = [1, 2, 3];
		using var observer = list.Transform(number => number.ToString()).ObserveChanges();
		list.Clear();
		observer.LastObservedValue.Reset.Should().BeTrue();
	}

	[Fact]
	public void ShouldObserveRemovedItems()
	{
		SourceList<int> list = [1, 2, 3];
		using var observer = list.Transform(number => number.ToString()).ObserveChanges();
		list.RemoveRange(0, 2);
		observer.LastObservedValue.OldItems.Should().ContainInOrder("1", "2");
	}
}