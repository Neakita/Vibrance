namespace Vibrance.Utilities;

internal sealed class ConfigurableObserver<T> : IObserver<T>
{
	public required IObserver<T> Observer { get; set; }

	public void OnNext(T value)
	{
		Observer.OnNext(value);
	}

	public void OnCompleted()
	{
	}

	public void OnError(Exception error)
	{
		throw error;
	}
}