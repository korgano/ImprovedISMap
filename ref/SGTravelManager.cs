using System;
using System.Collections;
using BattleTech.Save.SaveGameStructure;
using BattleTech.UI;
using HBS;
using HBS.FSM;
using Localize;
using UnityEngine;
using UnityEngine.Events;

namespace BattleTech
{
	// Token: 0x02001089 RID: 4233
	public class SGTravelManager
	{
		// Token: 0x170018EC RID: 6380
		// (get) Token: 0x06008E26 RID: 36390 RVA: 0x002419CB File Offset: 0x0023FBCB
		public SimGameTravelStatus TravelState
		{
			get
			{
				return (SimGameTravelStatus)this.travelState.currentState.enumValue;
			}
		}

		// Token: 0x170018ED RID: 6381
		// (get) Token: 0x06008E27 RID: 36391 RVA: 0x002419E2 File Offset: 0x0023FBE2
		public bool InTransition
		{
			get
			{
				return this.TravelState == SimGameTravelStatus.TRANSITION_ANIMATING;
			}
		}

		// Token: 0x06008E28 RID: 36392 RVA: 0x002419ED File Offset: 0x0023FBED
		public SGTravelManager()
		{
			this.travelState = new GenericMachine<SimGameTravelStatus>(0);
		}

		// Token: 0x06008E29 RID: 36393 RVA: 0x00241A0F File Offset: 0x0023FC0F
		public SGTravelManager(SimGameTravelStatus startState)
		{
			this.travelState = new GenericMachine<SimGameTravelStatus>((int)startState);
		}

		// Token: 0x06008E2A RID: 36394 RVA: 0x00241A34 File Offset: 0x0023FC34
		public void InitializeStateMachine(SimGameState simGameState)
		{
			this.simState = simGameState;
			this.travelState.AssignCanEnter(1, new Func<bool>(this.AtJumpPoint_CanEnter));
			this.travelState.AssignOnEnter(1, new Action(this.AtJumpPoint_OnEnter));
			this.travelState.AssignOnExit(1, new Action(this.AtJumpPoint_OnExit));
			this.travelState.AssignCanEnter(0, new Func<bool>(this.InSystem_CanEnter));
			this.travelState.AssignOnEnter(0, new Action(this.InSystem_OnEnter));
			this.travelState.AssignOnExit(0, new Action(this.InSystem_OnExit));
			this.travelState.AssignCanEnter(5, new Func<bool>(this.TransitionAnimating_CanEnter));
			this.travelState.AssignOnEnter(5, new Action(this.TransitionAnimating_OnEnter));
			this.travelState.AssignOnExit(5, new Action(this.TransitionAnimating_OnExit));
			this.travelState.AssignCanEnter(4, new Func<bool>(this.TransitFromJump_CanEnter));
			this.travelState.AssignOnEnter(4, new Action(this.TransitFromJump_OnEnter));
			this.travelState.AssignOnExit(4, new Action(this.TransitFromJump_OnExit));
			this.travelState.AssignCanEnter(3, new Func<bool>(this.TransitToJump_CanEnter));
			this.travelState.AssignOnEnter(3, new Action(this.TransitToJump_OnEnter));
			this.travelState.AssignOnExit(3, new Action(this.TransitToJump_OnExit));
			this.travelState.AssignCanEnter(6, new Func<bool>(this.Unknown_CanEnter));
			this.travelState.AssignOnEnter(6, new Action(this.Unknown_OnEnter));
			this.travelState.AssignOnExit(6, new Action(this.Unknown_OnExit));
			this.travelState.AssignCanEnter(2, new Func<bool>(this.WarmingEngines_CanEnter));
			this.travelState.AssignOnEnter(2, new Action(this.WarmingEngines_OnEnter));
			this.travelState.AssignOnExit(2, new Action(this.WarmingEngines_OnExit));
		}

		// Token: 0x06008E2B RID: 36395 RVA: 0x00241C40 File Offset: 0x0023FE40
		public void ResetTempData()
		{
			this.PreTransitionState = SimGameTravelStatus.UNKNOWN;
			this.PostTransitionState = SimGameTravelStatus.UNKNOWN;
		}

