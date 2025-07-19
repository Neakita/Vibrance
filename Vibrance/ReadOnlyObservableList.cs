using Vibrance.Changes;

namespace Vibrance;

public interface ReadOnlyObservableList<out T> :
	IReadOnlyList<T>,
	IObservable<IndexedChange<T>>,
	IDisposable
{
	public static ReadOnlyObservableList<T> Empty { get; } = new ObservableList<T>();
}