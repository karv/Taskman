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
		[JsonIgnore]
		public TimeSpan TotalActivityTime
		{
			get{ return ActivityTime.Duration; }
		}

		/// <summary>
		/// The begin time
		/// </summary>
		[JsonIgnore]
		public DateTime BeginTime { get { return ActivityTime.Min (); } }

		/// <summary>
		/// The termination time
		/// </summary>
		[JsonIgnore]
		public DateTime TerminationTime
		{
			get
			{
				if (Status == TaskStatus.Completed)
					return ActivityTime.Max ();

				throw new Exception ("Cannot get the finished time from an unfinished task.");
			}
		}

		[JsonProperty ("Subtasks")]
		readonly HashSet<int> _subtasks;

		/// <summary>
		/// The status if this task
		/// </summary>
		public TaskStatus Status;

		/// <summary>
		/// Gets a value indicating whether this instance is disposed.
		/// </summary>
		[JsonIgnore]
		public bool IsDisposed { get; private set; }

		public void Initialize (TaskCollection coll)
		{
			_collection = coll;
		}

		/// <summary>
		/// The task collection that manages this task
		/// </summary>
		[JsonIgnore]
		TaskCollection _collection;

		/// <summary>
		/// If not root, returns the master task, <c>null</c> otherwise.
		/// </summary>
		[JsonIgnore]
		public Task MasterTask { get { return masterId == 0 ? null : Collection.GetById (masterId); } }

		[JsonProperty ("MasterId")]
		readonly int masterId;

		/// <summary>
		/// Gets a value indicating whether this task is root.
		/// </summary>
		[JsonIgnore]
		public bool IsRoot { get { return masterId == 0; } }

		/// <summary>
		/// Gets a new <see cref="Array"/> containing all the immediate subtasks
		/// </summary>
		public Task[] GetSubtasks ()
		{
			var ret = _subtasks.Select (z => Collection.GetById (z)).ToArray ();
			return ret;
		}

		/// <summary>
		/// Enumerate recursively all the subtasks
		/// </summary>
		public IEnumerable<Task> EnumerateRecursiveSubtasks ()
		{
			yield return this;
			foreach (var task in GetSubtasks ())
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
			Collection.Add (ret);
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
			CreationTime = DateTime.Now;
			ActivityTime = new SegmentedTimeSpan ();
			_subtasks = new HashSet<int> ();
			_id = Collection.GetUnusedId ();
		}

		[JsonConstructor]
		Task (int MasterId, SegmentedTimeSpan ActivityTime, int Id, int [] Subtasks)
		{
			_id = Id;
			masterId = MasterId;
			this.ActivityTime = ActivityTime;
			_subtasks = new HashSet<int> (Subtasks);
		}

		internal Task (TaskCollection collection, Task masterTask)
		{
			if (masterTask == null)
				throw new ArgumentNullException ("masterTask");
			if (collection == null)
				throw new ArgumentNullException ("collection");
			if (!collection.Contains (masterTask))
				throw new InvalidOperationException ("Master task is not in the collection");

			_id = collection.GetUnusedId ();
			CreationTime = DateTime.Now;
			masterId = masterTask.Id;
			masterTask._subtasks.Add (Id);
			_subtasks = new HashSet<int> ();
		}
	}
}