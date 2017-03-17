
namespace Taskman
{

	public interface IIdentificable
	{
		int Id { get; }

		void Remove ();

		void Initialize (TaskCollection coll);
	}
}