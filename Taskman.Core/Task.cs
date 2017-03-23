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

		/// <summary>
		/// Remove a category from this <see cref="Task"/>
		/// </summary>
		/// <param name="catId">The category identifier</param>
		public void RemoveCategory (int catId)
		{
			_cats.Remove (catId);
		}

		/// <summary>
		/// Add a category from this <see cref="Task"/>
		/// </summary>
		/// <param name="catId">The category identifier</param>
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

		void IIdentificable.Initialize (TaskCollection coll)
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

		/// <summary>
		/// Determines whether this instance has the specified category
		/// </summary>
		/// <param name="cat">The category</param>
		public bool HasCategory (Category cat)
		{
			return _cats.Contains (cat.Id);
		}

		/// <summary>
		/// Determines whether this instance has the specified category
		/// </summary>
		/// <param name="catId">The category identifier</param>
		public bool HasCategory (int catId)
		{
			return _cats.Contains (catId);
		}

		/// <summary>
		/// Sets whether this task has a specified category
		/// </summary>
		/// <param name="catId">Category identifier.</param>
		/// <param name="value">If <c>true</c>, the category will be added, otherwise is removed</param>
		// TEST
		public void SetCategory (int catId, bool value)
		{
			if (value)
				AddCategory (catId);
			else
				RemoveCategory (catId);
		}

		/// <summary>
		/// Sets whether this task has a specified category
		/// </summary>
		/// <param name="cat">Category</param>
		/// <param name="value">If <c>true</c>, the category will be added, otherwise is removed</param>
		public void SetCategory (Category cat, bool value)
		{
			SetCategory (cat.Id, value);
		}

		/// <summary>
		/// Remove and dispose ths task.
		/// </summary>
		public void Remove ()
		{
			if (IsDisposed)
				throw new ObjectDisposedException ("task is disposed");

			MasterTask?._subtasks.Remove (Id);
			var ret = _collection._collection.Remove (this);
			if (ret)
				Dispose ();
		}

		#region Dependency

		[JsonProperty ("Dependents")]
		readonly HashSet<int> _dependencyIds;

		/// <summary>
		/// Gets an array with the requiered finalized task to mark this as active or complete
		/// </summary>
		public Task[] RequieredTasks ()
		{
			return _dependencyIds.Select (Collection.GetById<Task>).ToArray ();
		}

		/// <summary>
		/// Enumerates the incomplete nodes from the dependency tree
		/// </summary>
		public IEnumerable<Task> EnumerateRecursivelyIncompleteTasks ()
		{
			foreach (var taskId in _dependencyIds)
			{
				var depTask = Collection.GetById<Task> (taskId);
				if (depTask.Status != TaskStatus.Completed)
				{
					yield return depTask;
					foreach (var subId in depTask.EnumerateRecursivelyIncompleteTasks ())
						yield return subId;
				}
			}
		}

		void addRecursivelyDependency (ICollection<int> accumulativeRet, bool includeSelf = false)
		{
			if (includeSelf)
				accumulativeRet.Add (Id);
			foreach (var depId in _dependencyIds)
			{
				var dep = Collection.GetById<Task> (depId);
				dep.addRecursivelyDependency (accumulativeRet, true);
			}
		}

		/// <summary>
		/// Enumerates the dependency tree (xcludes itself)
		/// </summary>
		public IEnumerable<int> EnumerateRecursivelyDependency (bool includeSelf = false)
		{
			var ret = new List<int> ();
			addRecursivelyDependency (ret, includeSelf);

			return ret;
		}

		/// <summary>
		/// Gets a value indicating whether this task has incomplete dependencies.
		/// </summary>
		[JsonIgnore]
		public bool HasIncompleteDependencies
		{ get { return EnumerateRecursivelyIncompleteTasks ().Any (); } }

		/// <summary>
		/// Adds a dependency
		/// </summary>
		/// <param name="taskId">Task identifier.</param>
		public void AddDependency (int taskId)
		{
			if (EnumerateRecursivelyDependency ().Contains (Id))
				// Circular dependency
				throw new CircularDependencyException (this);
			_dependencyIds.Add (taskId);
			if (Status == TaskStatus.Completed && HasIncompleteDependencies)
				Status = TaskStatus.Inactive;
		}

		/// <summary>
		/// Removes a dependency
		/// </summary>
		/// <param name="taskId">Task identifier.</param>
		public void RemoveDependency (int taskId)
		{
			_dependencyIds.Remove (taskId);
		}

		#endregion

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
			_dependencyIds = new HashSet<int> ();
			_cats = new HashSet<int> ();
			_id = Collection.GetUnusedId ();
		}

		[JsonConstructor]
		Task (int MasterId, 
		      SegmentedTimeSpan ActivityTime, 
		      int Id, 
		      int [] Subtasks, 
		      int [] Categories,
		      int [] Dependents)
		{
			_id = Id;
			masterId = MasterId;
			this.ActivityTime = ActivityTime ?? SegmentedTimeSpan.Empty;
			_cats = new HashSet<int> (Categories);
			_subtasks = new HashSet<int> (Subtasks);
			_dependencyIds = new HashSet<int> (Dependents);
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
			_dependencyIds = new HashSet<int> ();
		}
	}
}