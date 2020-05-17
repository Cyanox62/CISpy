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
		internal static Dictionary<ReferenceHub, bool> spies = new Dictionary<ReferenceHub, bool> ();
		private List<ReferenceHub> ffPlayers = new List<ReferenceHub>();

		private bool isDisplayFriendly = false;
		private bool haltRound = false;
		//private bool isDisplaySpy = false;

		private Random rand = new Random();

		public void OnWaitingForPlayers()
		{
			Configs.ReloadConfigs();
		}

		public void OnRoundStart()
		{
			haltRound = false;
			spies.Clear();
			ffPlayers.Clear();
			if (rand.Next(1, 101) <= Configs.guardSpawnChance)
			{
				ReferenceHub player = Player.GetHubs().FirstOrDefault(x => x.GetRole() == RoleType.FacilityGuard);
				if (player != null)
				{
					MakeSpy(player);
				}
			}
		}

		public void OnTeamRespawn(ref TeamRespawnEvent ev)
		{
			if (!ev.IsChaos && rand.Next(1, 101) <= Configs.spawnChance && ev.ToRespawn.Count >= Configs.minimumSquadSize)
			{
				List<ReferenceHub> respawn = new List<ReferenceHub>(ev.ToRespawn);
				Timing.CallDelayed(0.1f, () =>
				{
					Log.Warn(Configs.spyRoles.Count.ToString());
					List<ReferenceHub> roleList = respawn.Where(x => Configs.spyRoles.Contains((int)x.GetRole())).ToList();
					if (roleList.Count > 0)
					{
						ReferenceHub player = roleList[rand.Next(roleList.Count)];
						if (player != null)
						{
							MakeSpy(player);
						}
					}
				});
			}
		}

		public void OnSetClass(SetClassEvent ev)
		{
			if (spies.ContainsKey(ev.Player))
			{
				Timing.CallDelayed(0.1f, () => spies.Remove(ev.Player));
			}
		}

		public void OnCheckRoundEnd(ref CheckRoundEndEvent ev)
		{
			ev.Allow = !haltRound;
		}

		public void OnPlayerDie(ref PlayerDeathEvent ev)
		{
			if (spies.ContainsKey(ev.Player))
			{
				spies.Remove(ev.Player);
			}

			//ReferenceHub player = ev.Player;
			ReferenceHub scp035 = null;

			try
			{
				scp035 = TryGet035();
			}
			catch (Exception x)
			{
				Log.Debug("SCP-035 not installed, skipping method call...");
			}

			/*int playerid = ev.Player.queryProcessor.PlayerId;
			List<Team> pList = Player.GetHubs().Where(x => x.queryProcessor.PlayerId != playerid && !spies.ContainsKey(x) && x.queryProcessor.PlayerId != scp035?.queryProcessor.PlayerId).Select(x => x.GetTeam()).ToList();

			if ((!pList.Contains(Team.CHI) && !pList.Contains(Team.CDP)) ||
			((pList.Contains(Team.CDP) || pList.Contains(Team.CHI)) && !pList.Contains(Team.MTF) && !pList.Contains(Team.RSC)))
			{
				RevealSpies();
			}

			if ((pList.Contains(Team.CHI) || (pList.Contains(Team.CHI) && pList.Contains(Team.SCP)) || (pList.Contains(Team.CHI) && pList.Contains(Team.CDP)) && !pList.Contains(Team.RSC) && !pList.Contains(Team.MTF))
				RevealSpies();
			if ((pList.Contains(Team.SCP) || pList.Contains(Team.CDP)) && !pList.Contains(Team.RSC) && !pList.Contains(Team.MTF))
				RevealSpies();
			if ((pList.Contains(Team.RSC) || pList.Contains(Team.MTF) || (pList.Contains(Team.RSC) && pList.Contains(Team.MTF))) && !pList.Contains(Team.CHI) && !pList.Contains(Team.SCP) && !pList.Contains(Team.CDP))
				RevealSpies();*/
			List<ReferenceHub> pList = Player.GetHubs().Where(x => x.queryProcessor.PlayerId != scp035?.queryProcessor.PlayerId).ToList();
			haltRound = true;
			Timing.CallDelayed(0.1f, () =>
			{
				int MTFAliveCount = CountRoles(Team.MTF, pList);
				bool CiAlive = CountRoles(Team.CHI, pList) > 0;
				bool ScpAlive = CountRoles(Team.SCP, pList) > 0 + (scp035 != null ? 1 : 0);
				bool DClassAlive = CountRoles(Team.CDP, pList) > 0;
				bool ScientistsAlive = CountRoles(Team.RSC, pList) > 0;
				foreach (ReferenceHub player in pList.Where(x => x.GetTeam() == Team.MTF && spies.ContainsKey(x))) MTFAliveCount--;
				bool MTFAlive = MTFAliveCount > 0;

				if ((CiAlive || (CiAlive && ScpAlive) || (CiAlive && DClassAlive)) && !ScientistsAlive && !MTFAlive)
					RevealSpies();
				if ((ScpAlive || DClassAlive) && !ScientistsAlive && !MTFAlive)
					RevealSpies();
				if ((ScientistsAlive || MTFAlive || (ScientistsAlive && MTFAlive)) && !CiAlive && !ScpAlive && !DClassAlive)
					RevealSpies();
				haltRound = false;
			});
		}

		public void OnPlayerHurt(ref PlayerHurtEvent ev)
		{
			if (ffPlayers.Contains(ev.Attacker))
			{
				RemoveFF(ev.Attacker);
			}

			ReferenceHub scp035 = null;

			try
			{
				scp035 = TryGet035();
			} 
			catch (Exception x)
			{
				Log.Debug("SCP-035 not installed, skipping method call...");
			}

			if (spies.ContainsKey(ev.Player) && !spies.ContainsKey(ev.Attacker) && ev.Player.queryProcessor.PlayerId != ev.Attacker.queryProcessor.PlayerId && (ev.Attacker.GetTeam() == Team.CHI || ev.Attacker.GetTeam() == Team.CDP))
			{
				if (!isDisplayFriendly)
				{
					ev.Attacker.Broadcast(3, "You are shooting a <b><color=\"green\">CISpy!</color></b>", false);
					isDisplayFriendly = true;
				}
				Timing.CallDelayed(3f, () =>
				{
					isDisplayFriendly = false;
				});
				ev.Amount = 0;
			}
			else if (!spies.ContainsKey(ev.Player) && spies.ContainsKey(ev.Attacker) && (ev.Player.GetTeam() == Team.CHI || ev.Player.GetTeam() == Team.CDP) && ev.Player.queryProcessor.PlayerId != scp035?.queryProcessor.PlayerId)
			{
				ev.Amount = 0;
			}
			/*else if (spies.ContainsKey(ev.Attacker) && spies.ContainsKey(ev.Player))
			{
				if (!isDisplaySpy)
				{
					ev.Attacker.Broadcast(3, "You are shooting another <b><color=\"green\">CISpy!</color></b>", false);
					isDisplaySpy = true;
				}
				Timing.CallDelayed(3f, () =>
				{
					isDisplaySpy = false;
				});
				ev.Amount = 0;
			}*/ 
		}

		public void OnShoot(ref ShootEvent ev)
		{
			if (ev.Target == null) return;
			ReferenceHub target = Player.GetPlayer(ev.Target);
			if (target == null) return;

			ReferenceHub scp035 = null;

			try
			{
				scp035 = TryGet035();
			}
			catch (Exception x)
			{
				Log.Debug("SCP-035 not installed, skipping method call...");
			}

			if (spies.ContainsKey(ev.Shooter) && !spies.ContainsKey(target) && (Player.GetTeam(target) == Team.RSC || Player.GetTeam(target) == Team.MTF) && target.queryProcessor.PlayerId != scp035?.queryProcessor.PlayerId)
			{
				if (!spies[ev.Shooter])
				{
					spies[ev.Shooter] = true;
					ev.Shooter.Broadcast(10, $"You have attacked a {(target.GetTeam() == Team.MTF ? "<color=#00b0fc>Nine Tailed Fox" : "<color=#fcff8d>Scientist")}</color>, you are now able to be killed by <color=#00b0fc>Nine Tailed Fox</color> and <color=#fcff8d>Scientists</color>.", false);
				}
				GrantFF(ev.Shooter);
			}
			else if (spies.ContainsKey(target) && !spies.ContainsKey(ev.Shooter) && (ev.Shooter.GetTeam() == Team.MTF || ev.Shooter.GetTeam() == Team.RSC))
			{
				if (spies[target])
				{
					GrantFF(ev.Shooter);
				}
			}
		}
	}
}
