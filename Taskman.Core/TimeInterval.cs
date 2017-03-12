using System;

namespace Taskman
{
	public struct TimeInterval : IEquatable<TimeInterval>
	{
		public readonly DateTime StartTime;
		public readonly TimeSpan Duration;

		public DateTime EndTime
		{
			get
			{
				return StartTime + Duration;
			}
		}

		public bool Intersects (TimeInterval other)
		{
			return intersetcsRight (other) || other.intersetcsRight (this);
		}

		public TimeInterval Merge (TimeInterval left, TimeInterval right)
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

		#region IEquatable implementation

		public bool Equals (TimeInterval other)
		{
			return StartTime == other.StartTime && Duration == other.Duration;
		}

		public override bool Equals (object obj)
		{
			if (obj is TimeInterval)
			{
				var other = (TimeInterval)obj;
				return Equals (other);
			}
			return false;
		}

		public override int GetHashCode ()
		{
			return StartTime.GetHashCode () ^ Duration.GetHashCode ();
		}

		public override string ToString ()
		{
			return string.Format ("[{0} - {1}]", StartTime, EndTime);
		}

		public static bool operator == (TimeInterval left, TimeInterval right)
		{
			return left.Equals (right);
		}

		public static bool operator != (TimeInterval left, TimeInterval right)
		{
			return !left.Equals (right);
		}

		#endregion

		public TimeInterval (DateTime startTime, TimeSpan duration)
		{
			if (duration < TimeSpan.Zero)
				throw new InvalidOperationException ("duration cannot be negative.");
			
			StartTime = startTime;
			Duration = duration;
		}

		public TimeInterval (DateTime startTime, DateTime endTime)
			: this (startTime, endTime - startTime)
		{
		}
	}
}