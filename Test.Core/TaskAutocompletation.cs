using NUnit.Framework;
using Taskman;

namespace Test
{
	[TestFixture]
	public class TaskAutocompletation
	{
		public TaskCollection Collection;

		[SetUp]
		public void Setup ()
		{
			Collection = new TaskCollection ();
		}

		[Test]
		public void AutoMarkingCompleted ()
		{
			var root = Collection.AddNew ();
			root.AutoCompletable = true;
			var child0 = root.CreateSubtask ();
			var child1 = root.CreateSubtask ();
			Assert.AreEqual (TaskStatus.Inactive, root.Status);
			child0.Status = TaskStatus.Completed;
			Assert.AreEqual (TaskStatus.Inactive, root.Status);
			child1.Status = TaskStatus.Completed;
			Assert.AreEqual (TaskStatus.Completed, root.Status);
		}

		[Test]
		public void AutoMarkingCompletedFalse ()
		{
			var root = Collection.AddNew ();
			var child0 = root.CreateSubtask ();
			var child1 = root.CreateSubtask ();
			Assert.AreEqual (TaskStatus.Inactive, root.Status);
			child0.Status = TaskStatus.Completed;
			Assert.AreEqual (TaskStatus.Inactive, root.Status);
			child1.Status = TaskStatus.Completed;
			Assert.AreEqual (TaskStatus.Inactive, root.Status);
		}
	}
}