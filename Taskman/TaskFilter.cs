using System;
using System.Linq;
using Gtk;
using System.Collections.Generic;

namespace Taskman.Gui
{
	/// <summary>
	/// Task filtering options
	/// </summary>
	public class TaskFilter
	{
		/// <summary>
		/// Show complete tasks
		/// </summary>
		public bool ShowCompleted = true;
		/// <summary>
		/// Show inactive tasks
		/// </summary>
		public bool ShowInactive = true;
		/// <summary>
		/// Include category filters in this filter
		/// </summary>
		public bool FilterCats = true;

		/// <summary>
		/// The collection of tasks
		/// </summary>
		public readonly TaskCollection Tasks;

		/// <summary>
		/// The asignation that lists the current category filter
		/// </summary>
		public Func <List<Tuple<Category, bool>>> CatRules;

		/// <summary>
		/// Applies the filter.to a specified tree iterator
		/// </summary>
		/// <param name="model">The store model</param>
		/// <param name="iter">The current tree iterator</param>
		public bool ApplyFilter (ITreeModel model, TreeIter iter)
		{
			var taskId = (int)model.GetValue (iter, 0);
			var task = Tasks.GetById<Task> (taskId);
			var filter = CatRules ();
			return filter.All (z => task.HasCategory (z.Item1) == z.Item2) &&
			(ShowCompleted || task.Status != TaskStatus.Completed) &&
			(ShowInactive || task.Status == TaskStatus.Active);
		}

		/// <param name="tasks">Task collection</param>
		public TaskFilter (TaskCollection tasks)
		{
			Tasks = tasks;
		}
	}
}