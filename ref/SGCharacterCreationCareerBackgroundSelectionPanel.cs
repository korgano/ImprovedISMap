using System;
using System.Collections.Generic;
using BattleTech.Data;
using BattleTech.UI.TMProWrapper;
using HBS;
using Localize;
using UnityEngine;
using UnityEngine.Events;

namespace BattleTech.UI
{
	// Token: 0x02001FE7 RID: 8167
	[UIModule.PrefabNameAttr("uixPrfScrn_characterCreationCareerModeBackground-Screen")]
	public class SGCharacterCreationCareerBackgroundSelectionPanel : SGCharacterCreationBackgroundSelectionPanel
	{
		// Token: 0x17004229 RID: 16937
		// (get) Token: 0x060138CF RID: 80079 RVA: 0x004E7062 File Offset: 0x004E5262
		public string SummaryTitle
		{
			get
			{
				return this.summaryTitle;
			}
		}

		// Token: 0x1700422A RID: 16938
		// (get) Token: 0x060138D0 RID: 80080 RVA: 0x004E706A File Offset: 0x004E526A
		public string SummaryText
		{
			get
			{
				return this.summaryText;
			}
		}

		// Token: 0x1700422B RID: 16939
		// (get) Token: 0x060138D1 RID: 80081 RVA: 0x004E7072 File Offset: 0x004E5272
		private SGCharacterCreationWidget characterCreationWidget
		{
			get
			{
				if (!this._widget)
				{
					this._widget = Object.FindObjectOfType<SGCharacterCreationWidget>();
				}
				return this._widget;
			}
		}

		// Token: 0x1700422C RID: 16940
		// (get) Token: 0x060138D2 RID: 80082 RVA: 0x004E7092 File Offset: 0x004E5292
		private SGCharacterCreationNameAndAppearanceScreen nameAndAppearanceScreen
		{
			get
			{
				if (!this._nameAndAppearanceScreen)
				{
					this._nameAndAppearanceScreen = this.uiManager.GetOrCreateUIModule<SGCharacterCreationNameAndAppearanceScreen>("", true);
				}
				return this._nameAndAppearanceScreen;
			}
		}

		// Token: 0x060138D3 RID: 80083 RVA: 0x004E70C0 File Offset: 0x004E52C0
		public override void ReceiveButtonPress(string button)
		{
			if (button == "editNameAndAppearance")
			{
				this.characterCreationWidget.GoToPortraitCustomization();
				this.nameAndAppearanceScreen.RefreshRenderedPortrait();
				if (this.nameAndAppearanceScreen.isCustomized)
				{
					this.nameAndAppearanceScreen.PortraitSelection.ClearSelection();
				}
			}
		}

		// Token: 0x060138D4 RID: 80084 RVA: 0x004E7110 File Offset: 0x004E5310
		public override void Done()
		{
			SimGameResultAction simGameResultAction = new SimGameResultAction();
			simGameResultAction.Type = SimGameResultAction.ActionType.System_ShowSummaryOverlay;
			simGameResultAction.value = Strings.T("*A Mercenary's Career*");
			simGameResultAction.additionalValues = new string[1];
			simGameResultAction.additionalValues[0] = Strings.T("You have everything an industrious mercenary commander could want: a crew of talented misfits, a lance of ancient BattleMechs, and a derelict cargo ship to call home.\n\n It’s one thing to finish a mission in one piece, more or less. But it’s another to keep your MechWarriors happy and healthy, your ‘Mechs patched up and ready for action, and your balance sheet firmly in the black.\n\n As the rumors of another war of succession intensify, the Mercenary Review Board announces new evaluation protocols to assess the mercenary companies of the Periphery... with a score.");
			SimGameState.ApplyEventAction(simGameResultAction, null);
			base.Done();
		}

		// Token: 0x060138D5 RID: 80085 RVA: 0x004E7168 File Offset: 0x004E5368
		private void UpdatePilotSummary()
		{
			Pilot pilot = this.characterCreationWidget.CreatePilot();
			this.summary.Summarize(pilot.pilotDef, this.dataManager, base.playerBackground);
		}

		// Token: 0x060138D6 RID: 80086 RVA: 0x004E719E File Offset: 0x004E539E
		public override void SetupQuestions(DataManager dataManager)
		{
			base.SetupQuestions(dataManager);
			this.SetUpDropdowns();
			LoadingCurtain.Show();
			base.SetupBackground();
			base.StartCoroutine(this.characterCreationWidget.RandomlyGenerateNameAndAppearanceAndThen(delegate
			{
				LoadingCurtain.Hide(true);
				this.UpdatePilotSummary();
				base.Visible = true;
			}));
		}

