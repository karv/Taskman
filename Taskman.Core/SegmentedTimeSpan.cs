using System;
using System.Collections.Generic;

namespace Taskman
{
	public class SegmentedTimeSpan
	{
		/// <summary>
		/// The list of segments. Assumed sorted and disjoint
		/// </summary>
		readonly List<TimeSpan> segments;

		public SegmentedTimeSpan ()
		{
			segments = new List<TimeSpan> ();
		}
	}
}