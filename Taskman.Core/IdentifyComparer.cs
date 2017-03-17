using System.Collections.Generic;

namespace Taskman
{
	class IdentifyComparer : IEqualityComparer<IIdentificable>
	{
		public bool Equals (IIdentificable x, IIdentificable y)
		{
			return x.Id == y.Id;
		}

		public int GetHashCode (IIdentificable obj)
		{
			return obj.Id;
		}
	}
	
}