using System;
using System.Linq;
using Gtk;

namespace Taskman.Gui
{
	public static class TaskPropertyDialogMaker
	{
		public static Builder Builder;
		public static Task Task;

		public static bool AutoUpdateTask = true;

		public static Dialog Dialog { get; private set; }

		public static void BuildWindow ()
		{
			if (Dialog == null)
				throw new Exception ("TaskPropertyDialogMaker not initialized");
			
			Dialog.Title = string.Format ("Editando {0}", Task.Name);
			((Entry)Builder.GetObject ("EntryNombre")).Text = Task.Name;
			((Entry)Builder.GetObject ("EntryDescrip")).Text = Task.Descript;
			((Entry)Builder.GetObject ("EntryDuración")).Text = Task.TotalActivityTime.ToString ();

			buildCatStore ();

		}

		static void buildCatStore ()
		{
			var cats = Task.Collection.OfType<Category> ();
			var store = ((ListStore)Builder.GetObject ("CatStoreInDialog"));
			store.Clear ();

			foreach (var cat in cats)
				store.AppendValues (cat.Id, cat.Name, Task.HasCategory (cat));
		}

		static void dialog_Response (object o, ResponseArgs args)
		{
			if (AutoUpdateTask && args.ResponseId == ResponseType.Ok)
			{
				Task.Name = ((Entry)Builder.GetObject ("EntryNombre")).Text;
				Task.Descript = ((Entry)Builder.GetObject ("EntryDescrip")).Text;

				// Update tasks
				var catCol = (ListStore)Builder.GetObject ("CatStoreInDialog");
				catCol.Foreach (delegate(ITreeModel model, TreePath path, TreeIter iter)
				{
					var catId = (int)model.GetValue (iter, 0);
					var state = (bool)model.GetValue (iter, 2);
					Task.SetCategory (catId, state);
					return false;
				});

			}
			Dialog.Hide ();
			AfterResponse?.Invoke ();
		}

		public static System.Action AfterResponse;

		/*
		public TaskPropertyDialogMaker (Builder builder, Task task)
		{
			if (task == null)
				throw new ArgumentNullException ("task");
			if (builder == null)
				throw new ArgumentNullException ("builder");

			Builder = builder;
			Task = task;
		}

*/
		public static void Initialize ()
		{
			Dialog = (Dialog)Builder.GetObject ("TaskPropertyDialog");
			Dialog.Response += dialog_Response;

			((CellRendererToggle)Builder.GetObject ("TaskDialog CatList HasCatToggle")).Toggled += 
				delegate(object o, ToggledArgs args)
			{
				var store = (ListStore)Builder.GetObject ("CatStoreInDialog");
				TreeIter iter;
				store.GetIterFromString (out iter, args.Path);
				var newState = !(bool)store.GetValue (iter, 2);
				store.SetValue (iter, 2, newState);
			};


		}
	}
}