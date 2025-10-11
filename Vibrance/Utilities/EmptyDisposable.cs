namespace Vibrance.Utilities;

internal sealed class EmptyDisposable : IDisposable
{
	public static IDisposable Instance { get; } = new EmptyDisposable();

	public void Dispose()
	{
	}
}