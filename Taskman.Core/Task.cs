using System;
using System.Linq;
using System.Collections.Generic;

namespace Taskman
{
	public class Task : IEquatable<Task>
	{
		public string Name;
		public string Descript;

		readonly int _id;

		public int Id
		{
			get
			{
				if (IsDisposed)
					throw new InvalidOperationException ("Cannot get the Id from a disposed task.");
				return _id;
			}
		}

		public DateTime CreationTime;
		public DateTime BeginTime;
		public DateTime TerminationTime;

		readonly HashSet<Task> _subtasks;
		public TaskStatus Status;

		public bool IsDisposed { get; private set; }

		public readonly TaskCollection _collection;
		public readonly Task MasterTask;

		public Task[] GetSubtasks ()
		{
			return _subtasks.ToArray ();
		}

		protected IEnumerable<Task> EnumerateRecursiveSubtasks ()
		{
			yield return this;
			foreach (var task in _subtasks)
			{
				foreach (var sTask in task.EnumerateRecursiveSubtasks ())
					yield return sTask;
			}
		}

		public Task[] GetSubtasksRecursive ()
		{
			return EnumerateRecursiveSubtasks ().ToArray ();
		}

		public TaskCollection Collection
		{
			get
			{
				if (IsDisposed)
					throw new InvalidOperationException ("Cannot get the collection from a disposed task.");
				return _collection;
			}
		}

		#region IEquatable implementation

		bool IEquatable<Task>.Equals (Task other)
		{
			return other == null || other?.Id == Id;
		}

		#endregion

		public Task CreateSubtask ()
		{
			var ret = new Task (Collection, this);
			return ret;
		}

		public void Dispose ()
		{
			IsDisposed = true;
		}

		public override string ToString ()
		{
			return string.IsNullOrEmpty (Name) ? Id.ToString () : Name;
		}

		internal Task (TaskCollection collection)
		{
			if (collection == null)
				throw new ArgumentNullException ("collection");
			
			_collection = collection;
			_collection.Add (this);
			CreationTime = DateTime.Now;
			_subtasks = new HashSet<Task> (_collection.Comparer);
			_id = Collection.GetUnusedId ();
		}

		internal Task (TaskCollection collection, Task masterTask)
		{
			if (masterTask == null)
				throw new ArgumentNullException ("masterTask");
			if (collection == null)
				throw new ArgumentNullException ("collection");
			if (!collection.Contains (masterTask))
				throw new InvalidOperationException ("Master task is not in the collection");

			_collection = collection;
			_collection.Add (this);
			CreationTime = DateTime.Now;
			MasterTask = masterTask;
			MasterTask._subtasks.Add (this);
			_subtasks = new HashSet<Task> (_collection.Comparer);
			_id = Collection.GetUnusedId ();
		}
	}
}