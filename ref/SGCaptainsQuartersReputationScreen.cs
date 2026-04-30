using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BattleTech.UI
{
	// Token: 0x02001FBE RID: 8126
	[UIModule.PrefabNameAttr("uixPrfPanl_captainsQuarters_Reputation-Panel_V2")]
	public class SGCaptainsQuartersReputationScreen : UIModule
	{
		// Token: 0x060137B8 RID: 79800 RVA: 0x004E1F24 File Offset: 0x004E0124
		public void Init(SimGameState sim)
		{
			this.simState = sim;
			this.ButtonExit.OnClicked.RemoveAllListeners();
			this.ButtonExit.OnClicked.AddListener(new UnityAction(this.Dismiss));
			this.MRBFillBarWidget.InitData(null, this.simState);
			if (this.repScaleTooltips != null)
			{
				this.repScaleTooltips.SetAllTooltips(sim);
			}
			this.RefreshWidgets();
		}

		// Token: 0x060137B9 RID: 79801 RVA: 0x004E1F98 File Offset: 0x004E0198
		public void RefreshWidgets()
		{
			this.MRBFillBarWidget.FillInData();
			FactionValue invalidUnsetFactionValue = FactionEnumeration.GetInvalidUnsetFactionValue();
			foreach (SGFactionReputationWidget sgfactionReputationWidget in this.FactionPanelWidgets)
			{
				sgfactionReputationWidget.Init(this.simState, invalidUnsetFactionValue, null, false);
				sgfactionReputationWidget.gameObject.SetActive(false);
			}
			List<string> list = new List<string>();
			string text = Faction.AuriganRestoration.ToString();
			foreach (string text2 in this.simState.displayedFactions)
			{
				if (!(text2 == text))
				{
					list.Add(text2);
				}
			}
			int num = Mathf.Min(list.Count, this.FactionPanelWidgets.Count);
			for (int i = 0; i < num; i++)
			{
				SGFactionReputationWidget sgfactionReputationWidget2 = this.FactionPanelWidgets[i];
				sgfactionReputationWidget2.gameObject.SetActive(true);
				string text3 = list[i];
				FactionDef factionDef = this.simState.GetFactionDef(text3);
				if (factionDef != null)
				{
					sgfactionReputationWidget2.Init(this.simState, factionDef.FactionValue, new UnityAction(this.RefreshWidgets), false);
				}
			}
			if (this.simState.displayedFactions.Contains(text))
			{
				this.AuriganPanelWidget.gameObject.SetActive(true);
				this.AuriganPanelWidget.Init(this.simState, FactionEnumeration.GetAuriganRestorationFactionValue(), new UnityAction(this.RefreshWidgets), false);
				return;
			}
			this.AuriganPanelWidget.gameObject.SetActive(false);
		}

		// Token: 0x060137BA RID: 79802 RVA: 0x004E2158 File Offset: 0x004E0358
		public override bool HandleEscapeKeypress()
		{
			this.Dismiss();
			return true;
		}

		// Token: 0x060137BB RID: 79803 RVA: 0x004E2158 File Offset: 0x004E0358
		public override bool HandleEnterKeypress()
		{
			this.Dismiss();
			return true;
		}

		// Token: 0x060137BC RID: 79804 RVA: 0x004E2161 File Offset: 0x004E0361
		private void Dismiss()
		{
			this.uiManager.ResetFader(UIManagerRootType.UIRoot);
			this.simState.RoomManager.CptQuartersRoom.OnMenuClosed();
			base.Visible = false;
		}

		// Token: 0x0400C36B RID: 50027
		[SerializeField]
		protected string CampaignCompleteTag = "story_complete";

		// Token: 0x0400C36C RID: 50028
		[Header("======= MRB Widget =======")]
		[SerializeField]
		private AAR_MercBoardReputationWidget MRBFillBarWidget;

		// Token: 0x0400C36D RID: 50029
		[Header("======= Factions Widgets =======")]
		[SerializeField]
		private List<SGFactionReputationWidget> FactionPanelWidgets = new List<SGFactionReputationWidget>();

		// Token: 0x0400C36E RID: 50030
		[SerializeField]
		private SGFactionReputationWidget AuriganPanelWidget;

		// Token: 0x0400C36F RID: 50031
		[Header("======= TooltipStuff =======")]
		[SerializeField]
		private ReputationScaleTooltips repScaleTooltips;

		// Token: 0x0400C370 RID: 50032
		[Header("======= Screen Buttons =======")]
		[SerializeField]
		private HBSButton ButtonExit;

		// Token: 0x0400C371 RID: 50033
		private SimGameState simState;
	}
}
