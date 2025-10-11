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

	public IndexedChange<T> AsRemovalOrReplacement
	{
		get
		{
			if (NewItems.Count == 0)
				return new IndexedRemoval<T>
				{
					Index = 0,
					Items = OldItems
				};
			return new IndexedReplacement<T>
			{
				Index = 0,
				OldItems = OldItems,
				NewItems = NewItems
			};
		}
	}

	int IndexedChange<T>.OldIndex => OldItems.Count > 0 ? 0 : -1;
	int IndexedChange<T>.NewIndex => NewItems.Count > 0 ? 0 : -1;

	IndexedChangeFactory IndexedChange<T>.Factory => ResetFactory.Instance;

	NotifyCollectionChangedEventArgs IndexedChange<T>.ToNotifyCollectionChangedEventArgs()
	{
		return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
	}
}