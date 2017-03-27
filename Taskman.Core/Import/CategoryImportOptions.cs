
namespace Taskman.Import
{
	/// <summary>
	/// Options for <see cref="CategoryImporter"/>
	/// </summary>
	public struct CategoryImportOptions
	{
		/// <summary>
		/// If <c>true</c>, categories with same name will be considered as same
		/// </summary>
		public bool MergeSameName;

		/// <summary>
		/// Default options
		/// </summary>
		public static readonly CategoryImportOptions Default;

		static CategoryImportOptions ()
		{
			Default = new CategoryImportOptions
			{ MergeSameName = true };
		}
	}
}