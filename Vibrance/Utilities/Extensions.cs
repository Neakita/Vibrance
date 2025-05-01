namespace Vibrance.Utilities;

public static class Extensions
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

	public static List<T> MoveRange<T>(this List<T> list, int oldIndex, int count, int newIndex)
	{
		var items = list.GetRange(oldIndex, count);
		list.RemoveRange(oldIndex, count);
		list.InsertRange(newIndex, items);
		return items;
	}
}