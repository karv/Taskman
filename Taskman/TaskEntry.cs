using Gtk;

namespace Taskman.Gui
{
	[TreeNode]
	public class TaskEntry : TreeNode
	{
		readonly public Task Task;

		[TreeNodeValue (Column = 1)]
		public string Name
		{ 
			get
			{ 
				return Task.Name;
			} 
		}

		[TreeNodeValue (Column = 0)]
		public int TaskId
		{ 
			get
			{ 
				return Task.Id;
			} 
		}

		public TaskEntry (Task task)
		{
			if (task == null)
				throw new System.ArgumentNullException ("task");
			Task = task;
		}
	}
}