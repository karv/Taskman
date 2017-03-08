using Gtk;

namespace Taskman.Gui
{
	public class TaskEntry : TreeNode
	{
		readonly public Task Task;

		[TreeNodeValue (Column = 0)]
		public string Name { 
			get { 
				return Task.Name; 
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