		// Token: 0x06008E2C RID: 36396 RVA: 0x00241C50 File Offset: 0x0023FE50
		public bool AnimationInterrupt()
		{
			if (this.TravelState == SimGameTravelStatus.TRANSITION_ANIMATING)
			{
				if (this.currentTransitioningAnim != SimGameShipAnimation.JUMPSHIP_CHARGE_AND_JUMP && (this.currentTransitioningAnim != SimGameShipAnimation.JUMPSHIP_CHARGE_AND_JUMP_THEN_LEAVE || this.jumpshipChargeAndLeaveAnimCounter > 0))
				{
					this.simState.CameraController.spaceController.SkipAnimation();
				}
				return true;
			}
			return false;
		}

		// Token: 0x06008E2D RID: 36397 RVA: 0x00241C90 File Offset: 0x0023FE90
		public bool OnDayPassed()
		{
			if (this.simState.Starmap.ActivePath == null || this.simState.Starmap.TravelItem == null)
			{
				return false;
			}
			int num = this.simState.TravelTime;
			num--;
			this.simState.Starmap.PayTravelOrderCost(1);
			this.simState.Starmap.UpdateTravelItem();
			if (num > 0)
			{
				this.simState.SetTravelTime(num, null);
				return false;
			}
			this.HandleNextTravelStep();
			return this.PostTransitionState == SimGameTravelStatus.IN_SYSTEM;
		}

		// Token: 0x06008E2E RID: 36398 RVA: 0x00241D1C File Offset: 0x0023FF1C
		public bool CheckEnterRoomPendingTravelState()
		{
			if (this.travelStatePending != SimGameTravelStatus.UNKNOWN)
			{
				this.SetTravelState(this.travelStatePending, false);
				this.travelStatePending = SimGameTravelStatus.UNKNOWN;
				return true;
			}
			return false;
		}

		// Token: 0x06008E2F RID: 36399 RVA: 0x00241D40 File Offset: 0x0023FF40
		public void HandleNextTravelStep()
		{
			int num = this.simState.TravelTime;
			int count = this.simState.Starmap.ActivePath.Count;
			bool flag = this.simState.Starmap.GetDestinationSystem() == this.simState.Starmap.GetNextSystemInTravel();
			this.jumpshipChargeAndLeaveAnimCounter = 0;
			bool flag2 = this.pauseAtTravelSteps;
			SimGameTravelStatus simGameTravelStatus = this.TravelState;
			SimGameTravelStatus simGameTravelStatus2 = SimGameTravelStatus.UNKNOWN;
			switch (simGameTravelStatus)
			{
			case SimGameTravelStatus.AT_JUMP_POINT:
			case SimGameTravelStatus.TRANSIT_TO_JUMP:
				num = this.simState.Starmap.GetCurrentTravelIndexCost();
				simGameTravelStatus2 = SimGameTravelStatus.WARMING_ENGINES;
				break;
			case SimGameTravelStatus.WARMING_ENGINES:
				this.simState.Starmap.IncrementTravelIndexAndUpdateCurrentSystem();
				if (flag)
				{
					simGameTravelStatus2 = SimGameTravelStatus.TRANSIT_FROM_JUMP;
					num = this.simState.CurSystem.JumpDistance;
				}
				else
				{
					simGameTravelStatus2 = SimGameTravelStatus.WARMING_ENGINES;
					num = this.simState.Starmap.CurPlanet.Cost;
				}
				if (!this.simState.HasTravelContract)
				{
					this.simState.AddFunds(-this.simState.Constants.Finances.JumpShipCost, null, true, true);
				}
				break;
			case SimGameTravelStatus.TRANSIT_FROM_JUMP:
				simGameTravelStatus2 = SimGameTravelStatus.IN_SYSTEM;
				this.simState.Starmap.RemoveCompletedTravelOrder();
				this.simState.Starmap.CompletedTravelClear();
				this.simState.SetTimeMoving(false, false);
				this.simState.LogReport(string.Format("Arrived at {0} System Orbit", this.simState.Starmap.CurPlanet.System.Def.Description.Name));
				break;
			}
			if (simGameTravelStatus2 != SimGameTravelStatus.UNKNOWN)
			{
				this.SetTravelState(simGameTravelStatus2, false);
			}
			this.simState.SetTravelTime(num, null);
		}

