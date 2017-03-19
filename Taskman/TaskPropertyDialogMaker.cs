using System;
using Gtk;

namespace Taskman.Gui
{
	public class TaskPropertyDialogMaker
	{
		public readonly Builder Builder;
		public readonly Task Task;

		public Dialog Dialog { get; private set; }

		public void BuildWindow ()
		{
			if (Dialog != null)
				throw new Exception ();
			
			Dialog = (Dialog)Builder.GetObject ("TaskPropertyDialog");
			Dialog.Title = string.Format ("Editando {0}", Task.Name);
			((Entry)Builder.GetObject ("EntryNombre")).Text = Task.Name;
			((Entry)Builder.GetObject ("EntryDescrip")).Text = Task.Descript;

			Dialog.Response += dialog_Response;
		}

		void dialog_Response (object o, ResponseArgs args)
		{
			if (args.ResponseId == ResponseType.Ok)
			{
				Task.Name = ((Entry)Builder.GetObject ("EntryNombre")).Text;
				Task.Descript = ((Entry)Builder.GetObject ("EntryDescrip")).Text;
			}
			Dialog.Hide ();
			Dialog.Response -= dialog_Response;
			AfterResponse?.Invoke ();
			Dialog = null;
		}

		public System.Action AfterResponse;

		public static Dialog GetDialog (Builder builder, Task task)
		{
			var maker = new TaskPropertyDialogMaker (builder, task);
			maker.BuildWindow ();
			return maker.Dialog;
		}

		public TaskPropertyDialogMaker (Builder builder, Task task)
		{
			if (task == null)
				throw new ArgumentNullException ("task");
			if (builder == null)
				throw new ArgumentNullException ("builder");

			Builder = builder;
			Task = task;
		}
	}
}