using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Taskman;
using System;

namespace Test.Core
{
	[TestFixture]
	public class CollectionTests
	{
		TaskCollection Collection;

		[SetUp]
		public void Setup ()
		{
			Collection = new TaskCollection ();
			Assert.IsEmpty (Collection);
		}

		[Test]
		public void CreateRoot ()
		{
			var task = Collection.AddNew ();
			Assert.AreEqual (1, Collection.Count);
			Assert.IsTrue (Collection.Contains (task));

			Collection.AddNew ();
			Assert.AreEqual (2, Collection.Count);
			Assert.IsTrue (Collection.Contains (task));
		}

		[Test]
		public void EnumerateRoots ()
		{
			var task0 = Collection.AddNew ();
			var sub = task0.CreateSubtask ();
			var task1 = Collection.AddNew ();

			Assert.AreEqual (3, Collection.Count);
			var roots = new List<Task> (Collection.EnumerateRoots ());
			Assert.AreEqual (2, roots.Count);
			Assert.True (Collection.Contains (task0));
			Assert.True (Collection.Contains (task1));
			Assert.True (Collection.Contains (sub));

			Assert.True (roots.Contains (task0));
			Assert.True (roots.Contains (task1));
		}

		[Test]
		public void EnumerationRecursive ()
		{
			const int numRoots = 5;
			const int numTasks = 100;

			var r = new Random ();

			for (int i = 0; i < numRoots; i++)
				Collection.AddNew ();

			for (int i = 0; i < numTasks - numRoots; i++)
			{
				var ls = Collection.ToArray ();
				var master = ls [r.Next (ls.Length)];
				master.CreateSubtask ();
			}

			Assert.AreEqual (numTasks, Collection.Count);

			var l = Collection.EnumerateRoots ().ToArray ();
			var sum = 0;

			Assert.AreEqual (numRoots, l.Length);
			foreach (var root in l)
			{
				var subs = root.GetSubtasksRecursive ();
				sum += subs.Length;
				foreach (var task in subs)
					Assert.True (Collection.Contains (task));
			}
			Assert.AreEqual (numTasks, sum);
		}
	}
}