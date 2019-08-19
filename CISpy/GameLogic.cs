using Smod2;
using Smod2.API;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
		public static IEnumerator<float> CheckReveal(Player player = null)
		{
			yield return MEC.Timing.WaitForOneFrame;
			yield return MEC.Timing.WaitForOneFrame;
			yield return MEC.Timing.WaitForOneFrame;
			yield return MEC.Timing.WaitForOneFrame;
			if (player != null)
			{
				if (player.TeamRole.Role.Equals(Role.UNASSIGNED)) yield break;
				if (Plugin.SpyDict.ContainsKey(player.SteamId))
					Plugin.SpyDict.Remove(player.SteamId);
			}
			else yield return MEC.Timing.WaitForSeconds(0.4f);
			int MTFAliveCount = CountRoles(Smod2.API.Team.NINETAILFOX);
			bool CiAlive = CountRoles(Smod2.API.Team.CHAOS_INSURGENCY) > 0;
			bool ScpAlive = CountRoles(Smod2.API.Team.SCP) > 0;
			bool DClassAlive = CountRoles(Smod2.API.Team.CLASSD) > 0;
			bool ScientistsAlive = CountRoles(Smod2.API.Team.SCIENTIST) > 0;
			foreach (Player ply in PluginManager.Manager.Server.GetPlayers().Where(x => x.TeamRole.Team == Smod2.API.Team.NINETAILFOX && Plugin.SpyDict.ContainsKey(x.SteamId))) MTFAliveCount--;
			bool MTFAlive = MTFAliveCount > 0;

			if (CiAlive && !ScientistsAlive && !MTFAlive)
				Plugin.RevealSpies();
			if ((ScpAlive || DClassAlive) && !ScientistsAlive && !MTFAlive)
				Plugin.RevealSpies();
			if ((ScientistsAlive || MTFAlive) && !CiAlive && !ScpAlive && !DClassAlive)
				Plugin.RevealSpies();
		}
	}
}
