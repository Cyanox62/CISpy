using Exiled.API.Features;
using Exiled.Loader;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CISpy
{
	using Exiled.API.Enums;
	using System.Reflection;

	partial class EventHandlers
	{
		internal static void MakeSpy(Player player, bool isVulnerable = false, bool full = true)
		{
			try
			{
				if (!CISpy.instance.Config.SpawnWithGrenade && full)
				{
					for (int i = player.Items.Count - 1; i >= 0; i--)
					{
						if (player.Items.ElementAt(i).Type == ItemType.GrenadeHE)
						{
							player.RemoveItem(player.Items.ElementAt(i));
						}
					}
				}
				player.AddItem(ItemType.KeycardChaosInsurgency);
				spies.Add(player, isVulnerable);
				player.Broadcast(10, "<i><size=60>You are a <b><color=\"green\">CISpy</color></b></size>\nCheck your console by pressing [`] or [~] for more info.</i>");
				player.ReferenceHub.characterClassManager.TargetConsolePrint(player.ReferenceHub.scp079PlayerScript.connectionToClient, "You are a Chaos Insurgency Spy! You are immune to MTF for now, but as soon as you damage an MTF, your spy immunity will turn off.\n\nHelp Chaos win the round and kill as many MTF and Scientists as you can.", "yellow");
			} catch(Exception e)
			{
				Log.Error(e);
			}
		}

		private Player TryGet035()
		{
			Player scp035 = null;
			if (CISpy.isScp035)
				scp035 = (Player)Loader.Plugins.First(pl => pl.Name == "scp035").Assembly.GetType("scp035.API.Scp035Data").GetMethod("GetScp035", BindingFlags.Public | BindingFlags.Static).Invoke(null, null);
			return scp035;
		}

		private void RevealSpies()
		{
			foreach (KeyValuePair<Player, bool> spy in spies)
			{
				int health = (int)spy.Key.Health;
				Dictionary<global::ItemType, ushort> ammo = new Dictionary<global::ItemType, ushort>();
				foreach(global::ItemType ammoType in spy.Key.Ammo.Keys)
				{
					ammo.Add(ammoType, spy.Key.Ammo[ammoType]);
				}

				spy.Key.SetRole(RoleType.ChaosConscript, SpawnReason.ForceClass, true);

				Timing.CallDelayed(0.5f, () =>
				{
					spy.Key.Health = health;
					foreach (global::ItemType ammoType in ammo.Keys)
					{
						spy.Key.Ammo[ammoType] = ammo[ammoType];
					}
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
