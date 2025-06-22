using Vibrance.Changes;

namespace Vibrance;

public interface ReadOnlyObservableList<out T> :
	IReadOnlyList<T>,
	IObservable<IndexedChange<T>>,
	IDisposable;