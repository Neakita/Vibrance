namespace Vibrance.Utilities;

internal readonly struct Range
{
	public int Start { get; }
	public int End { get; }
	public int Count => End - Start + 1;

	public Range(int start, int end)
	{
		Start = start;
		End = end;
	}
}