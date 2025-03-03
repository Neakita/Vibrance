using System.Collections.Specialized;
using NSubstitute;

namespace Vibrance.Tests;

internal static class Extensions
{
	public static IDisposable ObserveChanges<T>(this IObservable<Change<T>> list, out IObserver<Change<T>> observer)
	{
		observer = Substitute.For<IObserver<Change<T>>>();
		return list.Subscribe(observer);
	}

	public static NotifyCollectionObserver ObserveNotifications(this INotifyCollectionChanged notifyCollection)
	{
		return new NotifyCollectionObserver(notifyCollection);
	}
}