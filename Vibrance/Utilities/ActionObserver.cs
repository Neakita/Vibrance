namespace Vibrance.Utilities;

internal sealed class ActionObserver<T> : IObserver<T>
{
	public ActionObserver(Action<T> action)
	{
		_action = action;
	}

	public void OnNext(T value)
	{
		_action(value);
	}

	public void OnCompleted()
	{
	}

	public void OnError(Exception error)
	{
		throw error;
	}

	private readonly Action<T> _action;
}