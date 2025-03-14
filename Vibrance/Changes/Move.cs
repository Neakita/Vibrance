using System.Collections;
using System.Collections.Specialized;
using Vibrance.Changes.Factories;

namespace Vibrance.Changes;

public sealed class Move<T> : IndexedChange<T>
{
	public required int OldIndex
	{
		get;
		init
		{
			if (value < 0)
				throw new ArgumentOutOfRangeException(
					nameof(value),
					value,
					$"{nameof(value)} for {nameof(OldIndex)} expected to be greater than or equal to zero");
			field = value;
		}
	}

	public required int NewIndex
	{
		get;
		init
		{
			if (value < 0)
				throw new ArgumentOutOfRangeException(
					nameof(value),
					value,
					$"{nameof(value)} for {nameof(NewIndex)} expected to be greater than or equal to zero");
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

	IReadOnlyList<T> Change<T>.OldItems => Items;
	IReadOnlyList<T> Change<T>.NewItems => Items;

	IndexedChangeFactory IndexedChange<T>.Factory => MoveFactory.Instance;

	NotifyCollectionChangedEventArgs IndexedChange<T>.ToNotifyCollectionChangedEventArgs()
	{
		return new NotifyCollectionChangedEventArgs(
			NotifyCollectionChangedAction.Move,
			(IList)Items,
			NewIndex,
			OldIndex);
	}
}