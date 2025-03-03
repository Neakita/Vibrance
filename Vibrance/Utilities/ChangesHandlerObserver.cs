namespace Vibrance.Utilities;

internal sealed class ChangesHandlerObserver<T> : IObserver<Change<T>>
{
	public ChangesHandlerObserver(ChangesHandler<T> handler)
	{
		_handler = handler;
	}

	public void OnNext(Change<T> value)
	{
		_handler.HandleChange(value);
	}

	public void OnCompleted()
	{
	}

	public void OnError(Exception error)
	{
		throw error;
	}

	private readonly ChangesHandler<T> _handler;
}