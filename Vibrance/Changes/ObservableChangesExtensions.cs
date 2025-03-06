using Vibrance.Utilities;

namespace Vibrance.Changes;

public static class ObservableChangesExtensions
{
	public static IObservable<Change<TDestination>> Transform<TSource, TDestination>(
		this IObservable<Change<TSource>> source,
		Func<TSource, TDestination> selector)
	{
		return Observable.Create<Change<TDestination>>(observer =>
			new ChangesTransformer<TSource, TDestination>(source, selector, observer));
	}

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

	public static IObservable<Change<T>> Sort<T>(this IObservable<Change<T>> source, IComparer<T>? comparer = null)
	{
		return Observable.Create<Change<T>>(observer => new ChangesSorter<T>(source, comparer, observer));
	}

	public static IObservable<Change<T>> Filter<T>(this IObservable<Change<T>> source, Func<T, bool> predicate)
	{
		return Observable.Create<Change<T>>(observer => new ChangesFilter<T>(source, predicate, observer));
	}
}