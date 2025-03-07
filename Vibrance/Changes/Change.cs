namespace Vibrance.Changes;

public sealed class Change<T>
{
	public static Change<T> ResetChange { get; } = new()
	{
		Reset = true
	};

	public PositionalReadOnlyList<T> OldItems { get; init; } = PositionalReadOnlyList<T>.Default;
	public PositionalReadOnlyList<T> NewItems { get; init; } = PositionalReadOnlyList<T>.Default;
	public bool Reset { get; init; }
}