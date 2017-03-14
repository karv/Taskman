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

		/// <summary>
		/// Adds a <see cref="TimeInterval"/>
		/// </summary>
		public void Add (TimeInterval interval)
		{
			var I = segments.Where (interval.Intersects);
			var newInt = interval;
			foreach (var intr in I)
				newInt = newInt.Merge (intr);
			segments.RemoveAll (I.Contains);
			if (!newInt.IsEmpty)
				segments.Add (newInt);

			checkOverlaps ();
			checkForEmpty ();
		}

		/// <summary>
		/// Gets an array containing the convex component.
		/// </summary>
		public TimeInterval[] GetComponents ()
		{
			return segments.ToArray ();
		}

		/// <summary>
		/// Gets the convex hull
		/// </summary>
		/// <returns>The hull.</returns>
		public TimeInterval ConvexHull ()
		{
			return new TimeInterval (Min (), Max ());
		}

		/// <summary>
		/// Gets the min
		/// </summary>
		public DateTime Min ()
		{
			return segments.Min (z => z.StartTime);
		}

		/// <summary>
		/// Get the max.
		/// </summary>
		public DateTime Max ()
		{
			return segments.Max (z => z.EndTime);
		}

		/// <summary>
		/// Gets a value indicating whether this instance is empty.
		/// </summary>
		/// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
		public bool IsEmpty
		{ get { return !segments.Any (); } }

		/// <summary>
		/// Determines whether a <see cref="DateTime"/> is included in this class
		/// </summary>
		public bool Contains (DateTime t)
		{
			return segments.Any (z => z.Contains (t));
		}

		/// <summary>
		/// </summary>
		public SegmentedTimeSpan ()
		{
			segments = new List<TimeInterval> ();
		}

		/// <summary>
		/// The empty value
		/// </summary>
		public static readonly SegmentedTimeSpan Empty;

		/// <param name="interval">A time interval E</param>
		public static implicit operator SegmentedTimeSpan (TimeInterval interval)
		{
			var ret = new SegmentedTimeSpan ();
			if (!interval.IsEmpty)
				ret.segments.Add (interval);
			return ret;
		}

		static SegmentedTimeSpan ()
		{
			Empty = new SegmentedTimeSpan ();
		}
	}
}