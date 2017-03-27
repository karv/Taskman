using System;
using System.Collections.Generic;
using System.Linq;

namespace Taskman.Import
{
	/// <summary>
	/// Provides methods to import <see cref="Category"/> from a file or a <see cref="TaskCollection"/>
	/// </summary>
	public static class CategoryImporter
	{
		/// <summary>
		/// Imports the categories from a file
		/// </summary>
		/// <returns>Collection of imported <see cref="Category"/></returns>
		/// <param name="coll">The collection where to import the categories</param>
		/// <param name="fileName">File name where is located the collection to import</param>
		public static Category[] ImportCategory (this TaskCollection coll,
		                                         string fileName)
		{
			return ImportCategory (coll, fileName, CategoryImportOptions.Default);
		}

		/// <summary>
		/// Imports the categories from a file
		/// </summary>
		/// <returns>Collection of imported <see cref="Category"/></returns>
		/// <param name="coll">The collection where to import the categories</param>
		/// <param name="importingCollection">The collection to import</param>
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

		/// <summary>
		/// Imports the categories from a file
		/// </summary>
		/// <returns>Collection of imported <see cref="Category"/></returns>
		/// <param name="coll">The collection where to import the categories</param>
		/// <param name="importingCollection">The collection to import</param>
		/// <param name="opts">Options</param>
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
				catch (Exception)
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