using Gtk;

namespace Taskman.Gui
{
	public class MainClass
	{
		public static void Main ()
		{
			GLib.ExceptionManager.UnhandledException += Exception_aru;

			Application.Init ();
			var app = new MainClass ();
			app.Initialize ();
			app.MainWindow.Show ();
			Application.Run ();
		}

		public void Initialize ()
		{
			var builder = new Builder ();
			builder.AddFromFile ("MainWin.glade");

			MainWindow = builder.GetObject ("MainWindow") as Window;
			//var gxml = new Glade.XML ("MainWin.glade", "", "");
			//gxml.Autoconnect (this);
		}

		Window MainWindow;

		static void Exception_aru (GLib.UnhandledExceptionArgs args)
		{
			//args.ExitApplication = false;
			System.Console.WriteLine (args);
		}
	}
}