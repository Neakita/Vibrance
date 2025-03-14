using System.Diagnostics.CodeAnalysis;

namespace Vibrance.Changes.Factories;

public interface IndexedChangeFactory
{
	bool CanCreateChange<T>(IndexedItems<T> oldItems, IndexedItems<T> newItems);
	IndexedChange<T> CreateChange<T>(IndexedItems<T> oldItems, IndexedItems<T> newItems);

	bool TryCreateChange<T>(IndexedItems<T> oldItems, IndexedItems<T> newItems, [MaybeNullWhen(false)] out IndexedChange<T> change)
	{
		if (CanCreateChange(oldItems, newItems))
		{
			change = CreateChange(oldItems, newItems);
			return true;
		}
		change = null;
		return false;
	}
}