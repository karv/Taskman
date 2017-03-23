using System.Linq;
using NUnit.Framework;
using Taskman;

namespace Test
{
	[TestFixture]
	public class Cats
	{
		public TaskCollection Coll;

		[SetUp]
		public void SetUp ()
		{
			Coll = new TaskCollection ();
		}

		[Test]
		public void AddCat ()
		{
			var cat = Coll.AddCategory ();
			Assert.NotNull (cat);
			Assert.True (Coll.OfType<Category> ().Any ());
		}

		[Test]
		public void AddFalseCat ()
		{
			var task = Coll.AddNew ();
			Assert.Throws<IdNotFoundException> (delegate
			{
				task.AddCategory (task.Id + 1);
			});
		}

		[Test]
		public void AddTrueCat ()
		{
			var task = Coll.AddNew ();
			var cat = Coll.AddCategory ();
			task.AddCategory (cat.Id);
		}

		[Test]
		public void RemoveCat ()
		{
			var cat = Coll.AddCategory ();
			cat.Remove ();
			Assert.IsEmpty (Coll.OfType<Category> ());
		}

		[Test]
		public void RemoveDependentCat ()
		{
			var task = Coll.AddNew ();
			var cat = Coll.AddCategory ();
			task.AddCategory (cat.Id);
			cat.Clear ();
			foreach (var t in Coll.EnumerateTasks ())
				Assert.False (t.HasCategory (cat));
			cat.Remove ();
		}
	}
}