		// Token: 0x06008E30 RID: 36400 RVA: 0x00241ED7 File Offset: 0x002400D7
		public void ForceTravelState(SimGameTravelStatus newState)
		{
			this.simState.UpdateCompanyStatsFromTravel(newState);
			this.travelState.SetState((int)newState, true, true);
		}

		// Token: 0x06008E31 RID: 36401 RVA: 0x00241EF4 File Offset: 0x002400F4
		public void SetTravelStateFromInterrupt(SimGameTravelStatus newStatus, bool force = false)
		{
			this.bInterruptedTravelBackToSystem = true;
			this.SetTravelState(newStatus, force);
			this.bInterruptedTravelBackToSystem = false;
		}

		// Token: 0x06008E32 RID: 36402 RVA: 0x00241F0C File Offset: 0x0024010C
		public void SetTravelState(SimGameTravelStatus newStatus, bool force = false)
		{
			if (this.simState.CurRoomState != DropshipLocation.SHIP && !force)
			{
				this.travelStatePending = newStatus;
				return;
			}
			if (!this.CanChangeState())
			{
				Debug.Log(string.Format("Attempted to change state to {0} but CanChangeState failed", newStatus.ToString()));
				return;
			}
			this.PreTransitionState = this.TravelState;
			this.PostTransitionState = newStatus;
			if (!this.travelState.CanEnter((int)newStatus))
			{
				this.ResetTempData();
				Debug.Log(string.Format("Attempted to change state to {0} but its CanEnter failed", newStatus.ToString()));
				return;
			}
			SimGameShipAnimation animationForStateChange = this.GetAnimationForStateChange(this.PreTransitionState, this.PostTransitionState);
			if (animationForStateChange == SimGameShipAnimation.INVALID)
			{
				this.ResetTempData();
				Debug.Log(string.Format("Attempted to change state from {0} to {1} but returned an invalid anim", this.PostTransitionState.ToString(), this.PreTransitionState.ToString()));
				return;
			}
			if (animationForStateChange == SimGameShipAnimation.NONE)
			{
				this.simState.UpdateCompanyStatsFromTravel(newStatus);
				this.travelState.SetState((int)newStatus, false, false);
				return;
			}
			if (this.simState.UXAttached)
			{
				this.simState.RoomManager.ShipRoom.RefreshData();
				if (!force)
				{
					this.BeginTransferAnim();
					return;
				}
			}
			else
			{
				Debug.LogWarning("[SetTravelState] Sim Game State wasn't ready!");
			}
		}

		// Token: 0x06008E33 RID: 36403 RVA: 0x000193C6 File Offset: 0x000175C6
		private bool CanChangeState()
		{
			return true;
		}

