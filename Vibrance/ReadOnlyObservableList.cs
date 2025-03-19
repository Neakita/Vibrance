using System.Collections.Specialized;
using System.ComponentModel;
using Vibrance.Changes;

namespace Vibrance;

public interface ReadOnlyObservableList<out T> :
	IReadOnlyList<T>,
	IObservable<IndexedChange<T>>,
	INotifyCollectionChanged,
	INotifyPropertyChanged,
	IDisposable;