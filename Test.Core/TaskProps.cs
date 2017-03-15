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
	}
}