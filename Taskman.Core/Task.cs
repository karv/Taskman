using System;
using System.Collections.Generic;

namespace Taskman
{

	public class Task : IEquatable<Task>
	{
		public string Name;
		public string Descript;

		public int Id { get; private set; }

		public DateTime CreationTime;
		public DateTime BeginTime;
		public DateTime TerminationTime;

		public HashSet<Task> Subtasks;
		public Task MasterTask;
		public TaskStatus Status;

		#region IEquatable implementation

		bool IEquatable<Task>.Equals (Task other)
		{
			return other == null || other?.Id == Id;
		}

		#endregion

		public static Task Create (TaskCollection coll)
		{
			var ret = new Task (coll.Comparer) { Id = coll.GetUnusedId () };
			coll.Add (ret);
			return ret;
		}

		public static Task Create (TaskCollection coll, Task masterTask)
		{
			if (masterTask == null)
				throw new ArgumentNullException ("masterTask");
			if (!coll.Contains (masterTask))
				throw new InvalidOperationException ("TaskCollection does not contains master task.");
			
			var ret = new Task (coll.Comparer) { Id = coll.GetUnusedId () };
			coll.Add (ret);
			return ret;
		}

		Task (IEqualityComparer<Task> comparer)
		{
			CreationTime = DateTime.Now;
			Subtasks = new HashSet<Task> (comparer);
		}
	}
}