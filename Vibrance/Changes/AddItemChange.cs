namespace Vibrance.Changes;

public sealed class AddItemChange<T> : Change<T>
{
	public T Item { get; }
	public int Index { get; }

	public AddItemChange(T item, int index)
	{
		Item = item;
		Index = index;
	}
}