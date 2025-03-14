using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Vibrance.Changes.Factories;

namespace Vibrance.Changes;

public sealed class Insert<T> : IndexedChange<T>
{
	public required int Index
	{
		get;
		init
		{
			if (value < 0)
				throw new ArgumentOutOfRangeException(
					nameof(value),
					value,
					$"{nameof(value)} for {nameof(Index)} expected to be greater than or equal to zero");
			field = value;
		}
	}

	public required IReadOnlyList<T> Items
	{
		get;
		init
		{
			if (value.Count == 0)
				throw new ArgumentException($"{nameof(value)} for {nameof(Items)} expected to have at least one item");
			field = value;
		}
	}

	int IndexedChange<T>.OldIndex => -1;
	int IndexedChange<T>.NewIndex => Index;
	IReadOnlyList<T> Change<T>.OldItems => ReadOnlyCollection<T>.Empty;
	IReadOnlyList<T> Change<T>.NewItems => Items;

	IndexedChangeFactory IndexedChange<T>.Factory => InsertFactory.Instance;

	NotifyCollectionChangedEventArgs IndexedChange<T>.ToNotifyCollectionChangedEventArgs()
	{
		return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, (IList)Items, Index);
	}
}