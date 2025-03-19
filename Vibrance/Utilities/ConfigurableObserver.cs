namespace Vibrance.Utilities;

internal sealed class ConfigurableObserver<T> : IObserver<T>
{
	public required Action<T> Action { get; set; }

	public void OnNext(T value)
	{
		Action(value);
	}

	public void OnCompleted()
	{
	}

	public void OnError(Exception error)
	{
		throw error;
	}
}