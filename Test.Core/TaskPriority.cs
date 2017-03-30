using NUnit.Framework;
using Taskman;

namespace Test
{
	[TestFixture]
	public class TaskPriority
	{
		public TaskCollection Collection;

		[SetUp]
		public void Setup ()
		{
			Collection = new TaskCollection ();
		}

		[Test]
		public void GoodCalculation ()
		{
			var root = Collection.AddNew ();
			var sub0 = root.CreateSubtask ();
			var sub1 = root.CreateSubtask ();
			sub0.SelfPriority = 3;
			sub1.SelfPriority = 2;

			Assert.AreEqual (3, root.RecursivePriority);
		}
	}
}