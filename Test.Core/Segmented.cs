using System;
using NUnit.Framework;
using Taskman;

namespace Test
{
	[TestFixture]
	public class Segmented
	{
		[Test]
		public void AddSegment ()
		{
			var sg = new SegmentedTimeSpan ();
			sg.Add (new TimeInterval (DateTime.Now, TimeSpan.FromSeconds (1)));
			Assert.AreEqual (1, sg.GetComponents ().Length);
			sg.Add (new TimeInterval (sg.Max (), TimeSpan.FromSeconds (1)));
			Assert.AreEqual (1, sg.GetComponents ().Length);
		}

		[Test]
		public void MaxMinEmptySegment ()
		{
			var sg = SegmentedTimeSpan.Empty;
			Assert.Throws<InvalidOperationException> (delegate
			{
				sg.Max ();
			});
			Assert.Throws<InvalidOperationException> (delegate
			{
				sg.Min ();
			});

		}
	}
}