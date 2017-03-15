using Gtk;
using GLib;
using System.Diagnostics;

namespace Taskman.Gui
{
	enum ColAssign
	{
		Id = 0,
		Name = 1,
		State = 2
	}

	public class MainClass
	{
		public Task GetSelectedTask ()
		{
			TreeIter selIter;
			if (TaskSelection.GetSelected (out selIter))
			{
				var id = (int)TaskStore.GetValue (selIter, 0);
				return Tasks.GetById (id);
			}
			return null;
		}

		public TreeIter? GetSelectedIter ()
		{
			TreeIter selIter;
			return TaskSelection.GetSelected (out selIter) ? new TreeIter? (selIter) : null;
		}

		public void Initialize ()
		{
			Builder.AddFromFile ("MainWin.glade");

			MainWindow = Builder.GetObject ("MainWindow") as Window;
			MainWindow.Destroyed += MainWindow_Destroyed;
			TaskStore = Builder.GetObject ("TaskStore") as TreeStore;
			TaskList = (TreeView)Builder.GetObject ("TaskView");

			TaskSelection = (TreeSelection)Builder.GetObject ("TaskSelection");

			TaskStore = new TreeStore (GType.Int, GType.String, GType.String);
			TaskList.Model = TaskStore;

			IdColumn = new TreeViewColumn { Title = "Id" };
			NameColumn = new TreeViewColumn { Title = "Nombre" };
			StateColumn = new TreeViewColumn { Title = "Estado" };

			var idCellRendererText = new CellRendererText ();
			var nameCellRendererText = new CellRendererText ();
			var stateCellRendererText = new CellRendererText ();

			nameCellRendererText.Editable = true;
			nameCellRendererText.Edited += nameChanged;

			IdColumn.PackStart (idCellRendererText, true);
			NameColumn.PackStart (nameCellRendererText, true);
			StateColumn.PackStart (stateCellRendererText, true);

			IdColumn.AddAttribute (idCellRendererText, "text", (int)ColAssign.Id);
			NameColumn.AddAttribute (nameCellRendererText, "text", (int)ColAssign.Name);
			StateColumn.AddAttribute (stateCellRendererText, "text", (int)ColAssign.State);

			TaskList.AppendColumn (IdColumn);
			TaskList.AppendColumn (StateColumn);
			TaskList.AppendColumn (NameColumn);

			StatusBar = Builder.GetObject ("Status bar") as Statusbar;

			NewTaskAction = Builder.GetObject ("actNewTask") as Action;
			NewChildTask = Builder.GetObject ("actNewChild") as Action;
			RemoveTask = Builder.GetObject ("actRemove") as Action;
			StartTask = Builder.GetObject ("actStart") as Action;
			StopTask = Builder.GetObject ("actStop") as Action;
			FinishTask = Builder.GetObject ("actFinish") as Action;

			((Action)Builder.GetObject ("actSave")).Activated += save;
			((Action)Builder.GetObject ("actLoad")).Activated += load;

			OnlyActiveFilter = (model, iter) => getTask (iter).Status == TaskStatus.Active;
			UnfinishedFilter = (model, iter) => getTask (iter).Status != TaskStatus.Completed;

			((Action)Builder.GetObject ("actFilterAll")).Activated += delegate
			{
				TaskList.Model = new TreeModelFilter (TaskStore, null);
			};
			((Action)Builder.GetObject ("actFilterActive")).Activated += delegate
			{
				TaskList.Model = new TreeModelFilter (TaskStore, null)
				{
					VisibleFunc = OnlyActiveFilter
				};
			};
			((Action)Builder.GetObject ("actFilterUnfinished")).Activated += delegate
			{
				TaskList.Model = new TreeModelFilter (TaskStore, null)
				{
					VisibleFunc = UnfinishedFilter
				};
			};

			NewTaskAction.Activated += newTask;
			NewChildTask.Activated += newChild;
			RemoveTask.Activated += removeTask;
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

			update (this, null);
			TaskSelection.Changed += update;
		}

