using Gtk;

namespace Taskman.Gui
{
	public partial class MainWin : Gtk.Window
	{
		public MainWin () :
			base (Gtk.WindowType.Toplevel)
		{
			Build ();
			deleteAction.Activated += deleteTask;
		}

		void 
		void deleteTask (object sender, System.EventArgs e)
		{
			System.Console.WriteLine ();
		}

		protected override void OnDestroyed ()
		{
			base.OnDestroyed ();
			Application.Quit ();
		}
	}
}