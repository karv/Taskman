using System;
using System.Collections.Generic;
using System.Linq;

namespace Taskman
{
	public class TaskCollection : IEnumerable<Task>
	{
		readonly HashSet<Task> _collection;

		public IEqualityComparer<Task> Comparer { get; }

		readonly Random _r = new Random ();

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

		public Task GetById (int id)
		{
			return _collection.FirstOrDefault (z => z.Id == id);
		}

		#region ICollection implementation

		internal void Add (Task item)
		{
			_collection.Add (item);
		}

		public Task AddNew ()
		{
			var ret = new Task (this);
			return ret;
		}

		public void Clear ()
		{
			foreach (var task in _collection)
				task.Dispose ();
			_collection.Clear ();
		}

		public bool Contains (Task item)
		{
			if (item.IsDisposed)
				throw new InvalidOperationException ("task is disposed");
			return _collection.Contains (item);
		}

		public bool Remove (Task item)
		{
			if (item.IsDisposed)
				throw new InvalidOperationException ("task is disposed");
			
			var ret = _collection.Remove (item);
			if (ret)
				item.Dispose ();
			return ret;
		}

		public int Count { get { return _collection.Count; } }

		#endregion

		#region IEnumerable implementation

		public IEnumerator<Task> GetEnumerator ()
		{
			return _collection.GetEnumerator ();
		}

		#endregion

		#region IEnumerable implementation

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		#endregion

		public IEnumerable<Task> EnumerateRoots ()
		{
			return _collection.Where (isRoot);
		}

		static bool isRoot (Task task)
		{
			return task.MasterTask == null;
		}

		public IEnumerable<Task> EnumerateInmediateChildOf (Task task)
		{
			return _collection.Where (z => Comparer.Equals (z.MasterTask, task));
		}

		public TaskCollection ()
		{
			Comparer = EqualityComparer<Task>.Default;
			_collection = new HashSet<Task> (Comparer);
		}
	}
}