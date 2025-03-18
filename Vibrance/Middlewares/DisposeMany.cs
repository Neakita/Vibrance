using Vibrance.Changes;

namespace Vibrance.Middlewares;

internal sealed class DisposeMany<T> : IndexedChangesMiddleware<T, T>
{
	protected override void HandleChange(IndexedChange<T> change)
	{
		foreach (var disposable in change.OldItems.OfType<IDisposable>())
			disposable.Dispose();
		DestinationObserver.OnNext(change);
	}
}