using System.Collections;
using System.Collections.Specialized;
using Vibrance.Changes.Factories;

namespace Vibrance.Changes;

public sealed class IndexedReplacement<T> : Replacement<T>, IndexedChange<T>
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