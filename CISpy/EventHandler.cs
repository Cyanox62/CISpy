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
						MEC.Timing.RunCoroutine(Plugin.MakeSpy(Guards[Plugin.rand.Next(Guards.Count)], 15), MEC.Segment.Update);
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
					ev.Attacker.PersonalBroadcast(5, Plugin.instance.shootingSpy, false);
					return;
				}

				if (Plugin.SpyDict.ContainsKey(ev.Attacker.SteamId) &&
					(ev.Player.TeamRole.Team == Smod2.API.Team.CLASSD ||
					ev.Player.TeamRole.Team == Smod2.API.Team.CHAOS_INSURGENCY))
				{
					ev.Attacker.PersonalClearBroadcasts();
					ev.Attacker.PersonalBroadcast(5, Plugin.instance.cantShoot, false);
					ev.Damage = 0;
					return;
				}

				if (Plugin.SpyDict.ContainsKey(ev.Attacker.SteamId) && Plugin.SpyDict.ContainsKey(ev.Player.SteamId))
				{
					ev.Damage = 0;
					ev.Attacker.PersonalClearBroadcasts();
					ev.Attacker.PersonalBroadcast(5, Plugin.instance.anotherCiSpy, false);
					return;
				}
				
				if (Plugin.SpyDict.ContainsKey(ev.Attacker.SteamId) && !Plugin.SpyDict.ContainsKey(ev.Player.SteamId)
					&& (ev.Player.TeamRole.Team == Smod2.API.Team.NINETAILFOX || ev.Player.TeamRole.Team == Smod2.API.Team.SCIENTIST))
				{
					if (!Plugin.SpyDict[ev.Attacker.SteamId])
					{
						Plugin.SpyDict[ev.Attacker.SteamId] = true;
						ev.Attacker.PersonalBroadcast(10, Plugin.instance.ableToBeKilled.Replace("{ROLE}", ev.Player.TeamRole.Team == Smod2.API.Team.NINETAILFOX ? Plugin.instance.ntf : Plugin.instance.scientist), false);
					}
					int health = ev.Player.GetHealth() - (int)ev.Damage;
					ev.Player.SetHealth(health);
					if (health >= 0)
					{
						ev.Player.PersonalBroadcast(7, Plugin.instance.killedBySpy, false);
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
