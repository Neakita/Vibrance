namespace Vibrance.Utilities;

internal static class ObservableExtensions
{
	public static IDisposable SubscribeAndGetInitialValue<T>(this IObservable<T> source, IObserver<T> observer, out T? initialValue)
	{
		T? observedInitialValue = default;
		ConfigurableObserver<T> configurableObserver = new()
		{
			Observer = new ActionObserver<T>(value =>
			{
				if (observedInitialValue != null)
					throw new InvalidOperationException("More than one initial value observed");
				observedInitialValue = value;
			})
		};
		var subscription = source.Subscribe(configurableObserver);
		configurableObserver.Observer = observer;
		initialValue = observedInitialValue;
		return subscription;
	}
}