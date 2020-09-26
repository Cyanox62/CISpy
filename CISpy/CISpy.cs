using Exiled.API.Features;
using Exiled.Loader;

namespace CISpy
{
	public class CISpy : Plugin<Config>
	{
		internal static CISpy instance;
		private EventHandlers ev;

		internal static bool isScp035 = false;

		public override void OnEnabled() 
		{
			base.OnEnabled();

			if (!Config.IsEnabled) return;
			instance = this;
			Check035();
			ev = new EventHandlers();

			Exiled.Events.Handlers.Server.RoundStarted += ev.OnRoundStart;
			Exiled.Events.Handlers.Server.RespawningTeam += ev.OnTeamRespawn;
			Exiled.Events.Handlers.Player.ChangingRole += ev.OnSetClass;
			Exiled.Events.Handlers.Player.Died += ev.OnPlayerDie;
			Exiled.Events.Handlers.Player.Hurting += ev.OnPlayerHurt;
			Exiled.Events.Handlers.Player.Shooting += ev.OnShoot;
			Exiled.Events.Handlers.Player.Left += ev.OnPlayerLeave;
			Exiled.Events.Handlers.Player.Handcuffing += ev.OnHandcuffing;
		}

		public override void OnDisabled() 
		{
			base.OnDisabled();

			Exiled.Events.Handlers.Server.RoundStarted -= ev.OnRoundStart;
			Exiled.Events.Handlers.Server.RespawningTeam -= ev.OnTeamRespawn;
			Exiled.Events.Handlers.Player.ChangingRole -= ev.OnSetClass;
			Exiled.Events.Handlers.Player.Died -= ev.OnPlayerDie;
			Exiled.Events.Handlers.Player.Hurting -= ev.OnPlayerHurt;
			Exiled.Events.Handlers.Player.Shooting -= ev.OnShoot;
			Exiled.Events.Handlers.Player.Left -= ev.OnPlayerLeave;
			Exiled.Events.Handlers.Player.Handcuffing -= ev.OnHandcuffing;

			ev = null;
		}

		public override string Name => "CiSpy";
		public override string Author => "Cyanox";

		internal void Check035()
		{
			foreach (var plugin in Loader.Plugins)
			{
				if (plugin.Name == "scp035")
				{
					isScp035 = true;
					return;
				}
			}
		}
	}
}
