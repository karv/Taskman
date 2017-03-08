using Gtk;

namespace Taskman.Gui
{
	public partial class MainWin : Window
	{
		public TaskCollection Collection;
		public TreeStore Data;
		public TreeViewColumn NameColumn;

		void initializeTaskList ()
		{
			TaskList.Model = Data;
			NameColumn = new TreeViewColumn ("Nombre", new CellRendererText (), "text", 0);
			TaskList.AppendColumn (NameColumn);
		}

		void deleteTask (object sender, System.EventArgs e)
		{
			System.Console.WriteLine ();
		}

		protected override void OnDestroyed ()
		{
			base.OnDestroyed ();
			Application.Quit ();
		}

		protected TaskEntry AddTask ()
		{
			var task = Task.Create (Collection);
			var entry = new TaskEntry (task);
			task.Name = "Tarea " + task.Id;
			Data.AddNode (entry);
			return entry;
		}

		public MainWin () :
			base (WindowType.Toplevel)
		{
			Collection = new TaskCollection ();
			Data = new TreeStore (typeof(Task));
			Build ();
			deleteAction.Activated += deleteTask;
			newAction.Activated += newFromRoot;

			initializeTaskList ();
		}

		TaskEntry addTask ()
		{
			var task = Task.Create (Collection);
			var entry = new TaskEntry (task);
			var rootNode = Data.AppendNode ();
			task.Name = "Tarea " + task.Id;
			Data.AppendValues (entry);
			return entry;
		}

		void newFromRoot (object sender, System.EventArgs e)
		{
			var entry = AddTask ();
			System.Console.WriteLine (entry.Name);
			TaskList.QueueDraw ();
		}
	}
}