using System;
using System.Collections.Generic;
using BattleTech.UI.Tooltips;
using UnityEngine;
using UnityEngine.UI;

namespace BattleTech.UI
{
	// Token: 0x0200205D RID: 8285
	public class SGNavigationActiveFactionWidget : MonoBehaviour
	{
		// Token: 0x06013D67 RID: 81255 RVA: 0x004FB934 File Offset: 0x004F9B34
		public void Init(SimGameState sim)
		{
			this.UpgradeToDataDrivenEnums();
			this.simState = sim;
			for (int i = 0; i < this.FactionIcons.Count; i++)
			{
				string text = this.FactionIDList[i];
				FactionDef factionDef = this.simState.GetFactionDef(text);
				if (factionDef != null)
				{
					this.FactionIcons[i].sprite = factionDef.GetSprite();
					HBSTooltip component = this.FactionIcons[i].GetComponent<HBSTooltip>();
					if (component != null)
					{
						component.SetDefaultStateData(TooltipUtilities.GetStateDataFromObject(factionDef));
					}
				}
			}
			this.DeactivateAllFactions();
		}

		// Token: 0x06013D68 RID: 81256 RVA: 0x004FB9C8 File Offset: 0x004F9BC8
		private void UpgradeToDataDrivenEnums()
		{
			if (this.FactionIDList == null || this.FactionIDList.Count == 0)
			{
				this.FactionIDList = new List<string>(this.FactionsList.Count);
				for (int i = 0; i < this.FactionsList.Count; i++)
				{
					this.FactionIDList.Add(this.FactionsList[i].ToString());
				}
			}
		}

		// Token: 0x06013D69 RID: 81257 RVA: 0x004FBA3B File Offset: 0x004F9C3B
		public void DeactivateAllFactions()
		{
			this.FactionButtons.ForEach(delegate(HBSButton btn)
			{
				btn.SetState(ButtonState.Disabled, false);
			});
		}

		// Token: 0x06013D6A RID: 81258 RVA: 0x004FBA68 File Offset: 0x004F9C68
		public void ActivateFactions(List<string> activeFactions, string OwnerFaction)
		{
			this.FactionButtons.ForEach(delegate(HBSButton button)
			{
				int num = this.FactionButtons.IndexOf(button);
				string text = this.FactionIDList[num];
				FactionValue factionByName = FactionEnumeration.GetFactionByName(text);
				bool flag;
				if (factionByName.IsAuriganRestoration)
				{
					flag = this.simState.displayedFactions.Contains(text);
				}
				else
				{
					flag = !factionByName.IsCareerIgnoredContractTarget || !this.simState.IsCareerMode();
				}
				button.gameObject.SetActive(flag);
				if (flag)
				{
					if (activeFactions.Contains(text) || text == OwnerFaction)
					{
						button.SetState(ButtonState.Enabled, false);
						return;
					}
					button.SetState(ButtonState.Disabled, false);
				}
			});
		}

		// Token: 0x06013D6B RID: 81259 RVA: 0x004FBAA8 File Offset: 0x004F9CA8
		private void OnDestroy()
		{
			if (this.FactionIcons == null)
			{
				return;
			}
			for (int i = 0; i < this.FactionIcons.Count; i++)
			{
				this.FactionIcons[i].sprite = null;
			}
		}

		// Token: 0x0400C870 RID: 51312
		[SerializeField]
		[HideInInspector]
		[Tooltip("List of all factions that are tied to the Faction Buttons (order is important)")]
		private List<Faction> FactionsList;

		// Token: 0x0400C871 RID: 51313
		[SerializeField]
		[Tooltip("List of all factions that are tied to the Faction Buttons (order is important)")]
		private List<string> FactionIDList = new List<string>();

		// Token: 0x0400C872 RID: 51314
		[SerializeField]
		[Tooltip("Buttons used to trigger Enabled or Disabled states for each faction (order correlates to Faction List)")]
		private List<HBSButton> FactionButtons;

		// Token: 0x0400C873 RID: 51315
		[SerializeField]
		[Tooltip("List of all faction image slots (order correlates to Factions List)")]
		private List<Image> FactionIcons;

		// Token: 0x0400C874 RID: 51316
		private SimGameState simState;
	}
}
