using System.Collections.ObjectModel;
using FluentAssertions;
using Vibrance.NotifyCollection;
using Vibrance.Tests.Utilities;

namespace Vibrance.Tests;

public sealed class NotifyCollectionAsObservableChangesTests
{
	[Fact]
	public void ShouldObserveInitialItems()
	{
		ObservableCollection<int> collection = [1, 2, 3];
		using var observer = collection.ToObservableChanges<int>().ObserveChanges();
		observer.LastObservedValue.NewItems.Should().Contain([1, 2, 3]);
	}

	[Fact]
	public void ShouldObserveNewItem()
	{
		ObservableCollection<int> collection = [1, 2, 3];
		using var observer = collection.ToObservableChanges<int>().ObserveChanges();
		collection.Add(4);
		observer.LastObservedValue.NewItems.Should().Contain(4);
	}

	[Fact]
	public void ShouldNotObserveWhenSubscribingOnEmpty()
	{
		ObservableCollection<int> collection = new();
		using var observer = collection.ToObservableChanges<int>().ObserveChanges();
		observer.ObservedValues.Should().BeEmpty();
	}
}