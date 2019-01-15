using Smod2.Commands;
using Smod2.API;
using Smod2;
using System.Collections.Generic;

namespace CISpy
{
	class CommandHandler : ICommandHandler
	{
		public string GetCommandDescription()
		{
			return "Turns a player into a spy.";
		}

		public string GetUsage()
		{
			return "(CIS / CISPY) (SPAWN / REVEAL)";
		}

		public string[] OnCall(ICommandSender sender, string[] args)
		{
			if (!Plugin.isEnabled) return new string[] { "Plugin is not enabled." };

			if (args.Length > 0)
			{
				switch (args[0].ToLower())
				{
					case "spawn":
						{
							if (args.Length > 1)
							{
								Player player = Plugin.GetPlayer(args[1], out player);
								if (player == null) return new string[] { $"Couldn't find player: { args[1] }" };
								if (Plugin.SpyDict.ContainsKey(player.SteamId)) return new string[] { player.Name + " is already a spy." };

								Plugin.MakeSpy(player);
								return new string[] { $"{ player.Name } has been made a spy." };
							}
							else
							{
								List<Player> PlayerList = PluginManager.Manager.Server.GetPlayers();
								Player player = PlayerList[Plugin.rand.Next(PlayerList.Count)];
								Plugin.MakeSpy(player);
								return new string[] { $"No player specified, selecting random player...\n{ player.Name } has been made a spy." };
							}
						}
					case "reveal":
						{
							Plugin.RevealSpies();
							return new string[] { "Spies have been revealed." };
						} 
				}
			}
			return new string[] { GetUsage() };
		}
	}
}
