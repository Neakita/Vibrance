namespace Vibrance.Utilities;

internal static class Extensions
{
	public static IEnumerable<Range> ToRanges(this IEnumerable<int> indexes)
	{
		using var indexesEnumerator = indexes.GetEnumerator();
		if (!indexesEnumerator.MoveNext())
			yield break;
		int startIndex = indexesEnumerator.Current;
		int previousIndex = startIndex;
		while (indexesEnumerator.MoveNext())
		{
			int currentIndex = indexesEnumerator.Current;
			if (currentIndex != previousIndex + 1)
			{
				yield return new Range(startIndex, previousIndex);
				startIndex = currentIndex;
			}
			previousIndex = currentIndex;
		}
		yield return new Range(startIndex, previousIndex);
	}

	public static List<T> GetRange<T>(this List<T> list, Range range)
	{
		return list.GetRange(range.Start, range.Count);
	}
}