
namespace Taskman
{
	/// <summary>
	/// Represents an object that can be retrieved by Id
	/// </summary>
	public interface IIdentificable
	{
		/// <summary>
		/// Gets the idetifier
		/// </summary>
		/// <value>The identifier.</value>
		int Id { get; }

		/// <summary>
		/// Remove and dispose this object
		/// </summary>
		void Remove ();

		/// <summary>
		/// Initialize this object
		/// </summary>
		void Initialize (TaskCollection coll);
	}
}