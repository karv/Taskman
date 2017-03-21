using System;
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
	}
}