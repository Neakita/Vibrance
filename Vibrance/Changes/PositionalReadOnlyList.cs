using System.Collections;
using System.Collections.ObjectModel;

namespace Vibrance.Changes;

public sealed class PositionalReadOnlyList<T> : IReadOnlyList<T>
{
	public static PositionalReadOnlyList<T> Default { get; } = new(ReadOnlyCollection<T>.Empty, -1); 

	public int StartIndex { get; }
	public int Count => _inner.Count;
	public T this[int index] => _inner[index];

	public PositionalReadOnlyList(IReadOnlyList<T> inner, int startIndex)
	{
		_inner = inner;
		StartIndex = startIndex;
	}

	public IList AsList() => (IList)_inner;

	public IEnumerator<T> GetEnumerator() => _inner.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	private readonly IReadOnlyList<T> _inner;
}