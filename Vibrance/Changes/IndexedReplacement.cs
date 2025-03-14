using System.Collections;
using System.Collections.Specialized;
using Vibrance.Changes.Factories;

namespace Vibrance.Changes;

public sealed class IndexedReplacement<T> : IndexedChange<T>
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

	public required IReadOnlyList<T> OldItems
	{
		get;
		init
		{
			if (value.Count == 0)
				throw new ArgumentException(
					$"{nameof(value)} for {nameof(OldItems)} expected to have at least one item",
					nameof(value));
			field = value;
		}
	}

	public required IReadOnlyList<T> NewItems
	{
		get;
		init
		{
			if (value.Count == 0)
				throw new ArgumentException(
					$"{nameof(value)} for {nameof(NewItems)} expected to have at least one item",
					nameof(value));
			field = value;
		}
	}

	int IndexedChange<T>.OldIndex => Index;
	int IndexedChange<T>.NewIndex => Index;

	IndexedChangeFactory IndexedChange<T>.Factory => IndexedReplacementFactory.Instance;

	NotifyCollectionChangedEventArgs IndexedChange<T>.ToNotifyCollectionChangedEventArgs()
	{
		return new NotifyCollectionChangedEventArgs(
			NotifyCollectionChangedAction.Replace,
			(IList)NewItems,
			(IList)OldItems, Index);
	}
}