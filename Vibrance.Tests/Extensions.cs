using System.Collections.Specialized;

namespace Vibrance.Tests;

internal static class Extensions
{
	public static RecordingObserver<T> ObserveChanges<T>(this IObservable<T> observable)
	{
		return new RecordingObserver<T>(observable);
	}

	public static NotifyCollectionObserver ObserveNotifications(this INotifyCollectionChanged notifyCollection)
	{
		return new NotifyCollectionObserver(notifyCollection);
	}
}