using System.Collections.Specialized;
using Vibrance.Changes;

namespace Vibrance.NotifyCollection;

public static class NotifyCollectionExtensions
{
	public static IObservable<Change<T>> ToObservableChanges<T>(this INotifyCollectionChanged collection)
	{
		return new NotifyCollectionObservable<T>(collection);
	}
}