using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Vibrance.Changes.Factories;

namespace Vibrance.Changes;

public sealed class Insertion<T> : Addition<T>, IndexedChange<T>
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

	int IndexedChange<T>.OldIndex => -1;
	int IndexedChange<T>.NewIndex => Index;

	IndexedChangeFactory IndexedChange<T>.Factory => InsertionFactory.Instance;

	public NotifyCollectionChangedEventArgs ToNotifyCollectionChangedEventArgs()
	{
		return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, (IList)Items, Index);
	}
}