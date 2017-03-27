using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GLib;
using Gtk;

namespace Taskman.Gui
{
	/// <summary>
	/// The main class
	/// </summary>
	public class MainClass
	{
		/// <summary>
		/// Gets the selected task.
		/// If no task is selected, return <c>null</c>
		/// </summary>
		public Task GetSelectedTask ()
		{
			TreeIter selIter;
			if (TaskSelection.GetSelected (out selIter))
			{
				selIter = CurrentFilter.ConvertIterToChildIter (selIter);
				var id = (int)TaskStore.GetValue (selIter, 0);
				return Tasks.GetById<Task> (id);
			}
			return null;
		}

		/// <summary>
		/// Gets the category selected in the category list.
		/// </summary>
		/// <returns>The selected category if any; <c>null</c> otherwise.</returns>
		public Category GetSelectedCat ()
		{
			TreeIter selIter;
			if (TaskSelection.GetSelected (out selIter))
			{
				selIter = CurrentFilter.ConvertIterToChildIter (selIter);
				var id = (int)TaskStore.GetValue (selIter, 0);
				return Tasks.GetById<Category> (id);
			}
			return null;
		}

		/// <summary>
		/// Gets the (non filtered) iter or null
		/// </summary>
		public TreeIter? GetSelectedIter ()
		{
			TreeIter selIter;
			if (TaskSelection.GetSelected (out selIter))
			{
				selIter = CurrentFilter.ConvertIterToChildIter (selIter);
				return selIter;
			}
			return null;
		}

		void buildCats ()
		{
			CatStore.Clear ();
			foreach (var cat in Tasks.OfType<Category> ())
				CatStore.AppendValues (cat.Id, cat.Name, true, true);
		}

		void expandTasks (object sender = null, System.EventArgs args = null)
		{
			TaskList.ExpandAll ();
		}

