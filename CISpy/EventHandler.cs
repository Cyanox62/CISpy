using Smod2;
using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using Smod2.EventSystem.Events;
using System.Collections.Generic;
using System.Linq;
using scp4aiur;
using scp035.API;

namespace CISpy
{
	partial class EventHandler : IEventHandlerWaitingForPlayers, IEventHandlerRoundStart, IEventHandlerRoundEnd, IEventHandlerTeamRespawn, IEventHandlerSetRole,
		IEventHandlerPlayerDie, IEventHandlerPlayerHurt
	{
		bool isDisplayFriendly = false;
		bool isDisplaySpy = false;
		bool isRoundStarted = false;

		public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
		{
			LoadConfig();
		}

		public void OnRoundStart(RoundStartEvent ev)
		{
			if (Plugin.isEnabled)
			{
				isRoundStarted = true;
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

		public void OnRoundEnd(RoundEndEvent ev)
		{
			isRoundStarted = false;
		}

		public void OnTeamRespawn(TeamRespawnEvent ev)
		{
			if (Plugin.isEnabled && !ev.SpawnChaos && Plugin.rand.Next(1, 101) <= Plugin.spawnChance && ev.PlayerList.Count >= Plugin.minSizeForSpy)
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
				Timing.InTicks(() =>
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
				}, 4);
			}
		}

		public void OnPlayerHurt(PlayerHurtEvent ev)
		{
			if (Plugin.isEnabled && ev.DamageType != DamageType.POCKET)
			{
				if (ev.Player.SteamId == ev.Attacker.SteamId || !isRoundStarted) return;

				// SCP-035
				Player scp035 = Scp035Data.GetScp035();
				if (ev.Attacker.TeamRole.Team == ev.Player.TeamRole.Team &&
					((Plugin.SpyDict.ContainsKey(ev.Attacker.SteamId) && ev.Player.PlayerId == scp035?.PlayerId) ||
					(Plugin.SpyDict.ContainsKey(ev.Player.SteamId) && ev.Attacker.PlayerId == scp035?.PlayerId)))
				{
					ev.Player.SetHealth(ev.Player.GetHealth() - (int)ev.Damage);
					return;
				}
				// END SCP-035

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

				if (Plugin.SpyDict.ContainsKey(ev.Attacker.SteamId) && Plugin.SpyDict.ContainsKey(ev.Player.SteamId))
				{
					ev.Damage = 0;
					if (!isDisplaySpy)
					{
						ev.Attacker.PersonalBroadcast(5, "You are shooting another <b><color=\"green\">CISpy</color></b>!", false);
						isDisplaySpy = true;
					}
					Timing.In(x =>
					{
						isDisplaySpy = false;
					}, 5);
				}
				
				if (Plugin.SpyDict.ContainsKey(ev.Attacker.SteamId) && !Plugin.SpyDict.ContainsKey(ev.Player.SteamId) && (ev.Player.TeamRole.Team == Smod2.API.Team.NINETAILFOX || ev.Player.TeamRole.Team == Smod2.API.Team.SCIENTIST))
				{
					if (!Plugin.SpyDict[ev.Attacker.SteamId])
					{
						Plugin.SpyDict[ev.Attacker.SteamId] = true;
						ev.Attacker.PersonalBroadcast(10, $"<color=#d0d0d0>You have attacked a {(ev.Player.TeamRole.Team == Smod2.API.Team.NINETAILFOX ? "<color=#00b0fc>Nine Tailed Fox" : "<color=#fcff8d>Scientist")}</color>, you are now able to be killed by <color=#00b0fc>Nine Tailed Fox</color> and <color=#fcff8d>Scientists</color>.</color>", false);
						ev.Player.SetHealth(ev.Player.GetHealth() - (int)ev.Damage);
					}
					else
					{
						ev.Player.SetHealth(ev.Player.GetHealth() - (int)ev.Damage);
					}
				}

				if (Plugin.SpyDict.ContainsKey(ev.Player.SteamId) && ev.Attacker.TeamRole.Team == Smod2.API.Team.CHAOS_INSURGENCY)
				{
					ev.Damage = 0;
					if (!isDisplayFriendly)
					{
						ev.Attacker.PersonalBroadcast(5, "You are shooting a <b><color=\"green\">CISpy</color></b>!", false);
						isDisplayFriendly = true;
					}
					Timing.In(x =>
					{
						isDisplayFriendly = false;
					}, 5);
				}

				if (Plugin.SpyDict.ContainsKey(ev.Player.SteamId) && (ev.Attacker.TeamRole.Team == Smod2.API.Team.NINETAILFOX || ev.Attacker.TeamRole.Team == Smod2.API.Team.SCIENTIST))
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
