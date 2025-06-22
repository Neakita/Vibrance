using System.Collections.Specialized;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using FluentAssertions;
using Vibrance.Changes;

namespace Vibrance.Tests;

public sealed class ReadOnlyNotifyingListTests
{
	[Fact]
	public void ShouldInvokeEventWithArgsFromChange()
	{
		var changesSubject = new Subject<IndexedChange<int>>();
		var observableList = new ChangeableObservableList<int>(changesSubject);
		var notifyingList = new ReadOnlyNotifyingList<int>(observableList);
		var actualNotifications = new List<NotifyCollectionChangedEventArgs>();
		notifyingList.CollectionChanged += OnCollectionChanged;
		var change = new Insertion<int>
		{
			Index = 0,
			Items = [1, 2, 3]
		};
		var expectedNotification = change.ToNotifyCollectionChangedEventArgs();
		changesSubject.OnNext(change);
		actualNotifications.Should().ContainSingle().Which.Should().BeEquivalentTo(expectedNotification);

		void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => actualNotifications.Add(e);
	}

	[Fact]
	public void ShouldNotSendItemsFromInitialChange()
	{
		var observableList = new ObservableList<int>([1, 2, 3]);
		var notifyingList = new ReadOnlyNotifyingList<int>(observableList);
		var actualNotifications = new List<NotifyCollectionChangedEventArgs>();
		notifyingList.CollectionChanged += OnCollectionChanged;
		actualNotifications.Should().BeEmpty();

		void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => actualNotifications.Add(e);
	}

	[Fact]
	public void ShouldNotifyCountChange()
	{
		var changesSubject = new Subject<IndexedChange<int>>();
		var observableList = new ChangeableObservableList<int>(changesSubject);
		var notifyingList = new ReadOnlyNotifyingList<int>(observableList);
		var actualNotifications = new List<PropertyChangedEventArgs>();
		notifyingList.PropertyChanged += OnPropertyChanged;

		var change = new Insertion<int>
		{
			Index = 0,
			Items = [1, 2, 3]
		};
		changesSubject.OnNext(change);
		actualNotifications.Should().ContainSingle(notification => notification.PropertyName == "Count");

		void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			actualNotifications.Add(e);
		}
	}

	[Fact]
	public void ShouldUnsubscribeWhenHandlersRemoved()
	{
		bool isDisposed = false;
		var observableList = new FakeReadOnlyObservableList<int>(_ => Disposable.Create(() => isDisposed = true));
		var notifyingList = new ReadOnlyNotifyingList<int>(observableList);

		notifyingList.CollectionChanged += OnCollectionChanged;
		notifyingList.PropertyChanged += OnPropertyChanged;

		notifyingList.CollectionChanged -= OnCollectionChanged;
		notifyingList.PropertyChanged -= OnPropertyChanged;

		isDisposed.Should().BeTrue();

		void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
		}

		void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
		}
	}
}