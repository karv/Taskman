using System.Linq;
using Newtonsoft.Json;

namespace Taskman
{
	public class Category : IIdentificable
	{
		readonly int id;

		public void Remove ()
		{
			foreach (var task in _collection.EnumerateTasks ().Where (z => z.HasCategory (this)))
			{
				task.RemoveCategory (Id);
			}
			_collection._collection.Remove (this);
		}

		public void Initialize (TaskCollection coll)
		{
			_collection = coll;
		}

		public int Id
		{
			get
			{
				return id;
			}
		}

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