using System.Collections.Specialized;

namespace Vibrance.NotifyCollection;

public static class NotifyCollectionExtensions
{
	public static IObservable<Change<T>> ToObservableChanges<T>(this INotifyCollectionChanged collection)
	{
		return new NotifyCollectionObservable<T>(collection);
	}
}