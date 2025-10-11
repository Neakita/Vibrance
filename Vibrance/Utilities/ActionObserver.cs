namespace Vibrance.Utilities;

internal sealed class ActionObserver<T>(Action<T> action) : IObserver<T>
{
	public void OnNext(T value)
	{
		action(value);
	}

	public void OnCompleted()
	{
	}

	public void OnError(Exception error)
	{
		throw error;
	}
}