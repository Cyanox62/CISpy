using EXILED;
using EXILED.Extensions;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CISpy
{
	partial class EventHandlers
	{
		private List<ReferenceHub> spies = new List<ReferenceHub>();
		private List<ReferenceHub> ffPlayers = new List<ReferenceHub>();

		private bool isRoundStarted = false;

		private Random rand = new Random();

		public void OnWaitingForPlayers()
		{
			Configs.ReloadConfigs();
		}

		public void OnRoundStart()
		{
			isRoundStarted = true;
			spies.Clear();
			if (rand.Next(1, 101) <= Configs.guardSpawnChance)
			{
				MakeSpy(Player.GetHubs().FirstOrDefault(x => x.GetRole() == RoleType.FacilityGuard));
			}
		}

		public void OnRoundEnd()
		{
			isRoundStarted = false;
		}

		public void OnTeamRespawn(ref TeamRespawnEvent ev)
		{
			if (!ev.IsChaos && rand.Next(1, 101) <= Configs.spawnChance && ev.ToRespawn.Count >= Configs.minimumSquadSize)
			{
				ReferenceHub player = ev.ToRespawn.Where(x => Configs.spyRoles.Contains((int)x.GetRole())).ToList()[rand.Next(ev.ToRespawn.Count)];
				if (player != null)
				{
					ev.ToRespawn.Remove(player);
					MakeSpy(player);
				}
			}
		}

		public void OnSetClass(SetClassEvent ev)
		{
			if (spies.Contains(ev.Player))
			{
				spies.Remove(ev.Player);
			}
		}

		public void OnPlayerDie(ref PlayerDeathEvent ev)
		{
			if (spies.Contains(ev.Player))
			{
				spies.Remove(ev.Player);
			}

			ReferenceHub player = ev.Player;

			Timing.CallDelayed(0.1f, () =>
			{
				List<Team> pList = Player.GetHubs().Select(x => x.GetTeam()).ToList();

				if ((player.GetTeam() == Team.CHI && !pList.Contains(Team.CHI)) || ((pList.Contains(Team.MTF) || pList.Contains(Team.RSC)) && !pList.Contains(Team.CDP) && !pList.Contains(Team.CHI) && !pList.Contains(Team.SCP) && !pList.Contains(Team.TUT)))
				{
					RevealSpies();
				}
			});
		}

		public void OnPlayerHurt(ref PlayerHurtEvent ev)
		{
			if (ffPlayers.Contains(ev.Attacker))
			{
				RemoveFF(ev.Attacker);
			}

			if ((ev.Attacker.GetTeam() == Team.CHI && spies.Contains(ev.Player)) || (ev.Player.GetTeam() == Team.CHI && spies.Contains(ev.Attacker)) ||
				(ev.Attacker.GetTeam() == Team.CDP && spies.Contains(ev.Player)) || (ev.Player.GetTeam() == Team.CDP && spies.Contains(ev.Attacker)))
			{
				ev.Amount = 0;
			}
		}

		public void OnShoot(ref ShootEvent ev)
		{
			if (ev.Target == null) return;
			ReferenceHub target = Player.GetPlayer(ev.Target);
			if (target == null) return;

			if ((spies.Contains(ev.Shooter) && Player.GetTeam(target) == Player.GetTeam(ev.Shooter)) || (spies.Contains(target) && Player.GetTeam(ev.Shooter) == Player.GetTeam(target)))
			{
				GrantFF(ev.Shooter);
			}
		}
	}
}
