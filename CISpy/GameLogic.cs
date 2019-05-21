using Smod2;
using Smod2.API;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using scp4aiur;

namespace CISpy
{
	partial class EventHandler
	{
		public void LoadConfig()
		{
			Plugin.isEnabled = Plugin.instance.GetConfigBool("cis_enabled");
			Plugin.guardChance = Plugin.instance.GetConfigInt("cis_guard_chance");
			Plugin.MTFRoles = Plugin.instance.GetConfigIntList("cis_spy_roles").Select(x => (Role)x).ToList();
			Plugin.canSpawnWithGrenade = Plugin.instance.GetConfigBool("cis_spawn_with_grenade");
			Plugin.spawnChance = Plugin.instance.GetConfigInt("cis_spawn_chance");
			Plugin.minSizeForSpy = Plugin.instance.GetConfigInt("cis_minimum_size");
		}

		public static int CountRoles(Role role)
		{
			int count = 0;
			foreach (Player pl in PluginManager.Manager.Server.GetPlayers())
				if (pl.TeamRole.Role == role)
					count++;
			return count;
		}

		public static int CountRoles(Smod2.API.Team team)
		{
			int count = 0;
			foreach (Player pl in PluginManager.Manager.Server.GetPlayers())
				if (pl.TeamRole.Team == team)
					count++;
			return count;
		}
	}
}
