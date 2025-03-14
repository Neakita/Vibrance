using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Vibrance.Changes.Factories;

namespace Vibrance.Changes;

public sealed class Reset<T> : IndexedChange<T>
{
	public required IReadOnlyList<T> OldItems
	{
		get;
		init
		{
			if (value.Count == 0)
				throw new ArgumentException($"{nameof(value)} for {nameof(OldItems)} expected to have at least one item", nameof(value));
			field = value;
		}
	}

	public IReadOnlyList<T> NewItems { get; init; } = ReadOnlyCollection<T>.Empty;

	int IndexedChange<T>.OldIndex => 0;
	int IndexedChange<T>.NewIndex => 0;

	IndexedChangeFactory IndexedChange<T>.Factory => ResetFactory.Instance;

	NotifyCollectionChangedEventArgs IndexedChange<T>.ToNotifyCollectionChangedEventArgs()
	{
		return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
	}
}