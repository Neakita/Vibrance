namespace Vibrance.Tests.Utilities;

internal sealed class RecordingObserver<T> : IObserver<T>, IDisposable
{
	public IReadOnlyCollection<T> ObservedValues => _observedValues;
	public T LastObservedValue => _observedValues.Last();

	public RecordingObserver(IObservable<T> observable)
	{
		_subscription = observable.Subscribe(this);
	}

	public void Dispose()
	{
		_subscription.Dispose();
	}

	void IObserver<T>.OnNext(T value)
	{
		_observedValues.Add(value);
	}

	void IObserver<T>.OnCompleted()
	{
	}

	void IObserver<T>.OnError(Exception error)
	{
	}

	private readonly IDisposable _subscription;
	private readonly List<T> _observedValues = new();
}