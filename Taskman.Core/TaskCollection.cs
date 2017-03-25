using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Taskman
{
	/// <summary>
	/// Manages a collection of tasks
	/// </summary>
	public class TaskCollection : IEnumerable<IIdentificable>
	{
		[JsonProperty ("Collection")]
		internal readonly HashSet<IIdentificable> _collection;

		/// <summary>
		/// Gets the tasks equality comparer
		/// </summary>
		public IEqualityComparer<IIdentificable> Comparer { get; }

		readonly Random _r = new Random ();

		/// <summary>
		/// Gets a new randomly generated unused id
		/// </summary>
		public int GetUnusedId ()
		{
			int id;
			do
			{
				id = _r.Next ();
			}
			while (id != 0 && GetById (id) != null);
			return id;
		}

		public bool ExistObject (int id)
		{
			return id == 0 || GetById (id) != null;
		}

		/// <summary>
		/// Enumerates the tasks
		/// </summary>
		public IEnumerable<Task> EnumerateTasks ()
		{
			return _collection.OfType<Task> ();
		}

		/// <summary>
		/// Enumerates the categories
		/// </summary>
		public IEnumerable<Category> EnumerateCategories ()
		{
			return _collection.OfType<Category> ();
		}

		/// <summary>
		/// Gets a task determined by its id
		/// </summary>
		public IIdentificable GetById (int id)
		{
			if (id == 0)
				return null;
			try
			{
				return _collection.FirstOrDefault (z => z.Id == id);
			}
			catch (Exception ex)
			{
				throw new IdNotFoundException (this, id, string.Format ("{0} not found", id), ex);
			}
		}

		/// <summary>
		/// Gets the object determined by its identifier
		/// </summary>
		/// <param name="id">Identifier</param>
		/// <typeparam name="T">Type of object to get</typeparam>
		public T GetById<T> (int id)
			where T : IIdentificable
		{
			return (T)GetById (id);
		}

		/// <summary>
		/// Initialize this instance.
		/// </summary>
		public void Initialize ()
		{
			foreach (var c in _collection.OfType<Task> ().Cast<IIdentificable> ())
				c.Initialize (this);
		}

		#region ICollection implementation

		internal void Add (IIdentificable item)
		{
			_collection.Add (item);
			item.Initialize (this);
		}

		/// <summary>
		/// Adds and returns a new task in root
		/// </summary>
		public Task AddNew ()
		{
			var ret = new Task (this);
			Add (ret); 
			return ret;
		}

		/// <summary>
		/// Add a new category to the collection
		/// </summary>
		public Category AddCategory ()
		{
			var ret = new Category (this);
			Add (ret);
			return ret;
		}

		/// <summary>
		/// Empties this collection, and disposes its tasks
		/// </summary>
		public void Clear ()
		{
			foreach (var task in _collection.OfType<Task> ())
				task.Dispose ();
			_collection.Clear ();
		}

		/// <summary>
		/// Saves this collection
		/// </summary>
		/// <param name="fileName">File name</param>
		public void Save (string fileName)
		{
			var f = new StreamWriter (fileName, false);
			var str = JsonConvert.SerializeObject (this, jsonSets);
			f.WriteLine (str);
			f.Close ();
		}

		/// <summary>
		/// Load from the specified fileName.
		/// </summary>
		/// <param name="fileName">File name.</param>
		public static TaskCollection Load (string fileName)
		{
			var f = new StreamReader (fileName);
			var str = f.ReadToEnd ();
			f.Close ();
			return JsonConvert.DeserializeObject<TaskCollection> (str, jsonSets);
		}


		/// <summary>
		/// Removes a task or category from this collection
		/// </summary>
		public void Remove (IIdentificable item)
		{
			item.Remove ();
		}

		/// <summary>
		/// Gets the number of tasks in this collection
		/// </summary>
		public int Count { get { return _collection.Count; } }

		#endregion

		#region IEnumerable implementation

		IEnumerator<IIdentificable> IEnumerable<IIdentificable>.GetEnumerator ()
		{
			return _collection.GetEnumerator ();
		}

		#endregion

		#region IEnumerable implementation

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			return _collection.GetEnumerator ();
		}

		#endregion

		/// <summary>
		/// Enumerates the root tasks
		/// </summary>
		/// <returns>The roots.</returns>
		public IEnumerable<Task> EnumerateRoots ()
		{
			return _collection.OfType<Task> ().Where (isRoot);
		}

		static bool isRoot (Task task)
		{
			return task.IsRoot;
		}

		static JsonSerializerSettings jsonSets = new JsonSerializerSettings
		{
			TypeNameHandling = TypeNameHandling.Objects,
			ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
			PreserveReferencesHandling = PreserveReferencesHandling.None,
			ObjectCreationHandling = ObjectCreationHandling.Reuse,
			NullValueHandling = NullValueHandling.Ignore,
			Formatting = Formatting.Indented
		};

		/// <summary>
		/// </summary>
		public TaskCollection ()
		{
			Comparer = new IdentifyComparer ();
			_collection = new HashSet<IIdentificable> (Comparer);
		}

		[JsonConstructor]
		TaskCollection (IEnumerable <IIdentificable> Collection)
		{
			Comparer = new IdentifyComparer ();
			_collection = new HashSet<IIdentificable> (Collection, Comparer);

			Initialize ();
		}
	}
}