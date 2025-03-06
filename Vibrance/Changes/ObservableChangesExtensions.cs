using Vibrance.Changes.Middlewares;
using Vibrance.Utilities;

namespace Vibrance.Changes;

public static class ObservableChangesExtensions
{
	public static IObservable<Change<TDestination>> Transform<TSource, TDestination>(
		this IObservable<Change<TSource>> source,
		Func<TSource, TDestination> selector) =>
		Observable.Create<Change<TDestination>>(observer => new TransformerMiddleware<TSource, TDestination>(selector)
		{
			DestinationObserver = observer,
			SourceObservable = source
		});

	public static IDisposable ToObservableList<T>(this IObservable<Change<T>> source,
		out ReadOnlyObservableList<T> result)
	{
		PostponedConfigurableObserver<Change<T>> subscriptionObserver = new();
		var subscription = source.Subscribe(subscriptionObserver);
		var innerListProvider = source as InnerListProvider<T> ?? subscription as InnerListProvider<T>;
		ChangesHandler<T> changesHandler;
		if (innerListProvider != null)
		{
			ChangeToNotifyCollectionAdapter<T> adapter = new(innerListProvider.Inner);
			changesHandler = adapter;
			result = adapter;
		}
		else
		{
			ObservableList<T> observableList = new();
			changesHandler = observableList;
			result = observableList;
		}

		subscriptionObserver.Observer = new ChangesHandlerObserver<T>(changesHandler);
		return subscription;
	}

	public static IObservable<Change<T>> Sort<T>(this IObservable<Change<T>> source, IComparer<T>? comparer = null) =>
		Observable.Create<Change<T>>(observer => new SorterMiddleware<T>(comparer)
		{
			DestinationObserver = observer,
			SourceObservable = source
		});

	public static IObservable<Change<T>> Filter<T>(this IObservable<Change<T>> source, Func<T, bool> predicate) =>
		Observable.Create<Change<T>>(observer => new FilterMiddleware<T>(predicate)
		{
			DestinationObserver = observer,
			SourceObservable = source
		});
}