		/// <summary>
		/// Initializes
		/// </summary>
		public void Initialize ()
		{
			Builder.AddFromFile ("MainWin.glade");
			TaskPropertyDialogMaker.Initialize ();

			MainWindow = Builder.GetObject ("MainWindow") as Window;
			MainWindow.Destroyed += app_quit;

			TaskStore = Builder.GetObject ("TaskStore") as TreeStore;
			TaskList = (TreeView)Builder.GetObject ("TaskView");

			TaskSelection = (TreeSelection)Builder.GetObject ("TaskSelection");

			//TaskStore = new TreeStore (GType.Int, GType.String, GType.String);
			TaskStore = (TreeStore)Builder.GetObject ("TaskStore");
			CatStore = (ListStore)Builder.GetObject ("CatStore");
			buildCats ();
			TaskList.Model = TaskStore;

			IdColumn = (TreeViewColumn)Builder.GetObject ("TaskIdColumn");
			NameColumn = (TreeViewColumn)Builder.GetObject ("TaskNameColumn");
			StateColumn = (TreeViewColumn)Builder.GetObject ("TaskStatusColumn");

			var idCellRendererText = new CellRendererText ();
			var nameCellRendererText = new CellRendererText ();
			var stateCellRendererText = new CellRendererText ();

			IdColumn.PackStart (idCellRendererText, true);
			NameColumn.PackStart (nameCellRendererText, true);
			StateColumn.PackStart (stateCellRendererText, true);

			IdColumn.AddAttribute (idCellRendererText, "text", (int)ColAssign.Id);
			NameColumn.AddAttribute (nameCellRendererText, "text", (int)ColAssign.Name);
			StateColumn.AddAttribute (stateCellRendererText, "text", (int)ColAssign.State);

			nameCellRendererText.Editable = true;
			nameCellRendererText.Edited += nameChanged;

			var catNameCellRender = (CellRendererText)Builder.GetObject ("CatStatusNameRender");
			catNameCellRender.Editable = true;
			catNameCellRender.Edited += 
				delegate(object o, EditedArgs args)
			{
				TreeIter iter;
				CatStore.GetIterFromString (out iter, args.Path);
				CatStore.SetValue (iter, 1, args.NewText);
				var catId = (int)CatStore.GetValue (iter, 0);
				Tasks.GetById<Category> (catId).Name = args.NewText;
			};


			StatusBar = Builder.GetObject ("Status bar") as Statusbar;

			NewTaskAction = Builder.GetObject ("actNewTask") as Gtk.Action;
			NewChildTask = Builder.GetObject ("actNewChild") as Gtk.Action;
			RemoveTask = Builder.GetObject ("actRemove") as Gtk.Action;
			StartTask = Builder.GetObject ("actStart") as Gtk.Action;
			StopTask = Builder.GetObject ("actStop") as Gtk.Action;
			FinishTask = Builder.GetObject ("actFinish") as Gtk.Action;
			EditTask = Builder.GetObject ("actEditTask") as Gtk.Action;

			((Gtk.Action)Builder.GetObject ("actSave")).Activated += save;
			((Gtk.Action)Builder.GetObject ("actSaveAs")).Activated += saveAs;
			((Gtk.Action)Builder.GetObject ("actLoad")).Activated += load;
			((Gtk.Action)Builder.GetObject ("actExit")).Activated += app_quit;
			((Gtk.Action)Builder.GetObject ("cat.AddCat")).Activated += delegate
			{
				var newCat = Tasks.AddCategory ();
				newCat.Name = "Cat";

				CatStore.AppendValues (newCat.Id, newCat.Name, true, true);
			};

			var toggleCatRender = ((CellRendererToggle)Builder.GetObject ("CatStatusCellRender"));
			toggleCatRender.Toggled += delegate(object o, ToggledArgs args)
			{
				TreeIter iter;
				if (!CatStore.GetIterFromString (out iter, args.Path))
					return;

				var state = (bool)CatStore.GetValue (iter, 3);
				var indet = (bool)CatStore.GetValue (iter, 2);

				if (indet)
				{
					indet = false;
					state = false;
				}
				else
				{
					if (state)
						indet = true;
					else
						state = true;
				}

				CatStore.SetValue (iter, 3, state);
				CatStore.SetValue (iter, 2, indet);

				CurrentFilter.Refilter ();
			};

			((Gtk.Action)Builder.GetObject ("actContractAll")).Activated += delegate
			{
				TaskList.CollapseAll ();
			};

			((Gtk.Action)Builder.GetObject ("actExpandAll")).Activated += expandTasks;

			((Gtk.Action)Builder.GetObject ("actFilterAll")).Activated += delegate
			{
				var togActive = (ToggleToolButton)Builder.GetObject ("togFilterActive");
				var togComplt = (ToggleToolButton)Builder.GetObject ("togFilterComplete");
				togActive.Active = false;
				togComplt.Active = false;
				FilterOptions.ShowCompleted = true;
				FilterOptions.ShowInactive = true;
				CurrentFilter.Refilter ();
			};
			((Gtk.Action)Builder.GetObject ("actFilterActive")).Activated += delegate
			{
				FilterOptions.ShowInactive = !FilterOptions.ShowInactive;
				CurrentFilter.Refilter ();
			};
			((Gtk.Action)Builder.GetObject ("actFilterUnfinished")).Activated += delegate
			{
				FilterOptions.ShowCompleted = !FilterOptions.ShowCompleted;
				CurrentFilter.Refilter ();
			};

			NewTaskAction.Activated += newTask;
			NewChildTask.Activated += newChild;
			RemoveTask.Activated += removeTask;
			EditTask.Activated += delegate
			{
				TaskPropertyDialogMaker.Task = GetSelectedTask ();
				TaskPropertyDialogMaker.BuildWindow ();
				TaskPropertyDialogMaker.AfterResponse = delegate
				{
					// iter.HasValue is asserted
					var iter = GetSelectedIter ().Value;
					reloadIter (iter);
				};

				TaskPropertyDialogMaker.Dialog.Run ();
			};
			StartTask.Activated += delegate
			{
				setTaskStatus (GetSelectedIter ().Value, TaskStatus.Active);
			};

			StopTask.Activated += delegate
			{
				setTaskStatus (GetSelectedIter ().Value, TaskStatus.Inactive);
			};

			FinishTask.Activated += delegate
			{
				setTaskStatus (GetSelectedIter ().Value, TaskStatus.Completed);
			};

			FilterOptions = new TaskFilter (Tasks);
			FilterOptions.CatRules = getCurrentCatFilter;
			CurrentFilter = new TreeModelFilter (TaskStore, null);
			CurrentFilter.VisibleFunc = FilterOptions.ApplyFilter;
			TaskList.Model = CurrentFilter;

			TaskSelection.Changed += updateSensibility;
			updateSensibility (this, null);
		}

