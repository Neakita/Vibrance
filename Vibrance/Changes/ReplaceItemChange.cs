namespace Vibrance.Changes;

public sealed class ReplaceItemChange<T> : Change<T>
{
	public T NewItem { get; }
	public T OldItem { get; }
	public int Index { get; }

	public ReplaceItemChange(T newItem, T oldItem, int index)
	{
		NewItem = newItem;
		OldItem = oldItem;
		Index = index;
	}
}