using Vibrance.Changes;

namespace Vibrance.Transform;

internal sealed class ChangesTransformer<TSource, TDestination> : IObservable<Change<TDestination>>
{
	public ChangesTransformer(IObservable<Change<TSource>> source, Func<TSource, TDestination> selector)
	{
		_source = source;
		_selector = selector;
	}

	public IDisposable Subscribe(IObserver<Change<TDestination>> observer)
	{
		return new TransformObserver<TSource, TDestination>(_source, _selector, observer);
	}

	private readonly IObservable<Change<TSource>> _source;
	private readonly Func<TSource, TDestination> _selector;
}