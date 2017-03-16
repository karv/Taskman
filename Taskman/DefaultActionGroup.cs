using System;

namespace Taskman.Gui
{
	public partial class DefaultActionGroup : Gtk.ActionGroup
	{
		public DefaultActionGroup () :
			base ("Taskman.Gui.DefaultActionGroup")
		{
			Build ();
		}
	}
}