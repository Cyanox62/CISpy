using System.Collections.Generic;

namespace CISpy
{
	class Configs
	{
		internal static List<int> spyRoles;

		internal static bool spawnWithGrenade;

		internal static int spawnChance;
		internal static int guardSpawnChance;
		internal static int minimumSquadSize;

		internal static void ReloadConfigs()
		{
			spyRoles = Plugin.Config.GetIntList("cis_spy_roles");
			if (spyRoles == null || spyRoles.Count == 0)
			{
				spyRoles = new List<int>() { 11, 12, 13 };
			}

			spawnWithGrenade = Plugin.Config.GetBool("cis_spawn_with_grenade", true);

			spawnChance = Plugin.Config.GetInt("cis_spawn_chance", 40);
			guardSpawnChance = Plugin.Config.GetInt("cis_guard_chance", 50);
			minimumSquadSize = Plugin.Config.GetInt("cis_minimum_size", 6);
		}
	}
}
