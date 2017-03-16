using System;
using NUnit.Framework;
using Taskman;

namespace Test
{
	[TestFixture]
	public class TaskProps
	{
		[Test]
		public void NoNullCtor ()
		{
			var col = new TaskCollection ();
			var task = col.AddNew ();
			Assert.IsNotNull (task);
			Assert.NotNull (task.ActivityTime);
		}

		static void switchAndTest (Task task, TaskStatus newStatus)
		{
			task.Status = newStatus;
			Assert.AreEqual (newStatus, task.Status);
		}

		[Test]
		public void StatusSetter ()
		{
			var col = new TaskCollection ();
			var task = col.AddNew ();

			switchAndTest (task, TaskStatus.Active);
			switchAndTest (task, TaskStatus.Completed);
			var acc0 = task.TotalActivityTime;
			Assert.Greater (acc0, TimeSpan.Zero);

			// Backward
			switchAndTest (task, TaskStatus.Active);
			switchAndTest (task, TaskStatus.Inactive);
			Assert.Greater (task.TotalActivityTime, acc0);
		}
	}
}