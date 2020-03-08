using System.Collections.Generic;

namespace CISpy.API
{
	public static class SpyData
	{
		public static Dictionary<ReferenceHub, bool> GetSpies()
		{
			return EventHandlers.spies;
		}

		public static void MakeSpy(ReferenceHub player, bool isVulenrable = false, bool full = true)
		{
			EventHandlers.MakeSpy(player, isVulenrable, full);
		}
	}
}
