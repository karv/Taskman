using Glade;
using Gtk;

namespace Taskman.Gui
{
	public class MainClass
	{
		public class GtkObjects
		{
			public readonly Window MainWindow;

			public GtkObjects ()
			{
				var gxml = new XML ("MainWin.glade", "", "");
				gxml.Autoconnect (this);

				var winfo = new WidgetInfo ();
				//var w2 = gxml.BuildWidget (winfo);
				var w = gxml.GetWidget ("MainWindow");
				MainWindow = w as Window;
			}
		}

		public static void Main ()
		{
			GLib.ExceptionManager.UnhandledException += Exception_aru;

			var GtkController = new GtkObjects ();


			Application.Init ();

			GtkController.MainWindow.Show ();

			Application.Run ();
		}

		static void Exception_aru (GLib.UnhandledExceptionArgs args)
		{
			System.Console.WriteLine (args);
		}
	}
}