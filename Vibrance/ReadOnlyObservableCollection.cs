using Vibrance.Changes;

namespace Vibrance;

public interface ReadOnlyObservableCollection<out T> :
	IReadOnlyCollection<T>,
	IObservable<Change<T>>,
	IDisposable;