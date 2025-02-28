namespace Vibrance.Changes;

public sealed class RemoveRangeChange<T> : Change<T>
{
	public IEnumerable<T> Items { get; }
	public int StartIndex { get; }
	public int Count { get; }

	public RemoveRangeChange(IEnumerable<T> items, int startIndex, int count)
	{
		Items = items;
		StartIndex = startIndex;
		Count = count;
	}
}