using Exiled.API.Enums;
using Exiled.API.Features;
using MEC;
using scp035.API;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CISpy
{
	partial class EventHandlers
	{
		internal static void MakeSpy(Player player, bool isVulnerable = false, bool full = true)
		{
			if (!CISpy.instance.Config.SpawnWithGrenade && full)
			{
				for (int i = player.Inventory.items.Count - 1; i >= 0; i--)
				{
					if (player.Inventory.items[i].id == ItemType.GrenadeFrag)
					{
						player.Inventory.items.RemoveAt(i);
					}
				}
			}
			player.Inventory.AddNewItem(ItemType.KeycardChaosInsurgency);
			spies.Add(player, isVulnerable);
			player.Broadcast(10, "<i><size=60>You are a <b><color=\"green\">CISpy</color></b></size>\nCheck your console by pressing [`] or [~] for more info.</i>");
			player.ReferenceHub.characterClassManager.TargetConsolePrint(player.ReferenceHub.scp079PlayerScript.connectionToClient, "You are a Chaos Insurgency Spy! You are immune to MTF for now, but as soon as you damage an MTF, your spy immunity will turn off.\n\nHelp Chaos win the round and kill as many MTF and Scientists as you can.", "yellow");
		}

		private Player TryGet035()
		{
			return Scp035Data.GetScp035();
		}

		private void RevealSpies()
		{
			foreach (KeyValuePair<Player, bool> spy in spies)
			{
				Inventory.SyncListItemInfo items = new Inventory.SyncListItemInfo();
				foreach (var item in spy.Key.Inventory.items) items.Add(item);
				Vector3 pos = spy.Key.Position;
				Vector3 rot = spy.Key.Rotation;
				int health = (int)spy.Key.Health;
				uint ammo1 = spy.Key.Ammo[(int)AmmoType.Nato556];
				uint ammo2 = spy.Key.Ammo[(int)AmmoType.Nato762];
				uint ammo3 = spy.Key.Ammo[(int)AmmoType.Nato9];

				spy.Key.SetRole(RoleType.ChaosInsurgency);

				Timing.CallDelayed(0.3f, () =>
				{
					spy.Key.Position = pos;
					spy.Key.Rotation = rot;
					spy.Key.Inventory.items.Clear();
					foreach (var item in items) spy.Key.Inventory.AddNewItem(item.id);
					spy.Key.Health = health;
					spy.Key.Ammo[(int)AmmoType.Nato556] = ammo1;
					spy.Key.Ammo[(int)AmmoType.Nato762] = ammo2;
					spy.Key.Ammo[(int)AmmoType.Nato9] = ammo3;
				});

				spy.Key.Broadcast(10, "<i>Your fellow <color=\"green\">Chaos Insurgency</color> have died.\nYou have been revealed!</i>");
			}
			spies.Clear();
		}

		private void GrantFF(Player player)
		{
			player.IsFriendlyFireEnabled = true;
			ffPlayers.Add(player);
		}

		private void RemoveFF(Player player)
		{
			player.IsFriendlyFireEnabled = false;
			ffPlayers.Remove(player);
		}

		private int CountRoles(Team team, List<Player> pList)
		{
			int count = 0;
			foreach (Player pl in pList) if (pl.Team == team) count++;
			return count;
		}

		private void CheckSpies(Player exclude = null)
		{
			Player scp035 = null;

			try
			{
				scp035 = TryGet035();
			}
			catch (Exception x)
			{
				Log.Debug("SCP-035 not installed, skipping method call...");
			}

			int playerid = -1;
			if (exclude != null) playerid = exclude.Id;
			List<Player> pList = Player.List.Where(x =>
			x.Id != playerid &&
			x.Id != scp035?.Id &&
			!spies.ContainsKey(x)).ToList();

			bool CiAlive = CountRoles(Team.CHI, pList) > 0;
			bool ScpAlive = CountRoles(Team.SCP, pList) > 0 + (scp035 != null ? 1 : 0);
			bool DClassAlive = CountRoles(Team.CDP, pList) > 0;
			bool ScientistsAlive = CountRoles(Team.RSC, pList) > 0;
			bool MTFAlive = CountRoles(Team.MTF, pList) > 0;

			if
			(
				((CiAlive || (CiAlive && ScpAlive) || (CiAlive && DClassAlive)) && !ScientistsAlive && !MTFAlive) ||
				((ScpAlive || DClassAlive) && !ScientistsAlive && !MTFAlive) ||
				((ScientistsAlive || MTFAlive || (ScientistsAlive && MTFAlive)) && !CiAlive && !ScpAlive && !DClassAlive)
			)
			{
				RevealSpies();
			}
		}
	}
}
