using Gtk;
using GLib;

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
			GType.Register (GType.Int, typeof (int));
			Builder.AddFromFile ("MainWin.glade");

			MainWindow = Builder.GetObject ("MainWindow") as Window;
			MainWindow.Destroyed += MainWindow_Destroyed;
			TaskStore = Builder.GetObject ("TaskStore") as TreeStore;
			TaskList = (TreeView)Builder.GetObject ("TaskView");

			TaskSelection = (TreeSelection)Builder.GetObject ("TaskSelection");

			TaskStore = new TreeStore (GType.Int, GType.String);
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
			TaskList.AppendColumn (NameColumn);

			NewTaskAction = Builder.GetObject ("actNewTask") as Action;
			NewChildTask = Builder.GetObject ("actNewChild") as Action;
			RemoveTask = Builder.GetObject ("actRemove") as Action;
			StartTask = Builder.GetObject ("actStart") as Action;
			StopTask = Builder.GetObject ("actStop") as Action;
			FinishTask = Builder.GetObject ("actFinish") as Action;

			NewTaskAction.Activated += newTask;
			NewChildTask.Activated += newChild;
			RemoveTask.Activated += removeTask;

			update (this, null);
			TaskSelection.Changed += update;
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
			System.Diagnostics.Debug.WriteLine (string.Format ("renamed task to {0}", task.Name));
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
				task = Task.Create (Tasks);
				task.Name = "Nueva tarea";
				return TaskStore.AppendValues (task.Id, task.Name);
			}
			else
			{
				task = Task.Create (Tasks, (int)TaskStore.GetValue (iter.Value, (int)ColAssign.Id));
				task.Name = task.MasterTask.Name + ".Nueva tarea";
				return TaskStore.AppendValues (iter.Value, task.Id, task.Name);
			}
		}

		public readonly TaskCollection Tasks;

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

		public readonly Builder Builder;
		Window MainWindow;

		public MainClass ()
		{
			Builder = new Builder ();
			Tasks = new TaskCollection ();
		}

		public static readonly CellRenderer NameRenderer;

		static MainClass ()
		{
			NameRenderer = new CellRendererText ();
			NameRenderer = new CellRendererText
			{
				Style = Pango.Style.Normal
			};
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
			throw new System.Exception (args.ToString ());
		}
	}
}