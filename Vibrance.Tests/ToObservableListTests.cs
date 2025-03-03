using System.Collections.Specialized;
using FluentAssertions;
using Vibrance.Tests.Utilities;

namespace Vibrance.Tests;

public sealed class ToObservableListTests
{
	[Fact]
	public void ObservableListShouldContainInitialItems()
	{
		SourceList<int> sourceList = [1, 2, 3];
		using var subscription = sourceList.ToObservableList(out var observableList);
		observableList.Should().Contain([1, 2, 3]);
	}

	[Fact]
	public void ShouldObserveResetWhenClearing()
	{
		SourceList<int> sourceList = [1, 2, 3];
		using var subscription = sourceList.ToObservableList(out var observableList);
		using var observer = observableList.ObserveNotifications();
		sourceList.Clear();
		NotifyCollectionChangedEventArgs expectedArgs = new(NotifyCollectionChangedAction.Reset);
		observer.LastObservedArgs.Should().BeEquivalentTo(expectedArgs);
	}

	[Fact]
	public void ShouldObserveReplace()
	{
		SourceList<string> sourceList = ["1", "2", "3"];
		using var subscription = sourceList.ToObservableList(out var observableList);
		using var observer = observableList.ObserveNotifications();
		sourceList[1] = "4";
		NotifyCollectionChangedEventArgs expectedArgs = new(NotifyCollectionChangedAction.Replace, "4", "2", 1);
		observer.LastObservedArgs.Should().BeEquivalentTo(expectedArgs);
	}

	[Fact]
	public void ShouldObserveMove()
	{
		SourceList<int> sourceList = [1, 2, 3];
		using var subscription = sourceList.ToObservableList(out var observableList);
		using var observer = observableList.ObserveNotifications();
		sourceList.Move(0, 1);
		NotifyCollectionChangedEventArgs expectedArgs = new(NotifyCollectionChangedAction.Move, 1, 1, 0);
		observer.LastObservedArgs.Should().BeEquivalentTo(expectedArgs);
	}

	[Fact]
	public void ShouldObserveRemove()
	{
		SourceList<int> sourceList = [1, 2, 3];
		using var subscription = sourceList.ToObservableList(out var observableList);
		using var observer = observableList.ObserveNotifications();
		sourceList.RemoveAt(1);
		NotifyCollectionChangedEventArgs expectedArgs = new(NotifyCollectionChangedAction.Remove, 2, 1);
		observer.LastObservedArgs.Should().BeEquivalentTo(expectedArgs);
	}

	[Fact]
	public void ShouldObserveInsert()
	{
		SourceList<int> sourceList = [1, 2, 3];
		using var subscription = sourceList.ToObservableList(out var observableList);
		using var observer = observableList.ObserveNotifications();
		sourceList.Insert(1, 4);
		NotifyCollectionChangedEventArgs expectedArgs = new(NotifyCollectionChangedAction.Add, 4, 1);
		observer.LastObservedArgs.Should().BeEquivalentTo(expectedArgs);
	}
}