		// Token: 0x06008E34 RID: 36404 RVA: 0x00242040 File Offset: 0x00240240
		public SimGameShipAnimation GetAnimationForStateChange(SimGameTravelStatus preState, SimGameTravelStatus postState)
		{
			switch (preState)
			{
			case SimGameTravelStatus.IN_SYSTEM:
				switch (postState)
				{
				case SimGameTravelStatus.IN_SYSTEM:
				case SimGameTravelStatus.AT_JUMP_POINT:
				case SimGameTravelStatus.WARMING_ENGINES:
				case SimGameTravelStatus.TRANSIT_FROM_JUMP:
				case SimGameTravelStatus.TRANSITION_ANIMATING:
				case SimGameTravelStatus.UNKNOWN:
					return SimGameShipAnimation.INVALID;
				case SimGameTravelStatus.TRANSIT_TO_JUMP:
					return SimGameShipAnimation.LEAVE_PLANET;
				}
				break;
			case SimGameTravelStatus.AT_JUMP_POINT:
				switch (postState)
				{
				case SimGameTravelStatus.IN_SYSTEM:
				case SimGameTravelStatus.AT_JUMP_POINT:
				case SimGameTravelStatus.TRANSIT_TO_JUMP:
				case SimGameTravelStatus.TRANSITION_ANIMATING:
				case SimGameTravelStatus.UNKNOWN:
					return SimGameShipAnimation.INVALID;
				case SimGameTravelStatus.WARMING_ENGINES:
					return SimGameShipAnimation.NONE;
				case SimGameTravelStatus.TRANSIT_FROM_JUMP:
					return SimGameShipAnimation.LEAVE_JUMPSHIP;
				}
				break;
			case SimGameTravelStatus.WARMING_ENGINES:
				switch (postState)
				{
				case SimGameTravelStatus.IN_SYSTEM:
				case SimGameTravelStatus.TRANSIT_TO_JUMP:
				case SimGameTravelStatus.TRANSITION_ANIMATING:
				case SimGameTravelStatus.UNKNOWN:
					return SimGameShipAnimation.INVALID;
				case SimGameTravelStatus.AT_JUMP_POINT:
				case SimGameTravelStatus.WARMING_ENGINES:
					return SimGameShipAnimation.JUMPSHIP_CHARGE_AND_JUMP;
				case SimGameTravelStatus.TRANSIT_FROM_JUMP:
					if (this.bInterruptedTravelBackToSystem)
					{
						return SimGameShipAnimation.LEAVE_JUMPSHIP;
					}
					return SimGameShipAnimation.JUMPSHIP_CHARGE_AND_JUMP_THEN_LEAVE;
				}
				break;
			case SimGameTravelStatus.TRANSIT_TO_JUMP:
				switch (postState)
				{
				case SimGameTravelStatus.IN_SYSTEM:
				case SimGameTravelStatus.TRANSIT_TO_JUMP:
				case SimGameTravelStatus.TRANSIT_FROM_JUMP:
				case SimGameTravelStatus.TRANSITION_ANIMATING:
				case SimGameTravelStatus.UNKNOWN:
					return SimGameShipAnimation.INVALID;
				case SimGameTravelStatus.AT_JUMP_POINT:
				case SimGameTravelStatus.WARMING_ENGINES:
					return SimGameShipAnimation.DOCK_AT_JUMPSHIP;
				}
				break;
			case SimGameTravelStatus.TRANSIT_FROM_JUMP:
				if (postState == SimGameTravelStatus.IN_SYSTEM)
				{
					return SimGameShipAnimation.ARRIVE_AT_PLANET;
				}
				if (postState - SimGameTravelStatus.AT_JUMP_POINT <= 5)
				{
					return SimGameShipAnimation.INVALID;
				}
				break;
			case SimGameTravelStatus.UNKNOWN:
				return SimGameShipAnimation.NONE;
			}
			return SimGameShipAnimation.NONE;
		}

		// Token: 0x06008E35 RID: 36405 RVA: 0x00242135 File Offset: 0x00240335
		public void BeginTransferAnim()
		{
			this.travelState.SetState(5, false, false);
			LazySingletonBehavior<UIManager>.Instance.PushEscKeyHandler(delegate
			{
				this.AnimationInterrupt();
			});
		}

		// Token: 0x06008E36 RID: 36406 RVA: 0x0024215C File Offset: 0x0024035C
		public void ReturnFromTransferAnim()
		{
			if (this.TravelState == SimGameTravelStatus.TRANSITION_ANIMATING)
			{
				if (this.currentTransitioningAnim == SimGameShipAnimation.JUMPSHIP_CHARGE_AND_JUMP_THEN_LEAVE)
				{
					if (this.jumpshipChargeAndLeaveAnimCounter == 0)
					{
						this.jumpshipChargeAndLeaveAnimCounter++;
						return;
					}
					this.jumpshipChargeAndLeaveAnimCounter = 0;
				}
				this.simState.UpdateCompanyStatsFromTravel(this.PostTransitionState);
				this.travelState.SetState((int)this.PostTransitionState, false, false);
				LazySingletonBehavior<UIManager>.Instance.PopEscKeyHandler();
				this.simState.RoomManager.RefreshDisplay();
			}
		}

		// Token: 0x06008E37 RID: 36407 RVA: 0x002421DC File Offset: 0x002403DC
		protected bool AtJumpPoint_CanEnter()
		{
			switch (this.PreTransitionState)
			{
			case SimGameTravelStatus.IN_SYSTEM:
			case SimGameTravelStatus.TRANSIT_FROM_JUMP:
			case SimGameTravelStatus.UNKNOWN:
				return false;
			case SimGameTravelStatus.AT_JUMP_POINT:
			case SimGameTravelStatus.WARMING_ENGINES:
			case SimGameTravelStatus.TRANSIT_TO_JUMP:
			case SimGameTravelStatus.TRANSITION_ANIMATING:
				return true;
			default:
				return true;
			}
		}

