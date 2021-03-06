using System;
using Taskman;
using Newtonsoft.Json;

namespace Taskman
{
	/// <summary>
	/// Represents a time interval
	/// </summary>
	public struct TimeInterval : IEquatable<TimeInterval>
	{
		/// <summary>
		/// The start time
		/// </summary>
		public readonly DateTime StartTime;
		/// <summary>
		/// The duration of this interval.
		/// </summary>
		public readonly TimeSpan Duration;

		/// <summary>
		/// Gets the end time
		/// </summary>
		[JsonIgnore]
		public DateTime EndTime
		{
			get
			{
				return StartTime + Duration;
			}
		}

		/// <summary>
		/// Determines if this interval intersect another one.
		/// </summary>
		public bool Intersects (TimeInterval other)
		{
			return intersetcsRight (other) || other.intersetcsRight (this);
		}

		/// <summary>
		/// Determines wheter a <see cref="DateTime"/> is in this interval.
		/// </summary>
		public bool Contains (DateTime time)
		{
			return StartTime <= time && time <= EndTime;
		}

		/// <summary>
		/// Returns a new interval representing the union of this and other <see cref="TimeInterval"/>
		/// </summary>
		public TimeInterval Merge (TimeInterval other)
		{
			return Merge (this, other);
		}

		/// <summary>
		/// Returns a new interval representing the union of other two
		/// </summary>
		/// <exception cref="InvalidOperationException">Throws when the intervals do not intersect</exception>
		public static TimeInterval Merge (TimeInterval left, TimeInterval right)
		{
			if (!left.Intersects (right))
				throw new InvalidOperationException ("Cannot merge disjoint intervals");

			return new TimeInterval (
				min (left.StartTime, right.StartTime),
				max (left.EndTime, right.EndTime));
		}

		static DateTime max (DateTime a, DateTime b)
		{
			return a < b ? b : a;
		}

		static DateTime min (DateTime a, DateTime b)
		{
			return a < b ? a : b;
		}

		bool intersetcsRight (TimeInterval other)
		{
			return StartTime <= other.StartTime && other.StartTime <= StartTime + Duration;
		}

		/// <summary>
		/// Gets a value indicating whether this instance is empty.
		/// </summary>
		/// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
		public bool IsEmpty { get { return Duration == TimeSpan.Zero; } }

		#region IEquatable implementation

		/// <summary>
		/// Determines whether the specified <see cref="Taskman.TimeInterval"/> is equal to the current 
		/// <see cref="Taskman.TimeInterval"/>.
		/// </summary>
		public bool Equals (TimeInterval other)
		{
			if (Duration == TimeSpan.Zero)
				return other.Duration == TimeSpan.Zero;
			return StartTime == other.StartTime && Duration == other.Duration;
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to the current 
		/// <see cref="Taskman.TimeInterval"/>.
		/// </summary>
		public override bool Equals (object obj)
		{
			if (obj is TimeInterval)
			{
				var other = (TimeInterval)obj;
				return Equals (other);
			}
			return false;
		}

		/// <summary>
		/// Serves as a hash function for a <see cref="Taskman.TimeInterval"/> object.
		/// </summary>
		public override int GetHashCode ()
		{
			if (Duration == TimeSpan.Zero)
				return 0;
			return StartTime.GetHashCode () ^ Duration.GetHashCode ();
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="Taskman.TimeInterval"/>.
		/// </summary>
		public override string ToString ()
		{
			return string.Format ("[{0} - {1}]", StartTime, EndTime);
		}

		/// <param name="left">Left.</param>
		/// <param name="right">Right.</param>
		public static bool operator == (TimeInterval left, TimeInterval right)
		{
			return left.Equals (right);
		}

		/// <param name="left">Left.</param>
		/// <param name="right">Right.</param>0
		public static bool operator != (TimeInterval left, TimeInterval right)
		{
			return !left.Equals (right);
		}

		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="Taskman.TimeInterval"/> struct.
		/// </summary>
		/// <param name="startTime">Start time</param>
		/// <param name="duration">Duration</param>
		[JsonConstructor]
		public TimeInterval (DateTime startTime, TimeSpan duration)
		{
			if (duration < TimeSpan.Zero)
				throw new InvalidOperationException ("Duration cannot be negative.");

			StartTime = startTime;
			Duration = duration;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Taskman.TimeInterval"/> struct.
		/// </summary>
		/// <param name="startTime">Start time</param>
		/// <param name="endTime">End time</param>
		public TimeInterval (DateTime startTime, DateTime endTime)
			: this (startTime, endTime - startTime)
		{
		}

		/// <summary>
		/// The empty value
		/// </summary>
		public static readonly TimeInterval Empty;

		static TimeInterval ()
		{
			Empty = new TimeInterval (DateTime.MinValue, TimeSpan.Zero);
		}
	}
}