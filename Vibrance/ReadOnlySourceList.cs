using Vibrance.Changes;

namespace Vibrance;

public interface ReadOnlySourceList<out T> : IReadOnlyList<T>, IObservable<IndexedChange<T>>, IDisposable;