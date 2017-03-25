using System.Linq;
using NUnit.Framework;
using Taskman;
using Taskman.Import;

namespace Test
{
	[TestFixture]
	public class Import
	{
		public TaskCollection BaseColl;
		public TaskCollection ImportColl;

		[SetUp]
		public void Setup ()
		{
			BaseColl = new TaskCollection ();
			ImportColl = new TaskCollection ();
		}

		[Test]
		public void SimpleImport ()
		{
			var baseCat = BaseColl.AddCategory ();
			baseCat.Name = "base";
			var importCat = ImportColl.AddCategory ();
			importCat.Name = "import";
			BaseColl.ImportCategory (ImportColl);

			Assert.AreEqual (2, BaseColl.EnumerateCategories ().Count ());
		}

		[Test]
		public void MergeImport ()
		{
			var baseCat = BaseColl.AddCategory ();
			baseCat.Name = "base";
			var importCat = ImportColl.AddCategory ();
			importCat.Name = "base";
			BaseColl.ImportCategory (ImportColl);

			Assert.AreEqual (1, BaseColl.EnumerateCategories ().Count ());
		}
	}
}