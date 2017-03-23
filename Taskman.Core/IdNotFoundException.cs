using System;

namespace Taskman
{
	/// <summary>
	/// Identifier not found exception.
	/// </summary>
	[Serializable]
	public class IdNotFoundException : Exception
	{
		/// <summary>
		/// The <see cref="TaskCollection"/>
		/// </summary>
		public readonly TaskCollection Collection;
		/// <summary>
		/// The not-found id
		/// </summary>
		public readonly int Id;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:IdNotFoundException"/> class
		/// </summary>
		public IdNotFoundException (
			TaskCollection collection, 
			int id, 
			string message = "", 
			Exception inner = null)
			: base (message,
			        inner)
		{
			Collection = collection;
			Id = id;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:IdNotFoundException"/> class
		/// </summary>
		/// <param name="context">The contextual information about the source or destination.</param>
		/// <param name="info">The object that holds the serialized object data.</param>
		protected IdNotFoundException (System.Runtime.Serialization.SerializationInfo info,
		                               System.Runtime.Serialization.StreamingContext context)
			: base (info,
			        context)
		{
			Collection = null;
			Id = 0;
		}
	}
}