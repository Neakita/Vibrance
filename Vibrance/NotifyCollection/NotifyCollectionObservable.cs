using System.Collections.Specialized;
using Vibrance.Changes;

namespace Vibrance.NotifyCollection;

internal sealed class NotifyCollectionObservable<T> : IObservable<Change<T>>
{
	public NotifyCollectionObservable(INotifyCollectionChanged collection)
	{
		_collection = collection;
	}

	public IDisposable Subscribe(IObserver<Change<T>> observer)
	{
		if (_collection is IReadOnlyCollection<T> collection)
			SendInitialItems(observer, collection);
		return new NotifyCollectionSubscription<T>(_collection, observer);
	}

	private readonly INotifyCollectionChanged _collection;

	private static void SendInitialItems(IObserver<Change<T>> observer, IReadOnlyCollection<T> collection)
	{
		if (collection.Count == 1)
			observer.OnNext(new AddItemChange<T>(collection.Single(), 0));
		else if (collection.Count > 1)
			observer.OnNext(new AddRangeChange<T>(collection, 0, collection.Count));
	}
}