using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Taskman;

namespace Test
{
	[TestFixture]
	public class TaskProps
	{
		TaskCollection Collection;

		[TestFixtureSetUp]
		public void Setup ()
		{
			Collection = new TaskCollection ();
		}

		[Test]
		public void NoNullCtor ()
		{
			var task = Collection.AddNew ();
			Assert.IsNotNull (task);
			Assert.NotNull (task.ActivityTime);

			var subTask = task.CreateSubtask ();
			Assert.IsNotNull (subTask);
			Assert.NotNull (subTask.ActivityTime);
		}

		static void switchAndTest (Task task, TaskStatus newStatus)
		{
			task.Status = newStatus;
			Assert.AreEqual (newStatus, task.Status);
		}

		[Test]
		public void StatusSetter ()
		{
			var task = Collection.AddNew ();

			switchAndTest (task, TaskStatus.Active);
			switchAndTest (task, TaskStatus.Completed);
			var acc0 = task.TotalActivityTime;
			Assert.Greater (acc0, TimeSpan.Zero);

			// Backward
			switchAndTest (task, TaskStatus.Active);
			switchAndTest (task, TaskStatus.Inactive);
			Assert.Greater (task.TotalActivityTime, acc0);
		}

		[Test]
		public void DisposedTask ()
		{
			var task = Collection.AddNew ();
			Assert.False (task.IsDisposed);
			task.Remove ();

			Assert.True (task.IsDisposed);
			Assert.Throws<ObjectDisposedException> (delegate
			{
				Console.WriteLine (task.Id);
			});
			Assert.Throws<ObjectDisposedException> (delegate
			{
				Console.WriteLine (task.Collection);
			});
			Assert.Throws<ObjectDisposedException> (delegate
			{
				Collection.Remove (task);
			});
		}

		[Test]
		public void RemoveChilds ()
		{
			var master = Collection.AddNew ();
			var child = master.CreateSubtask ();
			child.Remove ();

			foreach (var z in master.GetSubtasks ())
				Assert.NotNull (z);
		}

		[Test]
		public void AddRemoveDependency ()
		{
			var task = Collection.AddNew ();
			var dep = Collection.AddNew ();
			task.AddDependency (dep.Id);
			Assert.AreEqual (1, task.RequieredTasks ().Length);
			task.RemoveDependency (0);
			Assert.AreEqual (1, task.RequieredTasks ().Length);
			task.RemoveDependency (dep.Id);
			Assert.IsEmpty (task.RequieredTasks ());
		}

		[Test]
		public void AutoDependency ()
		{
			var r = new Random ();
			var circLen = r.Next (20);
			var task = Collection.AddNew ();
			var dep = task;
			for (int i = 0; i < circLen; i++)
			{
				var dep2 = Collection.AddNew ();
				dep.AddDependency (dep2.Id);
				dep = dep2;
			}
			Assert.Throws<CircularDependencyException> (delegate
			{
				// complete a dependency circle
				dep.AddDependency (task.Id);
			});
		}

		[Test]
		public void DedendenceDependence ()
		{
			var task = Collection.AddNew ();
			var dep = Collection.AddNew ();
			var dep2 = Collection.AddNew ();
			task.AddDependency (dep.Id);
			dep.AddDependency (dep2.Id);
			Assert.True (task.EnumerateRecursivelyIncompleteTasks ().Contains (dep2));
		}

		[Test]
		public void CompletlyComplete ()
		{
			var task = Collection.AddNew ();
			var dep = Collection.AddNew ();
			var dep2 = Collection.AddNew ();
			task.AddDependency (dep.Id);
			task.AddDependency (dep2.Id);
			Assert.True (task.HasIncompleteDependencies);
			dep.Status = TaskStatus.Completed;
			Assert.True (task.HasIncompleteDependencies);
			dep2.Status = TaskStatus.Completed;
			Assert.False (task.HasIncompleteDependencies);
		}

		[Test]
		public void EnumerateDependencyTask ()
		{
			var task = Collection.AddNew ();
			var dep = Collection.AddNew ();
			var dep2 = Collection.AddNew ();
			task.AddDependency (dep.Id);
			task.AddDependency (dep2.Id);
			var expectedSet = new HashSet<int> (new [] { dep.Id, dep2.Id });
			var enumera = task.EnumerateRecursivelyDependency ();
			Assert.True (expectedSet.SetEquals (enumera));
		}

		[Test]
		public void ClearMaster ()
		{
			var task = Collection.AddNew ();
			var sub = task.CreateSubtask ();
			Assert.True (sub.MasterTask == task);
			sub.RemoveMaster ();
			Assert.IsEmpty (task.GetSubtasks ());
			Assert.True (sub.MasterTask == null);
			Assert.True (sub.IsRoot);
		}

		[Test]
		public void RebaseTo ()
		{
			var task = Collection.AddNew ();
			var newBase = Collection.AddNew ();
			var sub = task.CreateSubtask ();
			Assert.True (sub.MasterTask == task);
			sub.Rebase (newBase.Id);
			Assert.AreEqual (newBase, sub.MasterTask);
			Assert.True (newBase.GetSubtasks ().Contains (sub));
		}

		/// <summary>
		/// Check whether rebase can produce infinite regresion
		/// </summary>
		[Test]
		public void RebaseIllFoundedOneStep ()
		{
			var task = Collection.AddNew ();
			Assert.Throws<CircularDependencyException> (delegate
			{
				// Rebase to itself
				task.Rebase (task.Id);
			});
		}

		/// <summary>
		/// Check whether rebase can produce infinite regresion
		/// </summary>
		[Test]
		public void RebaseIllFoundedTwoSteps ()
		{
			var task = Collection.AddNew ();
			var task2 = task.CreateSubtask ();
			Assert.Throws<CircularDependencyException> (delegate
			{
				// Rebase to itself
				task.Rebase (task2.Id);
			});
		}
	}
}