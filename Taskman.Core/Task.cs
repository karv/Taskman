﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Taskman
{
	/// <summary>
	/// A task
	/// </summary>
	public class Task : IEquatable<Task>, IIdentificable
	{
		#region General

		/// <summary>
		/// Displaying name
		/// </summary>
		public string Name;

		/// <summary>
		/// Priority of this task;
		/// not affected by childrens
		/// </summary>
		public int SelfPriority;

		/// <summary>
		/// Priority (recursive, operated by max) of this task;
		/// </summary>
		[JsonIgnore]
		public int RecursivePriority
		{
			get
			{
				var subTasks = GetSubtasks ();
				return subTasks.Length == 0 ? 
					SelfPriority : 
					Math.Max (SelfPriority, subTasks.Max (z => z.RecursivePriority));
			}
		}

		bool autoCompletable;

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Task"/> sets to complete whenever all its
		/// childs are completed
		/// </summary>
		/// <value><c>true</c> if auto completable; otherwise, <c>false</c>.</value>
		public bool AutoCompletable
		{
			get{ return autoCompletable; }
			set
			{
				autoCompletable = value;
				updateAutocompletableStatus ();
			}
		}

		void updateAutocompletableStatus ()
		{
			if (AutoCompletable && _subtasks.Any () && AllChildCompleted)
				Status = TaskStatus.Completed;
		}

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
						MasterTask?.updateAutocompletableStatus ();
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
						MasterTask?.updateAutocompletableStatus ();
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

		/// <summary>
		/// Marks this task as disposed, so it cannot have acces to <see cref="Collection"/>
		/// </summary>
		internal void Dispose ()
		{
			IsDisposed = true;
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

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="Taskman.Task"/>.
		/// </summary>
		public override string ToString ()
		{
			return string.IsNullOrEmpty (Name) ? Id.ToString () : Name;
		}

		#endregion

		#region Master-Child

		/// <summary>
		/// Returns <c>true</c> only when all its child <see cref="Task"/> are marked as 
		/// <see cref="TaskStatus.Completed"/>
		/// </summary>
		public bool AllChildCompleted
		{
			get{ return GetSubtasks ().All (z => z.Status == TaskStatus.Completed); }
		}

		/// <summary>
		/// Make this task a root task
		/// </summary>
		public void RemoveMaster ()
		{
			if (masterId == 0)
				return; // nothing to do

			MasterTask._subtasks.Remove (Id);
			masterId = 0;
		}

		[JsonProperty ("Subtasks")]
		internal readonly HashSet<int> _subtasks;

		/// <summary>
		/// If not root, returns the master task, <c>null</c> otherwise.
		/// </summary>
		[JsonIgnore]
		public Task MasterTask { get { return masterId == 0 ? null : Collection.GetById<Task> (masterId); } }

		[JsonProperty ("MasterId")]
		int masterId;

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
		/// Creates and returns a new subtask
		/// </summary>
		public Task CreateSubtask ()
		{
			var ret = new Task (Collection, this);
			Collection.Add (ret);
			updateAutocompletableStatus ();
			return ret;
		}

		#endregion

		#region Cats

		[JsonProperty ("Categories")]
		readonly HashSet<int> _cats;

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
			if (!Collection.ExistObject (catId))
				throw new IdNotFoundException (Collection, catId);
			_cats.Add (catId);
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

		#endregion

		#region Time

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

		#endregion

		#region Dynamics

		/// <summary>
		/// Changes the master of this task
		/// </summary>
		/// <param name="newMasterId">New master identifier, remind 0 = no-master</param>
		public void Rebase (int newMasterId)
		{
			var newMaster = Collection.GetById<Task> (newMasterId);
			RemoveMaster ();

			if (newMasterId == 0)
				return;

			// check for circularity
			var iterMaster = newMaster;
			while (iterMaster != null)
			{
				if (iterMaster == this)
					throw new CircularDependencyException (this);
				iterMaster = iterMaster.MasterTask;
			}

			masterId = newMasterId;
			MasterTask._subtasks.Add (Id);
		}

		#endregion

		#region IEquatable implementation

		bool IEquatable<Task>.Equals (Task other)
		{
			return other == null || other?.Id == Id;
		}

		#endregion

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
			var newDep = Collection.GetById<Task> (taskId);
			var depent = newDep.EnumerateRecursivelyDependency ();
			if (depent.Contains (Id))
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

		#region ctor

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
		      int [] Dependents,
		      TaskStatus Status)
		{
			_id = Id;
			Dependents = Dependents ?? new int[] { };
			Subtasks = Subtasks ?? new int[] { };
			Categories = Categories ?? new int[] { };
			masterId = MasterId;
			this.ActivityTime = ActivityTime ?? SegmentedTimeSpan.Empty;
			_cats = new HashSet<int> (Categories);
			_subtasks = new HashSet<int> (Subtasks);
			_dependencyIds = new HashSet<int> (Dependents);
			status = Status;
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

		#endregion
	}
}