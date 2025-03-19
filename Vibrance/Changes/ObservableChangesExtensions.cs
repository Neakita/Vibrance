using Vibrance.Middlewares;
using Vibrance.Middlewares.Sorting;
using Vibrance.Utilities;

namespace Vibrance.Changes;

public static class ObservableChangesExtensions
{
	public static IObservable<IndexedChange<TDestination>> Transform<TSource, TDestination>(
		this IObservable<IndexedChange<TSource>> source,
		Func<TSource, TDestination> selector) =>
		Observable.Create<IndexedChange<TDestination>>(observer => new Transformer<TSource, TDestination>(selector)
		{
			DestinationObserver = observer,
			SourceObservable = source
		});

	public static IDisposable ToObservableList<T>(this IObservable<IndexedChange<T>> source,
		out ReadOnlyObservableList<T> result)
	{
		PostponedConfigurableObserver<IndexedChange<T>> subscriptionObserver = new();
		var subscription = source.Subscribe(subscriptionObserver);
		var innerListProvider = source as InnerListProvider<T> ?? subscription as InnerListProvider<T>;
		IndexedChangesHandler<T> indexedChangesHandler;
		if (innerListProvider != null)
		{
			IndexedChangeToNotifyCollectionAdapter<T> adapter = new(innerListProvider.Inner);
			indexedChangesHandler = adapter;
			result = adapter;
		}
		else
		{
			ObservableList<T> observableList = new();
			indexedChangesHandler = observableList;
			result = observableList;
		}

		subscriptionObserver.Observer = new ChangesHandlerObserver<T>(indexedChangesHandler);
		return subscription;
	}

	public static IObservable<IndexedChange<T>> Sort<T>(this IObservable<IndexedChange<T>> source, IComparer<T>? comparer = null) =>
		Observable.Create<IndexedChange<T>>(observer => new Sorter<T>(comparer)
		{
			DestinationObserver = observer,
			SourceObservable = source
		});

	public static IObservable<IndexedChange<T>> Filter<T>(this IObservable<IndexedChange<T>> source, Func<T, bool> predicate) =>
		Observable.Create<IndexedChange<T>>(observer => new Filter<T>(predicate)
		{
			DestinationObserver = observer,
			SourceObservable = source
		});

	public static IObservable<IndexedChange<T>> Concatenate<T>(
		this IObservable<IndexedChange<T>> firstSource,
		IObservable<IndexedChange<T>> secondSource)
	{
		return Observable.Create<IndexedChange<T>>(observer => new Concat<T>(firstSource, secondSource, observer));
	}

	public static IObservable<IndexedChange<T>> DisposeMany<T>(this IObservable<IndexedChange<T>> source) =>
		Observable.Create<IndexedChange<T>>(observer => new DisposeMany<T>
		{
			DestinationObserver = observer,
			SourceObservable = source
		});

	public static ReadOnlySourceList<T> ToSourceList<T>(this IObservable<IndexedChange<T>> source)
	{
		if (source is InnerListProvider<T> innerListProvider)
			return new ExistingSourceListAdapter<T>(innerListProvider.Inner, source);
		PostponedConfigurableObserver<IndexedChange<T>> configurableObserver = new();
		var subscription = source.Subscribe(configurableObserver);
		if (subscription is InnerListProvider<T> subscriptionAsInnerListProvider)
		{
			ExistingSourceListAdapter<T> existingSourceListAdapter = new(subscriptionAsInnerListProvider.Inner, subscription);
			configurableObserver.Observer = new ActionObserver<IndexedChange<T>>(change => existingSourceListAdapter.NotifyObservers(change));
			return existingSourceListAdapter;
		}
		ChangesSourceListAdapter<T> changesSourceListAdapter = new(subscription);
		configurableObserver.Observer = new ActionObserver<IndexedChange<T>>(change => changesSourceListAdapter.ApplyChange(change));
		return changesSourceListAdapter;
	}
}