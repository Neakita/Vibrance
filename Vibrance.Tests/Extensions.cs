using NSubstitute;

namespace Vibrance.Tests;

internal static class Extensions
{
	public static IDisposable ObserverChanges(this SourceList<int> list, out IObserver<Change<int>> observer)
	{
		observer = Substitute.For<IObserver<Change<int>>>();
		return list.Subscribe(observer);
	}
}