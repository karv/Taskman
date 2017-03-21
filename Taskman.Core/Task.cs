using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Taskman
{
	/// <summary>
	/// A task
	/// </summary>
	public class Task : IEquatable<Task>, IIdentificable
	{
		/// <summary>
		/// Displaying name
		/// </summary>
		public string Name;

		string descript;

		/// <summary>
		/// Descption on this task
		/// </summary>
		public string Descript
		{
			get
			{
				return descript ?? string.Empty;
			}
			set
			{
				descript = value;
			}
		}


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
					throw new ObjectDisposedException ("Cannot get the Id from a disposed task.");
				return _id;
			}
		}

		[JsonProperty ("Categories")]
		readonly HashSet<int> _cats;

		DateTime contextualTime;
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
				if (status == TaskStatus.Completed)
					return ActivityTime.Max ();

				throw new Exception ("Cannot get the finished time from an unfinished task.");
			}
		}

		public void RemoveCategory (int catId)
		{
			_cats.Remove (catId);
		}

		public void AddCategory (int catId)
		{
			_cats.Add (catId);
		}

		[JsonProperty ("Subtasks")]
		internal readonly HashSet<int> _subtasks;

		TaskStatus status;

		/// <summary>
		/// The status if this task
		/// </summary>
		public TaskStatus Status
		{
			get
			{
				return status;
			}
			set
			{
				SetStatus (value);
			}
		}

		/// <summary>
		/// Sets the specified status, and updates <see cref="ActivityTime"/>
		/// </summary>
		protected void SetStatus (TaskStatus newStatus)
		{
			if (Status == newStatus)
				return;

			switch (Status)
			{
				case TaskStatus.Active:
					if (newStatus == TaskStatus.Completed)
					{
						var tInter = new TimeInterval (contextualTime, DateTime.Now);
						ActivityTime.Add (tInter);
						status = TaskStatus.Completed;
					}
					else
					{
						var tInter = new TimeInterval (contextualTime, DateTime.Now);
						ActivityTime.Add (tInter);
						status = TaskStatus.Inactive;
					}
					return;
				case TaskStatus.Completed:
					if (newStatus == TaskStatus.Active)
					{
						contextualTime = DateTime.Now;
						status = TaskStatus.Active;
					}
					else
					{
						status = TaskStatus.Inactive;
					}
					return;
				case TaskStatus.Inactive:
					if (newStatus == TaskStatus.Active)
					{
						contextualTime = DateTime.Now;
						status = TaskStatus.Active;
					}
					else
					{
						status = TaskStatus.Completed;
					}
					return;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance is disposed.
		/// </summary>
		[JsonIgnore]
		public bool IsDisposed { get; private set; }

		/// <summary>
		/// Initialize this task
		/// </summary>
		/// <param name="coll">Coll.</param>
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
		public Task MasterTask { get { return masterId == 0 ? null : Collection.GetById<Task> (masterId); } }

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
			var ret = _subtasks.Select (z => Collection.GetById<Task> (z)).ToArray ();
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
					throw new ObjectDisposedException ("Cannot get the collection from a disposed task.");
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
		internal void Dispose ()
		{
			IsDisposed = true;
		}

		public bool HasCategory (Category cat)
		{
			return _cats.Contains (cat.Id);
		}

		public void SetCategory (Category cat, bool value)
		{
			SetCategory (cat.Id, value);
		}

		// TEST
		public void SetCategory (int catId, bool value)
		{
			if (value)
				AddCategory (catId);
			else
				RemoveCategory (catId);
		}

		public bool HasCategory (int catId)
		{
			return _cats.Contains (catId);
		}

		public void Remove ()
		{
			if (IsDisposed)
				throw new ObjectDisposedException ("task is disposed");

			MasterTask?._subtasks.Remove (Id);
			var ret = _collection._collection.Remove (this);
			if (ret)
				Dispose ();
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
			ActivityTime = SegmentedTimeSpan.Empty;
			_subtasks = new HashSet<int> ();
			_cats = new HashSet<int> ();
			_id = Collection.GetUnusedId ();
		}

		[JsonConstructor]
		Task (int MasterId, 
		      SegmentedTimeSpan ActivityTime, 
		      int Id, 
		      int [] Subtasks, 
		      int [] Categories)
		{
			_id = Id;
			masterId = MasterId;
			this.ActivityTime = ActivityTime ?? SegmentedTimeSpan.Empty;
			_cats = new HashSet<int> (Categories);
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
			ActivityTime = SegmentedTimeSpan.Empty;
			_cats = new HashSet<int> ();
			_subtasks = new HashSet<int> ();
		}
	}
}