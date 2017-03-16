using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.IO;

namespace Taskman
{
	/// <summary>
	/// Manages a collection of tasks
	/// </summary>
	public class TaskCollection : IEnumerable<Task>
	{
		[JsonProperty ("Collection")]
		readonly HashSet<Task> _collection;

		/// <summary>
		/// Gets the tasks equality comparer
		/// </summary>
		public IEqualityComparer<Task> Comparer { get; }

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

		/// <summary>
		/// Gets a task determined by its id
		/// </summary>
		public Task GetById (int id)
		{
			return _collection.FirstOrDefault (z => z.Id == id);
		}

		/// <summary>
		/// Initialize this instance.
		/// </summary>
		public void Initialize ()
		{
			foreach (var c in _collection)
				c.Initialize (this);
		}

		#region ICollection implementation

		internal void Add (Task item)
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
		/// Empties this collection, and disposes its tasks
		/// </summary>
		public void Clear ()
		{
			foreach (var task in _collection)
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
			return JsonConvert.DeserializeObject<TaskCollection> (str);
		}


		/// <summary>
		/// Removes a task from this collection
		/// </summary>
		public bool Remove (Task item)
		{
			if (item.IsDisposed)
				throw new ObjectDisposedException ("task is disposed");
			
			var ret = _collection.Remove (item);
			if (ret)
				item.Dispose ();
			return ret;
		}

		/// <summary>
		/// Gets the number of tasks in this collection
		/// </summary>
		public int Count { get { return _collection.Count; } }

		#endregion

		#region IEnumerable implementation

		IEnumerator<Task> IEnumerable<Task>.GetEnumerator ()
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
			return _collection.Where (isRoot);
		}

		static bool isRoot (Task task)
		{
			return task.IsRoot;
		}

		static JsonSerializerSettings jsonSets = new JsonSerializerSettings
		{
			TypeNameHandling = TypeNameHandling.Auto,
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
			Comparer = EqualityComparer<Task>.Default;
			_collection = new HashSet<Task> (Comparer);
		}

		[JsonConstructor]
		TaskCollection (IEnumerable <Task> Collection)
		{
			Comparer = EqualityComparer<Task>.Default;
			_collection = new HashSet<Task> (Collection, Comparer);

			Initialize ();
		}
	}
}