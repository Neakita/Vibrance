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

	public static ReadOnlyObservableList<T> ToObservableList<T>(this IObservable<IndexedChange<T>> source)
	{
		return new ChangeableObservableList<T>(source);
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

	public static ReadOnlyNotifyingList<T> ToReadOnlyNotifyingList<T>(this ReadOnlyObservableList<T> observableList)
	{
		return new ReadOnlyNotifyingList<T>(observableList);
	}

	public static IObservable<IndexedChange<TTarget>> TransformMany<TSource, TTarget>(this IObservable<IndexedChange<TSource>> source, Func<TSource, IObservable<IndexedChange<TTarget>>> selector)
	{
		return Observable.Create<IndexedChange<TTarget>>(observer => new TransformMany<TSource, TTarget>(source, selector, observer));
	}
}