using Smod2;
using Smod2.API;
using Smod2.Attributes;
using System;
using System.Linq;
using System.Collections.Generic;
using Smod2.Lang;

namespace CISpy
{
	[PluginDetails(
	author = "Cyanox",
	name = "CISpy",
	description = "A plugin for SCP:SL",
	id = "cyan.cispy",
	version = "1.0.0",
	SmodMajor = 3,
	SmodMinor = 0,
	SmodRevision = 0
	)]
	public class Plugin : Smod2.Plugin
    {
		public static Plugin instance;

		//Configs
		public static bool isEnabled = false;
		public static int guardChance = 50;
		public static bool canSpawnWithGrenade = true;
		public static int spawnChance = 40;
		public static int minSizeForSpy = 6;
		public static List<Role> MTFRoles = new List<Role>();

		public static Dictionary<string, bool> SpyDict = new Dictionary<string, bool>();

		public static Random rand = new Random();

		[LangOption]
		public static readonly string broadcast = "<color=#d0d0d0>You are a <b><color=\"green\">CISpy</color></b>! Check your console by pressing [`] or [~] for more info.</color>";
		[LangOption]
		public static readonly string consoleMessage = "You are a Chaos Insurgency Spy! You are immune to MTF for now, but as soon as you damage an MTF," +
				" your spy immunity will turn off.\n\n" +
				"Help Chaos win the round and kill as many MTF and Scientists as you can.";
		[LangOption]
		public static readonly string lastCI = "<color=#d0d0d0>Your fellow <color=\"green\">Chaos Insurgency</color> have died, you have been revealed!</color>";
		[LangOption]
		public readonly string killedBySpy = "You were killed by a <b><color=\"green\">CISpy</color></b>!";
		[LangOption]
		public readonly string shootingSpy = "You are shooting a <b><color=\"green\">CISpy</color></b>!";
		[LangOption]
		public readonly string cantShoot = "A <b><color=\"green\">CISpy</color></b> can't shoot <b><color=\"green\">CI</color></b> or <b><color=\"orange\">Class-Ds</color></b>!";
		[LangOption]
		public readonly string anotherCiSpy = "You are shooting another <b><color=\"green\">CISpy</color></b>!";
		[LangOption]
		public readonly string ableToBeKilled = "<color=#d0d0d0>You have attacked a {ROLE}, you are now able to be killed by <color=#00b0fc>Nine Tailed Fox</color> and <color=#fcff8d>Scientists</color>.</color>";
		[LangOption]
		public readonly string ntf = "<color=#00b0fc>Nine Tailed Fox</color>";
		[LangOption]
		public readonly string scientist = "<color=#fcff8d>Scientist</color>";
		public override void OnEnable() { }

		public override void OnDisable() { }

		public override void Register()
		{
			instance = this;

			AddEventHandlers(new EventHandler(), Smod2.Events.Priority.High);

			AddCommands(new string[] { "cis", "cispy" }, new CommandHandler());

			AddConfig(new Smod2.Config.ConfigSetting("cis_enabled", true, false, true, "Enables CiSpy."));
			AddConfig(new Smod2.Config.ConfigSetting("cis_guard_chance", 50, false, true, "The chance for a facility guard to spawn as a spy at the start of the round."));
			AddConfig(new Smod2.Config.ConfigSetting("cis_spy_roles", new[] 
			{
				11,
				13
			}, false, true, "Which roles can be a spy."));
			AddConfig(new Smod2.Config.ConfigSetting("cis_spawn_with_grenade", true, false, true, "If spies should be able to spawn with frag grenades"));
			AddConfig(new Smod2.Config.ConfigSetting("cis_spawn_chance", 40, false, true, "The chance for a CISpy to spawn in an MTF wave."));
			AddConfig(new Smod2.Config.ConfigSetting("cis_minimum_size", 6, false, true, "The minimum squad size there must be for a spy to have a chance at spawning."));
		}

		public static int LevenshteinDistance(string s, string t)
		{
			int n = s.Length;
			int m = t.Length;
			int[,] d = new int[n + 1, m + 1];

			if (n == 0)
			{
				return m;
			}

			if (m == 0)
			{
				return n;
			}

			for (int i = 0; i <= n; d[i, 0] = i++)
			{
			}

			for (int j = 0; j <= m; d[0, j] = j++)
			{
			}

			for (int i = 1; i <= n; i++)
			{
				for (int j = 1; j <= m; j++)
				{
					int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

					d[i, j] = Math.Min(
						Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
						d[i - 1, j - 1] + cost);
				}
			}
			return d[n, m];
		}

