namespace Vibrance.Utilities;

internal sealed class PostponedConfigurableObserver<T> : IObserver<T>
{
	public IObserver<T>? Observer
	{
		get => _observer;
		set
		{
			if (_observer != null)
				throw new InvalidOperationException("Observer already set");
			ArgumentNullException.ThrowIfNull(value);
			_observer = value;
			if (_deferredValues == null)
				return;
			foreach (var deferredValue in _deferredValues)
				_observer.OnNext(deferredValue);
		}
	}

	public void OnNext(T value)
	{
		if (Observer == null)
		{
			_deferredValues ??= new List<T>();
			_deferredValues.Add(value);
			return;
		}
		Observer.OnNext(value);
	}

	public void OnCompleted()
	{
		if (Observer == null)
			throw new InvalidOperationException("Sequence completed but no observer was set to handle it");
		Observer.OnCompleted();
	}

	public void OnError(Exception error)
	{
		if (Observer == null)
			throw new InvalidOperationException("An exception has occured when no observer was set to handle it");
		Observer.OnError(error);
	}

	private List<T>? _deferredValues;
	private IObserver<T>? _observer;
}