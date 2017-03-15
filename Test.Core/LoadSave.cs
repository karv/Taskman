using NUnit.Framework;
using Taskman;

namespace Test
{
	[TestFixture]
	public class LoadSave
	{
		[Test]
		public void Clone ()
		{
			var fn = "test.txt";
			var tc = new TaskCollection ();
			tc.AddNew ();
			tc.AddNew ();
			tc.Save (fn);
			var tc2 = TaskCollection.Load (fn);

			Assert.AreEqual (tc.Count, tc2.Count);
		}
	}
}