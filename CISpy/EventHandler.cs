using System;
using Smod2;
using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using Smod2.EventSystem.Events;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using Object = UnityEngine.Object;
using scp4aiur;

namespace CISpy
{
	partial class EventHandler : IEventHandlerWaitingForPlayers, IEventHandlerRoundStart, IEventHandlerTeamRespawn, IEventHandlerSetRole,
		IEventHandlerPlayerDie, IEventHandlerPlayerHurt, IEventHandlerPlayerDropItem
	{
		public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
		{
			LoadConfig();
		}

		public void OnRoundStart(RoundStartEvent ev)
		{
			if (Plugin.isEnabled)
			{
				Plugin.RoleDict.Clear();

				int guardChance = Plugin.rand.Next(1, 101);
				if (guardChance <= Plugin.guardChance)
				{
					List<Player> Guards = new List<Player>();
					foreach (Player player in PluginManager.Manager.Server.GetPlayers().Where(x => x.TeamRole.Role == Role.FACILITY_GUARD))
						Guards.Add(player);

					if (Guards.Count > 0)
					{
						Player guardSpy = Guards[Plugin.rand.Next(Guards.Count)];
						Plugin.MakeSpy(guardSpy, 15);
					}
				}
			}
		}

		public void OnTeamRespawn(TeamRespawnEvent ev)
		{
			if (Plugin.isEnabled && !ev.SpawnChaos)
			{
				Timing.Next(() =>
				{
					Player player = ev.PlayerList[Plugin.rand.Next(ev.PlayerList.Count)];
					Plugin.MakeSpy(player);
				});
			}
		}

		public void OnSetRole(PlayerSetRoleEvent ev)
		{
			if (Plugin.isEnabled)
			{
				if (ev.Player.TeamRole.Role.Equals(Role.UNASSIGNED)) return;
				if (Plugin.RoleDict.ContainsKey(ev.Player.SteamId) && ev.Player.TeamRole.Team != Smod2.API.Team.NINETAILFOX && ev.Player.TeamRole.Team != Smod2.API.Team.CHAOS_INSURGENCY)
					Plugin.RoleDict.Remove(ev.Player.SteamId);
			}
		}

		public void OnPlayerDie(PlayerDeathEvent ev)
		{
			if (Plugin.isEnabled)
			{
				if (ev.Player.TeamRole.Role.Equals(Role.UNASSIGNED)) return;
				if (Plugin.RoleDict.ContainsKey(ev.Player.SteamId)) Plugin.RoleDict.Remove(ev.Player.SteamId);
			}
		}

		public void OnPlayerHurt(PlayerHurtEvent ev)
		{
			if (Plugin.isEnabled)
			{
				if (ev.Player.SteamId == ev.Attacker.SteamId) return;
				if (ev.Attacker.TeamRole.Team == Smod2.API.Team.CHAOS_INSURGENCY ||
					ev.Attacker.TeamRole.Team == Smod2.API.Team.CLASSD &&
					Plugin.RoleDict.ContainsKey(ev.Player.SteamId) &&
					ev.DamageType != DamageType.POCKET)
				{
					ev.Damage = 0;
				}

				if (Plugin.RoleDict.ContainsKey(ev.Attacker.SteamId) &&
					ev.Player.TeamRole.Team == Smod2.API.Team.CLASSD ||
					ev.Player.TeamRole.Team == Smod2.API.Team.CHAOS_INSURGENCY &&
					ev.DamageType != DamageType.POCKET)
				{
					ev.Damage = 0;
				}

			}
		}

		public void OnPlayerDropItem(PlayerDropItemEvent ev)
		{
			if (Plugin.isEnabled && ev.Item.ItemType == ItemType.CUP)
			{
				ChangeSpyRole(ev.Player);
			}
		}
	}
}
