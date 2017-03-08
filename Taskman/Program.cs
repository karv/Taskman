using Gtk;

namespace Taskman.Gui
{
	class MainClass
	{
		public static void Main ()
		{
			Application.Init ();
			var win = new MainWin ();
			win.Show ();
			Application.Run ();
		}
	}
}