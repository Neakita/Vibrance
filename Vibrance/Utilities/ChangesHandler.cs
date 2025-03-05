using Vibrance.Changes;

namespace Vibrance.Utilities;

internal interface ChangesHandler<T>
{
	void HandleChange(Change<T> change);
}