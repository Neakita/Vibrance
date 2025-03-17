namespace Vibrance.Utilities;

public readonly struct Range
{
	public static Range FromCount(int start, int count)
	{
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