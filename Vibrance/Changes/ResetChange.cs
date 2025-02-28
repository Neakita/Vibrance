namespace Vibrance.Changes;

public sealed class ResetChange<T> : Change<T>
{
	public IEnumerable<T> Items { get; }

	public ResetChange(IEnumerable<T> items)
	{
		Items = items;
	}
}