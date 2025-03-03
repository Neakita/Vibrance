using System.Collections.Specialized;
using FluentAssertions;
using NSubstitute;

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
		var handler = observableList.ObserveNotifications();
		sourceList.Clear();
		handler.Received().Invoke(observableList, Arg.Is<NotifyCollectionChangedEventArgs>(args => args.Action == NotifyCollectionChangedAction.Reset));
	}

	[Fact]
	public void ShouldObserveReplace()
	{
		SourceList<int> sourceList = [1, 2, 3];
		using var subscription = sourceList.ToObservableList(out var observableList);
		var handler = observableList.ObserveNotifications();
		sourceList[1] = 4;
		handler.Received().Invoke(observableList, Arg.Is<NotifyCollectionChangedEventArgs>(args => args.Action == NotifyCollectionChangedAction.Replace && args.OldItems != null && args.OldItems.Contains(2) && args.NewItems != null && args.NewItems.Contains(4) && args.NewStartingIndex == 1));
	}

	[Fact]
	public void ShouldObserveMove()
	{
		SourceList<int> sourceList = [1, 2, 3];
		using var subscription = sourceList.ToObservableList(out var observableList);
		var handler = observableList.ObserveNotifications();
		sourceList.Move(0, 1);
		handler.Received().Invoke(observableList, Arg.Is<NotifyCollectionChangedEventArgs>(args => args.Action == NotifyCollectionChangedAction.Move && args.OldStartingIndex == 0 && args.NewStartingIndex == 1));
	}

	[Fact]
	public void ShouldObserveRemove()
	{
		SourceList<int> sourceList = [1, 2, 3];
		using var subscription = sourceList.ToObservableList(out var observableList);
		var handler = observableList.ObserveNotifications();
		sourceList.RemoveAt(1);
		handler.Received().Invoke(observableList, Arg.Is<NotifyCollectionChangedEventArgs>(args => args.Action == NotifyCollectionChangedAction.Remove && args.OldStartingIndex == 1 && args.OldItems != null && args.OldItems.Contains(2)));
	}

	[Fact]
	public void ShouldObserveInsert()
	{
		SourceList<int> sourceList = [1, 2, 3];
		using var subscription = sourceList.ToObservableList(out var observableList);
		var handler = observableList.ObserveNotifications();
		sourceList.Insert(1, 4);
		handler.Received().Invoke(observableList, Arg.Is<NotifyCollectionChangedEventArgs>(args => args.Action == NotifyCollectionChangedAction.Add && args.NewStartingIndex == 1 && args.NewItems != null && args.NewItems.Contains(4)));
	}
}