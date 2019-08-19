using Smod2;
using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using Smod2.EventSystem.Events;
using System.Collections.Generic;
using System.Linq;

namespace CISpy
{
	partial class EventHandler : IEventHandlerWaitingForPlayers, IEventHandlerRoundStart, IEventHandlerRoundEnd, IEventHandlerTeamRespawn, IEventHandlerSetRole,
		IEventHandlerPlayerDie, IEventHandlerPlayerHurt, IEventHandlerLateDisconnect
	{
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
				MEC.Timing.RunCoroutine(Plugin.MakeSpy(ev.PlayerList[Plugin.rand.Next(ev.PlayerList.Count)]), MEC.Segment.Update);
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
				if (Plugin.SpyDict.ContainsKey(ev.Killer.SteamId)) ev.Player.PersonalBroadcast(7, "You were killed by a <b><color=\"green\">CISpy</color></b>!", false);
				MEC.Timing.RunCoroutine(CheckReveal(ev.Player));
			}
		}

		public void OnPlayerHurt(PlayerHurtEvent ev)
		{
			if (Plugin.isEnabled && ev.DamageType != DamageType.POCKET)
			{
				if (ev.Player.SteamId == ev.Attacker.SteamId || !isRoundStarted) return;
				if ((ev.Attacker.TeamRole.Team == Smod2.API.Team.CHAOS_INSURGENCY ||
					ev.Attacker.TeamRole.Team == Smod2.API.Team.CLASSD) &&
					Plugin.SpyDict.ContainsKey(ev.Player.SteamId))
				{
					ev.Damage = 0;
					ev.Attacker.PersonalClearBroadcasts();
					ev.Attacker.PersonalBroadcast(5, "You are shooting a <b><color=\"green\">CISpy</color></b>!", false);
					return;
				}

				if (Plugin.SpyDict.ContainsKey(ev.Attacker.SteamId) &&
					(ev.Player.TeamRole.Team == Smod2.API.Team.CLASSD ||
					ev.Player.TeamRole.Team == Smod2.API.Team.CHAOS_INSURGENCY))
				{
					ev.Attacker.PersonalClearBroadcasts();
					ev.Attacker.PersonalBroadcast(5, "A <b><color=\"green\">CISpy</color></b> can't shoot <b><color=\"green\">CI</color></b> or <b><color=\"orange\">Class-Ds</color></b>!", false);
					ev.Damage = 0;
					return;
				}

				if (Plugin.SpyDict.ContainsKey(ev.Attacker.SteamId) && Plugin.SpyDict.ContainsKey(ev.Player.SteamId))
				{
					ev.Damage = 0;
					ev.Attacker.PersonalClearBroadcasts();
					ev.Attacker.PersonalBroadcast(5, "You are shooting another <b><color=\"green\">CISpy</color></b>!", false);
					return;
				}
				
				if (Plugin.SpyDict.ContainsKey(ev.Attacker.SteamId) && !Plugin.SpyDict.ContainsKey(ev.Player.SteamId)
					&& (ev.Player.TeamRole.Team == Smod2.API.Team.NINETAILFOX || ev.Player.TeamRole.Team == Smod2.API.Team.SCIENTIST))
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
					return;
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

		public void OnLateDisconnect(LateDisconnectEvent ev)
		{
			// In case a player disconnects
			CheckReveal();
		}
	}
}
