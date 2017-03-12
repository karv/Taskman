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


		public void Initialize ()
		{
			GType.Register (GType.Int, typeof (int));
			Builder.AddFromFile ("MainWin.glade");

			MainWindow = Builder.GetObject ("MainWindow") as Window;
			MainWindow.Destroyed += MainWindow_Destroyed;
			TaskStore = Builder.GetObject ("TaskStore") as TreeStore;
			TaskList = (TreeView)Builder.GetObject ("TaskView");

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

			var actNewTask = Builder.GetObject ("actNewTask") as Action;
			actNewTask.Activated += newTask;
		}

		void MainWindow_Destroyed (object sender, System.EventArgs e)
		{
			Gtk.Application.Quit ();
		}

		void newTask (object sender, System.EventArgs e)
		{
			var task = Task.Create (Tasks);
			System.Console.WriteLine (task);
			task.Name = "Nueva tarea";
			TaskStore.AppendValues (task.Id, task.Name);
		}

		public readonly TaskCollection Tasks;
		public TreeViewColumn NameColumn;
		public TreeViewColumn IdColumn;
		public TreeStore TaskStore;
		public TreeView TaskList;

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