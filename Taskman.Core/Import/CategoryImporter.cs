using System;
using System.Collections.Generic;
using System.Linq;

namespace Taskman.Import
{
	public static class CategoryImporter
	{
		public static Category[] ImportCategory (this TaskCollection coll,
		                                         string fileName)
		{
			return ImportCategory (coll, fileName, CategoryImportOptions.Default);
		}

		public static Category[] ImportCategory (this TaskCollection coll,
		                                         TaskCollection importingCollection)
		{
			return ImportCategory (coll, importingCollection, CategoryImportOptions.Default);
		}

		/// <summary>
		/// Imports the categories from a <see cref="TaskCollection"/> in a file
		/// </summary>
		/// <returns>The affected categories</returns>
		/// <param name="coll">The collection where to import to</param>
		/// <param name="fileName">File name containing the JSON of the importing <see cref="TaskCollection"/></param>
		/// <param name="opts">Import options</param>
		public static Category[] ImportCategory (this TaskCollection coll,
		                                         string fileName,
		                                         CategoryImportOptions opts)
		{
			var otherColl = TaskCollection.Load (fileName);
			return ImportCategory (coll, otherColl, opts);
		}

		public static Category[] ImportCategory (this TaskCollection coll,
		                                         TaskCollection importingCollection,
		                                         CategoryImportOptions opts)
		{
			var importCats = new HashSet<Category> (importingCollection.EnumerateCategories ());
			var ret = new List<Category> ();
			foreach (var cat in importCats)
				ret.Add (mergeCat (coll, cat, opts));

			return ret.ToArray ();
		}

		static Category mergeCat (TaskCollection coll, Category importCat, CategoryImportOptions opts)
		{
			if (opts.MergeSameName)
			{
				try
				{
					return coll.EnumerateCategories ().First (
						z => string.Equals (z.Name, importCat.Name, StringComparison.CurrentCultureIgnoreCase));
				}
				catch (Exception ex)
				{
					return importCat.clone (coll);
				}
			}
			return importCat.clone (coll);
		}

		static Category clone (this Category obj, TaskCollection coll)
		{
			var ret = coll.AddCategory ();
			ret.Name = obj.Name;
			return ret;
		}
	}
}