		public static Player GetPlayer(string args, out Player playerOut)
		{
			int maxNameLength = 31, LastnameDifference = 31;
			Player plyer = null;
			string str1 = args.ToLower();
			foreach (Player pl in PluginManager.Manager.Server.GetPlayers(str1))
			{
				if (!pl.Name.ToLower().Contains(args.ToLower())) continue; //{ goto NoPlayer; }
				if (str1.Length < maxNameLength)
				{
					int x = maxNameLength - str1.Length;
					int y = maxNameLength - pl.Name.Length;
					string str2 = pl.Name;
					for (int i = 0; i < x; i++)
					{
						str1 += "z";
					}
					for (int i = 0; i < y; i++)
					{
						str2 += "z";
					}
					int nameDifference = LevenshteinDistance(str1, str2);
					if (nameDifference < LastnameDifference)
					{
						LastnameDifference = nameDifference;
						plyer = pl;
					}
				}
				//NoPlayer:;
			}
			playerOut = plyer;
			return playerOut;
		}

		public static IEnumerator<float> MakeSpy(Player player, int role = -1)
		{
			yield return MEC.Timing.WaitForOneFrame;
			Role MTFRole = (role == -1) ? MTFRoles[rand.Next(MTFRoles.Count)] : (Role)role;
			player.ChangeRole(MTFRole);
			foreach (Smod2.API.Item item in player.GetInventory())
			{
				if (!canSpawnWithGrenade && item.ItemType == ItemType.FRAG_GRENADE)
				{
					item.Remove();
					player.GiveItem(ItemType.FLASHBANG);
				}
			}
			SpyDict.Add(player.SteamId, false);
			player.PersonalBroadcast(10, broadcast, false);
			player.SendConsoleMessage(consoleMessage);
		}


		public static void ChangeSpyRole(Player player)
		{
			List<Smod2.API.Item> inventory = player.GetInventory();
			Vector pos = player.GetPosition();
			UnityEngine.GameObject pObj = (UnityEngine.GameObject)player.GetGameObject();
			float rot = pObj.GetComponent<PlyMovementSync>().rotation;

			int health = player.GetHealth();
			int ammo5 = player.GetAmmo(AmmoType.DROPPED_5);
			int ammo7 = player.GetAmmo(AmmoType.DROPPED_7);
			int ammo9 = player.GetAmmo(AmmoType.DROPPED_9);

			if (player.TeamRole.Team.Equals(Smod2.API.Team.NINETAILFOX))
				player.ChangeRole(Smod2.API.Role.CHAOS_INSURGENCY);

			foreach (Smod2.API.Item item in player.GetInventory()) { item.Remove(); }
			foreach (Smod2.API.Item item in inventory) player.GiveItem(item.ItemType);

			player.SetAmmo(AmmoType.DROPPED_5, ammo5);
			player.SetAmmo(AmmoType.DROPPED_7, ammo7);
			player.SetAmmo(AmmoType.DROPPED_9, ammo9);

			player.Teleport(pos, false);
			player.SetHealth(health);

			MEC.Timing.RunCoroutine(RotatePlayer(pObj, rot), MEC.Segment.Update);
		}

		private static IEnumerator<float> RotatePlayer(UnityEngine.GameObject pObj, float rot)
		{
			yield return MEC.Timing.WaitForSeconds(0.1f);
			pObj.GetComponent<PlyMovementSync>().SetRotation(rot - pObj.GetComponent<PlyMovementSync>().rotation);
		}

		public static Player FindPlayer(string steamid)
		{
			foreach (Player player in PluginManager.Manager.Server.GetPlayers())
				if (player.SteamId == steamid)
					return player;
			return null;
		}

		public static void RevealSpies()
		{
			foreach (Player player in SpyDict.Select(x => FindPlayer(x.Key)).Where(x => x != null && x.TeamRole.Team != Smod2.API.Team.CHAOS_INSURGENCY))
			{
				ChangeSpyRole(player);
				player.PersonalBroadcast(10, lastCI, false);
			}
		}
	}
}