		List<Tuple<Category, bool>> getCurrentCatFilter ()
		{
			var ret = new List<Tuple<Category, bool>> ();
			CatStore.Foreach (
				delegate(ITreeModel model, TreePath path, TreeIter iter)
				{
					var indet = (bool)model.GetValue (iter, 2);
					if (!indet)
					{
						var state = (bool)model.GetValue (iter, 3);
						var catId = (int)model.GetValue (iter, 0);
						var cat = Tasks.GetById<Category> (catId);
						ret.Add (new Tuple<Category, bool> (cat, state));
					}
					return false;
				}
			);

			return ret;
		}

		void load (object sender, System.EventArgs e)
		{
			var filter = (FileFilter)Builder.GetObject ("FileFilter");
			filter.Name = "TaskManager";
			var fileChooser = new  FileChooserDialog ("Abrir...", null, FileChooserAction.Open);
			fileChooser.AddButton ("_Abrir", ResponseType.Ok);
			fileChooser.AddButton ("_Cancelar", ResponseType.Cancel);
			fileChooser.DefaultResponse = ResponseType.Ok;
			fileChooser.AddFilter (filter);
			var resp = (ResponseType)fileChooser.Run ();

			if (resp == ResponseType.Ok)
			{
				try
				{
					Tasks = TaskCollection.Load (fileChooser.Filename);
					FilterOptions.Tasks = Tasks;
					rebuildStore ();
					buildCats ();
					CurrentFile = fileChooser.Filename;
					StatusBar.Push (0, "Archivo cargado");
					expandTasks ();
				}
				catch (Exception ex)
				{
					StatusBar.Push (0, "Error cargando archivo");
					Debug.WriteLine ("Something is wrong.\n" + ex);
				}
			}
			fileChooser.Destroy ();
		}

		void rebuildStore ()
		{
			TaskStore.Clear ();
			CurrentFilter.ClearCache ();
			foreach (var task in Tasks.EnumerateRoots ())
				addHerTaskToStore (task);
			CurrentFilter.Refilter ();
		}

		void addHerTaskToStore (Task task, TreeIter? father = null)
		{
			var iter = addTask (father, task);
			foreach (var child in task.GetSubtasks ())
				addHerTaskToStore (child, iter);
			
		}

		void saveAs (object sender, System.EventArgs e)
		{
			var filter = (FileFilter)Builder.GetObject ("FileFilter");
			filter.Name = "TaskManager";
			var fileChooser = new  FileChooserDialog ("Guardar...", MainWindow, FileChooserAction.Save);
			fileChooser.AddButton ("_Guardar", ResponseType.Ok);
			fileChooser.AddButton ("_Cancelar", ResponseType.Cancel);
			fileChooser.DefaultResponse = ResponseType.Ok;
			fileChooser.DoOverwriteConfirmation = true;
			fileChooser.AddFilter (filter);
			var resp = (ResponseType)fileChooser.Run ();

			if (resp == ResponseType.Ok)
			{
				try
				{
					CurrentFile = fileChooser.Filename;
					Tasks.Save (CurrentFile);
					StatusBar.Push (0, "Guardado");
				}
				catch (Exception ex)
				{
					StatusBar.Push (0, "Error guardando archivo");
					Debug.WriteLine (ex);
				}
			}
			fileChooser.Destroy ();

		}

