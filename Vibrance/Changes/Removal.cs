using System.Collections.ObjectModel;

namespace Vibrance.Changes;

public class Removal<T> : Change<T>
{
	public required IReadOnlyList<T> Items { get; init; }

	IReadOnlyList<T> Change<T>.OldItems => Items;
	IReadOnlyList<T> Change<T>.NewItems => ReadOnlyCollection<T>.Empty;
}