using System;

namespace Taskman
{
	
	[Serializable]
	public class CircularDependencyException : Exception
	{
		readonly Task Task;

		TaskCollection Collection { get { return Task.Collection; } }

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