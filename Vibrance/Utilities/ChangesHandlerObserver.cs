using Vibrance.Changes;

namespace Vibrance.Utilities;

internal sealed class ChangesHandlerObserver<T> : IObserver<IndexedChange<T>>
{
	public ChangesHandlerObserver(IndexedChangesHandler<T> handler)
	{
		_handler = handler;
	}

	public void OnNext(IndexedChange<T> value)
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

	private readonly IndexedChangesHandler<T> _handler;
}