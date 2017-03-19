using System;
using System.Linq;
using Gtk;
using System.Collections.Generic;

namespace Taskman.Gui
{
	// TEST
	public class TaskFilter
	{
		public bool ShowCompleted = true;
		public bool ShowInactive = true;

		public bool FilterCats = true;

		public readonly TaskCollection Tasks;

		public Func <List<Tuple<Category, bool>>> CatRules;

		public bool ApplyFilter (ITreeModel model, TreeIter iter)
		{
			var taskId = (int)model.GetValue (iter, 0);
			var task = Tasks.GetById<Task> (taskId);
			var filter = CatRules ();
			return filter.All (z => task.HasCategory (z.Item1) == z.Item2) &&
			(ShowCompleted || task.Status != TaskStatus.Completed) &&
			(ShowInactive || task.Status == TaskStatus.Active);
		}

		public TaskFilter (TaskCollection tasks)
		{
			Tasks = tasks;
		}
	}
}

