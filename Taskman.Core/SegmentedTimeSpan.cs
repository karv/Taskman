using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Taskman
{
	public class SegmentedTimeSpan
	{
		/// <summary>
		/// A mutable list of segments. Assumed sorted and disjoint
		/// </summary>
		readonly List<TimeInterval> segments;


		[Conditional ("DEBUG")]
		public void checkOverlaps ()
		{
			for (int i = 0; i < segments.Count - 1; i++)
				for (int j = i + 1; j < segments.Count; j++)
					if (segments [i].Intersects (segments [j]))
						throw new Exception ();
		}

		public SegmentedTimeSpan ()
		{
			segments = new List<TimeInterval> ();
		}

		public static implicit operator SegmentedTimeSpan (TimeInterval interval)
		{
			var ret = new SegmentedTimeSpan ();
			ret.segments.Add (interval);
		}
	}
}