		// Token: 0x060138D7 RID: 80087 RVA: 0x004E71D8 File Offset: 0x004E53D8
		private void SetUpDropdowns()
		{
			for (int i = 0; i < this.dropdowns.Length; i++)
			{
				int num = this.choiceQuestions[i];
				BackgroundQuestion backgroundQuestion = this.questions[num];
				List<string> list = new List<string>();
				foreach (BackgroundQuestionOption backgroundQuestionOption in backgroundQuestion.choices)
				{
					list.Add(backgroundQuestionOption.text);
				}
				this.dropdowns[i].dropdown.ClearOptions();
				this.dropdowns[i].dropdown.AddOptions(list);
				this.dropdowns[i].options = backgroundQuestion.choices.ToArray();
				this.dropdowns[i].SetPanel(this);
				this.dropdowns[i].questionText.SetText(backgroundQuestion.premise);
				this.dropdowns[i].dropdown.onValueChanged.AddListener(new UnityAction<int>(this.dropdowns[i].SelectOption));
			}
			for (int j = 0; j < this.dropdowns.Length; j++)
			{
				this.dropdowns[j].SelectOption(this.dropdowns[j].dropdown.value);
				this.dropdowns[j].onSelectionChanged = delegate
				{
					this.summary.Summarize(this.onPilotUpdated().pilotDef, this.dataManager, base.playerBackground);
				};
			}
		}

		// Token: 0x060138D8 RID: 80088 RVA: 0x004E7348 File Offset: 0x004E5548
		public override bool HandleEnterKeypress()
		{
			this.summary.ReceiveButtonPress("confirm");
			return true;
		}

		// Token: 0x060138D9 RID: 80089 RVA: 0x004E735B File Offset: 0x004E555B
		public override bool HandleEscapeKeypress()
		{
			base.GetComponentInChildren<OverlayMenu>().ReceiveButtonPress("Quit");
			return true;
		}

		// Token: 0x0400C49C RID: 50332
		[SerializeField]
		protected SGCharacterCreationCareerBackgroundSelectionPanel.BackgroundDropdown[] dropdowns;

		// Token: 0x0400C49D RID: 50333
		public SGCharacterCreationSummaryScreen summary;

		// Token: 0x0400C49E RID: 50334
		[SerializeField]
		protected string summaryTitle;

		// Token: 0x0400C49F RID: 50335
		[SerializeField]
		protected string summaryText;

		// Token: 0x0400C4A0 RID: 50336
		private SGCharacterCreationWidget _widget;

		// Token: 0x0400C4A1 RID: 50337
		private SGCharacterCreationNameAndAppearanceScreen _nameAndAppearanceScreen;

		// Token: 0x0400C4A2 RID: 50338
		public Func<Pilot> onPilotUpdated;

		// Token: 0x02001FE8 RID: 8168
		[Serializable]
		public class BackgroundDropdown
		{
			// Token: 0x060138DD RID: 80093 RVA: 0x004E73AC File Offset: 0x004E55AC
			public void SetPanel(SGCharacterCreationCareerBackgroundSelectionPanel p)
			{
				this.panel = p;
			}

			// Token: 0x060138DE RID: 80094 RVA: 0x004E73B8 File Offset: 0x004E55B8
			public void SelectOption(int optionNum)
			{
				this.options[optionNum].PlayQuestionChoiceMusicTrigger();
				this.backgroundPanel.SetData(this.options[optionNum].bgDef, SceneSingletonBehavior<UnityGameInstance>.Instance.Game.DataManager);
				this.options[optionNum].ApplyTo(this.panel.playerBackground);
				if (this.onSelectionChanged != null)
				{
					this.onSelectionChanged();
				}
			}

			// Token: 0x0400C4A3 RID: 50339
			public HBS_Dropdown dropdown;

			// Token: 0x0400C4A4 RID: 50340
			public LocalizableText questionText;

			// Token: 0x0400C4A5 RID: 50341
			public CharacterDescriptionBackgroundPanel backgroundPanel;

			// Token: 0x0400C4A6 RID: 50342
			public BackgroundQuestionOption[] options;

			// Token: 0x0400C4A7 RID: 50343
			public Action onSelectionChanged;

			// Token: 0x0400C4A8 RID: 50344
			private SGCharacterCreationCareerBackgroundSelectionPanel panel;
		}
	}
}
