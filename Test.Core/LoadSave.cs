using System.Linq;
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
			const string fn = "test00";
			var tc = new TaskCollection ();
			tc.AddNew ();
			tc.AddNew ();
			tc.Save (fn);
			var tc2 = TaskCollection.Load (fn);

			Assert.AreEqual (tc.Count, tc2.Count);
		}

		[Test]
		public void PreserveId ()
		{
			const string fn = "test01";
			var tc = new TaskCollection ();
			var oldTask = tc.AddNew ();

			tc.Save (fn);
			var tc2 = TaskCollection.Load (fn);
			var f = tc2.EnumerateRoots ().First ();

			Assert.AreEqual (oldTask.Id, f.Id);
		}

		[Test]
		public void SaveChild ()
		{
			const string fn = "test02";
			var tc = new TaskCollection ();
			var oldTask = tc.AddNew ();
			oldTask.CreateSubtask ();

			tc.Save (fn);
			var tc2 = TaskCollection.Load (fn);

			Assert.AreEqual (tc.Count, tc2.Count);
			Assert.AreEqual (tc.EnumerateRoots ().Count (), tc2.EnumerateRoots ().Count ());
			Assert.IsTrue (tc2.GetById<Task> (oldTask.Id).GetSubtasks ().Any ());
		}
	}
}