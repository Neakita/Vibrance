using Vibrance.Changes;

namespace Vibrance.Utilities;

internal interface IndexedChangesHandler<in T>
{
	void HandleChange(IndexedChange<T> change);
}