		void saveOn (string fileName)
		{
			try
			{
				Tasks.Save (fileName);
				StatusBar.Push (0, "Guardado");
			}
			catch (Exception ex)
			{
				StatusBar.Push (0, "Algo salió mal al guardar");
				Debug.WriteLine (ex);
			}
		}

		void save (object sender, System.EventArgs e)
		{
			if (string.IsNullOrEmpty (CurrentFile))
				saveAs (sender, e);
			else
				saveOn (CurrentFile);
		}

		void setTaskStatus (TreeIter iter, TaskStatus status)
		{
			var task = getTask (iter);
			task.Status = status;
			reloadIter (iter);
		}

		void reloadIter (TreeIter iter)
		{
			var task = getTask (iter);
			TaskStore.SetValue (iter, (int)ColAssign.Name, task.Name);
			TaskStore.SetValue (iter, (int)ColAssign.State, task.Status.ToString ());
		}

		Task getTask (TreeIter iter)
		{
			var id = (int)TaskStore.GetValue (iter, (int)ColAssign.Id);
			return Tasks.GetById<Task> (id);
		}

		void removeTask (object sender, System.EventArgs e)
		{
			var task = GetSelectedTask ();
			var iter = GetSelectedIter ().Value;
			Tasks.Remove (task);
			TaskStore.Remove (ref iter);
		}

		void nameChanged (object o, EditedArgs args)
		{
			TreeIter iter;
			CurrentFilter.GetIterFromString (out iter, args.Path);
			var storeIter = CurrentFilter.ConvertIterToChildIter (iter);
			var id = (int)TaskStore.GetValue (storeIter, (int)ColAssign.Id);
			var task = Tasks.GetById<Task> (id);
			task.Name = args.NewText;
			Debug.WriteLine (string.Format ("{1} renamed task to {0}", task.Name, task.Id));
			TaskStore.SetValue (storeIter, (int)ColAssign.Name, task.Name);
		}

		void updateSensibility (object sender, System.EventArgs e)
		{
			var selTask = GetSelectedTask ();
			foreach (var act in new [] {NewChildTask, RemoveTask, StartTask, StopTask, FinishTask, EditTask})
			#pragma warning disable 618
			act.Sensitive = selTask != null;
			#pragma warning restore 618
		}

		void app_quit (object sender, System.EventArgs e)
		{
			var saveDial = (Dialog)Builder.GetObject ("SaveQuit Dialog");
			var r = (ResponseType)(saveDial.Run ());

			if (r == ResponseType.Yes)
				save (sender, e);
			
			Gtk.Application.Quit ();
		}

		void newTask (object sender, System.EventArgs e)
		{
			var iter = addTask (null);
			setCursorOnIter (iter);
		}

		void newChild (object sender, System.EventArgs e)
		{
			var iter = addTask (GetSelectedIter ());

			setCursorOnIter (iter);
		}


		void setCursorOnIter (TreeIter storeIter)
		{
			var childIter = CurrentFilter.ConvertChildIterToIter (storeIter);
			var path = CurrentFilter.GetPath (childIter);
			if (path != null)
			{
				TaskList.ExpandToPath (path);
				TaskSelection.SelectIter (childIter);
				TaskList.SetCursor (path, NameColumn, true);
			}
		}

		/// <summary>
		/// </summary>
		/// <returns>The store level new task's iter</returns>
		/// <param name="iter">Store level iter</param>
		TreeIter addTask (TreeIter? iter)
		{
			Task task;
			if (iter == null)
			{
				task = Tasks.AddNew ();
				task.Name = "Nueva tarea";
				return TaskStore.AppendValues (task.Id, task.Name, task.Status.ToString ());
			}
			else
			{
				var master = Tasks.GetById<Task> ((int)TaskStore.GetValue (iter.Value, (int)ColAssign.Id));
				task = master.CreateSubtask ();
				task.Name = task.MasterTask.Name + ".Nueva tarea";
				//var storeIter = CurrentFilter.ConvertIterToChildIter (iter.Value);
				return TaskStore.AppendValues (iter.Value, task.Id, task.Name, task.Status.ToString ());
				//return CurrentFilter.ConvertChildIterToIter (ret);
			}
		}

