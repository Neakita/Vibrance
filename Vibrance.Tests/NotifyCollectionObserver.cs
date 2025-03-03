using System.Collections.Specialized;

namespace Vibrance.Tests;

internal sealed class NotifyCollectionObserver : IDisposable
{
	public NotifyCollectionChangedEventArgs LastObservedArgs => _observedArgs.Last();
	
	public NotifyCollectionObserver(INotifyCollectionChanged notifyCollection)
	{
		notifyCollection.CollectionChanged += OnCollectionChanged;
		_notifyCollection = notifyCollection;
	}

	public void Dispose()
	{
		_notifyCollection.CollectionChanged += OnCollectionChanged;
	}

	private readonly INotifyCollectionChanged _notifyCollection;
	private readonly List<NotifyCollectionChangedEventArgs> _observedArgs = new();

	private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
	{
		_observedArgs.Add(args);
	}
}