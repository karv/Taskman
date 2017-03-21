
namespace Taskman
{
	/// <summary>
	/// Competation status of a <see cref="Task"/>
	/// </summary>
	public enum TaskStatus
	{
		/// <summary>
		/// Nor completed nor active
		/// </summary>
		Inactive,
		/// <summary>
		/// The <see cref="Task"/> is active
		/// </summary>
		Active,
		/// <summary>
		/// The <see cref="Task"/> is competed
		/// </summary>
		Completed
	}
}