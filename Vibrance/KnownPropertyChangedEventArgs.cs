using System.ComponentModel;

namespace Vibrance;

internal static class KnownPropertyChangedEventArgs
{
	internal static readonly PropertyChangedEventArgs CountChangedEventArgs = new("Count");
	internal static readonly PropertyChangedEventArgs IndexerChangedEventArgs = new("Item[]");
}