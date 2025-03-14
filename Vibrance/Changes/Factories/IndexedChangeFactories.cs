using System.Collections.Specialized;

namespace Vibrance.Changes.Factories;

internal static class IndexedChangeFactories
{
	public static IndexedChangeFactory GetByNotifyCollectionChangedAction(NotifyCollectionChangedAction action)
	{
		return action switch
		{
			NotifyCollectionChangedAction.Add => InsertionFactory.Instance,
			NotifyCollectionChangedAction.Remove => IndexedRemovalFactory.Instance,
			NotifyCollectionChangedAction.Replace => IndexedReplacementFactory.Instance,
			NotifyCollectionChangedAction.Move => MoveFactory.Instance,
			NotifyCollectionChangedAction.Reset => ResetFactory.Instance,
			_ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
		};
	}
}