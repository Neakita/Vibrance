using System.Collections.Specialized;
using System.ComponentModel;

namespace Vibrance;

public interface ReadOnlyObservableList<out T> : IReadOnlyList<T>, INotifyCollectionChanged, INotifyPropertyChanged;