using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Taskman
{
	/// <summary>
	/// Represents a set of non overlaping <see cref="TimeInterval"/>
	/// </summary>
	public class SegmentedTimeSpan
	{
		/// <summary>
		/// A mutable list of segments. Assumed sorted and disjoint
		/// </summary>
		readonly List<TimeInterval> segments;

		[Conditional ("DEBUG")]
		void checkOverlaps ()
		{
			for (int i = 0; i < segments.Count - 1; i++)
				for (int j = i + 1; j < segments.Count; j++)
					if (segments [i].Intersects (segments [j]))
						throw new Exception ();
		}

		/// <summary>
		/// </summary>
		public SegmentedTimeSpan ()
		{
			segments = new List<TimeInterval> ();
		}

		/// <param name="interval">A time interval E</param>
		public static implicit operator SegmentedTimeSpan (TimeInterval interval)
		{
			var ret = new SegmentedTimeSpan ();
			ret.segments.Add (interval);
			return ret;
		}
	}
}