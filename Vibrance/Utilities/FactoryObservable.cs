namespace Vibrance.Utilities;

internal sealed class FactoryObservable<T> : IObservable<T>
{
	public FactoryObservable(Func<IObserver<T>, IDisposable> factory)
	{
		_factory = factory;
	}

	public IDisposable Subscribe(IObserver<T> observer)
	{
		return _factory(observer);
	}

	private readonly Func<IObserver<T>, IDisposable> _factory;
}