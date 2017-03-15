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
			while (GetById (id) != null);
			return id;
		}

		/// <summary>
		/// Gets a task determined by its id
		/// </summary>
		public Task GetById (int id)
		{
			return _collection.FirstOrDefault (z => z.Id == id);
		}

		#region ICollection implementation

		internal void Add (Task item)
		{
			_collection.Add (item);
		}

		/// <summary>
		/// Adds and returns a new task in root
		/// </summary>
		public Task AddNew ()
		{
			var ret = new Task (this);
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

		public void Save (string fileName)
		{
			var f = new StreamWriter (fileName, false);
			var str = JsonConvert.SerializeObject (this, jsonSets);
			f.WriteLine (str);
			f.Close ();
		}

		public static TaskCollection Load (string fileName)
		{
			var f = new StreamReader (fileName);
			var str = f.ReadToEnd ();
			return JsonConvert.DeserializeObject<TaskCollection> (str);
		}

		/// <summary>
		/// Determines whether a task is contained in this collection
		/// </summary>
		public bool Contains (Task item)
		{
			if (item.IsDisposed)
				throw new InvalidOperationException ("task is disposed");
			return _collection.Contains (item);
		}

		/// <summary>
		/// Removes a task from this collection
		/// </summary>
		public bool Remove (Task item)
		{
			if (item.IsDisposed)
				throw new InvalidOperationException ("task is disposed");
			
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

		static JsonSerializerSettings jsonSets = new JsonSerializerSettings ()
		{
			TypeNameHandling = TypeNameHandling.Auto,
			ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
			PreserveReferencesHandling = PreserveReferencesHandling.All,
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
		TaskCollection (Task [] Collection)
		{
			Comparer = EqualityComparer<Task>.Default;
			_collection = new HashSet<Task> (Collection, Comparer);
		}
	}
}