		void load (object sender, System.EventArgs e)
		{
			var fileChooser = new  FileChooserDialog ("Abrir...", null, FileChooserAction.Open);
			fileChooser.AddButton ("_Abrir", ResponseType.Ok);
			fileChooser.AddButton ("_Cancelar", ResponseType.Cancel);
			fileChooser.DefaultResponse = ResponseType.Ok;
			var resp = (ResponseType)fileChooser.Run ();
			// i = -5 : Ok
			// i = -4 : esc
			// i = -6 : cancel button
			if (resp == ResponseType.Ok)
			{
				try
				{
					Tasks = TaskCollection.Load (fileChooser.Filename);
					rebuildStore ();
					StatusBar.Push (0, "Archivo cargado");
				}
				catch (System.Exception ex)
				{
					StatusBar.Push (0, "Error cargando archivo");
					Debug.WriteLine ("Something wrong.\n" + ex);
				}
			}
			fileChooser.Destroy ();
		}

		void rebuildStore ()
		{
			TaskStore.Clear ();
			foreach (var task in Tasks)
			{
				addHerTaskToStore (task);
			}
		}

		void addHerTaskToStore (Task task, TreeIter? father = null)
		{
			var iter = addTask (father, task);
			foreach (var child in task.GetSubtasks ())
				addHerTaskToStore (child, iter);
		}

		void save (object sender, System.EventArgs e)
		{
			try
			{
				Tasks.Save ("tasks");
				StatusBar.Push (0, "Guardado");
			}
			catch (System.Exception ex)
			{
				StatusBar.Push (0, "Algo salió mal al guardar archivo");
			}
		}

		void setTaskStatus (TreeIter iter, TaskStatus status)
		{
			getTask (iter).Status = status;
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
			return Tasks.GetById (id);
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
			TaskStore.GetIterFromString (out iter, args.Path);
			var id = (int)TaskStore.GetValue (iter, (int)ColAssign.Id);
			var task = Tasks.GetById (id);
			task.Name = args.NewText;
			Debug.WriteLine (string.Format ("renamed task to {0}", task.Name));
			TaskStore.SetValue (iter, (int)ColAssign.Name, task.Name);
		}

		void update (object sender, System.EventArgs e)
		{
			var selTask = GetSelectedTask ();
			foreach (var act in new [] {NewChildTask, RemoveTask, StartTask, StopTask, FinishTask})
				act.Sensitive = selTask != null;
		}

		void MainWindow_Destroyed (object sender, System.EventArgs e)
		{
			Gtk.Application.Quit ();
		}

		void newTask (object sender, System.EventArgs e)
		{
			var iter = addTask (null);
			var path = TaskStore.GetPath (iter);
			TaskList.ExpandToPath (path);
			TaskSelection.SelectIter (iter);
		}

		void newChild (object sender, System.EventArgs e)
		{
			var iter = addTask (GetSelectedIter ());
			var path = TaskStore.GetPath (iter);
			TaskList.ExpandToPath (path);
			TaskSelection.SelectIter (iter);
		}

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
				var master = Tasks.GetById ((int)TaskStore.GetValue (iter.Value, (int)ColAssign.Id));
				task = master.CreateSubtask ();

				task.Name = task.MasterTask.Name + ".Nueva tarea";
				return TaskStore.AppendValues (iter.Value, task.Id, task.Name, task.Status.ToString ());
			}
		}

		TreeIter addTask (TreeIter? iter, Task task)
		{
			if (iter == null)
			{
				return TaskStore.AppendValues (task.Id, task.Name, task.Status.ToString ());
			}
			else
			{
				var master = Tasks.GetById ((int)TaskStore.GetValue (iter.Value, (int)ColAssign.Id));
				return TaskStore.AppendValues (iter.Value, task.Id, task.Name, task.Status.ToString ());
			}
		}

		public TaskCollection Tasks;

		public Statusbar StatusBar;
		public TreeViewColumn NameColumn;
		public TreeViewColumn IdColumn;
		public TreeViewColumn StateColumn;
		public TreeStore TaskStore;
		public TreeView TaskList;
		public TreeSelection TaskSelection;

		#region Actions

		public Action NewTaskAction;
		public Action NewChildTask;
		public Action RemoveTask;
		public Action StartTask;
		public Action StopTask;
		public Action FinishTask;

		#endregion

		#region TaskFilter

		public TreeModelFilterVisibleFunc OnlyActiveFilter;
		public TreeModelFilterVisibleFunc UnfinishedFilter;

		#endregion

		public readonly Builder Builder;
		Window MainWindow;

		public MainClass ()
		{
			Builder = new Builder ();
			Tasks = new TaskCollection ();
		}

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
			//throw new System.Exception (args.ToString ());
		}
	}
}