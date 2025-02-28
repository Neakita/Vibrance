namespace Vibrance.Changes;

public sealed class RemoveItemChange<T> : Change<T>
{
	public T Item { get; }
	public int Index { get; }

	public RemoveItemChange(T item, int index)
	{
		Item = item;
		Index = index;
	}
}