		// Token: 0x06008E38 RID: 36408 RVA: 0x0000D184 File Offset: 0x0000B384
		protected void AtJumpPoint_OnEnter()
		{
		}

		// Token: 0x06008E39 RID: 36409 RVA: 0x0000D184 File Offset: 0x0000B384
		protected void AtJumpPoint_OnExit()
		{
		}

		// Token: 0x06008E3A RID: 36410 RVA: 0x0024221C File Offset: 0x0024041C
		protected bool InSystem_CanEnter()
		{
			switch (this.PreTransitionState)
			{
			case SimGameTravelStatus.IN_SYSTEM:
			case SimGameTravelStatus.TRANSIT_FROM_JUMP:
			case SimGameTravelStatus.TRANSITION_ANIMATING:
				return true;
			case SimGameTravelStatus.AT_JUMP_POINT:
			case SimGameTravelStatus.WARMING_ENGINES:
			case SimGameTravelStatus.TRANSIT_TO_JUMP:
			case SimGameTravelStatus.UNKNOWN:
				return false;
			default:
				return true;
			}
		}

		// Token: 0x06008E3B RID: 36411 RVA: 0x0024225C File Offset: 0x0024045C
		protected void InSystem_OnEnter()
		{
			if (this.ArriveAtSystemFromTravel)
			{
				this.ArriveAtSystemFromTravel = false;
				this.simState.RoomManager.SetNavDrawerCollapsed(false);
				this.simState.RoomManager.RefreshDisplay();
				if (!this.simState.HasTravelContract)
				{
					if (this.simState.ActiveFlashpoint != null && this.simState.ActiveFlashpoint.CurSystem == this.simState.CurSystem && this.simState.ActiveFlashpoint.CurStatus == Flashpoint.Status.SELECTED_ENROUTE)
					{
						this.QueueFlashpointEnterSystemPopup();
					}
					else if (this.simState.GetFlashpointInSystem(this.simState.CurSystem) != null)
					{
						this.QueueFlashpointEnterSystemPopup();
					}
					else
					{
						this.DisplayEnteredOrbitPopup();
					}
				}
				else if (this.simState.OnBreadcrumbArrival())
				{
					this.simState.SetBreadcrumbArrived(true);
				}
				AudioEventManager.SetMusicState(AudioState_Music_State.Sim_Game, AudioSwitch_Mission_Status.argo, AudioState_Player_State.alive, false);
			}
		}

		// Token: 0x06008E3C RID: 36412 RVA: 0x0000D184 File Offset: 0x0000B384
		protected void InSystem_OnExit()
		{
		}

		// Token: 0x06008E3D RID: 36413 RVA: 0x000193C6 File Offset: 0x000175C6
		protected bool TransitionAnimating_CanEnter()
		{
			return true;
		}

		// Token: 0x06008E3E RID: 36414 RVA: 0x0024233C File Offset: 0x0024053C
		protected void TransitionAnimating_OnEnter()
		{
			LazySingletonBehavior<UIManager>.Instance.FadeUICanvasGroup(0f, 0.25f);
			SimGameShipAnimation animationForStateChange = this.GetAnimationForStateChange(this.PreTransitionState, this.PostTransitionState);
			AudioEventManager.SetAmbience(AudioState_ambiences.sim_space);
			this.currentTransitioningAnim = animationForStateChange;
			this.BeginTransitionAnimation();
		}

		// Token: 0x06008E3F RID: 36415 RVA: 0x00242383 File Offset: 0x00240583
		private void BeginTransitionAnimation()
		{
			SceneSingletonBehavior<UnityGameInstance>.Instance.StartCoroutine(this.TransitionAnimationStart());
		}

