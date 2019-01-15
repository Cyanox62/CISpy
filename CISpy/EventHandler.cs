using Smod2;
using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using Smod2.EventSystem.Events;
using System.Collections.Generic;
using System.Linq;
using scp4aiur;

namespace CISpy
{
	partial class EventHandler : IEventHandlerWaitingForPlayers, IEventHandlerRoundStart, IEventHandlerTeamRespawn, IEventHandlerSetRole,
		IEventHandlerPlayerDie, IEventHandlerPlayerHurt
	{
		public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
		{
			LoadConfig();
		}

		public void OnRoundStart(RoundStartEvent ev)
		{
			if (Plugin.isEnabled)
			{
				Plugin.SpyDict.Clear();
				if (Plugin.rand.Next(1, 101) <= Plugin.guardChance)
				{
					List<Player> Guards = new List<Player>();
					foreach (Player player in PluginManager.Manager.Server.GetPlayers().Where(x => x.TeamRole.Role == Role.FACILITY_GUARD))
						Guards.Add(player);

					if (Guards.Count > 0)
						Plugin.MakeSpy(Guards[Plugin.rand.Next(Guards.Count)], 15);
				}
			}
		}

		public void OnTeamRespawn(TeamRespawnEvent ev)
		{
			if (Plugin.isEnabled && !ev.SpawnChaos)
			{
				Timing.Next(() =>
				{
					Plugin.MakeSpy(ev.PlayerList[Plugin.rand.Next(ev.PlayerList.Count)]);
				});
			}
		}

		public void OnSetRole(PlayerSetRoleEvent ev)
		{
			if (Plugin.isEnabled)
			{
				if (ev.Player.TeamRole.Role.Equals(Role.UNASSIGNED)) return;
				if (Plugin.SpyDict.ContainsKey(ev.Player.SteamId) && ev.Player.TeamRole.Team != Smod2.API.Team.NINETAILFOX && ev.Player.TeamRole.Team != Smod2.API.Team.CHAOS_INSURGENCY)
					Plugin.SpyDict.Remove(ev.Player.SteamId);
			}
		}

		public void OnPlayerDie(PlayerDeathEvent ev)
		{
			if (Plugin.isEnabled)
			{
				if (ev.Player.TeamRole.Role.Equals(Role.UNASSIGNED)) return;
				if (Plugin.SpyDict.ContainsKey(ev.Player.SteamId))
					Plugin.SpyDict.Remove(ev.Player.SteamId);

				int MTFAliveCount = CountRoles(Smod2.API.Team.NINETAILFOX);
				bool CiAlive = CountRoles(Smod2.API.Team.CHAOS_INSURGENCY) > 0;
				bool ScpAlive = CountRoles(Smod2.API.Team.SCP) > 0;
				bool DClassAlive = CountRoles(Smod2.API.Team.CLASSD) > 0;
				bool ScientistsAlive = CountRoles(Smod2.API.Team.SCIENTIST) > 0;
				foreach (Player player in PluginManager.Manager.Server.GetPlayers().Where(x => x.TeamRole.Team == Smod2.API.Team.NINETAILFOX && Plugin.SpyDict.ContainsKey(x.SteamId))) MTFAliveCount--;
				bool MTFAlive = MTFAliveCount > 0;

				if ((CiAlive || (CiAlive && ScpAlive) || (CiAlive && DClassAlive)) && !ScientistsAlive && !MTFAlive)
					Plugin.RevealSpies();
				if ((ScpAlive || DClassAlive) && !ScientistsAlive && !MTFAlive)
					Plugin.RevealSpies();
				if ((ScientistsAlive || MTFAlive || (ScientistsAlive && MTFAlive)) && !CiAlive && !ScpAlive && !DClassAlive)
					Plugin.RevealSpies();
			}
		}

		public void OnPlayerHurt(PlayerHurtEvent ev)
		{
			if (Plugin.isEnabled && ev.DamageType != DamageType.POCKET)
			{
				if (ev.Player.SteamId == ev.Attacker.SteamId) return;
				if ((ev.Attacker.TeamRole.Team == Smod2.API.Team.CHAOS_INSURGENCY ||
					ev.Attacker.TeamRole.Team == Smod2.API.Team.CLASSD) &&
					Plugin.SpyDict.ContainsKey(ev.Player.SteamId))
				{
					ev.Damage = 0;
				}

				if (Plugin.SpyDict.ContainsKey(ev.Attacker.SteamId) &&
					(ev.Player.TeamRole.Team == Smod2.API.Team.CLASSD ||
					ev.Player.TeamRole.Team == Smod2.API.Team.CHAOS_INSURGENCY))
				{
					ev.Damage = 0;
				}
				
				if (Plugin.SpyDict.ContainsKey(ev.Attacker.SteamId) && (ev.Player.TeamRole.Team == Smod2.API.Team.NINETAILFOX || ev.Player.TeamRole.Team == Smod2.API.Team.SCIENTIST))
				{
					if (!Plugin.SpyDict[ev.Attacker.SteamId])
					{
						Plugin.SpyDict[ev.Attacker.SteamId] = true;
						ev.Player.PersonalBroadcast(10, $"You have attacked a {(ev.Player.TeamRole.Team == Smod2.API.Team.NINETAILFOX ? "<color=\"blue\">Nine Tailed Fox" : "<color=\"yellow\">Scientist")}</color>, you are now able to be killed by <color=\"blue\">Nine Tailed Fox and <color=\"yellow\">Scientists.", false);
						ev.Player.SetHealth(ev.Player.GetHealth() - (int)ev.Damage);
					}
				}

				if (Plugin.SpyDict.ContainsKey(ev.Player.SteamId) && ev.Attacker.TeamRole.Team == Smod2.API.Team.NINETAILFOX)
				{
					if (Plugin.SpyDict[ev.Player.SteamId])
					{
						ev.Player.SetHealth(ev.Player.GetHealth() - (int)ev.Damage);
					}
				}
			}
		}
	}
}
