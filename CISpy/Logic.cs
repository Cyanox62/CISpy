using EXILED.Extensions;
using MEC;
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
			spies.Add(player);
			player.Broadcast(10, "<color=#d0d0d0><size=60>You are a <b><color=\"green\">CISpy</color></b></size>\nCheck your console by pressing [`] or [~] for more info.</color>", false);
			player.characterClassManager.TargetConsolePrint(player.scp079PlayerScript.connectionToClient, "You are a Chaos Insurgency Spy! You are immune to MTF for now, but as soon as you damage an MTF, your spy immunity will turn off.\n\nHelp Chaos win the round and kill as many MTF and Scientists as you can.", "yellow");
		}

		private void RevealSpies()
		{
			foreach (ReferenceHub spy in spies)
			{
				var items = spy.inventory.items;
				Vector3 pos = spy.transform.position;
				Quaternion rot = spy.transform.rotation;
				int health = (int)spy.playerStats.health;
				string ammo = spy.ammoBox.Networkamount;

				spy.characterClassManager.SetClassID(RoleType.ChaosInsurgency);

				Timing.CallDelayed(0.1f, () =>
				{
					spy.SetPosition(pos);
					spy.SetRotation(rot.x, rot.y);
					spy.playerStats.health = health;
					spy.ammoBox.Networkamount = ammo;
					foreach (var item in items)
					{
						spy.inventory.AddNewItem(item.id);
					}
				});

				spy.Broadcast(10, "<color=#d0d0d0>Your fellow <color=\"green\">Chaos Insurgency</color> have died, you have been revealed!</color>", false);
			}
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