		TreeIter addTask (TreeIter? iter, Task task)
		{
			return iter == null ? 
				TaskStore.AppendValues (task.Id, task.Name, task.Status.ToString ()) : 
				TaskStore.AppendValues (iter.Value, task.Id, task.Name, task.Status.ToString ());
		}

		/// <summary>
		/// The collecion of <see cref="Task"/>
		/// </summary>
		public TaskCollection Tasks;

		/// <summary>
		/// The status bar.
		/// </summary>
		public Statusbar StatusBar;

		/// <summary>
		/// The name column
		/// </summary>
		public TreeViewColumn NameColumn;
		/// <summary>
		/// The Id column
		/// </summary>
		public TreeViewColumn IdColumn;
		/// <summary>
		/// The status column
		/// </summary>
		public TreeViewColumn StateColumn;
		/// <summary>
		/// The tree model
		/// </summary>
		public TreeStore TaskStore;
		/// <summary>
		/// The widged that displays <see cref="TaskStore"/>
		/// </summary>
		public TreeView TaskList;
		/// <summary>
		/// The selection of <see cref="TaskList"/>
		/// </summary>
		public TreeSelection TaskSelection;
		/// <summary>
		/// The store for all categories and its filter state
		/// </summary>
		public ListStore CatStore;
		/// <summary>
		/// The selection for the category list
		/// </summary>
		public TreeSelection CatSelector;

		#region Actions

		/// <summary>
		/// New task
		/// </summary>
		public Gtk.Action NewTaskAction;
		/// <summary>
		/// New child
		/// </summary>
		public Gtk.Action NewChildTask;
		/// <summary>
		/// Remove selected task
		/// </summary>
		public Gtk.Action RemoveTask;
		/// <summary>
		/// Change status to active
		/// </summary>
		public Gtk.Action StartTask;
		/// <summary>
		/// Change status to inactive
		/// </summary>
		public Gtk.Action StopTask;
		/// <summary>
		/// Marks the selected task as finished
		/// </summary>
		public Gtk.Action FinishTask;

		/// <summary>
		/// Edit selected task in the edition dialog
		/// </summary>
		public Gtk.Action EditTask;

		#endregion

		#region TaskFilter

		/// <summary>
		/// The filter visibility Only Active
		/// </summary>
		public TreeModelFilterVisibleFunc OnlyActiveFilter;
		/// <summary>
		/// The filter visibility Show only unfinished
		/// </summary>
		public TreeModelFilterVisibleFunc UnfinishedFilter;

		/// <summary>
		/// The current filter
		/// </summary>
		public TreeModelFilter CurrentFilter;

		/// <summary>
		/// The filter used to display the task list
		/// </summary>
		public TaskFilter FilterOptions;

		#endregion

		/// <summary>
		/// The current editing file, <c>null</c> if not set
		/// </summary>
		public string CurrentFile;

		/// <summary>
		/// The link from glade
		/// </summary>
		public readonly Builder Builder;
		Window MainWindow;

		/// <summary>
		/// </summary>
		public MainClass ()
		{
			Builder = new Builder ();
			Tasks = new TaskCollection ();
			TaskPropertyDialogMaker.Builder = Builder;
		}

		/// <summary>
		/// The entry point of the program, where the program control starts and ends.
		/// </summary>
		public static void Main ()
		{
			ExceptionManager.UnhandledException += Exception_aru;

			Gtk.Application.Init ();
			var app = new MainClass ();
			app.Initialize ();
			app.MainWindow.Show ();
			Gtk.Application.Run ();
		}


		static void Exception_aru (UnhandledExceptionArgs args)
		{
			Console.WriteLine (args);
			//throw new System.Exception (args.ToString ());
		}
	}
}