		// Token: 0x06008E40 RID: 36416 RVA: 0x00242396 File Offset: 0x00240596
		private IEnumerator TransitionAnimationStart()
		{
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			switch (this.currentTransitioningAnim)
			{
			case SimGameShipAnimation.ORBIT_PLANET:
				this.simState.CameraController.spaceController.Orbit(this.PostTransitionState, false);
				break;
			case SimGameShipAnimation.DOCK_AT_JUMPSHIP:
				this.simState.CameraController.spaceController.Dock(this.PostTransitionState, false);
				WwiseManager.PostTrigger<AudioTriggerList>(AudioTriggerList.jump_initiated, WwiseManager.MusicAudioObject);
				AudioEventManager.SetMusicState(AudioState_Music_State.Sim_Game, AudioSwitch_Mission_Status.jump_travel, AudioState_Player_State.alive, false);
				break;
			case SimGameShipAnimation.LEAVE_JUMPSHIP:
				this.simState.CameraController.spaceController.ToPlanet(this.PostTransitionState, false);
				break;
			case SimGameShipAnimation.JUMPSHIP_CHARGE_AND_JUMP:
				this.simState.CameraController.spaceController.JumpDrive(this.PostTransitionState, false);
				break;
			case SimGameShipAnimation.JUMPSHIP_CHARGE_AND_JUMP_THEN_LEAVE:
				this.simState.CameraController.spaceController.JumpDriveFinal(this.PostTransitionState, false);
				break;
			case SimGameShipAnimation.ARRIVE_AT_PLANET:
				this.simState.CameraController.spaceController.Orbit(this.PostTransitionState, false);
				AudioEventManager.SetMusicState(AudioState_Music_State.Sim_Game, AudioSwitch_Mission_Status.arrival, AudioState_Player_State.alive, false);
				break;
			case SimGameShipAnimation.LEAVE_PLANET:
				this.simState.CameraController.spaceController.ToJumpship(this.PostTransitionState, false);
				break;
			}
			yield break;
		}

		// Token: 0x06008E41 RID: 36417 RVA: 0x002423A8 File Offset: 0x002405A8
		protected void TransitionAnimating_OnExit()
		{
			LazySingletonBehavior<UIManager>.Instance.FadeUICanvasGroup(1f, 0.25f);
			if (this.PostTransitionState == SimGameTravelStatus.IN_SYSTEM)
			{
				this.ArriveAtSystemFromTravel = true;
			}
			if (this.PostTransitionState == SimGameTravelStatus.TRANSIT_FROM_JUMP)
			{
				AudioEventManager.SetAmbience(AudioState_ambiences.sim_space);
				AudioEventManager.SetMusicState(AudioState_Music_State.Sim_Game, AudioSwitch_Mission_Status.Argo_TimeAdvancing, AudioState_Player_State.alive, false);
			}
			if (!this.pauseAtTravelSteps && !this.ArriveAtSystemFromTravel)
			{
				this.simState.SetTimeMoving(true, true);
			}
			this.travelState.SetState((int)this.PostTransitionState, false, false);
			this.currentTransitioningAnim = SimGameShipAnimation.INVALID;
		}

		// Token: 0x06008E42 RID: 36418 RVA: 0x0024242C File Offset: 0x0024062C
		protected bool TransitFromJump_CanEnter()
		{
			switch (this.PreTransitionState)
			{
			case SimGameTravelStatus.IN_SYSTEM:
			case SimGameTravelStatus.WARMING_ENGINES:
			case SimGameTravelStatus.TRANSIT_FROM_JUMP:
			case SimGameTravelStatus.TRANSITION_ANIMATING:
				return true;
			case SimGameTravelStatus.AT_JUMP_POINT:
			case SimGameTravelStatus.TRANSIT_TO_JUMP:
			case SimGameTravelStatus.UNKNOWN:
				return false;
			default:
				return true;
			}
		}

		// Token: 0x06008E43 RID: 36419 RVA: 0x0000D184 File Offset: 0x0000B384
		protected void TransitFromJump_OnEnter()
		{
		}

		// Token: 0x06008E44 RID: 36420 RVA: 0x0000D184 File Offset: 0x0000B384
		protected void TransitFromJump_OnExit()
		{
		}

		// Token: 0x06008E45 RID: 36421 RVA: 0x0024246C File Offset: 0x0024066C
		protected bool TransitToJump_CanEnter()
		{
			switch (this.PreTransitionState)
			{
			case SimGameTravelStatus.IN_SYSTEM:
			case SimGameTravelStatus.WARMING_ENGINES:
			case SimGameTravelStatus.TRANSIT_TO_JUMP:
			case SimGameTravelStatus.TRANSITION_ANIMATING:
				return true;
			case SimGameTravelStatus.AT_JUMP_POINT:
			case SimGameTravelStatus.TRANSIT_FROM_JUMP:
			case SimGameTravelStatus.UNKNOWN:
				return false;
			default:
				return true;
			}
		}

