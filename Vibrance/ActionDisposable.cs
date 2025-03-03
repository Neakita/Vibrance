namespace Vibrance;

internal sealed class ActionDisposable : IDisposable
{
	public ActionDisposable(Action action)
	{
		_action = action;
	}

	public void Dispose()
	{
		_action();
	}

	private readonly Action _action;
}