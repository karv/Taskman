using System;

namespace Taskman
{
	/// <summary>
	/// Circular dependency exception.
	/// </summary>
	[Serializable]
	public class CircularDependencyException : Exception
	{
		/// <summary>
		/// The root of the circularity
		/// </summary>
		public readonly Task Task;

		/// <summary>
		/// The task collection
		/// </summary>
		public TaskCollection Collection { get { return Task.Collection; } }

		/// <summary>
		/// </summary>
		public CircularDependencyException (Task task, string message = "")
			: base (message)
		{
			Task = task;
		}

		/// <summary>
		/// </summary>
		public CircularDependencyException (Task task, Exception inner, string message = "")
			: base (message, inner)
		{
			Task = task;
		}

		/// <summary>
		/// </summary>
		protected CircularDependencyException (System.Runtime.Serialization.SerializationInfo info,
		                                       System.Runtime.Serialization.StreamingContext context)
			: base (info,
			        context)
		{
		}
	}
}