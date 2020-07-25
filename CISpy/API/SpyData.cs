using Exiled.API.Features;
using System.Collections.Generic;

namespace CISpy.API
{
	public static class SpyData
	{
		public static Dictionary<Player, bool> GetSpies()
		{
			return EventHandlers.spies;
		}

		public static void MakeSpy(Player player, bool isVulenrable = false, bool full = true)
		{
			EventHandlers.MakeSpy(player, isVulenrable, full);
		}
	}
}
