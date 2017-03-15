using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Taskman
{
	/// <summary>
	/// A task
	/// </summary>
	public class Task : IEquatable<Task>
	{
		/// <summary>
		/// Displaying name
		/// </summary>
		public string Name;
		/// <summary>
		/// Descption on this task
		/// </summary>
		public string Descript;

		[JsonIgnore]
		readonly int _id;

		/// <summary>
		/// Gets the unique identifier in the collection
		/// </summary>
		public int Id
		{
			get
			{
				if (IsDisposed)
					throw new InvalidOperationException ("Cannot get the Id from a disposed task.");
				return _id;
			}
		}

		/// <summary>
		/// The creation time
		/// </summary>
		public DateTime CreationTime;

		/// <summary>
		/// The time of this task as active
		/// </summary>
		public SegmentedTimeSpan ActivityTime;

		/// <summary>
		/// Gets a <see cref="TimeSpan"/> representing the tile this task being active.
		/// </summary>
		public TimeSpan TotalActivityTime
		{
			get{ return ActivityTime.Duration; }
		}

		/// <summary>
		/// The begin time
		/// </summary>
		public DateTime BeginTime { get { return ActivityTime.Min (); } }

		/// <summary>
		/// The termination time
		/// </summary>
		public DateTime TerminationTime
		{
			get
			{
				if (Status == TaskStatus.Completed)
					return ActivityTime.Max ();

				throw new Exception ("Cannot get the finished time from an unfinished task.");
			}
		}

		[JsonIgnore]
		readonly HashSet<Task> _subtasks;

		IEnumerable<int> subTaskId
		{
			get { return _subtasks.Select (z => z.Id); }
		}

		/// <summary>
		/// The status if this task
		/// </summary>
		public TaskStatus Status;

		/// <summary>
		/// Gets a value indicating whether this instance is disposed.
		/// </summary>
		public bool IsDisposed { get; private set; }

		/// <summary>
		/// The task collection that manages this task
		/// </summary>
		[JsonProperty ("Collection")]
		public readonly TaskCollection _collection;
		/// <summary>
		/// If not root, returns the master task, <c>null</c> otherwise.
		/// </summary>
		public readonly Task MasterTask;

		/// <summary>
		/// Gets a value indicating whether this task is root.
		/// </summary>
		public bool IsRoot { get { return MasterTask == null; } }

		/// <summary>
		/// Gets a new <see cref="Array"/> containing all the immediate subtasks
		/// </summary>
		public Task[] GetSubtasks ()
		{
			return _subtasks.ToArray ();
		}

		/// <summary>
		/// Enumerate recursively all the subtasks
		/// </summary>
		protected IEnumerable<Task> EnumerateRecursiveSubtasks ()
		{
			yield return this;
			foreach (var task in _subtasks)
			{
				foreach (var sTask in task.EnumerateRecursiveSubtasks ())
					yield return sTask;
			}
		}

		/// <summary>
		/// Gets a new <see cref="Array"/> contaning all the (hereditary) subtasks
		/// </summary>
		public Task[] GetSubtasksRecursive ()
		{
			return EnumerateRecursiveSubtasks ().ToArray ();
		}

		/// <summary>
		/// Gets the collection of tasks
		/// </summary>
		[JsonIgnore]
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

		/// <summary>
		/// Creates and returns a new subtask
		/// </summary>
		public Task CreateSubtask ()
		{
			var ret = new Task (Collection, this);
			return ret;
		}

		/// <summary>
		/// Marks this task as disposed, so it cannot have acces to <see cref="Collection"/>
		/// </summary>
		public void Dispose ()
		{
			IsDisposed = true;
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="Taskman.Task"/>.
		/// </summary>
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
			ActivityTime = new SegmentedTimeSpan ();
			_subtasks = new HashSet<Task> (_collection.Comparer);
			_id = Collection.GetUnusedId ();
		}

		[JsonConstructor]
		internal Task (TaskCollection Collection, Task MasterTask)
		{
			if (MasterTask == null)
				throw new ArgumentNullException ("MasterTask");
			if (Collection == null)
				throw new ArgumentNullException ("Collection");
			if (!Collection.Contains (MasterTask))
				throw new InvalidOperationException ("Master task is not in the collection");

			_collection = Collection;
			_collection.Add (this);
			CreationTime = DateTime.Now;
			this.MasterTask = MasterTask;
			this.MasterTask._subtasks.Add (this);
			_subtasks = new HashSet<Task> (_collection.Comparer);
			_id = Collection.GetUnusedId ();
		}
	}
}