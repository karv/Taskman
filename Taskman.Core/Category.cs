using System.Linq;
using Newtonsoft.Json;

namespace Taskman
{
	/// <summary>
	/// Represents a class of <see cref="Task"/>
	/// </summary>
	public class Category : IIdentificable
	{
		readonly int id;

		/// <summary>
		/// Removes this category
		/// </summary>
		public void Remove ()
		{
			foreach (var task in _collection.EnumerateTasks ().Where (z => z.HasCategory (this)))
				task.RemoveCategory (Id);
			_collection._collection.Remove (this);
		}

		void IIdentificable.Initialize (TaskCollection coll)
		{
			_collection = coll;
		}

		/// <summary>
		/// Gets the identifier
		/// </summary>
		/// <value>The identifier.</value>
		public int Id
		{
			get
			{
				return id;
			}
		}

		/// <summary>
		/// The name of the category
		/// </summary>
		public string Name;

		[JsonIgnore]
		TaskCollection _collection;

		[JsonConstructor]
		Category (int Id)
		{
			id = Id;
		}

		internal Category (TaskCollection collection)
		{
			if (collection == null)
				throw new System.ArgumentNullException ("collection");
			_collection = collection;
			id = _collection.GetUnusedId ();
		}
	}
}