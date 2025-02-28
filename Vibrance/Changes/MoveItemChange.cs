namespace Vibrance.Changes;

public sealed class MoveItemChange<T> : Change<T>
{
	public T Item { get; }
	public int OldIndex { get; }
	public int NewIndex { get; }

	public MoveItemChange(T item, int oldIndex, int newIndex)
	{
		Item = item;
		OldIndex = oldIndex;
		NewIndex = newIndex;
	}
}