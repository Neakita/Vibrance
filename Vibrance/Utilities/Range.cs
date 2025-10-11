namespace Vibrance.Utilities;

internal readonly struct Range
{
	public static Range FromCount(int start, int count)
	{
		if (count <= 0)
			throw new ArgumentOutOfRangeException(nameof(count), count, "Count expected to be greater than zero");
		return new Range(start, start + count - 1);
	}

	public int Start { get; }
	public int End { get; }
	public int Count => End - Start + 1;

	public Range(int start, int end)
	{
		Start = start;
		End = end;
	}
}