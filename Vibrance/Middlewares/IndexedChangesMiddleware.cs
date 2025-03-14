using System.Diagnostics.CodeAnalysis;
using Vibrance.Changes;

namespace Vibrance.Middlewares;

internal abstract class IndexedChangesMiddleware<TSource, TDestination> : IObserver<IndexedChange<TSource>>, IDisposable
{
	public required IObserver<IndexedChange<TDestination>> DestinationObserver { protected get; init; }
	public required IObservable<IndexedChange<TSource>> SourceObservable
	{
		[MemberNotNull(nameof(_disposable))] init => _disposable = value.Subscribe(this);
	}

	public void OnNext(IndexedChange<TSource> change)
	{
		HandleChange(change);
	}

	public void OnCompleted()
	{
		DestinationObserver.OnCompleted();
	}

	public void OnError(Exception error)
	{
		DestinationObserver.OnError(error);
	}

	public void Dispose()
	{
		_disposable.Dispose();
	}

	protected abstract void HandleChange(IndexedChange<TSource> change);

	private IDisposable _disposable;
}