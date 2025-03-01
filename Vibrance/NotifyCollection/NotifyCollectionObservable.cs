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
		if (collection.Count == 0)
			return;
		Change<T> change = new()
		{
			NewItems = collection.ToList(),
			NewItemsStartIndex = 0
		};
		observer.OnNext(change);
	}
}