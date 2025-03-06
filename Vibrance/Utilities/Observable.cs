namespace Vibrance.Utilities;

internal static class Observable
{
	public static IObservable<T> Create<T>(Func<IObserver<T>, IDisposable> factory)
	{
		return new FactoryObservable<T>(factory);
	}
}