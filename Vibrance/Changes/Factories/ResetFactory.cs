namespace Vibrance.Changes.Factories;

internal sealed class ResetFactory : IndexedChangeFactory
{
	public static ResetFactory Instance { get; } = new();

	public bool CanCreateChange<T>(IndexedItems<T> oldItems, IndexedItems<T> newItems)
	{
		return oldItems is { Count: > 0, Index: 0 } && newItems.Index <= 0;
	}

	public IndexedChange<T> CreateChange<T>(IndexedItems<T> oldItems, IndexedItems<T> newItems)
	{
		if (!CanCreateChange(oldItems, newItems))
		{
			if (newItems.Count == 0)
				return IndexedRemovalFactory.Instance.CreateChange(oldItems, newItems);
			IndexedReplacementFactory.Instance.CreateChange(oldItems, newItems);
		}
		if (oldItems.Count == 0)
			throw new ArgumentException($"{nameof(oldItems)} expected count to be greater than zero");
		if (oldItems.Index != 0)
			throw new ArgumentException($"{nameof(oldItems)} expected to have index equal to zero");
		if (newItems.Index is not 0 and not -1)
			throw new ArgumentException($"{nameof(oldItems)} expected to have index equal to zero or -1");
		return new Reset<T>
		{
			OldItems = oldItems.List,
			NewItems = newItems.List
		};
	}
}