		// Token: 0x06008E46 RID: 36422 RVA: 0x002424A9 File Offset: 0x002406A9
		protected void TransitToJump_OnEnter()
		{
			this.simState.SetTimeMoving(true, true);
		}

		// Token: 0x06008E47 RID: 36423 RVA: 0x0000D184 File Offset: 0x0000B384
		protected void TransitToJump_OnExit()
		{
		}

		// Token: 0x06008E48 RID: 36424 RVA: 0x000193C6 File Offset: 0x000175C6
		protected bool Unknown_CanEnter()
		{
			return true;
		}

		// Token: 0x06008E49 RID: 36425 RVA: 0x0000D184 File Offset: 0x0000B384
		protected void Unknown_OnEnter()
		{
		}

		// Token: 0x06008E4A RID: 36426 RVA: 0x0000D184 File Offset: 0x0000B384
		protected void Unknown_OnExit()
		{
		}

		// Token: 0x06008E4B RID: 36427 RVA: 0x002424B8 File Offset: 0x002406B8
		protected bool WarmingEngines_CanEnter()
		{
			switch (this.PreTransitionState)
			{
			case SimGameTravelStatus.IN_SYSTEM:
			case SimGameTravelStatus.TRANSIT_FROM_JUMP:
			case SimGameTravelStatus.UNKNOWN:
				return false;
			case SimGameTravelStatus.AT_JUMP_POINT:
			case SimGameTravelStatus.WARMING_ENGINES:
			case SimGameTravelStatus.TRANSIT_TO_JUMP:
			case SimGameTravelStatus.TRANSITION_ANIMATING:
				return true;
			default:
				return true;
			}
		}

		// Token: 0x06008E4C RID: 36428 RVA: 0x0000D184 File Offset: 0x0000B384
		protected void WarmingEngines_OnEnter()
		{
		}

		// Token: 0x06008E4D RID: 36429 RVA: 0x0000D184 File Offset: 0x0000B384
		protected void WarmingEngines_OnExit()
		{
		}

		// Token: 0x06008E4E RID: 36430 RVA: 0x002424F8 File Offset: 0x002406F8
		public void DisplayEnteredOrbitPopup()
		{
			bool flag = true;
			FactionValue ownerValue = this.simState.CurSystem.OwnerValue;
			if (this.simState.GetReputation(ownerValue) <= SimGameReputation.LOATHED)
			{
				flag = false;
			}
			if (flag)
			{
				this.simState.GetInterruptQueue().QueueTravelPauseNotification("Arrived", Strings.T("We've arrived at {0}.", new object[] { this.simState.Starmap.CurPlanet.System.Def.Description.Name }), this.simState.GetCrewPortrait(SimGameCrew.Crew_Sumire), "notification_travelcomplete", new Action(this.OnArrivedAtPlanet), "Visit Store", new Action(this.SaveNow), "Continue");
			}
			else
			{
				this.simState.GetInterruptQueue().QueueTravelPauseNotification("Arrived", Strings.T("We've arrived at {0}.", new object[] { this.simState.Starmap.CurPlanet.System.Def.Description.Name }), this.simState.GetCrewPortrait(SimGameCrew.Crew_Sumire), "notification_travelcomplete", new Action(this.SaveNow), "Continue", null, null);
			}
			if (!this.simState.TimeMoving)
			{
				this.simState.GetInterruptQueue().DisplayIfAvailable();
			}
		}

		// Token: 0x06008E4F RID: 36431 RVA: 0x00242644 File Offset: 0x00240844
		public void QueueFlashpointEnterSystemPopup()
		{
			Flashpoint flashpointInSystem = this.simState.GetFlashpointInSystem(this.simState.CurSystem);
			if (flashpointInSystem != null)
			{
				this.simState.GetInterruptQueue().QueueFlashpointEnteredSystemNotificationEntry(flashpointInSystem, new UnityAction(this.OnFlashpointPopupAccepted), new UnityAction(this.simState.RoomManager.ShipRoom.RefreshData));
			}
		}

