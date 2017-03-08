using Gtk;

namespace Taskman.Gui
{
	class MainClass
	{
		public static void Main ()
		{
			GLib.ExceptionManager.UnhandledException += Exception_aru;
			Application.Init ();
			var win = new MainWin ();
			win.Show ();
			Application.Run ();
		}

		static void Exception_aru (GLib.UnhandledExceptionArgs args)
		{
			System.Console.WriteLine (args);
		}
	}
}