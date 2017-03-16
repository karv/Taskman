using System.Diagnostics;
using System.Linq;
using GLib;
using Gtk;

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
				selIter = CurrentFilter.ConvertIterToChildIter (selIter);
				var id = (int)TaskStore.GetValue (selIter, 0);
				return Tasks.GetById (id);
			}
			return null;
		}

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

		public void Initialize ()
		{
			Builder.AddFromFile ("MainWin.glade");

			MainWindow = Builder.GetObject ("MainWindow") as Window;
			MainWindow.Destroyed += app_quit;

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
			((Action)Builder.GetObject ("actSaveAs")).Activated += saveAs;
			((Action)Builder.GetObject ("actLoad")).Activated += load;
			((Action)Builder.GetObject ("actExit")).Activated += app_quit;

			OnlyActiveFilter = delegate(ITreeModel model, TreeIter iter)
			{
				var baseTask = getTask (iter);
				return baseTask.Status == TaskStatus.Active ||
				baseTask.EnumerateRecursiveSubtasks ().Any (z => z.Status == TaskStatus.Active);
			};
			UnfinishedFilter = delegate(ITreeModel model, TreeIter iter)
			{
				var baseTask = getTask (iter);
				return baseTask.Status != TaskStatus.Completed ||
				baseTask.EnumerateRecursiveSubtasks ().Any (z => z.Status != TaskStatus.Active);
			};

			((Action)Builder.GetObject ("actFilterAll")).Activated += delegate
			{
				CurrentFilter = new TreeModelFilter (TaskStore, null);
				TaskList.Model = CurrentFilter;
			};
			((Action)Builder.GetObject ("actFilterActive")).Activated += delegate
			{
				CurrentFilter = new TreeModelFilter (TaskStore, null)
				{
					VisibleFunc = OnlyActiveFilter
				};
				TaskList.Model = CurrentFilter;
			};
			((Action)Builder.GetObject ("actFilterUnfinished")).Activated += delegate
			{
				CurrentFilter = new TreeModelFilter (TaskStore, null)
				{
					VisibleFunc = UnfinishedFilter
				};
				TaskList.Model = CurrentFilter;
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

			CurrentFilter = new TreeModelFilter (TaskStore, null);
			TaskList.Model = CurrentFilter;

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

			if (resp == ResponseType.Ok)
			{
				try
				{
					Tasks = TaskCollection.Load (fileChooser.Filename);
					rebuildStore ();
					CurrentFile = fileChooser.Filename;
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
			foreach (var task in Tasks.EnumerateRoots ())
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

		void saveAs (object sender, System.EventArgs e)
		{
			var fileChooser = new  FileChooserDialog ("Guardar...", MainWindow, FileChooserAction.Save);
			fileChooser.AddButton ("_Guardar", ResponseType.Ok);
			fileChooser.AddButton ("_Cancelar", ResponseType.Cancel);
			fileChooser.DefaultResponse = ResponseType.Ok;
			fileChooser.DoOverwriteConfirmation = true;
			var resp = (ResponseType)fileChooser.Run ();

			if (resp == ResponseType.Ok)
			{
				try
				{
					CurrentFile = fileChooser.Filename;
					Tasks.Save (CurrentFile);
					StatusBar.Push (0, "Guardado");
				}
				catch (System.Exception ex)
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
			catch (System.Exception ex)
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

		public TreeModelFilter CurrentFilter;

		#endregion

		public string CurrentFile;
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