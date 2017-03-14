using System;
using NUnit.Framework;
using Taskman;

namespace Test.Core
{
	[TestFixture]
	public class Interval
	{
		[Test]
		public void NegativeTime ()
		{
			Assert.Throws<InvalidOperationException> (delegate
			{
				new TimeInterval (DateTime.Now, TimeSpan.FromHours (-1));
			});
		}

		[Test]
		public void Intersections ()
		{
			var t0 = DateTime.Now;
			var t1 = t0 + TimeSpan.FromSeconds (1);
			var t2 = t0 + TimeSpan.FromSeconds (2);
			var t3 = t0 + TimeSpan.FromSeconds (3);

			var inter0 = new TimeInterval (t0, t2);
			var inter1 = new TimeInterval (t1, t3);

			Assert.True (inter0.Intersects (inter1));

			var inter2 = new TimeInterval (t0, t1);
			var inter3 = new TimeInterval (t2, t3);
			Assert.False (inter2.Intersects (inter3));
		}

		[Test]
		public void Merging ()
		{
			var t0 = DateTime.Now;
			var t1 = t0 + TimeSpan.FromSeconds (1);
			var t2 = t0 + TimeSpan.FromSeconds (2);
			var t3 = t0 + TimeSpan.FromSeconds (3);

			var inter0 = new TimeInterval (t0, t2);
			var inter1 = new TimeInterval (t1, t3);
			var inter2 = new TimeInterval (t0, t1);
			var inter3 = new TimeInterval (t2, t3);

			var bigInterval = inter0.Merge (inter1);
			Assert.AreEqual (TimeSpan.FromSeconds (3), bigInterval.Duration);
			Assert.AreEqual (t0, bigInterval.StartTime);

			Assert.Throws<InvalidOperationException> (delegate
			{
				inter2.Merge (inter3);
			});
		}
	}
}