		// Token: 0x06008E50 RID: 36432 RVA: 0x002426A8 File Offset: 0x002408A8
		public void DisplayFlashpointStartPopup(Action OnComplete)
		{
			Flashpoint activeFlashpoint = this.simState.ActiveFlashpoint;
			if (activeFlashpoint != null)
			{
				LazySingletonBehavior<UIManager>.Instance.GetOrCreatePopupModule<SG_FlashpointInfoPopup>("", true).SetData(this.simState, activeFlashpoint, new UnityAction(this.OnFlashpointPopupAccepted), new UnityAction(this.simState.RoomManager.ShipRoom.RefreshData));
			}
			this.simState.StopPlayMode();
		}

		// Token: 0x06008E51 RID: 36433 RVA: 0x00242714 File Offset: 0x00240914
		private void OnFlashpointPopupAccepted()
		{
			Flashpoint flashpointInSystem = this.simState.GetFlashpointInSystem(this.simState.CurSystem);
			if (flashpointInSystem != this.simState.ActiveFlashpoint)
			{
				this.simState.SetActiveFlashpoint(flashpointInSystem);
				return;
			}
			if (this.simState.ActiveFlashpoint.CurStatus == Flashpoint.Status.IN_PROGRESS)
			{
				this.simState.RoomManager.SetQueuedUIActivationID(DropshipMenuType.Contract, DropshipLocation.CMD_CENTER, true);
				this.simState.SetSimRoomState(DropshipLocation.CMD_CENTER);
				return;
			}
			this.simState.ActiveFlashpoint.SetStatus(Flashpoint.Status.IN_PROGRESS);
			this.simState.ActiveFlashpoint.MilestoneCheck(false);
		}

		// Token: 0x06008E52 RID: 36434 RVA: 0x002427A9 File Offset: 0x002409A9
		private void OnArrivedAtPlanet()
		{
			this.simState.TriggerSaveNow(SaveReason.SIM_GAME_ARRIVED_AT_PLANET, SimGameState.TriggerSaveNowOption.QUEUE_IF_NEEDED);
			this.simState.RoomManager.ForceShipRoomChangeOfRoom(DropshipLocation.SHOP);
		}

		// Token: 0x06008E53 RID: 36435 RVA: 0x002427CF File Offset: 0x002409CF
		private void SaveNow()
		{
			this.simState.TriggerSaveNow(SaveReason.SIM_GAME_ARRIVED_AT_PLANET, SimGameState.TriggerSaveNowOption.QUEUE_IF_NEEDED);
		}

		// Token: 0x06008E54 RID: 36436 RVA: 0x002427E0 File Offset: 0x002409E0
		public void NewSystemJumpPopup()
		{
			this.simState.SetTimeMoving(false, true);
			PauseNotification.Show("Arrived", Strings.T("We've jumped to the {0} system.", new object[] { this.simState.Starmap.CurPlanet.System.Def.Description.Name }), this.simState.GetCrewPortrait(SimGameCrew.Crew_Sumire), "", true, null, "Pause Here", new Action(this.OnJumpPopupContinue), "Continue");
		}

		// Token: 0x06008E55 RID: 36437 RVA: 0x002424A9 File Offset: 0x002406A9
		public void OnJumpPopupContinue()
		{
			this.simState.SetTimeMoving(true, true);
		}

		// Token: 0x04005862 RID: 22626
		private SimGameState simState;

		// Token: 0x04005863 RID: 22627
		public SimGameTravelStatus PreTransitionState;

		// Token: 0x04005864 RID: 22628
		public SimGameTravelStatus PostTransitionState;

		// Token: 0x04005865 RID: 22629
		protected GenericMachine<SimGameTravelStatus> travelState;

		// Token: 0x04005866 RID: 22630
		public bool pauseAtTravelSteps;

		// Token: 0x04005867 RID: 22631
		private SimGameTravelStatus travelStatePending = SimGameTravelStatus.UNKNOWN;

		// Token: 0x04005868 RID: 22632
		private bool ArriveAtSystemFromTravel;

		// Token: 0x04005869 RID: 22633
		private bool bInterruptedTravelBackToSystem;

		// Token: 0x0400586A RID: 22634
		private SimGameShipAnimation currentTransitioningAnim = SimGameShipAnimation.INVALID;

		// Token: 0x0400586B RID: 22635
		private int jumpshipChargeAndLeaveAnimCounter;
	}
}
