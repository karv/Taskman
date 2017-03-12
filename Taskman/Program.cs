using Gtk;
using GLib;

namespace Taskman.Gui
{
	public class MainClass
	{
		public static void Main ()
		{
			ExceptionManager.UnhandledException += Exception_aru;

			Gtk.Application.Init ();
			var app = new MainClass ();
			app.Initialize ();
			app.MainWindow.Show ();
			Gtk.Application.Run ();
		}

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

			var idCellRendererText = new CellRendererText ();
			var nameCellRendererText = new CellRendererText ();

			IdColumn.PackStart (idCellRendererText, true);
			NameColumn.PackStart (nameCellRendererText, true);

			IdColumn.AddAttribute (idCellRendererText, "text", 0);
			NameColumn.AddAttribute (nameCellRendererText, "text", 1);


			TaskList.AppendColumn (IdColumn);
			TaskList.AppendColumn (NameColumn);
			//IdColumn = (TreeViewColumn)Builder.GetObject ("IdColumn");
			//new TreeViewColumn ("Nombre", new CellRendererText (), "text", 0);
			//TaskList.AppendColumn (NameColumn);

			NewTaskAction = Builder.GetObject ("actNewTask") as Action;
			NewChildTask = Builder.GetObject ("actNewChild") as Action;
			RemoveTask = Builder.GetObject ("actRemove") as Action;
			StartTask = Builder.GetObject ("actStart") as Action;
			StopTask = Builder.GetObject ("actStop") as Action;
			FinishTask = Builder.GetObject ("actFinish") as Action;

			NewTaskAction.Activated += newTask;

			update (this, null);
			TaskSelection.Changed += update;
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
			var task = Task.Create (Tasks);
			task.Name = "Nueva tarea";
			TaskStore.AppendValues (task.Id, task.Name);
		}

		public readonly TaskCollection Tasks;
		public TreeViewColumn NameColumn;
		public TreeViewColumn IdColumn;
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

		static void Exception_aru (UnhandledExceptionArgs args)
		{
			//args.ExitApplication = false;
			System.Console.WriteLine (args);
		}
	}

}