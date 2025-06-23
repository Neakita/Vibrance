namespace Vibrance.Changes;

public class Replacement<T> : Change<T>
{
	public required IReadOnlyList<T> OldItems
	{
		get;
		init
		{
			if (value.Count == 0)
				throw new ArgumentException(
					$"{nameof(value)} for {nameof(OldItems)} expected to have at least one item",
					nameof(value));
			field = value;
		}
	}

	public required IReadOnlyList<T> NewItems
	{
		get;
		init
		{
			if (value.Count == 0)
				throw new ArgumentException(
					$"{nameof(value)} for {nameof(NewItems)} expected to have at least one item",
					nameof(value));
			field = value;
		}
	}
}