using EXILED;
using EXILED.Extensions;
using MEC;
using scp035.API;
using System.Collections.Generic;
using UnityEngine;

namespace CISpy
{
	partial class EventHandlers
	{
		private void MakeSpy(ReferenceHub player)
		{
			if (!Configs.spawnWithGrenade)
			{
				for (int i = player.inventory.items.Count - 1; i >= 0; i--)
				{
					if (player.inventory.items[i].id == ItemType.GrenadeFrag)
					{
						player.inventory.items.RemoveAt(i);
					}
				}
			}
			spies.Add(player, false);
			player.Broadcast(10, "<size=60>You are a <b><color=\"green\">CISpy</color></b></size>\nCheck your console by pressing [`] or [~] for more info.", false);
			player.characterClassManager.TargetConsolePrint(player.scp079PlayerScript.connectionToClient, "You are a Chaos Insurgency Spy! You are immune to MTF for now, but as soon as you damage an MTF, your spy immunity will turn off.\n\nHelp Chaos win the round and kill as many MTF and Scientists as you can.", "yellow");
		}

		private ReferenceHub TryGet035()
		{
			return Scp035Data.GetScp035();
		}

		private void RevealSpies()
		{
			foreach (KeyValuePair<ReferenceHub, bool> spy in spies)
			{
				Inventory.SyncListItemInfo items = new Inventory.SyncListItemInfo();
				foreach (var item in spy.Key.inventory.items) items.Add(item);
				Vector3 pos = spy.Key.transform.position;
				Quaternion rot = spy.Key.transform.rotation;
				int health = (int)spy.Key.playerStats.health;
				string ammo = spy.Key.ammoBox.Networkamount;

				spy.Key.SetRole(RoleType.ChaosInsurgency);

				Timing.CallDelayed(0.3f, () =>
				{
					spy.Key.plyMovementSync.OverridePosition(pos, 0f);
					spy.Key.SetRotation(rot.x, rot.y);
					spy.Key.inventory.items.Clear();
					foreach (var item in items) spy.Key.inventory.AddNewItem(item.id);
					spy.Key.playerStats.health = health;
					spy.Key.ammoBox.Networkamount = ammo;
				});

				spy.Key.Broadcast(10, "Your fellow <color=\"green\">Chaos Insurgency</color> have died.\nYou have been revealed!", false);
			}
			spies.Clear();
		}

		private void GrantFF(ReferenceHub player)
		{
			player.weaponManager.NetworkfriendlyFire = true;
			ffPlayers.Add(player);
		}

		private void RemoveFF(ReferenceHub player)
		{
			player.weaponManager.NetworkfriendlyFire = false;
			ffPlayers.Remove(player);
		}
	}
}
