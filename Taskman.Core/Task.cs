using System;

namespace Taskman
{
	public enum TaskStatus
	{
		Inactive,
		Active,
		Completed
	}

	public class Task
	{
		public string Name;
		public string Descript;

		public DateTime CreationTime;
		public DateTime BeginTime;
		public DateTime TerminationTime;

		public TaskStatus Status;

		public Task ()
		{
			CreationTime = DateTime.Now;
		}
	}
}