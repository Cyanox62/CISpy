using System;
using Smod2;
using Smod2.Commands;
using Smod2.API;

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
			return "(SC / SPYCUP) (PLAYER)";
		}

		public string[] OnCall(ICommandSender sender, string[] args)
		{
			if (!Plugin.isEnabled) return new string[] { "Plugin is not enabled." };

			if (args.Length > 0)
			{
				Player player = Plugin.GetPlayer(args[0], out player);
				if (player == null) return new string[] { "Couldn't find player: " + args[0] };
				if (Plugin.RoleDict.ContainsKey(player.SteamId)) return new string[] { player.Name + " is already a spy." };

				Plugin.MakeSpy(player);

				PluginManager.Manager.Logger.Info(Plugin.instance.Details.id, player.Name + " has become a spy.");
				return new string[] { player.Name + " has been made a spy." };
			}
			else
			{
				return new string[] { GetUsage() };
			}
		}
	}
}
