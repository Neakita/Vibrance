namespace Vibrance.Utilities;

internal static class ObservableExtensions
{
	public static IDisposable SubscribeAndGetInitialValue<T>(this IObservable<T> source, Action<T> action, out T? initialValue)
	{
		T? observedInitialValue = default;
		ConfigurableObserver<T> configurableObserver = new()
		{
			Action = value =>
			{
				if (observedInitialValue != null)
					throw new InvalidOperationException("More than one initial value observed");
				observedInitialValue = value;
			}
		};
		var subscription = source.Subscribe(configurableObserver);
		configurableObserver.Action = action;
		initialValue = observedInitialValue;
		return subscription;
	}

	public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> action)
	{
		return source.Subscribe(new ActionObserver<T>(action));
	}
}