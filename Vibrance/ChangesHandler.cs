namespace Vibrance;

internal interface ChangesHandler<T>
{
	void HandleChange(Change<T> change);
}