using System.Diagnostics.CodeAnalysis;

namespace Vibrance.Changes.Middlewares;

internal abstract class ChangesMiddleware<TSource, TDestination> : IObserver<Change<TSource>>, IDisposable
{
	public required IObserver<Change<TDestination>> DestinationObserver { protected get; init; }
	public required IObservable<Change<TSource>> SourceObservable
	{
		[MemberNotNull(nameof(_disposable))] init => _disposable = value.Subscribe(this);
	}

	public void OnNext(Change<TSource> change)
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
		_disposable?.Dispose();
	}

	protected abstract void HandleChange(Change<TSource> change);

	private IDisposable _disposable;
}