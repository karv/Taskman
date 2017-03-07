using System;
namespace Taskman
{
	public partial class DefaultActionGroup : Gtk.ActionGroup
	{
		public DefaultActionGroup() :
				base("Taskman.DefaultActionGroup")
		{
			this.Build();
		}
	}
}
