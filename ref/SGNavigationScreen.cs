using System;
using System.Collections;
using System.Collections.Generic;
using BattleTech.Framework;
using BattleTech.UI.TMProWrapper;
using BattleTech.UI.Tooltips;
using HBS;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace BattleTech.UI
{
	// Token: 0x02002062 RID: 8290
	[UIModule.PrefabNameAttr("uixPrfPanl_NAV_locationDetailsHUD")]
	public class SGNavigationScreen : UIModule, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
	{
		// Token: 0x06013D8B RID: 81291 RVA: 0x004FC470 File Offset: 0x004FA670
		public void Init(SimGameState simGame, SGRoomController_Navigation myNavRoom = null)
		{
			this.simState = simGame;
			this.SystemViewPopulator.Init(this.simState);
			this.TransitStatusWidget.DisplayTransitState(this.simState);
			this.TravelPopup = LazySingletonBehavior<UIManager>.Instance.GetOrCreatePopupModule<SGNavigationTravelPopup>("", true);
			this.TravelPopup.Init(this.simState);
			this.TravelPopup.Visible = false;
			this.TravelButton.OnClicked.RemoveAllListeners();
			this.TravelButton.OnClicked.AddListener(new UnityAction(this.OnTravelButtonClicked));
			this.BackButton.OnClicked.RemoveAllListeners();
			this.BackButton.OnClicked.AddListener(new UnityAction(this.Dismiss));
			this.AllCallouts.ForEach(new Action<SGNavStarSystemCallout>(this.DisposeCallout));
			this.flashpointButtonController.Init(simGame, new UnityAction<Flashpoint>(this.FlashpointButtonResponse), new UnityAction<Flashpoint>(this.FlashpointButtonResponse));
			this.BuildDifficultyDropdown();
			this.BiomeDropdown.InitBiomeDataLists();
			foreach (GameObject gameObject in this.InfoPanelsToHide)
			{
				gameObject.SetActive(false);
			}
		}

		// Token: 0x06013D8C RID: 81292 RVA: 0x004FC5C4 File Offset: 0x004FA7C4
		public void RefreshWidget()
		{
			if (this.simState.Starmap != null)
			{
				this.simState.Starmap.StarSystemRouted.RemoveListener(new UnityAction<StarSystem>(this.OnSystemRouted));
				this.simState.Starmap.StarSystemRouted.AddListener(new UnityAction<StarSystem>(this.OnSystemRouted));
				this.simState.Starmap.StarSystemHovered.RemoveListener(new UnityAction<StarSystem>(this.OnSystemHovered));
				this.simState.Starmap.StarSystemHovered.AddListener(new UnityAction<StarSystem>(this.OnSystemHovered));
				if (this.simState.Starmap.CurSelected == null)
				{
					this.simState.Starmap.SetSelectedSystem(this.simState.CurSystem);
				}
				this.OnSystemRouted(this.simState.Starmap.CurSelected.System);
			}
			this.TransitStatusWidget.DisplayTransitState(this.simState);
			this.RefreshAllCallouts();
			this.simState.Starmap.Screen.RefreshSystems();
			this.RefreshSystemIndicators();
		}

		// Token: 0x06013D8D RID: 81293 RVA: 0x004FC6E8 File Offset: 0x004FA8E8
		private void RefreshAllCallouts()
		{
			foreach (SGNavStarSystemCallout sgnavStarSystemCallout in this.AllCallouts.ToArray())
			{
				this.DisposeCallout(sgnavStarSystemCallout);
			}
			this.CalloutsBySystem.Clear();
			this.TravelCallouts.Clear();
			this.DifficultyCallouts.Clear();
			this.HoverCallout = null;
			this.ShowTravelContractCallouts();
			this.ShowArgoCallout();
		}

		// Token: 0x06013D8E RID: 81294 RVA: 0x004FC750 File Offset: 0x004FA950
		private void RefreshSystemIndicators()
		{
			if (this.simState != null && this.simState.Starmap != null)
			{
				this.ResetSpecialIndicators();
				if (this.StoreDropdown.value > 0)
				{
					this.SetStoreFilterByIndex(this.StoreDropdown.value);
				}
				else if (this.BiomeDropdown.value > 0)
				{
					this.SetBiomeByIndex(this.BiomeDropdown.value);
				}
				this.ShowFlashpointSystems();
				this.ShowSpecialSystems();
				this.RefreshDifficultyCallouts();
			}
		}

		// Token: 0x06013D8F RID: 81295 RVA: 0x004FC7D0 File Offset: 0x004FA9D0
		private void ShowArgoCallout()
		{
			this.CurrentSystemCallout = this.GetSystemCallout(this.simState.CurSystem.ID);
			this.CurrentSystemCallout.SetIconMode(SGNavStarSystemCallout.IconMode.Argo, null);
			this.CurrentSystemCallout.LabelText = this.simState.SpaceController.currentShip.ToString();
			this.simState.RequestItem<Sprite>(this.simState.Player1sMercUnitHeraldryDef.textureLogoID, delegate(Sprite resource)
			{
				this.CurrentSystemCallout.SetIconMode(SGNavStarSystemCallout.IconMode.Icon, resource);
			}, BattleTechResourceType.Sprite);
		}

		// Token: 0x06013D90 RID: 81296 RVA: 0x004FC858 File Offset: 0x004FAA58
		private void CreateDifficultyCallouts(int difficulty)
		{
			foreach (SGNavStarSystemCallout sgnavStarSystemCallout in this.DifficultyCallouts)
			{
				sgnavStarSystemCallout.ShowDifficultyWidget(false);
			}
			this.DifficultyCallouts.Clear();
			if (difficulty < 0)
			{
				return;
			}
			for (int i = 0; i < this.simState.Starmap.PlanetList.Count; i++)
			{
				StarSystem system = this.simState.Starmap.PlanetList[i].System;
				if (this.simState.GetNormalizedDifficulty(system.Def) == difficulty)
				{
					this.DifficultyCallouts.Add(this.GetSystemCallout(this.simState.Starmap.PlanetList[i].System.ID));
				}
			}
			Biome.BIOMESKIN currentlySelectedBiomeFromDropdown = this.GetCurrentlySelectedBiomeFromDropdown();
			bool flag = currentlySelectedBiomeFromDropdown != Biome.BIOMESKIN.UNDEFINED && currentlySelectedBiomeFromDropdown != Biome.BIOMESKIN.generic;
			foreach (SGNavStarSystemCallout sgnavStarSystemCallout2 in this.DifficultyCallouts)
			{
				if (flag)
				{
					if (sgnavStarSystemCallout2.SystemRenderer.IsHightlightActive(true))
					{
						sgnavStarSystemCallout2.ShowDifficultyWidget(true);
						sgnavStarSystemCallout2.SetDifficulty(difficulty);
					}
				}
				else
				{
					sgnavStarSystemCallout2.ShowDifficultyWidget(true);
					sgnavStarSystemCallout2.SetDifficulty(difficulty);
				}
			}
		}

		// Token: 0x06013D91 RID: 81297 RVA: 0x004FC9C4 File Offset: 0x004FABC4
		public void OnDifficultySelectionChanged(int index)
		{
			if (index < 0 || index > 10)
			{
				return;
			}
			if (index == 0)
			{
				this.CreateDifficultyCallouts(-1);
			}
			else
			{
				this.CreateDifficultyCallouts(index);
			}
			this.RefreshSystemIndicators();
		}

		// Token: 0x06013D92 RID: 81298 RVA: 0x004FC9EC File Offset: 0x004FABEC
		public void SetBiomeCallouts(Biome.BIOMESKIN biome)
		{
			foreach (StarSystemNode starSystemNode in this.simState.Starmap.PlanetList)
			{
				StarmapSystemRenderer systemRenderer = this.simState.Starmap.Screen.GetSystemRenderer(starSystemNode.System.ID);
				if (biome == Biome.BIOMESKIN.UNDEFINED || starSystemNode.System.Def.SupportedBiomes.Contains(biome))
				{
					systemRenderer.SetBiome(biome);
				}
			}
		}

		// Token: 0x06013D93 RID: 81299 RVA: 0x004FCA88 File Offset: 0x004FAC88
		public void SetStoreCallouts(string store)
		{
			foreach (StarSystemNode starSystemNode in this.simState.Starmap.PlanetList)
			{
				StarmapSystemRenderer systemRenderer = this.simState.Starmap.Screen.GetSystemRenderer(starSystemNode.System.ID);
				if (!string.IsNullOrEmpty(store) && systemRenderer.system.System.Def.SystemShopItems.Contains(store))
				{
					systemRenderer.SetStore(store);
				}
				else
				{
					systemRenderer.SetStore(null);
				}
			}
		}

		// Token: 0x06013D94 RID: 81300 RVA: 0x004FCB34 File Offset: 0x004FAD34
		public void OnBiomeSelectionChanged(int index)
		{
			this.SetBiomeByIndex(index);
			if (index > 0)
			{
				this.StoreDropdown.SetDropdownIndexWithoutCallback(0);
			}
			this.RefreshSystemIndicators();
		}

		// Token: 0x06013D95 RID: 81301 RVA: 0x004FCB53 File Offset: 0x004FAD53
		public void OnStoreSelectionChanged(int index)
		{
			this.SetStoreFilterByIndex(index);
			if (index > 0)
			{
				this.BiomeDropdown.SetDropdownIndexWithoutCallback(0);
			}
			this.RefreshSystemIndicators();
		}

		// Token: 0x06013D96 RID: 81302 RVA: 0x004FCB74 File Offset: 0x004FAD74
		public void SetStoreFilterByIndex(int index)
		{
			if (index < 0 || index > this.StoreDropdown.options.Count)
			{
				return;
			}
			SGNavigationStoreDropdown.StoreData dataFromIndex = this.StoreDropdown.GetDataFromIndex(index);
			this.storeDropdownTooltip.SetDefaultStateData(TooltipUtilities.GetStateDataFromObject(this.StoreDropdown.GetToolipForDropdown(index)));
			if (dataFromIndex == null)
			{
				return;
			}
			this.SetBiomeCallouts(Biome.BIOMESKIN.UNDEFINED);
			this.SetStoreCallouts(dataFromIndex.storeID);
		}

		// Token: 0x06013D97 RID: 81303 RVA: 0x004FCBDC File Offset: 0x004FADDC
		public void SetBiomeByIndex(int index)
		{
			if (index < 0 || index > this.BiomeDropdown.options.Count)
			{
				return;
			}
			SGBiomeDropdown.BiomeData dataFromIndex = this.BiomeDropdown.GetDataFromIndex(index);
			if (index == 0)
			{
				this.SetBiomeCallouts(Biome.BIOMESKIN.UNDEFINED);
				return;
			}
			this.SetBiomeCallouts(Biome.BIOMESKIN.UNDEFINED);
			switch (dataFromIndex.biomeType)
			{
			case Biome.BIOMESKIN.UNDEFINED:
			case Biome.BIOMESKIN.generic:
				break;
			case Biome.BIOMESKIN.highlandsSpring:
			case Biome.BIOMESKIN.highlandsFall:
			case Biome.BIOMESKIN.lowlandsSpring:
			case Biome.BIOMESKIN.lowlandsFall:
			case Biome.BIOMESKIN.lowlandsCoastal:
				this.SetBiomeCallouts(Biome.BIOMESKIN.highlandsFall);
				this.SetBiomeCallouts(Biome.BIOMESKIN.highlandsSpring);
				this.SetBiomeCallouts(Biome.BIOMESKIN.lowlandsCoastal);
				this.SetBiomeCallouts(Biome.BIOMESKIN.lowlandsFall);
				this.SetBiomeCallouts(Biome.BIOMESKIN.lowlandsSpring);
				return;
			case Biome.BIOMESKIN.desertParched:
			case Biome.BIOMESKIN.badlandsParched:
				this.SetBiomeCallouts(Biome.BIOMESKIN.badlandsParched);
				this.SetBiomeCallouts(Biome.BIOMESKIN.desertParched);
				return;
			case Biome.BIOMESKIN.lunarVacuum:
				this.SetBiomeCallouts(Biome.BIOMESKIN.lunarVacuum);
				return;
			case Biome.BIOMESKIN.martianVacuum:
				this.SetBiomeCallouts(Biome.BIOMESKIN.martianVacuum);
				return;
			case Biome.BIOMESKIN.polarFrozen:
				this.SetBiomeCallouts(Biome.BIOMESKIN.polarFrozen);
				return;
			case Biome.BIOMESKIN.tundraFrozen:
				this.SetBiomeCallouts(Biome.BIOMESKIN.tundraFrozen);
				return;
			case Biome.BIOMESKIN.jungleTropical:
				this.SetBiomeCallouts(Biome.BIOMESKIN.jungleTropical);
				return;
			case Biome.BIOMESKIN.urbanHighTech:
				this.SetBiomeCallouts(Biome.BIOMESKIN.urbanHighTech);
				break;
			default:
				return;
			}
		}

		// Token: 0x06013D98 RID: 81304 RVA: 0x004FCCD2 File Offset: 0x004FAED2
		protected Biome.BIOMESKIN GetCurrentlySelectedBiomeFromDropdown()
		{
			return this.BiomeDropdown.GetDataFromIndex(this.BiomeDropdown.value).biomeType;
		}

		// Token: 0x06013D99 RID: 81305 RVA: 0x004FCCEF File Offset: 0x004FAEEF
		public void ShowTravelContractCallouts()
		{
			this.GetAcceptableTravelContractsFromSystem().ForEach(delegate(Contract contract)
			{
				SGNavStarSystemCallout systemCallout = this.GetSystemCallout(contract.TargetSystem);
				if (contract.Override.contractDisplayStyle == ContractDisplayStyle.BaseCampaignRestoration || contract.Override.contractDisplayStyle == ContractDisplayStyle.BaseCampaignStory)
				{
					FactionValue teamFaction = contract.GetTeamFaction("ecc8d4f2-74b4-465d-adf6-84445e5dfc230");
					FactionDef factionDef = this.simState.GetFactionDef(teamFaction.Name);
					Sprite sprite = FactionEnumeration.GetTravelContractFactionValue().FactionDef.GetSprite();
					if (factionDef != null)
					{
						sprite = factionDef.GetSprite();
					}
					systemCallout.SetIconMode(SGNavStarSystemCallout.IconMode.Icon, sprite);
					systemCallout.LabelText = this.simState.PriorityMissionTitle;
					return;
				}
				systemCallout.SetIconMode(SGNavStarSystemCallout.IconMode.Alert, null);
				systemCallout.LabelText = this.simState.TravelContractTitle;
			});
		}

		// Token: 0x06013D9A RID: 81306 RVA: 0x004FCD08 File Offset: 0x004FAF08
		private void ShowFlashpointSystems()
		{
			foreach (Flashpoint flashpoint in this.simState.AvailableFlashpoints)
			{
				if (flashpoint.CurStatus != Flashpoint.Status.WAITING_FOR_DATA)
				{
					this.GetSystemFlashpoint(flashpoint);
				}
			}
		}

		// Token: 0x06013D9B RID: 81307 RVA: 0x004FCD6C File Offset: 0x004FAF6C
		private void ShowSpecialSystems()
		{
			for (int i = 0; i < this.simState.Starmap.PlanetList.Count; i++)
			{
				this.GetSystemSpecialIndicator(this.simState.Starmap.PlanetList[i].System.ID);
			}
		}

		// Token: 0x06013D9C RID: 81308 RVA: 0x004FCDC0 File Offset: 0x004FAFC0
		private void RefreshDifficultyCallouts()
		{
			int value = this.DifficultyDropdown.value;
			if (value > 0)
			{
				this.CreateDifficultyCallouts(value);
				return;
			}
			this.CreateDifficultyCallouts(-1);
		}

		// Token: 0x06013D9D RID: 81309 RVA: 0x004FCDEC File Offset: 0x004FAFEC
		private SGNavStarSystemCallout GetSystemCallout(string systemId)
		{
			if (!this.CalloutsBySystem.ContainsKey(systemId))
			{
				SGNavStarSystemCallout sgnavStarSystemCallout = this.CreateSystemCallout();
				StarSystem system = this.simState.Starmap.GetSystemByID(systemId).System;
				sgnavStarSystemCallout.Init(this.simState.Starmap.Screen, system);
				this.CalloutsBySystem.Add(systemId, sgnavStarSystemCallout);
			}
			return this.CalloutsBySystem[systemId];
		}

		// Token: 0x06013D9E RID: 81310 RVA: 0x004FCE58 File Offset: 0x004FB058
		private SGNavStarSystemCallout CreateSystemCallout()
		{
			GameObject gameObject = this.simState.DataManager.PooledInstantiate("uixPrfIndc_NAV_locationInfoCalloutV2-Element", BattleTechResourceType.UIModulePrefabs, null, null, this.FlyoutContainer);
			gameObject.transform.localScale = Vector3.one;
			SGNavStarSystemCallout component = gameObject.GetComponent<SGNavStarSystemCallout>();
			this.AllCallouts.Add(component);
			return component;
		}

		// Token: 0x06013D9F RID: 81311 RVA: 0x004FCEB8 File Offset: 0x004FB0B8
		private void DisposeCallout(SGNavStarSystemCallout callout)
		{
			callout.SetIconMode(SGNavStarSystemCallout.IconMode.None, null);
			callout.NameText = "";
			callout.LabelText = "";
			this.simState.DataManager.PoolGameObject("uixPrfIndc_NAV_locationInfoCalloutV2-Element", callout.gameObject);
			this.AllCallouts.Remove(callout);
		}

		// Token: 0x06013DA0 RID: 81312 RVA: 0x004FCF0C File Offset: 0x004FB10C
		private void ResetSpecialIndicators()
		{
			foreach (string text in this.specialIndicatorSystems)
			{
				this.simState.Starmap.Screen.GetSystemRenderer(text).ResetSpecialIndicators();
			}
			this.specialIndicatorSystems.Clear();
		}

		// Token: 0x06013DA1 RID: 81313 RVA: 0x004FCF80 File Offset: 0x004FB180
		private StarmapSystemRenderer GetSystemFlashpoint(Flashpoint flashpoint)
		{
			StarmapSystemRenderer systemRenderer = this.simState.Starmap.Screen.GetSystemRenderer(flashpoint.CurSystem.ID);
			systemRenderer.SetFlashpointAvailable(false);
			systemRenderer.SetFlashpointActive(false);
			systemRenderer.SetFlashpointMiniCampaign(false);
			switch (flashpoint.CurStatus)
			{
			case Flashpoint.Status.AVAILABLE:
			case Flashpoint.Status.SELECTED_ENROUTE:
				if (flashpoint.Def.isHeavyMetalCampaign)
				{
					systemRenderer.SetFlashpointMiniCampaign(true);
				}
				else
				{
					systemRenderer.SetFlashpointAvailable(true);
				}
				break;
			case Flashpoint.Status.PAUSED:
			case Flashpoint.Status.IN_PROGRESS:
				systemRenderer.SetFlashpointActive(true);
				break;
			}
			if (!this.specialIndicatorSystems.Contains(flashpoint.CurSystem.ID))
			{
				this.specialIndicatorSystems.Add(flashpoint.CurSystem.ID);
			}
			return systemRenderer;
		}

		// Token: 0x06013DA2 RID: 81314 RVA: 0x004FD03C File Offset: 0x004FB23C
		private StarmapSystemRenderer GetSystemSpecialIndicator(string systemID)
		{
			StarmapSystemRenderer systemRenderer = this.simState.Starmap.Screen.GetSystemRenderer(systemID);
			StarSystem system = systemRenderer.system.System;
			FactionValue factionValue = ((!system.Def.FactionShopOwnerValue.IsInvalidUnset) ? system.Def.FactionShopOwnerValue : system.Def.OwnerValue);
			bool flag = this.simState.IsSystemFactionStore(system, factionValue);
			bool flag2 = this.simState.IsSystemBlackMarket(system);
			bool flag3 = systemRenderer.IsHightlightActive(false);
			if (!flag && !flag2 && !flag3)
			{
				return null;
			}
			systemRenderer.SetFactionCapital(FactionEnumeration.GetNoFactionValue());
			systemRenderer.SetBlackMarket(false);
			int value = this.DifficultyDropdown.value;
			bool flag4 = false;
			if (value > 0 && this.simState.GetNormalizedDifficulty(system.Def) == value)
			{
				flag4 = true;
			}
			if (!flag3)
			{
				if (flag && this.simState.IsFactionAlly(factionValue, null))
				{
					systemRenderer.SetFactionCapital(factionValue);
				}
				else if (flag2 && this.simState.CompanyTags.Contains(this.simState.Constants.Story.BlackMarketTag))
				{
					systemRenderer.SetBlackMarket(true);
				}
			}
			else if (value <= 0 || flag4)
			{
				systemRenderer.ShowCurrentHighlightedObject();
			}
			if (!this.specialIndicatorSystems.Contains(systemID))
			{
				this.specialIndicatorSystems.Add(systemID);
			}
			return systemRenderer;
		}

		// Token: 0x06013DA3 RID: 81315 RVA: 0x004FD18C File Offset: 0x004FB38C
		private void ActivatePanels(bool activate)
		{
			foreach (GameObject gameObject in this.InfoPanelsToHide)
			{
				if (gameObject != null)
				{
					gameObject.SetActive(activate);
				}
			}
		}

		// Token: 0x06013DA4 RID: 81316 RVA: 0x004FD1E8 File Offset: 0x004FB3E8
		public void OnSystemRouted(StarSystem selectedSystem)
		{
			this.ActivatePanels(selectedSystem != null);
			if (selectedSystem == null)
			{
				return;
			}
			int projectedTravelTime = this.simState.Starmap.ProjectedTravelTime;
			int projectedTravelCost = this.simState.Starmap.ProjectedTravelCost;
			bool flag = this.GetAcceptableTravelContractsFromSystem().Find((Contract c) => selectedSystem.ID == c.TargetSystem) != null;
			this.SystemViewPopulator.UpdateRoutedSystem();
			this.ContractAvailableIndicator.SetActive(flag);
			if (!this.simState.MeetsRequirements(selectedSystem.Def.TravelRequirements.ToArray()))
			{
				string text = "Travel Unavailable";
				foreach (KeyValuePair<string, string> keyValuePair in this.simState.GetTravelRestrictions())
				{
					if (!this.simState.CompanyTags.Contains(keyValuePair.Key))
					{
						text = keyValuePair.Value;
						break;
					}
				}
				this.DisableTravel(true, this.uiManager.UIColorRefs.red, text);
			}
			else if (selectedSystem.ID == this.simState.CurSystem.ID)
			{
				this.DisableTravel(true, this.uiManager.UIColorRefs.white, "Current Location");
			}
			else if (this.simState.Starmap.Destination != null && selectedSystem.ID == this.simState.Starmap.Destination.System.ID)
			{
				this.DisableTravel(true, this.uiManager.UIColorRefs.white, "Current Destination");
			}
			else if (!flag && this.simState.Funds < projectedTravelCost)
			{
				this.TravelButton.SetState(ButtonState.Disabled, false);
			}
			else
			{
				this.DisableTravel(false, this.uiManager.UIColorRefs.white, null);
			}
			Flashpoint flashpointInSystem = this.simState.GetFlashpointInSystem(selectedSystem);
			this.flashpointButtonController.RefreshFlashpointButtons(flashpointInSystem, selectedSystem);
			if (flashpointInSystem != null && flashpointInSystem != UnityGameInstance.BattleTechGame.Simulation.ActiveFlashpoint)
			{
				this.ShowFlashpointPopup(selectedSystem);
			}
		}

		// Token: 0x06013DA5 RID: 81317 RVA: 0x004FD43C File Offset: 0x004FB63C
		private void DisableTravel(bool disable, Color textColor, string disableReason = null)
		{
			this.TravelButton.SetState(disable ? ButtonState.Disabled : ButtonState.Enabled, false);
			this.TravelEnabledSection.SetActive(!disable);
			this.TravelDisabledSection.SetActive(disable);
			this.TravelDisabledField.color = textColor;
			if (!string.IsNullOrEmpty(disableReason))
			{
				this.TravelDisabledField.SetText(disableReason, Array.Empty<object>());
			}
		}

		// Token: 0x06013DA6 RID: 81318 RVA: 0x004FD49D File Offset: 0x004FB69D
		public void OnSystemHovered(StarSystem hoveredSystem)
		{
			if (hoveredSystem != null)
			{
				LazySingletonBehavior<TooltipManager>.Instance.SpawnTooltip(hoveredSystem, hoveredSystem.GetSystemTooltipID(), 1UL, 0.5f, null, false);
				return;
			}
			LazySingletonBehavior<TooltipManager>.Instance.ClearTooltip();
		}

		// Token: 0x06013DA7 RID: 81319 RVA: 0x004FD4C8 File Offset: 0x004FB6C8
		private List<Contract> GetAllContracts()
		{
			List<Contract> list = new List<Contract>();
			list.AddRange(this.simState.GlobalContracts);
			list.AddRange(this.simState.CurSystem.SystemContracts);
			list.AddRange(this.simState.CurSystem.SystemBreadcrumbs);
			return list;
		}

		// Token: 0x06013DA8 RID: 81320 RVA: 0x004FD518 File Offset: 0x004FB718
		private List<Contract> GetContractsAtSystem(StarSystem system)
		{
			return this.GetAllContracts().FindAll((Contract contract) => contract.TargetSystem == system.ID);
		}

		// Token: 0x06013DA9 RID: 81321 RVA: 0x004FD549 File Offset: 0x004FB749
		private List<Contract> GetAllTravelContractsFromSystem()
		{
			return this.GetAllContracts().FindAll((Contract contract) => contract.Override.travelOnly && contract.TargetSystem != this.simState.CurSystem.ID);
		}

		// Token: 0x06013DAA RID: 81322 RVA: 0x004FD562 File Offset: 0x004FB762
		private List<Contract> GetAcceptableTravelContractsFromSystem()
		{
			return this.GetAllContracts().FindAll(delegate(Contract contract)
			{
				bool flag = this.simState.ContractUserMeetsReputation(contract);
				return contract.Override.travelOnly && contract.TargetSystem != this.simState.CurSystem.ID && flag;
			});
		}

		// Token: 0x06013DAB RID: 81323 RVA: 0x004FD57C File Offset: 0x004FB77C
		private List<Contract> GetAcceptableTravelContractsAtSystem(StarSystem system)
		{
			return this.GetAllContracts().FindAll(delegate(Contract contract)
			{
				bool flag = this.simState.ContractUserMeetsReputation(contract);
				return contract.Override.travelOnly && contract.TargetSystem == system.ID && flag;
			});
		}

		// Token: 0x06013DAC RID: 81324 RVA: 0x004FD5B4 File Offset: 0x004FB7B4
		private void OnTravelButtonClicked()
		{
			if (this.simState.TravelState == SimGameTravelStatus.TRANSIT_FROM_JUMP)
			{
				this.simState.ShowChangeDestinationDuringTransitNotification();
				return;
			}
			bool flag = this.GetAcceptableTravelContractsAtSystem(this.simState.Starmap.CurSelected.System).Count > 0;
			int projectedTravelCost = this.simState.Starmap.ProjectedTravelCost;
			int projectedTravelTime = this.simState.Starmap.ProjectedTravelTime;
			this.waitForPointerUp = false;
			this.TravelPopup.Display(this.simState.Starmap.CurSelected.System, projectedTravelTime, projectedTravelCost, flag);
			this.TravelPopup.BtnSetCourse.OnClicked.AddListener(new UnityAction(this.OnTravelCourseAccepted));
			this.TravelPopup.BtnViewContract.OnClicked.AddListener(new UnityAction(this.OnViewContract));
		}

		// Token: 0x06013DAD RID: 81325 RVA: 0x004FD690 File Offset: 0x004FB890
		private void OnTravelCourseAccepted()
		{
			if (this.simState.ActiveTravelContract != null)
			{
				Action cleanup = delegate
				{
					this.uiManager.ResetFader(UIManagerRootType.PopupRoot);
					this.simState.Starmap.Screen.AllowInput(true);
				};
				this.simState.CreateBreakContractWarning(delegate
				{
					cleanup();
					this.simState.OnBreadcrumbCancelledByUser();
					this.simState.Starmap.SetActivePath();
					this.simState.SetSimRoomState(DropshipLocation.SHIP);
				}, cleanup);
				this.simState.Starmap.Screen.AllowInput(false);
				this.uiManager.SetFaderColor(this.uiManager.UILookAndColorConstants.PopupBackfill, UIManagerFader.FadePosition.FadeInBack, UIManagerRootType.PopupRoot, true);
				return;
			}
			this.simState.Starmap.SetActivePath();
			this.simState.SetSimRoomState(DropshipLocation.SHIP);
		}

		// Token: 0x06013DAE RID: 81326 RVA: 0x004FD738 File Offset: 0x004FB938
		public void FlashpointButtonResponse(Flashpoint FP)
		{
			StarSystem system = this.simState.Starmap.CurSelected.System;
			this.ShowFlashpointPopup(system);
		}

		// Token: 0x06013DAF RID: 81327 RVA: 0x004FD764 File Offset: 0x004FB964
		public void OnFlashpointAccepted()
		{
			Flashpoint FP = this.simState.GetFlashpointInSystem(this.simState.Starmap.CurSelected.System);
			this.FlashpointPopup = null;
			this.simState.Starmap.Screen.AllowInput(true);
			if (FP != null)
			{
				bool alreadyAtFlashpoint = FP.CurSystem == this.simState.CurSystem;
				if (this.simState.ActiveTravelContract != null)
				{
					Action cleanup = delegate
					{
						this.uiManager.ResetFader(UIManagerRootType.PopupRoot);
						this.simState.Starmap.Screen.AllowInput(true);
					};
					this.simState.CreateBreakContractWarning(delegate
					{
						cleanup();
						this.simState.OnBreadcrumbCancelledByUser();
						if (alreadyAtFlashpoint)
						{
							this.simState.SetActiveFlashpoint(FP);
						}
						else
						{
							this.simState.SetActiveFlashpoint(FP);
							this.simState.Starmap.SetActivePath();
						}
						this.simState.SetSimRoomState(DropshipLocation.SHIP);
					}, cleanup);
					this.simState.Starmap.Screen.AllowInput(false);
					this.uiManager.SetFaderColor(this.uiManager.UILookAndColorConstants.PopupBackfill, UIManagerFader.FadePosition.FadeInBack, UIManagerRootType.PopupRoot, true);
					return;
				}
				if (alreadyAtFlashpoint && this.simState.TravelState == SimGameTravelStatus.IN_SYSTEM)
				{
					if (FP != this.simState.ActiveFlashpoint)
					{
						this.simState.SetActiveFlashpoint(FP);
						return;
					}
					if (this.simState.ActiveFlashpoint.CurStatus == Flashpoint.Status.IN_PROGRESS)
					{
						this.simState.RoomManager.SetQueuedUIActivationID(DropshipMenuType.Contract, DropshipLocation.CMD_CENTER, true);
						this.simState.SetSimRoomState(DropshipLocation.CMD_CENTER);
						return;
					}
					if (this.simState.ActiveFlashpoint.CurStatus == Flashpoint.Status.SELECTED_ENROUTE)
					{
						this.simState.SetSimRoomState(DropshipLocation.SHIP);
						return;
					}
					this.simState.SetActiveFlashpoint(FP);
					return;
				}
				else
				{
					if (FP == this.simState.ActiveFlashpoint && this.simState.ActiveFlashpoint.CurStatus == Flashpoint.Status.SELECTED_ENROUTE)
					{
						this.simState.SetSimRoomState(DropshipLocation.SHIP);
						return;
					}
					if (!alreadyAtFlashpoint)
					{
						this.simState.Starmap.SetActivePath();
						this.simState.SetSimRoomState(DropshipLocation.SHIP);
					}
				}
			}
		}

		// Token: 0x06013DB0 RID: 81328 RVA: 0x004FD995 File Offset: 0x004FBB95
		public void OnFlashpointPopupCanceled()
		{
			this.simState.Starmap.Screen.AllowInput(true);
			this.FlashpointPopup = null;
		}

		// Token: 0x06013DB1 RID: 81329 RVA: 0x004FD9B4 File Offset: 0x004FBBB4
		public void ShowFlashpointPopup(StarSystem selectedSystem)
		{
			if (!base.Visible)
			{
				return;
			}
			this.simState.Starmap.Screen.AllowInput(false);
			List<Flashpoint> availableFlashpoints = UnityGameInstance.BattleTechGame.Simulation.AvailableFlashpoints;
			Flashpoint flashpointInSystem = UnityGameInstance.BattleTechGame.Simulation.GetFlashpointInSystem(selectedSystem);
			if (flashpointInSystem != null)
			{
				this.FlashpointPopup = LazySingletonBehavior<UIManager>.Instance.GetOrCreatePopupModule<SG_FlashpointInfoPopup>("", true);
				this.FlashpointPopup.SetData(this.simState, flashpointInSystem, new UnityAction(this.OnFlashpointAccepted), new UnityAction(this.OnFlashpointPopupCanceled));
			}
		}

		// Token: 0x06013DB2 RID: 81330 RVA: 0x004FDA44 File Offset: 0x004FBC44
		public void ResetDropdowns()
		{
			if (this.DifficultyDropdown != null)
			{
				this.DifficultyDropdown.value = -1;
				if (this.DifficultyDropdown.DropdownVisible)
				{
					this.DifficultyDropdown.Hide();
				}
			}
			if (this.BiomeDropdown != null)
			{
				this.BiomeDropdown.value = -1;
				if (this.BiomeDropdown.DropdownVisible)
				{
					this.BiomeDropdown.Hide();
				}
			}
			if (this.StoreDropdown != null)
			{
				this.StoreDropdown.value = -1;
				if (this.StoreDropdown.DropdownVisible)
				{
					this.StoreDropdown.Hide();
				}
			}
		}

		// Token: 0x06013DB3 RID: 81331 RVA: 0x004FDAE7 File Offset: 0x004FBCE7
		public void BuildDifficultyDropdown()
		{
			if (this.DifficultyDropdown == null)
			{
				return;
			}
			this.DifficultyDropdown.SetupDifficultyEntries();
			this.DifficultyDropdown.onValueChanged.AddListener(new UnityAction<int>(this.OnDifficultySelectionChanged));
			this.OnDifficultySelectionChanged(0);
		}

		// Token: 0x06013DB4 RID: 81332 RVA: 0x004FDB26 File Offset: 0x004FBD26
		public void BuildBiomeDropdown()
		{
			if (this.BiomeDropdown == null)
			{
				return;
			}
			this.BiomeDropdown.SetupBiomeEntries(this.simState);
			this.BiomeDropdown.SetSelectionCallback(new UnityAction<int>(this.OnBiomeSelectionChanged));
			this.OnBiomeSelectionChanged(0);
		}

		// Token: 0x06013DB5 RID: 81333 RVA: 0x004FDB66 File Offset: 0x004FBD66
		public void BuildStoreDropdown()
		{
			if (this.StoreDropdown == null)
			{
				return;
			}
			this.StoreDropdown.SetupStoreEntries(this.simState);
			this.StoreDropdown.SetSelectionCallback(new UnityAction<int>(this.OnStoreSelectionChanged));
		}

		// Token: 0x06013DB6 RID: 81334 RVA: 0x004FDBA0 File Offset: 0x004FBDA0
		public void LeaveRoomPopupCheck()
		{
			if (this.FlashpointPopup != null && this.FlashpointPopup.Visible)
			{
				this.FlashpointPopup.ForceClose();
				this.simState.Starmap.Screen.AllowInput(true);
			}
			if (this.TravelPopup != null && this.TravelPopup.Visible)
			{
				this.TravelPopup.Dismiss();
				this.simState.Starmap.Screen.AllowInput(true);
			}
		}

		// Token: 0x06013DB7 RID: 81335 RVA: 0x004FDC28 File Offset: 0x004FBE28
		private void OnViewContract()
		{
			List<Contract> contractsAtSystem = this.GetContractsAtSystem(this.simState.Starmap.CurSelected.System);
			if (contractsAtSystem.Count > 0)
			{
				this.simState.potentialTravelContract = contractsAtSystem[0];
			}
			this.simState.RoomManager.SetQueuedUIActivationID(DropshipMenuType.Contract, DropshipLocation.CMD_CENTER, true);
			this.simState.SetSimRoomState(DropshipLocation.CMD_CENTER);
		}

		// Token: 0x06013DB8 RID: 81336 RVA: 0x004FDC8C File Offset: 0x004FBE8C
		public void OnPointerDown(PointerEventData eventData)
		{
			this.waitForPointerUp = true;
			this.simState.Starmap.Screen.AllowInput(false);
		}

		// Token: 0x06013DB9 RID: 81337 RVA: 0x0000D184 File Offset: 0x0000B384
		public void OnPointerUp(PointerEventData eventData)
		{
		}

		// Token: 0x06013DBA RID: 81338 RVA: 0x004FDCAB File Offset: 0x004FBEAB
		public void OnPointerEnter(PointerEventData eventData)
		{
			if (!Input.GetMouseButton(0))
			{
				this.simState.Starmap.Screen.AllowInput(false);
			}
		}

		// Token: 0x06013DBB RID: 81339 RVA: 0x004FDCCC File Offset: 0x004FBECC
		public void OnPointerExit(PointerEventData eventData)
		{
			if (!this.waitForPointerUp && !this.TravelPopup.Visible)
			{
				if (this.FlashpointPopup != null && this.FlashpointPopup.Visible)
				{
					return;
				}
				this.simState.Starmap.Screen.AllowInput(true);
			}
		}

		// Token: 0x06013DBC RID: 81340 RVA: 0x004FDD20 File Offset: 0x004FBF20
		protected override void Update()
		{
			if (this.waitForPointerUp && Input.GetMouseButtonUp(0))
			{
				this.simState.Starmap.Screen.AllowInput(true);
				this.waitForPointerUp = false;
			}
		}

		// Token: 0x06013DBD RID: 81341 RVA: 0x004FDD50 File Offset: 0x004FBF50
		private void Dismiss()
		{
			if (this.BiomeDropdown.DropdownVisible || this.DifficultyDropdown.DropdownVisible || this.StoreDropdown.DropdownVisible)
			{
				base.StartCoroutine(this.DelayedDismiss(0.16f));
				return;
			}
			this.simState.RoomManager.NavRoom.ExitNavScreen();
		}

		// Token: 0x06013DBE RID: 81342 RVA: 0x004FDDAC File Offset: 0x004FBFAC
		public override bool HandleEscapeKeypress()
		{
			return base.ClickButtonIfVisible(this.BackButton);
		}

		// Token: 0x06013DBF RID: 81343 RVA: 0x004FDDBA File Offset: 0x004FBFBA
		private IEnumerator DelayedDismiss(float delay)
		{
			yield return new WaitForSeconds(delay);
			this.Dismiss();
			yield break;
		}

		// Token: 0x06013DC0 RID: 81344 RVA: 0x004FDDD0 File Offset: 0x004FBFD0
		public override bool HandleEnterKeypress()
		{
			return base.ClickButtonIfVisible(this.TravelButton);
		}

		// Token: 0x0400C88F RID: 51343
		private const string CALLOUT_PREFAB = "uixPrfIndc_NAV_locationInfoCalloutV2-Element";

		// Token: 0x0400C890 RID: 51344
		private const string FLASHPOINT_PREFAB = "uixPrfIndc_NAV_flashpoint-Element";

		// Token: 0x0400C891 RID: 51345
		[SerializeField]
		private HBSButton BackButton;

		// Token: 0x0400C892 RID: 51346
		[SerializeField]
		private SGSystemViewPopulator SystemViewPopulator;

		// Token: 0x0400C893 RID: 51347
		[SerializeField]
		private SGShipTransitStatus TransitStatusWidget;

		// Token: 0x0400C894 RID: 51348
		[SerializeField]
		private Transform FlyoutContainer;

		// Token: 0x0400C895 RID: 51349
		[SerializeField]
		private HBSButton TravelButton;

		// Token: 0x0400C896 RID: 51350
		[SerializeField]
		private GameObject ContractAvailableIndicator;

		// Token: 0x0400C897 RID: 51351
		[SerializeField]
		private GameObject TravelEnabledSection;

		// Token: 0x0400C898 RID: 51352
		[SerializeField]
		private GameObject TravelDisabledSection;

		// Token: 0x0400C899 RID: 51353
		[SerializeField]
		private LocalizableText TravelDisabledField;

		// Token: 0x0400C89A RID: 51354
		[SerializeField]
		private List<GameObject> InfoPanelsToHide = new List<GameObject>();

		// Token: 0x0400C89B RID: 51355
		[SerializeField]
		private SGFlashpointButtonController flashpointButtonController;

		// Token: 0x0400C89C RID: 51356
		[SerializeField]
		private GameObject FilterDropdownContainerObj;

		// Token: 0x0400C89D RID: 51357
		[SerializeField]
		private SGDifficultyDropdown DifficultyDropdown;

		// Token: 0x0400C89E RID: 51358
		[SerializeField]
		private SGBiomeDropdown BiomeDropdown;

		// Token: 0x0400C89F RID: 51359
		[SerializeField]
		private SGNavigationStoreDropdown StoreDropdown;

		// Token: 0x0400C8A0 RID: 51360
		[SerializeField]
		private HBSTooltip storeDropdownTooltip;

		// Token: 0x0400C8A1 RID: 51361
		private SGNavigationTravelPopup TravelPopup;

		// Token: 0x0400C8A2 RID: 51362
		private SG_FlashpointInfoPopup FlashpointPopup;

		// Token: 0x0400C8A3 RID: 51363
		private List<SGNavStarSystemCallout> AllCallouts = new List<SGNavStarSystemCallout>();

		// Token: 0x0400C8A4 RID: 51364
		private Dictionary<string, SGNavStarSystemCallout> CalloutsBySystem = new Dictionary<string, SGNavStarSystemCallout>();

		// Token: 0x0400C8A5 RID: 51365
		private List<SGNavStarSystemCallout> TravelCallouts = new List<SGNavStarSystemCallout>();

		// Token: 0x0400C8A6 RID: 51366
		private List<SGNavStarSystemCallout> DifficultyCallouts = new List<SGNavStarSystemCallout>();

		// Token: 0x0400C8A7 RID: 51367
		private List<string> specialIndicatorSystems = new List<string>();

		// Token: 0x0400C8A8 RID: 51368
		private SGNavStarSystemCallout CurrentSystemCallout;

		// Token: 0x0400C8A9 RID: 51369
		protected SGNavStarSystemCallout HoverCallout;

		// Token: 0x0400C8AA RID: 51370
		private SimGameState simState;

		// Token: 0x0400C8AB RID: 51371
		private bool waitForPointerUp;
	}
}
