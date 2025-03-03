using Vibrance.Transform;

namespace Vibrance;

public static class ObservableChangesExtensions
{
	public static IObservable<Change<TDestination>> Transform<TSource, TDestination>(
		this IObservable<Change<TSource>> source,
		Func<TSource, TDestination> selector)
	{
		return new ChangesTransformer<TSource, TDestination>(source, selector);
	}
}