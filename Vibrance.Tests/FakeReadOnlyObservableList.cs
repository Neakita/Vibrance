using System.Collections;
using Vibrance.Changes;

namespace Vibrance.Tests;

internal sealed class FakeReadOnlyObservableList<T> : ReadOnlyObservableList<T>
{
	public int Count => throw new NotSupportedException();

	public T this[int index] => throw new NotSupportedException();

	public FakeReadOnlyObservableList(Func<IObserver<IndexedChange<T>>, IDisposable> subscribe)
	{
		_subscribe = subscribe;
	}

	public IDisposable Subscribe(IObserver<IndexedChange<T>> observer)
	{
		return _subscribe(observer);
	}

	public IEnumerator<T> GetEnumerator()
	{
		throw new NotSupportedException();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		throw new NotSupportedException();
	}

	public void Dispose()
	{
		throw new NotSupportedException();
	}

	private readonly Func<IObserver<IndexedChange<T>>, IDisposable> _subscribe;
}