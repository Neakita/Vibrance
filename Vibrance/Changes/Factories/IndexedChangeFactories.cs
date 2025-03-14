using System.Collections.Specialized;

namespace Vibrance.Changes.Factories;

internal static class IndexedChangeFactories
{
	public static IndexedChangeFactory GetByNotifyCollectionChangedAction(NotifyCollectionChangedAction action)
	{
		return action switch
		{
			NotifyCollectionChangedAction.Add => InsertFactory.Instance,
			NotifyCollectionChangedAction.Remove => IndexedRemoveFactory.Instance,
			NotifyCollectionChangedAction.Replace => IndexedReplaceFactory.Instance,
			NotifyCollectionChangedAction.Move => MoveFactory.Instance,
			NotifyCollectionChangedAction.Reset => ResetFactory.Instance,
			_ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
		};
	}
}