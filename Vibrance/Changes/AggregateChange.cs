namespace Vibrance.Changes;

public sealed class AggregateChange<T> : Change<T>
{
	public IReadOnlyCollection<Change<T>> Changes { get; }

	public AggregateChange(IReadOnlyCollection<Change<T>> changes)
	{
		Changes = changes;
	}
}