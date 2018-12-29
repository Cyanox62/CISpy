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
			Plugin.disguiseCooldown = Plugin.instance.GetConfigFloat("cis_cooldown");
			Plugin.guardChance = Plugin.instance.GetConfigInt("cis_guard_chance");
			Plugin.MTFRoles = Plugin.instance.GetConfigIntList("cis_spy_roles").Select(x => (Role)x).ToList();
		}

		public void ReturnCup(Player player)
		{
			List<Smod2.API.Item> inv = player.GetInventory();
			if (inv.Count > 7 && !player.HasItem(ItemType.CUP))
			{
				inv.Remove(inv[7]);
				PluginManager.Manager.Server.Map.SpawnItem(inv[7].ItemType, player.GetPosition(), Vector.Zero);
			}
			player.GiveItem(ItemType.CUP);
		}

		public void ChangeSpyRole(Player player)
		{
			List<Smod2.API.Item> inventory = player.GetInventory();
			Vector pos = player.GetPosition();
			GameObject pObj = (GameObject)player.GetGameObject();
			float rot = pObj.GetComponent<PlyMovementSync>().rotation;

			int health = player.GetHealth();
			int ammo5 = player.GetAmmo(AmmoType.DROPPED_5);
			int ammo7 = player.GetAmmo(AmmoType.DROPPED_7);
			int ammo9 = player.GetAmmo(AmmoType.DROPPED_9);

			if (player.TeamRole.Team.Equals(Smod2.API.Team.NINETAILFOX))
				player.ChangeRole(Smod2.API.Role.CHAOS_INSURGENCY);
			else if (player.TeamRole.Team.Equals(Smod2.API.Team.CHAOS_INSURGENCY))
				player.ChangeRole(Plugin.RoleDict[player.SteamId]);

			foreach (Smod2.API.Item item in player.GetInventory()) { item.Remove(); }
			foreach (Smod2.API.Item item in inventory) { player.GiveItem(item.ItemType); }

			player.SetAmmo(AmmoType.DROPPED_5, ammo5);
			player.SetAmmo(AmmoType.DROPPED_7, ammo7);
			player.SetAmmo(AmmoType.DROPPED_9, ammo9);

			player.Teleport(pos, false);
			player.SetHealth(health);

			Timing.In(x =>
			{
				ReturnCup(player);
			}, Plugin.disguiseCooldown);

			Timing.In(x =>
			{
				pObj.GetComponent<PlyMovementSync>().SetRotation(rot - pObj.GetComponent<PlyMovementSync>().rotation);
			}, 0.1f);
		}
	}
}
