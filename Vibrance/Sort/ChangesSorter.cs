using Vibrance.Changes;

namespace Vibrance.Sort;

internal sealed class ChangesSorter<T> : IObservable<Change<T>>
{
	public ChangesSorter(IObservable<Change<T>> source, IComparer<T> comparer)
	{
		_source = source;
		_comparer = comparer;
	}

	public IDisposable Subscribe(IObserver<Change<T>> observer)
	{
		return new SortSubscription<T>(_source, _comparer, observer);
	}

	private readonly IObservable<Change<T>> _source;
	private readonly IComparer<T> _comparer;
}