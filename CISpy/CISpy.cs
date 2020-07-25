using Exiled.API.Features;

namespace CISpy
{
	public class CISpy : Plugin<Config>
	{
		internal static CISpy instance;
		private EventHandlers ev;

		public override void OnEnabled() 
		{
			base.OnEnabled();

			if (!Config.IsEnabled) return;
			instance = this;
			ev = new EventHandlers();

			Exiled.Events.Handlers.Server.RoundStarted += ev.OnRoundStart;
			Exiled.Events.Handlers.Server.RespawningTeam += ev.OnTeamRespawn;
			Exiled.Events.Handlers.Player.ChangingRole += ev.OnSetClass;
			Exiled.Events.Handlers.Player.Died += ev.OnPlayerDie;
			Exiled.Events.Handlers.Player.Hurting += ev.OnPlayerHurt;
			Exiled.Events.Handlers.Player.Shooting += ev.OnShoot;
			Exiled.Events.Handlers.Player.Left += ev.OnPlayerLeave;
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

			ev = null;
		}

		public override string Name => "CiSpy";
	}
}
