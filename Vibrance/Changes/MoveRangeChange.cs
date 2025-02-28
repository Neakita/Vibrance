namespace Vibrance.Changes;

public sealed class MoveRangeChange<T> : Change<T>
{
	public IEnumerable<T> Items { get; }
	public int OldStartIndex { get; }
	public int NewStartIndex { get; }
	public int Count { get; }

	public MoveRangeChange(IEnumerable<T> items, int oldStartIndex, int newStartIndex, int count)
	{
		Items = items;
		OldStartIndex = oldStartIndex;
		NewStartIndex = newStartIndex;
		Count = count;
	}
}