using scp4aiur;
using Smod2;
using Smod2.API;
using Smod2.Attributes;
using System;
using System.Collections.Generic;

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
		public static Smod2.Plugin instance;

		//Configs
		public static bool isEnabled = false;
		public static float disguiseCooldown = 10f;
		public static int guardChance = 50;
		public static List<Role> MTFRoles = new List<Role>();


		public static Dictionary<string, Role> RoleDict = new Dictionary<string, Role>();

		public static Random rand = new Random();

		public override void OnEnable() { }

		public override void OnDisable() { }

		public override void Register()
		{
			instance = this;

			Timing.Init(this);

			AddEventHandlers(new EventHandler());

			AddCommands(new string[] { "sc", "spycup" }, new CommandHandler());

			AddConfig(new Smod2.Config.ConfigSetting("cis_enabled", true, Smod2.Config.SettingType.BOOL, true, "Enables CiSpy."));
			AddConfig(new Smod2.Config.ConfigSetting("cis_cooldown", 10f, Smod2.Config.SettingType.FLOAT, true, "Determines the cooldown from switching classes."));
			AddConfig(new Smod2.Config.ConfigSetting("cis_guard_chance", 50, Smod2.Config.SettingType.NUMERIC, true, "The chance for a facility guard to spawn as a spy at the start of the round."));
			AddConfig(new Smod2.Config.ConfigSetting("cis_spy_roles", new[] 
			{
				11,
				13
			}, Smod2.Config.SettingType.NUMERIC_LIST, true, "Which roles can be a spy."));
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
				if (!pl.Name.ToLower().Contains(args.ToLower())) { goto NoPlayer; }
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
				NoPlayer:;
			}
			playerOut = plyer;
			return playerOut;
		}

		public static void MakeSpy(Player player, int role = -1)
		{
			Role MTFRole = (role == -1) ? MTFRoles[rand.Next(MTFRoles.Count)] : (Role)role;
			player.ChangeRole(MTFRole);
			RoleDict.Add(player.SteamId, MTFRole);
			player.GiveItem(ItemType.CUP);
		}
	}
}
