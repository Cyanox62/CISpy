using EXILED;

namespace CISpy
{
	public class Plugin : EXILED.Plugin
	{
		private EventHandlers ev;

		public override void OnEnable() 
		{
			if (!Config.GetBool("cis_enabled", true)) return;

			ev = new EventHandlers();

			Events.WaitingForPlayersEvent += ev.OnWaitingForPlayers;
			Events.RoundStartEvent += ev.OnRoundStart;
			Events.TeamRespawnEvent += ev.OnTeamRespawn;
			Events.SetClassEvent += ev.OnSetClass;
			Events.PlayerDeathEvent += ev.OnPlayerDie;
			Events.PlayerHurtEvent += ev.OnPlayerHurt;
			Events.ShootEvent += ev.OnShoot;
		}

		public override void OnDisable() 
		{
			Events.WaitingForPlayersEvent -= ev.OnWaitingForPlayers;
			Events.RoundStartEvent -= ev.OnRoundStart;
			Events.TeamRespawnEvent -= ev.OnTeamRespawn;
			Events.SetClassEvent -= ev.OnSetClass;
			Events.PlayerDeathEvent -= ev.OnPlayerDie;
			Events.PlayerHurtEvent -= ev.OnPlayerHurt;
			Events.ShootEvent -= ev.OnShoot;

			ev = null;
		}

		public override void OnReload() { }

		public override string getName { get; } = "CISpy";
	}
}
