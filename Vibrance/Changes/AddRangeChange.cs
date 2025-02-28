namespace Vibrance.Changes;

public sealed class AddRangeChange<T> : Change<T>
{
	public IEnumerable<T> Items { get; }
	public int StartIndex { get; }
	public int Count { get; }

	public AddRangeChange(IEnumerable<T> items, int startIndex, int count)
	{
		Items = items;
		StartIndex = startIndex;
		Count = count;
	}
}