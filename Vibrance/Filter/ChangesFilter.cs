using Vibrance.Changes;

namespace Vibrance.Filter;

internal sealed class ChangesFilter<T> : IObservable<Change<T>>
{
	public ChangesFilter(IObservable<Change<T>> source, Func<T, bool> predicate)
	{
		_source = source;
		_predicate = predicate;
	}

	public IDisposable Subscribe(IObserver<Change<T>> observer)
	{
		return new FilterSubscription<T>(_source, _predicate, observer);
	}

	private readonly IObservable<Change<T>> _source;
	private readonly Func<T, bool> _predicate;
}