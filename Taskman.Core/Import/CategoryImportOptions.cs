
namespace Taskman.Import
{
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