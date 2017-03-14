using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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

		void checkForEmpty ()
		{
			foreach (var s in segments)
			{
				if (s.IsEmpty)
					throw new Exception ();
			}
		}

		public void Add (TimeInterval interval)
		{
			var I = segments.Where (interval.Intersects);
			var newInt = TimeInterval.Empty;
			foreach (var intr in I)
				newInt = newInt.Merge (intr);
			segments.RemoveAll (I.Contains);
			if (!newInt.IsEmpty)
				segments.Add (newInt);

			checkOverlaps ();
			checkForEmpty ();
		}

		public TimeInterval[] GetComponents ()
		{
			return segments.ToArray ();
		}

		public TimeInterval ConvexHull ()
		{
			return new TimeInterval (Min (), Max ());
		}

		public DateTime Min ()
		{
			return segments.Min (z => z.StartTime);
		}

		public DateTime Max ()
		{
			return segments.Max (z => z.EndTime);
		}

		public bool IsEmpty
		{ get { return !segments.Any (); } }

		/// <summary>
		/// </summary>
		public SegmentedTimeSpan ()
		{
			segments = new List<TimeInterval> ();
		}

		public static readonly TimeInterval Empty;

		/// <param name="interval">A time interval E</param>
		public static implicit operator SegmentedTimeSpan (TimeInterval interval)
		{
			var ret = new SegmentedTimeSpan ();
			if (!interval.IsEmpty)
				ret.segments.Add (interval);
			return ret;
		}
	}
}