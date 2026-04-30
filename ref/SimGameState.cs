using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BattleTech.Data;
using BattleTech.Framework;
using BattleTech.Portraits;
using BattleTech.Save;
using BattleTech.Save.SaveGameStructure;
using BattleTech.Save.Test;
using BattleTech.Serialization;
using BattleTech.StringInterpolation;
using BattleTech.UI;
using BattleTech.UI.Tooltips;
using fastJSON;
using HBS;
using HBS.Collections;
using HBS.Logging;
using isogame;
using Localize;
using SVGImporter;
using UnityEngine;
using UnityEngine.Events;

namespace BattleTech
{
	// Token: 0x020010C9 RID: 4297
	[SerializableContract("SimGameState")]
	public class SimGameState
	{
		// Token: 0x1700193A RID: 6458
		// (get) Token: 0x06009042 RID: 36930 RVA: 0x0024ED4B File Offset: 0x0024CF4B
		public bool TimeMoving
		{
			get
			{
				return this.CurRoomState == DropshipLocation.SHIP && this.RoomManager.TimeMoving() && this.canTimeElapse;
			}
		}

		// Token: 0x06009043 RID: 36931 RVA: 0x0024ED6C File Offset: 0x0024CF6C
		public DateTime GetCampaignStartDate()
		{
			if (this.privateCampaignStartDate == null)
			{
				DateTime dateTime = SimGameState.year3025;
				DateTime.TryParse(this.campaignStartDate, out dateTime);
				this.privateCampaignStartDate = new DateTime?(dateTime);
			}
			return this.privateCampaignStartDate.Value;
		}

		// Token: 0x06009044 RID: 36932 RVA: 0x0024EDB1 File Offset: 0x0024CFB1
		public void SetCampaignStartDate(DateTime campaignStartDate)
		{
			this.privateCampaignStartDate = new DateTime?(campaignStartDate);
			this.campaignStartDate = campaignStartDate.ToString();
		}

		// Token: 0x06009045 RID: 36933 RVA: 0x0024EDCC File Offset: 0x0024CFCC
		public void SetCampaignStartDate(string campaignStartDate)
		{
			DateTime dateTime;
			if (!DateTime.TryParse(campaignStartDate, out dateTime))
			{
				dateTime = SimGameState.year3025;
			}
			this.privateCampaignStartDate = new DateTime?(dateTime);
			campaignStartDate = dateTime.ToString();
		}

		// Token: 0x1700193B RID: 6459
		// (get) Token: 0x06009046 RID: 36934 RVA: 0x0024EDFE File Offset: 0x0024CFFE
		// (set) Token: 0x06009047 RID: 36935 RVA: 0x0024EE06 File Offset: 0x0024D006
		public int DaysPassed
		{
			get
			{
				return this.daysPassed;
			}
			private set
			{
				this.daysPassed = value;
				this.companyStats.Set<int>("Day", this.daysPassed);
			}
		}

		// Token: 0x1700193C RID: 6460
		// (get) Token: 0x06009048 RID: 36936 RVA: 0x0024EE28 File Offset: 0x0024D028
		public DateTime CurrentDate
		{
			get
			{
				return this.GetCampaignStartDate().AddDays((double)this.DaysPassed);
			}
		}

		// Token: 0x1700193D RID: 6461
		// (get) Token: 0x06009049 RID: 36937 RVA: 0x0024EE4A File Offset: 0x0024D04A
		// (set) Token: 0x0600904A RID: 36938 RVA: 0x0024EE52 File Offset: 0x0024D052
		public StarSystem CurSystem { get; private set; }

		// Token: 0x1700193E RID: 6462
		// (get) Token: 0x0600904B RID: 36939 RVA: 0x0024EE5B File Offset: 0x0024D05B
		public HeraldryDef Player2sMercUnitHeraldryDef
		{
			get
			{
				return this.Player1sMercUnitHeraldryDef;
			}
		}

		// Token: 0x1700193F RID: 6463
		// (get) Token: 0x0600904C RID: 36940 RVA: 0x0024EE63 File Offset: 0x0024D063
		public List<string> PurchasedArgoUpgrades
		{
			get
			{
				return this.purchasedArgoUpgrades;
			}
		}

		// Token: 0x17001940 RID: 6464
		// (get) Token: 0x0600904D RID: 36941 RVA: 0x0024EE6B File Offset: 0x0024D06B
		public List<ShipModuleUpgrade> ShipUpgrades
		{
			get
			{
				return this.shipUpgrades;
			}
		}

		// Token: 0x17001941 RID: 6465
		// (get) Token: 0x0600904E RID: 36942 RVA: 0x0024EE73 File Offset: 0x0024D073
		// (set) Token: 0x0600904F RID: 36943 RVA: 0x0024EE7B File Offset: 0x0024D07B
		public int DayRemainingInQuarter { get; private set; }

		// Token: 0x17001942 RID: 6466
		// (get) Token: 0x06009050 RID: 36944 RVA: 0x0024EE84 File Offset: 0x0024D084
		// (set) Token: 0x06009051 RID: 36945 RVA: 0x0024EE8C File Offset: 0x0024D08C
		public List<TechDef> MechTechs { get; private set; }

		// Token: 0x17001943 RID: 6467
		// (get) Token: 0x06009052 RID: 36946 RVA: 0x0024EE95 File Offset: 0x0024D095
		// (set) Token: 0x06009053 RID: 36947 RVA: 0x0024EE9D File Offset: 0x0024D09D
		public List<TechDef> MedTechs { get; private set; }

		// Token: 0x17001944 RID: 6468
		// (get) Token: 0x06009054 RID: 36948 RVA: 0x0024EEA6 File Offset: 0x0024D0A6
		// (set) Token: 0x06009055 RID: 36949 RVA: 0x0024EEAE File Offset: 0x0024D0AE
		public List<WorkOrderEntry> MechLabQueue { get; private set; }

		// Token: 0x17001945 RID: 6469
		// (get) Token: 0x06009056 RID: 36950 RVA: 0x0024EEB7 File Offset: 0x0024D0B7
		// (set) Token: 0x06009057 RID: 36951 RVA: 0x0024EEBF File Offset: 0x0024D0BF
		public WorkOrderEntry_Notification FinancialReportNotification { get; private set; }

		// Token: 0x17001946 RID: 6470
		// (get) Token: 0x06009058 RID: 36952 RVA: 0x0024EEC8 File Offset: 0x0024D0C8
		// (set) Token: 0x06009059 RID: 36953 RVA: 0x0024EED0 File Offset: 0x0024D0D0
		public DropshipType CurDropship { get; private set; }

		// Token: 0x17001947 RID: 6471
		// (get) Token: 0x0600905A RID: 36954 RVA: 0x0024EED9 File Offset: 0x0024D0D9
		// (set) Token: 0x0600905B RID: 36955 RVA: 0x0024EEE1 File Offset: 0x0024D0E1
		public SimGameState.SimGameType SimGameMode { get; private set; }

		// Token: 0x17001948 RID: 6472
		// (get) Token: 0x0600905C RID: 36956 RVA: 0x0024EEEA File Offset: 0x0024D0EA
		public List<string> UsedRoninIDs
		{
			get
			{
				return this.usedRoninIDs;
			}
		}

		// Token: 0x17001949 RID: 6473
		// (get) Token: 0x0600905D RID: 36957 RVA: 0x0024EEF2 File Offset: 0x0024D0F2
		public List<string> IgnoredContractEmployers
		{
			get
			{
				return this.ignoredContractEmployers;
			}
		}

		// Token: 0x1700194A RID: 6474
		// (get) Token: 0x0600905E RID: 36958 RVA: 0x0024EEFA File Offset: 0x0024D0FA
		public List<string> IgnoredContractTargets
		{
			get
			{
				return this.ignoredContractTargets;
			}
		}

		// Token: 0x1700194B RID: 6475
		// (get) Token: 0x0600905F RID: 36959 RVA: 0x0024EF02 File Offset: 0x0024D102
		public Dictionary<string, StarSystem> StarSystemDictionary
		{
			get
			{
				return this.starDict;
			}
		}

		// Token: 0x1700194C RID: 6476
		// (get) Token: 0x06009060 RID: 36960 RVA: 0x0024EF0A File Offset: 0x0024D10A
		public Dictionary<string, List<StarSystem>> FactionStoreStarSystemsDictionary
		{
			get
			{
				return this.factStoreDict;
			}
		}

		// Token: 0x1700194D RID: 6477
		// (get) Token: 0x06009061 RID: 36961 RVA: 0x0024EF12 File Offset: 0x0024D112
		// (set) Token: 0x06009062 RID: 36962 RVA: 0x0024EF1A File Offset: 0x0024D11A
		public Dictionary<EconomyScale, int> ExpenditureMoraleValue { get; private set; }

		// Token: 0x1700194E RID: 6478
		// (get) Token: 0x06009063 RID: 36963 RVA: 0x0024EF23 File Offset: 0x0024D123
		// (set) Token: 0x06009064 RID: 36964 RVA: 0x0024EF2B File Offset: 0x0024D12B
		public Contract CompletedContract { get; private set; }

		// Token: 0x1700194F RID: 6479
		// (get) Token: 0x06009065 RID: 36965 RVA: 0x0024EF34 File Offset: 0x0024D134
		// (set) Token: 0x06009066 RID: 36966 RVA: 0x0024EF3C File Offset: 0x0024D13C
		public Contract PendingMilestoneContract { get; private set; }

		// Token: 0x17001950 RID: 6480
		// (get) Token: 0x06009067 RID: 36967 RVA: 0x0024EF45 File Offset: 0x0024D145
		// (set) Token: 0x06009068 RID: 36968 RVA: 0x0024EF4D File Offset: 0x0024D14D
		public bool IsPendingMilestoneContractBreadcrumb { get; private set; }

		// Token: 0x17001951 RID: 6481
		// (get) Token: 0x06009069 RID: 36969 RVA: 0x0024EF56 File Offset: 0x0024D156
		// (set) Token: 0x0600906A RID: 36970 RVA: 0x0024EF5E File Offset: 0x0024D15E
		public DropshipLocation CurRoomState { get; private set; }

		// Token: 0x17001952 RID: 6482
		// (get) Token: 0x0600906B RID: 36971 RVA: 0x0024EF67 File Offset: 0x0024D167
		// (set) Token: 0x0600906C RID: 36972 RVA: 0x0024EF6F File Offset: 0x0024D16F
		public Dictionary<string, Dictionary<int, List<AbilityDef>>> AbilityTree { get; private set; }

		// Token: 0x17001953 RID: 6483
		// (get) Token: 0x0600906D RID: 36973 RVA: 0x0024EF78 File Offset: 0x0024D178
		// (set) Token: 0x0600906E RID: 36974 RVA: 0x0024EF80 File Offset: 0x0024D180
		public SimGameSubstitutionListDef SimGameSubtitutions { get; private set; }

		// Token: 0x17001954 RID: 6484
		// (get) Token: 0x0600906F RID: 36975 RVA: 0x0024EF89 File Offset: 0x0024D189
		public Dictionary<SimGameCrew, CastDef> Crew
		{
			get
			{
				return this._crewDefs;
			}
		}

		// Token: 0x17001955 RID: 6485
		// (get) Token: 0x06009070 RID: 36976 RVA: 0x0024EF91 File Offset: 0x0024D191
		// (set) Token: 0x06009071 RID: 36977 RVA: 0x0024EF99 File Offset: 0x0024D199
		public BaseDescriptionDef PriorityMissionDescription { get; private set; }

		// Token: 0x17001956 RID: 6486
		// (get) Token: 0x06009072 RID: 36978 RVA: 0x0024EFA2 File Offset: 0x0024D1A2
		public string PriorityMissionTitle
		{
			get
			{
				if (this.PriorityMissionDescription != null)
				{
					return this.PriorityMissionDescription.Name;
				}
				return "Priority Mission";
			}
		}

		// Token: 0x17001957 RID: 6487
		// (get) Token: 0x06009073 RID: 36979 RVA: 0x0024EFBD File Offset: 0x0024D1BD
		public string TravelContractTitle
		{
			get
			{
				return "Travel Contract";
			}
		}

		// Token: 0x17001958 RID: 6488
		// (get) Token: 0x06009074 RID: 36980 RVA: 0x0024EFC4 File Offset: 0x0024D1C4
		public Dictionary<long, BaseDescriptionDef> ContractTypeDescriptions
		{
			get
			{
				return this._contractTypeDescriptions;
			}
		}

		// Token: 0x17001959 RID: 6489
		// (get) Token: 0x06009075 RID: 36981 RVA: 0x0024EFCC File Offset: 0x0024D1CC
		public List<PilotDef> RoninPilots
		{
			get
			{
				return this._roninPilots;
			}
		}

		// Token: 0x1700195A RID: 6490
		// (get) Token: 0x06009076 RID: 36982 RVA: 0x0024EFD4 File Offset: 0x0024D1D4
		public SimGameInterruptManager InterruptQueue
		{
			get
			{
				return this.interruptQueue;
			}
		}

		// Token: 0x1700195B RID: 6491
		// (get) Token: 0x06009077 RID: 36983 RVA: 0x0024EFDC File Offset: 0x0024D1DC
		public int CurResolvePerTurn
		{
			get
			{
				if (this.CombatConstants == null || this.CombatConstants.MoraleConstants.BaselineAddFromSimGameThresholds == null)
				{
					return 0;
				}
				MoraleConstantsDef moraleConstants = this.CombatConstants.MoraleConstants;
				for (int i = moraleConstants.BaselineAddFromSimGameThresholds.Length - 1; i >= 0; i--)
				{
					if (this.Morale >= moraleConstants.BaselineAddFromSimGameThresholds[i])
					{
						return moraleConstants.BaselineAddFromSimGameValues[i];
					}
				}
				return 0;
			}
		}

		// Token: 0x1700195C RID: 6492
		// (get) Token: 0x06009078 RID: 36984 RVA: 0x0024F040 File Offset: 0x0024D240
		public List<StarSystem> StarSystems
		{
			get
			{
				return this.starSystems;
			}
		}

		// Token: 0x1700195D RID: 6493
		// (get) Token: 0x06009079 RID: 36985 RVA: 0x0024F048 File Offset: 0x0024D248
		public Pilot Commander
		{
			get
			{
				return this.commander;
			}
		}

		// Token: 0x1700195E RID: 6494
		// (get) Token: 0x0600907A RID: 36986 RVA: 0x0024F050 File Offset: 0x0024D250
		public TagSet CommanderTags
		{
			get
			{
				return this.commander.pilotDef.PilotTags;
			}
		}

		// Token: 0x1700195F RID: 6495
		// (get) Token: 0x0600907B RID: 36987 RVA: 0x0024F062 File Offset: 0x0024D262
		public StatCollection CommanderStats
		{
			get
			{
				return this.commander.StatCollection;
			}
		}

		// Token: 0x17001960 RID: 6496
		// (get) Token: 0x0600907C RID: 36988 RVA: 0x0024F06F File Offset: 0x0024D26F
		public TagSet CompanyTags
		{
			get
			{
				return this.companyTags;
			}
		}

		// Token: 0x17001961 RID: 6497
		// (get) Token: 0x0600907D RID: 36989 RVA: 0x0024F077 File Offset: 0x0024D277
		public StatCollection CompanyStats
		{
			get
			{
				return this.companyStats;
			}
		}

		// Token: 0x17001962 RID: 6498
		// (get) Token: 0x0600907E RID: 36990 RVA: 0x0024F07F File Offset: 0x0024D27F
		public int Funds
		{
			get
			{
				return this.companyStats.GetValue<int>("Funds");
			}
		}

		// Token: 0x17001963 RID: 6499
		// (get) Token: 0x0600907F RID: 36991 RVA: 0x0024F091 File Offset: 0x0024D291
		public int MissionsAttempted
		{
			get
			{
				return this.companyStats.GetValue<int>("COMPANY_MissionsAttempted");
			}
		}

		// Token: 0x17001964 RID: 6500
		// (get) Token: 0x06009080 RID: 36992 RVA: 0x0024F0A3 File Offset: 0x0024D2A3
		public float GlobalDifficulty
		{
			get
			{
				return Mathf.Min(this.companyStats.GetValue<float>("Difficulty") + (float)this.Constants.Story.ContractDifficultyMod, this.Constants.Story.GlobalContractDifficultyMax);
			}
		}

		// Token: 0x17001965 RID: 6501
		// (get) Token: 0x06009081 RID: 36993 RVA: 0x0024F0DC File Offset: 0x0024D2DC
		public int StartingQuarterFunds
		{
			get
			{
				return this.companyStats.GetValue<int>("COMPANY_MonthlyStartingFunds");
			}
		}

		// Token: 0x17001966 RID: 6502
		// (get) Token: 0x06009082 RID: 36994 RVA: 0x0024F0EE File Offset: 0x0024D2EE
		public int StartingQuarterMorale
		{
			get
			{
				return this.companyStats.GetValue<int>("COMPANY_MonthlyStartingMorale");
			}
		}

		// Token: 0x17001967 RID: 6503
		// (get) Token: 0x06009083 RID: 36995 RVA: 0x0024F100 File Offset: 0x0024D300
		public int TravelTime
		{
			get
			{
				return this.companyStats.GetValue<int>("TravelTime");
			}
		}

		// Token: 0x17001968 RID: 6504
		// (get) Token: 0x06009084 RID: 36996 RVA: 0x0024F112 File Offset: 0x0024D312
		public EconomyScale ExpenditureLevel
		{
			get
			{
				return (EconomyScale)this.companyStats.GetValue<int>("ExpenseLevel");
			}
		}

		// Token: 0x17001969 RID: 6505
		// (get) Token: 0x06009085 RID: 36997 RVA: 0x0024F124 File Offset: 0x0024D324
		public int Morale
		{
			get
			{
				return this.companyStats.GetValue<int>("Morale");
			}
		}

		// Token: 0x1700196A RID: 6506
		// (get) Token: 0x06009086 RID: 36998 RVA: 0x0024F136 File Offset: 0x0024D336
		public int MedTechSkill
		{
			get
			{
				return this.companyStats.GetValue<int>("MedTechSkill");
			}
		}

		// Token: 0x1700196B RID: 6507
		// (get) Token: 0x06009087 RID: 36999 RVA: 0x0024F148 File Offset: 0x0024D348
		public int MechTechSkill
		{
			get
			{
				return this.companyStats.GetValue<int>("MechTechSkill");
			}
		}

		// Token: 0x1700196C RID: 6508
		// (get) Token: 0x06009088 RID: 37000 RVA: 0x0024F15A File Offset: 0x0024D35A
		public int DailyUpgradeValue
		{
			get
			{
				return this.companyStats.GetValue<int>("UpgradeValue");
			}
		}

		// Token: 0x1700196D RID: 6509
		// (get) Token: 0x06009089 RID: 37001 RVA: 0x0024F16C File Offset: 0x0024D36C
		public Contract ActiveTravelContract
		{
			get
			{
				return this.activeBreadcrumb;
			}
		}

		// Token: 0x1700196E RID: 6510
		// (get) Token: 0x0600908A RID: 37002 RVA: 0x0024F174 File Offset: 0x0024D374
		public bool HasTravelContract
		{
			get
			{
				return this.activeBreadcrumb != null;
			}
		}

		// Token: 0x1700196F RID: 6511
		// (get) Token: 0x0600908B RID: 37003 RVA: 0x0024F17F File Offset: 0x0024D37F
		public List<Contract> GlobalContracts
		{
			get
			{
				return this.globalContracts;
			}
		}

		// Token: 0x17001970 RID: 6512
		// (get) Token: 0x0600908C RID: 37004 RVA: 0x0024F187 File Offset: 0x0024D387
		public bool UXAttached
		{
			get
			{
				return this.HasInitStateBits(SimGameState.InitStates.UX_SYSTEMS_CREATED);
			}
		}

		// Token: 0x17001971 RID: 6513
		// (get) Token: 0x0600908D RID: 37005 RVA: 0x0024F190 File Offset: 0x0024D390
		public bool DebugMode
		{
			get
			{
				return this.AllowDebug;
			}
		}

		// Token: 0x17001972 RID: 6514
		// (get) Token: 0x0600908E RID: 37006 RVA: 0x0024F198 File Offset: 0x0024D398
		public Contract SelectedContract
		{
			get
			{
				return this._selectedContract;
			}
		}

		// Token: 0x17001973 RID: 6515
		// (get) Token: 0x0600908F RID: 37007 RVA: 0x0024F1A0 File Offset: 0x0024D3A0
		public bool IsSelectedContractTravel
		{
			get
			{
				return this._selectedContractTravel;
			}
		}

		// Token: 0x17001974 RID: 6516
		// (get) Token: 0x06009090 RID: 37008 RVA: 0x0024F1A8 File Offset: 0x0024D3A8
		public bool IsSelectedContractedSimulated
		{
			get
			{
				return this._selectedContractSimulated;
			}
		}

		// Token: 0x17001975 RID: 6517
		// (get) Token: 0x06009091 RID: 37009 RVA: 0x0024F1B0 File Offset: 0x0024D3B0
		public bool IsSelectedContractForced
		{
			get
			{
				return this._selectedContractForced;
			}
		}

		// Token: 0x06009092 RID: 37010 RVA: 0x0024F1B8 File Offset: 0x0024D3B8
		public void SetSelectedContract(Contract c = null, bool travelOnly = false, bool simulated = false)
		{
			this._selectedContract = c;
			this._selectedContractTravel = travelOnly;
			this._selectedContractSimulated = simulated;
			this._selectedContractForced = false;
		}

		// Token: 0x06009093 RID: 37011 RVA: 0x0024F1D6 File Offset: 0x0024D3D6
		public void SetSelectedContractSimulated(bool simulated)
		{
			this._selectedContractSimulated = simulated;
		}

		// Token: 0x17001976 RID: 6518
		// (get) Token: 0x06009094 RID: 37012 RVA: 0x0024F1DF File Offset: 0x0024D3DF
		public bool InterruptQueueOpen
		{
			get
			{
				return this.interruptQueue.IsOpen;
			}
		}

		// Token: 0x17001977 RID: 6519
		// (get) Token: 0x06009095 RID: 37013 RVA: 0x0024F1EC File Offset: 0x0024D3EC
		public SimGameConstantOverride ConstantOverrides
		{
			get
			{
				return this.constantOverrides;
			}
		}

		// Token: 0x17001978 RID: 6520
		// (get) Token: 0x06009096 RID: 37014 RVA: 0x0024F1F4 File Offset: 0x0024D3F4
		public SimGameDifficulty DifficultySettings
		{
			get
			{
				return this.difficultySettings;
			}
		}

		// Token: 0x17001979 RID: 6521
		// (get) Token: 0x06009097 RID: 37015 RVA: 0x0024F1FC File Offset: 0x0024D3FC
		public Flashpoint ActiveFlashpoint
		{
			get
			{
				return this.activeFlashpoint;
			}
		}

		// Token: 0x1700197A RID: 6522
		// (get) Token: 0x06009098 RID: 37016 RVA: 0x0024F204 File Offset: 0x0024D404
		public bool IsInFlashpoint
		{
			get
			{
				return this.ActiveFlashpoint != null && this.ActiveFlashpoint.CurStatus == Flashpoint.Status.IN_PROGRESS;
			}
		}

		// Token: 0x1700197B RID: 6523
		// (get) Token: 0x06009099 RID: 37017 RVA: 0x0024F21E File Offset: 0x0024D41E
		public List<Flashpoint> AvailableFlashpoints
		{
			get
			{
				return this.availableFlashpointList;
			}
		}

		// Token: 0x1700197C RID: 6524
		// (get) Token: 0x0600909A RID: 37018 RVA: 0x0024F226 File Offset: 0x0024D426
		public List<FlashpointDef> FlashpointPool
		{
			get
			{
				return this.flashpointPool;
			}
		}

		// Token: 0x1700197D RID: 6525
		// (get) Token: 0x0600909B RID: 37019 RVA: 0x0024F22E File Offset: 0x0024D42E
		public string CompanyName
		{
			get
			{
				return this.Player1sMercUnitHeraldryDef.Description.Name;
			}
		}

		// Token: 0x0600909C RID: 37020 RVA: 0x0024F240 File Offset: 0x0024D440
		public SimGameState()
		{
			this.NetworkRandom = new NetworkRandom();
			this.interruptQueue = new SimGameInterruptManager(this);
			this.interruptQueue.onActiveInterruptQueueComplete = new Action(this.OnInterruptQueueComplete);
			this.NetworkRandom.seed = (int)DateTime.Now.Ticks;
		}

		// Token: 0x0600909D RID: 37021 RVA: 0x0024F70B File Offset: 0x0024D90B
		public void Init(GameInstance game, SimGameState.SimGameType campaignType, bool allowDebug, SimGameDifficulty difficulty)
		{
			if (this.HasInitStateBits(SimGameState.InitStates.INITIALIZED))
			{
				SimGameState.logger.LogError("SIM GAME STATE ALREADY INITIALZIED");
				return;
			}
			this._OnInit(game, difficulty);
			this._OnFirstPlayInit(campaignType, allowDebug);
			this.SetInitStateBits(SimGameState.InitStates.REQUEST_DEFS_LOAD);
			this.SetInitStateBits(SimGameState.InitStates.INITIALIZED);
		}

		// Token: 0x0600909E RID: 37022 RVA: 0x0024F74C File Offset: 0x0024D94C
		public void InitFromSave(GameInstance game, GameInstanceSave gameInstanceSave)
		{
			if (this.HasInitStateBits(SimGameState.InitStates.INITIALIZED))
			{
				SimGameState.logger.LogError("SIM GAME STATE ALREADY INITIALZIED");
				return;
			}
			this._OnInit(game, null);
			this._OnInitFromSave(gameInstanceSave);
			this.SetInitStateBits(SimGameState.InitStates.FROM_SAVE);
			this.SetInitStateBits(SimGameState.InitStates.REQUEST_DEFS_LOAD);
			this.SetInitStateBits(SimGameState.InitStates.INITIALIZED);
		}

		// Token: 0x0600909F RID: 37023 RVA: 0x0024F79C File Offset: 0x0024D99C
		public void AttachUX()
		{
			if (this.HasInitStateBits(SimGameState.InitStates.UX_SYSTEMS_CREATED))
			{
				SimGameState.logger.LogWarning("[AttachUX] Head already attached");
				return;
			}
			if (this.HasInitStateBits(SimGameState.InitStates.ASYNC_ATTACHING_UX_STATE))
			{
				SimGameState.logger.LogWarning("[AttachUX] Head attachment already in progress");
				return;
			}
			if (this.HasInitStateBits(SimGameState.InitStates.REQUEST_ATTACH_UX_STATE))
			{
				SimGameState.logger.LogWarning("[AttachUX] Head attachment request already active");
				return;
			}
			SimGameState.logger.Log("[AttachUX] Attaching head!");
			this.SetInitStateBits(SimGameState.InitStates.REQUEST_ATTACH_UX_STATE);
		}

		// Token: 0x060090A0 RID: 37024 RVA: 0x0024F814 File Offset: 0x0024DA14
		public void DetatchUX()
		{
			LazySingletonBehavior<UIManager>.Instance.ResetScreenshotMode();
			if (this.HasInitStateBits(SimGameState.InitStates.ASYNC_ATTACHING_UX_STATE))
			{
				SimGameState.logger.LogError("[DetatchUX] Can't detatch head while head is being attached!");
				return;
			}
			if (this.HasInitStateBits(SimGameState.InitStates.REQUEST_ATTACH_UX_STATE))
			{
				SimGameState.logger.Log("[DetatchUX] DetatchHead while in requesting head attachment state cancels request.");
				this.RemoveInitStateBits(SimGameState.InitStates.REQUEST_ATTACH_UX_STATE);
				return;
			}
			if (!this.HasInitStateBits(SimGameState.InitStates.UX_SYSTEMS_CREATED))
			{
				SimGameState.logger.Log("[DetatchUX] Head is not attached. Nothing to do");
				return;
			}
			SimGameState.logger.Log("[DetatchUX] Detatching head.");
			this._OnBeginDetatchUX();
		}

		// Token: 0x060090A1 RID: 37025 RVA: 0x0024F89B File Offset: 0x0024DA9B
		public void SimGameUXCreatorLoaded()
		{
			this.RespondToUXSystemsCreated();
		}

		// Token: 0x060090A2 RID: 37026 RVA: 0x0024F8A3 File Offset: 0x0024DAA3
		public void Destroy()
		{
			SimGameState.logger.Log("Destroy");
			this.DetatchUX();
			this._OnDestroy();
		}

		// Token: 0x060090A3 RID: 37027 RVA: 0x0024F8C0 File Offset: 0x0024DAC0
		private void SetUIVisible(bool isOn)
		{
			this.RoomManager.SetAllUIVisibility(isOn);
		}

		// Token: 0x060090A4 RID: 37028 RVA: 0x0024F8D0 File Offset: 0x0024DAD0
		public string GenerateSimGameUID()
		{
			if (this.HasInitStateBits(SimGameState.InitStates.FROM_SAVE) && !this.HasInitStateBits(SimGameState.InitStates.HEADLESS_STATE))
			{
				Debug.LogError("Generating SIM GAME UID while still loading!");
			}
			this.UIDCount += 1U;
			return "SGRef_" + this.UIDCount;
		}

		// Token: 0x1700197E RID: 6526
		// (get) Token: 0x060090A5 RID: 37029 RVA: 0x0024F91D File Offset: 0x0024DB1D
		// (set) Token: 0x060090A6 RID: 37030 RVA: 0x0024F925 File Offset: 0x0024DB25
		public WorkOrderEntry_TravelGeneric TravelOrder { get; private set; }

		// Token: 0x1700197F RID: 6527
		// (get) Token: 0x060090A7 RID: 37031 RVA: 0x0024F92E File Offset: 0x0024DB2E
		// (set) Token: 0x060090A8 RID: 37032 RVA: 0x0024F936 File Offset: 0x0024DB36
		public TaskManagementElement TravelItem { get; private set; }

		// Token: 0x060090A9 RID: 37033 RVA: 0x0024F93F File Offset: 0x0024DB3F
		public void SetTravelOrder(WorkOrderEntry_TravelGeneric newTravelOrder)
		{
			this.TravelOrder = newTravelOrder;
		}

		// Token: 0x060090AA RID: 37034 RVA: 0x0024F948 File Offset: 0x0024DB48
		public void SetTravelItem(TaskManagementElement newTravelItem)
		{
			this.TravelItem = newTravelItem;
		}

		// Token: 0x060090AB RID: 37035 RVA: 0x0024F954 File Offset: 0x0024DB54
		public void Update()
		{
			if (!this.UXAttached || !this.HasSimShipBeenSet || this.Saving)
			{
				return;
			}
			if (this.HandleCompleteBreadcrumbProcess())
			{
				return;
			}
			if (this.CompletedContract != null)
			{
				if (this.VideoPlayerActive)
				{
					return;
				}
				if (this.CharacterCreation.isActiveAndEnabled)
				{
					return;
				}
				if (this.Credits != null)
				{
					return;
				}
				if (!this.interruptQueue.IsOpen)
				{
					this.ResolveCompleteContract();
				}
				return;
			}
			else if (this.PendingMilestoneContract != null)
			{
				if (this.VideoPlayerActive)
				{
					return;
				}
				if (this.CharacterCreation.isActiveAndEnabled)
				{
					return;
				}
				if (this.Credits != null)
				{
					return;
				}
				if (this.interruptQueue.IsOpen)
				{
					return;
				}
				this.ForceTakeContract(this.PendingMilestoneContract, this.IsPendingMilestoneContractBreadcrumb);
				this.PendingMilestoneContract = null;
				this.IsPendingMilestoneContractBreadcrumb = false;
				return;
			}
			else
			{
				if (this._forceInterruptCheck)
				{
					this.interruptQueue.DisplayIfAvailable();
					this._forceInterruptCheck = false;
				}
				if (SimGameOptionsMenu.isUp)
				{
					return;
				}
				if (this.Constants.Debug.AllowSimGameNoUIMode && BTInput.Instance.Toggle_UI().WasPressed)
				{
					LazySingletonBehavior<UIManager>.Instance.ToggleAllUIOnOff();
				}
				if (this.AllowDebug)
				{
					SimGameState_Debug.SimDebugUpdate();
				}
				if (!this.TimeMoving || this.TravelManager.InTransition)
				{
					return;
				}
				if (this.interruptQueue.HasQueue)
				{
					if (!this.interruptQueue.IsOpen)
					{
						this.interruptQueue.DisplayIfAvailable();
					}
					return;
				}
				this.realTimeElapsed += this.BattleTechGame.DeltaTime;
				float num = 0f;
				if (this.TimeMoving)
				{
					num = (ActiveOrDefaultSettings.CloudSettings.speedUpSimgame ? this.Constants.Time.DayElapseTimeFast : this.Constants.Time.DayElapseTimeNormal);
					if (this.RoomManager != null)
					{
						this.RoomManager.UpdateTimePassed(this.realTimeElapsed);
					}
				}
				if (this.realTimeElapsed >= num)
				{
					this.realTimeElapsed = 0f;
					this.OnDayPassed(0);
				}
				return;
			}
		}

		// Token: 0x060090AC RID: 37036 RVA: 0x0024FB45 File Offset: 0x0024DD45
		private bool HandleCompleteBreadcrumbProcess()
		{
			if (!this.completeBreadcrumbProcessQueued)
			{
				return false;
			}
			if (this.interruptQueue.IsOpen)
			{
				return true;
			}
			this.completeBreadcrumbProcessQueued = false;
			this.FinishCompleteBreadcrumbProcess();
			return true;
		}

		// Token: 0x060090AD RID: 37037 RVA: 0x0024FB6E File Offset: 0x0024DD6E
		public bool IsGameOverCondition(bool showPopup = false)
		{
			if (this.Funds < this.Constants.Story.MaximumDebt)
			{
				if (showPopup)
				{
					this.interruptQueue.QueueLossOutcome();
				}
				return true;
			}
			return false;
		}

		// Token: 0x060090AE RID: 37038 RVA: 0x0024FB9A File Offset: 0x0024DD9A
		public void ShowOutOfFundsOutcome()
		{
			this.ShowCampaignOutcome(SGCampaignOutcomeScreen.ScreenMode.OUT_OF_FUNDS, null, null, null, null);
		}

		// Token: 0x060090AF RID: 37039 RVA: 0x0024FBA8 File Offset: 0x0024DDA8
		public void ShowCampaignOutcome(SGCampaignOutcomeScreen.ScreenMode mode, string title = null, string body = null, string additional = null, Action callback = null)
		{
			SGCampaignOutcomeScreen sgcampaignOutcomeScreen = LazySingletonBehavior<UIManager>.Instance.CreatePopupModule<SGCampaignOutcomeScreen>("", true);
			sgcampaignOutcomeScreen.SetMode(mode);
			if (!string.IsNullOrEmpty(title))
			{
				sgcampaignOutcomeScreen.SetTitle(Interpolator.Interpolate(title, this.Context, true));
			}
			if (!string.IsNullOrEmpty(body))
			{
				sgcampaignOutcomeScreen.SetDescription(Interpolator.Interpolate(body, this.Context, true));
			}
			if (!string.IsNullOrEmpty(additional))
			{
				sgcampaignOutcomeScreen.SetAdditionalMessaging(Interpolator.Interpolate(additional, this.Context, true));
			}
			sgcampaignOutcomeScreen.AddOnPooledAction(callback);
		}

		// Token: 0x060090B0 RID: 37040 RVA: 0x0024FC28 File Offset: 0x0024DE28
		public void ShowNegativeBalancePopup()
		{
			this.interruptQueue.QueuePauseNotification("In The Red", Strings.T("Captain, we're now in debt. If we exceed {0} of debt, we will have to disolve the Company", new object[] { SimGameState.GetCBillString(this.Constants.Story.MaximumDebt) }), this.GetCrewPortrait(SimGameCrew.Crew_Darius), "notification_lowfunds", new Action(this.OnNotificationDismissed), "Continue", null, null);
		}

		// Token: 0x060090B1 RID: 37041 RVA: 0x0024FC90 File Offset: 0x0024DE90
		public void ShowGameLossWarning(int daysRemaining)
		{
			this.interruptQueue.QueuePauseNotification("The End Is Near", Strings.T("We have less than {0} days to get our debt under control. If we don't get above {1} of debt, it's over.", new object[]
			{
				daysRemaining,
				SimGameState.GetCBillString(this.Constants.Story.MaximumDebt)
			}), this.GetCrewPortrait(SimGameCrew.Crew_Darius), "notification_lowfunds", new Action(this.OnNotificationDismissed), "Continue", null, null);
		}

		// Token: 0x060090B2 RID: 37042 RVA: 0x0024FD00 File Offset: 0x0024DF00
		public void ShowLowFundsWarning()
		{
			if (!this.CompanyStats.ContainsStatistic("COMPANY_NotificationViewed_LowFunds"))
			{
				this.CompanyStats.AddStatistic<int>("COMPANY_NotificationViewed_LowFunds", -1);
			}
			this.interruptQueue.QueuePauseNotification("Funds Running Low", "We're getting low on C-bills, Commander. I suggest keeping a careful eye on our expenses until we can bring in some more income. Worst comes to worst, we can sell off some extra equipment in the local store.", this.GetCrewPortrait(SimGameCrew.Crew_Darius), "notification_lowfunds", new Action(this.OnNotificationDismissed), "Continue", null, null);
			this.CompanyStats.Set<int>("COMPANY_NotificationViewed_LowFunds", this.DaysPassed);
		}

		// Token: 0x060090B3 RID: 37043 RVA: 0x0024FD80 File Offset: 0x0024DF80
		public void ShowDangeriousLowFundsWarning()
		{
			this.interruptQueue.QueuePauseNotification("Funds Dangerously Low!", Strings.T("Our situation is dire, {0}. We need to do whatever it takes to generate some C-bills now or we're going to be out of the merc business soon!", new object[] { this.Commander.Callsign }), this.GetCrewPortrait(SimGameCrew.Crew_Darius), "notification_lowfunds", new Action(this.OnNotificationDismissed), "Continue", null, null);
			this.CompanyStats.Set<int>("COMPANY_NotificationViewed_LowFunds", this.DaysPassed);
		}

		// Token: 0x060090B4 RID: 37044 RVA: 0x0024FDF4 File Offset: 0x0024DFF4
		public void ShowMechWarriorTrainingNotif()
		{
			this.interruptQueue.QueuePauseNotification("MechWarrior Training Required", Strings.T("Our MechWarriors are gaining in experience and need your guidance, {0}. If you head to the Barracks, you can direct their training.", new object[] { this.Commander.Callsign }), this.GetCrewPortrait(SimGameCrew.Crew_Darius), null, new Action(this.OnNotificationDismissed), "Continue", null, null);
		}

		// Token: 0x060090B5 RID: 37045 RVA: 0x0024FE4C File Offset: 0x0024E04C
		public void ShowArgoUpgradeNeededNotif()
		{
			this.interruptQueue.QueuePauseNotification("Argo Upgrade Needed", "We can improve the Argo, Commander. Stop by Engineering and let me know what you want to upgrade.", this.GetCrewPortrait(SimGameCrew.Crew_Farah), null, new Action(this.OnNotificationDismissed), "Continue", null, null);
			this.CompanyStats.Set<int>("COMPANY_NotificationViewed_ArgoUpgradeNeeded", this.DaysPassed);
		}

		// Token: 0x060090B6 RID: 37046 RVA: 0x0024FEA4 File Offset: 0x0024E0A4
		public void ShowMechRepairsNeededNotif()
		{
			this.interruptQueue.QueuePauseNotification("BattleMech Repairs Needed!", "We're gonna need to do some 'Mech repairs before our next contract, Boss. Can't go into combat like this! See me in the MechBay when you're ready.", this.GetCrewPortrait(SimGameCrew.Crew_Yang), null, new Action(this.OnNotificationDismissed), "Continue", null, null);
			this.CompanyStats.Set<int>("COMPANY_NotificationViewed_BattleMechRepairsNeeded", this.DaysPassed);
		}

		// Token: 0x060090B7 RID: 37047 RVA: 0x0024FEFC File Offset: 0x0024E0FC
		public void ShowFlashpointFailedNotif(Action continueAction, string flashpointName)
		{
			this.interruptQueue.QueuePauseNotification(Strings.T("Flashpoint failed: {0}", new object[] { flashpointName }), Strings.T("That didn't go as planned, Commander. We'll have to cut our losses and regroup. If we keep an eye out, we might have an opportunity to retry this engagement at a later time, so let's learn from it."), this.GetCrewPortrait(SimGameCrew.Crew_Darius), null, continueAction, "Continue", null, null);
		}

		// Token: 0x060090B8 RID: 37048 RVA: 0x0024FF44 File Offset: 0x0024E144
		public void ShowFlashpointFailedPopup(Action continueAction, string flashpointName)
		{
			PauseNotification.Show(Strings.T("Flashpoint failed: {0}", new object[] { flashpointName }), Strings.T("That didn't go as planned, Commander. We'll have to cut our losses and regroup. If we keep an eye out, we might have an opportunity to retry this engagement at a later time, so let's learn from it."), this.GetCrewPortrait(SimGameCrew.Crew_Darius), null, false, continueAction, "Continue", null, null);
		}

		// Token: 0x060090B9 RID: 37049 RVA: 0x0024FF88 File Offset: 0x0024E188
		public void ShowFlashpointCompletedWithoutRewardNotif(Action continueAction, string flashpointName)
		{
			this.interruptQueue.QueuePauseNotification(Strings.T("Flashpoint abandoned: {0}", new object[] { flashpointName }), Strings.T("You have elected to discontinue an active flashpoint.\n\nNote that your reputation may be affected by your decision.You can review your reputation in the CPT QUARTERS."), this.GetCrewPortrait(SimGameCrew.Crew_Darius), null, continueAction, "Continue", null, null);
		}

		// Token: 0x060090BA RID: 37050 RVA: 0x0000D184 File Offset: 0x0000B384
		private void OnNotificationDismissed()
		{
		}

		// Token: 0x060090BB RID: 37051 RVA: 0x0024FFCF File Offset: 0x0024E1CF
		private void OnGameOverAccepted()
		{
			LevelLoader.LoadScene("MainMenu", "Insterstitial_Cleanup");
			UnityGameInstance.BattleTechGame.ClearSimulation();
		}

		// Token: 0x060090BC RID: 37052 RVA: 0x0024FFEC File Offset: 0x0024E1EC
		public void RefreshTravelStatus()
		{
			switch (this.TravelState)
			{
			case SimGameTravelStatus.IN_SYSTEM:
				this.CameraController.spaceController.Orbit(this.TravelState, true);
				return;
			case SimGameTravelStatus.AT_JUMP_POINT:
			case SimGameTravelStatus.WARMING_ENGINES:
				this.CameraController.spaceController.Dock(this.TravelState, true);
				return;
			case SimGameTravelStatus.TRANSIT_TO_JUMP:
				this.CameraController.spaceController.ToJumpship(this.TravelState, true);
				return;
			case SimGameTravelStatus.TRANSIT_FROM_JUMP:
				this.CameraController.spaceController.ToPlanet(this.TravelState, true);
				return;
			default:
				return;
			}
		}

		// Token: 0x060090BD RID: 37053 RVA: 0x0025007A File Offset: 0x0024E27A
		public void QueueUpdateMilestoneCheck()
		{
			this.needQueuedMilestoneCheck = true;
		}

		// Token: 0x060090BE RID: 37054 RVA: 0x00250084 File Offset: 0x0024E284
		public bool UpdateMilestones()
		{
			if (this.activeFlashpoint != null)
			{
				return false;
			}
			if (!this.CompanyTags.Contains(this.Constants.Story.SystemUseMilestoneTag))
			{
				return false;
			}
			bool flag = false;
			for (int i = 0; i < this.milestones.Count; i++)
			{
				SimGameMilestoneDef simGameMilestoneDef = this.milestones[i];
				if (!this.ConsumedMilestones.Contains(simGameMilestoneDef.Description.Id))
				{
					bool flag2 = true;
					if (simGameMilestoneDef.Requirements != null)
					{
						for (int j = 0; j < this.milestones[i].Requirements.Length; j++)
						{
							if (!this.MeetsRequirements(this.milestones[i].Requirements[j], null))
							{
								flag2 = false;
								break;
							}
						}
					}
					if (flag2)
					{
						if (!simGameMilestoneDef.Repeatable)
						{
							this.ConsumedMilestones.Add(simGameMilestoneDef.Description.Id);
						}
						AchievementMilestoneMessage achievementMilestoneMessage = new AchievementMilestoneMessage(simGameMilestoneDef.Description.Id);
						this.MessageCenter.PublishMessage(achievementMilestoneMessage);
						flag |= SimGameState.ApplySimGameEventResult(new List<SimGameEventResult>(simGameMilestoneDef.Results));
						break;
					}
				}
			}
			if (this.needQueuedMilestoneCheck)
			{
				this.needQueuedMilestoneCheck = false;
				flag |= this.UpdateMilestones();
			}
			if (this.interruptQueue.HasQueue && !this.interruptQueue.IsOpen)
			{
				this.interruptQueue.DisplayIfAvailable();
			}
			return flag;
		}

		// Token: 0x060090BF RID: 37055 RVA: 0x002501E4 File Offset: 0x0024E3E4
		private void OnNewQuarterBegin()
		{
			this.CompanyStats.ModifyStat<int>("Mission", 0, "COMPANY_MonthlyStartingFunds", StatCollection.StatOperation.Set, this.Funds, -1, true);
			this.CompanyStats.ModifyStat<int>("Mission", 0, "COMPANY_MonthlyStartingMorale", StatCollection.StatOperation.Set, this.Morale, -1, true);
		}

		// Token: 0x17001980 RID: 6528
		// (get) Token: 0x060090C0 RID: 37056 RVA: 0x00250231 File Offset: 0x0024E431
		// (set) Token: 0x060090C1 RID: 37057 RVA: 0x00250239 File Offset: 0x0024E439
		public bool DebugForceEventOnPlanetArrival { get; set; }

		// Token: 0x060090C2 RID: 37058 RVA: 0x00250244 File Offset: 0x0024E444
		private void OnDayPassed(int timeLapse = 0)
		{
			int num = ((timeLapse > 0) ? timeLapse : 1);
			bool flag = this.CompanyTags.Contains(this.Constants.Story.SystemUseTimeTag);
			if (flag)
			{
				this.DaysPassed += num;
				this.DayRemainingInQuarter -= num;
				if (this.IsInBreadcrumbArrival)
				{
					this.SetBreadcrumbArrived(false);
				}
				this.FinancialReportNotification.PayCost(num);
			}
			if (timeLapse == 0 && flag)
			{
				if (this.TravelManager.OnDayPassed())
				{
					this.HandleDebugForceFinancialReportOnPlanetArrival();
					this.HandleDebugForceEventOnPlanetArrival();
				}
				this.ReportDay();
				if (!this.UpdateMilestones())
				{
					this.interruptQueue.QueueEventTest();
				}
				this.RoomManager.RefreshDay();
			}
			this.UpdateInjuries();
			this.UpdateTempResults();
			this.UpdateMechLabWorkQueue(true);
			this.UpdateArgoUpgrades(true);
			if (this.IsGameOverCondition(true))
			{
				return;
			}
			if (this.SimGameMode == SimGameState.SimGameType.CAREER)
			{
				if (this.IsCareerModeComplete() && !this.IsCareerModeLocked())
				{
					this.OnCareerModeCompleted();
				}
				this.GetRawCareerModeDifficulty();
			}
			for (int i = this.GlobalContracts.Count - 1; i >= 0; i--)
			{
				if (this.GlobalContracts[i].OnDayPassed())
				{
					this.GlobalContracts.RemoveAt(i);
				}
			}
			this.CurSystem.UpdateSystemDay();
			int value = this.CompanyStats.GetValue<int>("ExperiencePerDayCap");
			List<Pilot> list = new List<Pilot>(this.PilotRoster);
			list.Add(this.commander);
			for (int j = 0; j < list.Count; j++)
			{
				Pilot pilot = list[j];
				if (pilot.CanPilot && pilot.TotalXP <= value)
				{
					pilot.AddExperience(0, "Daily XP", this.CompanyStats.GetValue<int>("ExperiencePerDay"));
				}
			}
			int num2 = this.DaysPassed - this.CompanyStats.GetValue<int>("COMPANY_NotificationViewed_LowFunds");
			int expenditures = this.GetExpenditures(false);
			if (this.Funds <= this.Constants.Story.MaximumDebt && this.DayRemainingInQuarter < 7 && this.DayRemainingInQuarter > 0)
			{
				this.ShowGameLossWarning(this.DayRemainingInQuarter);
			}
			else if (this.DayRemainingInQuarter <= 0)
			{
				int funds = this.Funds;
				int num3 = Mathf.Abs(this.DayRemainingInQuarter / this.Constants.Finances.QuarterLength) + 1;
				this.DayRemainingInQuarter = this.Constants.Finances.QuarterLength + this.DayRemainingInQuarter;
				this.RoomManager.RemoveWorkQueueEntry(this.FinancialReportNotification, false);
				this.FinancialReportNotification = new WorkOrderEntry_Notification(WorkOrderType.FinancialReport, "Financial Report", "Financial Report", "");
				this.FinancialReportNotification.SetCost(this.DayRemainingInQuarter);
				this.FinancialReportItem = this.RoomManager.AddWorkQueueEntry(this.FinancialReportNotification);
				this.RoomManager.SortTimeline();
				this.ProRateRefund = 0;
				this.DeductQuarterlyFunds(num3);
				this.LogReport(string.Format("End of Month: {0} remaining (Was {1})", this.Funds, funds));
				SimGameState.logger.Log("Monthly Autosave");
				if (this.IsGameOverCondition(true))
				{
					return;
				}
			}
			else if (this.Funds < expenditures * this.Constants.Story.NotifDangerLowFundsQtrRemainThreshold && this.Constants.Story.NotifDangerLowFundsRecurrence > 0 && num2 >= this.Constants.Story.NotifDangerLowFundsRecurrence)
			{
				this.ShowDangeriousLowFundsWarning();
			}
			else if (this.Funds < expenditures * this.Constants.Story.NotifLowFundsQtrRemainThreshold && this.Constants.Story.NotifLowFundsRecurrence > 0 && num2 >= this.Constants.Story.NotifLowFundsRecurrence)
			{
				this.ShowLowFundsWarning();
			}
			int num4 = this.DaysPassed - this.CompanyStats.GetValue<int>("COMPANY_NotificationViewed_ArgoUpgradeNeeded");
			if (this.Constants.Story.NotifArgoUpgradesRecurrence > 0 && this.CurDropship == DropshipType.Argo && this.ShipUpgrades.Count < this.Constants.Story.NotifArgoUpgradesCutoffThreshold && num4 > this.Constants.Story.NotifArgoUpgradesRecurrence)
			{
				this.ShowArgoUpgradeNeededNotif();
			}
			this.FlashpointDayPassed();
			this.ReputationDayPassed();
			this.RoomManager.RefreshDisplay();
			if (this.interruptQueue.HasQueue && !this.interruptQueue.IsOpen)
			{
				this.interruptQueue.DisplayIfAvailable();
			}
		}

		// Token: 0x060090C3 RID: 37059 RVA: 0x00250698 File Offset: 0x0024E898
		private void HandleDebugForceFinancialReportOnPlanetArrival()
		{
			if (!DebugBridge.ForceFinancialReportOnPlanetArrival)
			{
				return;
			}
			DebugBridge.ForceFinancialReportOnPlanetArrival = false;
			int dayRemainingInQuarter = this.DayRemainingInQuarter;
			this.DaysPassed += dayRemainingInQuarter;
			this.DayRemainingInQuarter = 0;
			this.FinancialReportNotification.PayCost(dayRemainingInQuarter);
		}

		// Token: 0x060090C4 RID: 37060 RVA: 0x002506DC File Offset: 0x0024E8DC
		private void HandleDebugForceEventOnPlanetArrival()
		{
			if (!DebugBridge.ForceEventOnPlanetArrival)
			{
				return;
			}
			DebugBridge.ForceEventOnPlanetArrival = false;
			this.DebugForceEventOnPlanetArrival = true;
		}

		// Token: 0x060090C5 RID: 37061 RVA: 0x002506F3 File Offset: 0x0024E8F3
		public void SetBreadcrumbArrived(bool arrived)
		{
			this.IsInBreadcrumbArrival = arrived;
		}

		// Token: 0x060090C6 RID: 37062 RVA: 0x002506FC File Offset: 0x0024E8FC
		public bool AttemptEvents(bool incrementOnFailure = true, bool specialOnly = false)
		{
			if (this.IsInBreadcrumbArrival)
			{
				return false;
			}
			bool flag = false;
			if (this.DebugForceEventOnPlanetArrival)
			{
				this.DebugForceEventOnPlanetArrival = false;
				this.SetSystemOwnerReputation();
				return this.companyEventTracker.ForceDebugEvent();
			}
			if (this.CompanyTags.Contains(this.Constants.Story.SystemUseEventsTag) && !specialOnly)
			{
				this.LogReport("% Chance for Non-Morale Event: " + this.companyEventTracker.EventChance);
				this.LogReport("Company Event Invoked: " + this.companyEventTracker.AttemptEvent(true).ToString());
				this.LogReport("% Chance for Morale Event: " + this.moraleEventTracker.EventChance);
				flag = this.moraleEventTracker.AttemptEvent(true);
				this.LogReport("Morale Event Invoked: " + flag.ToString());
			}
			for (int i = 0; i < this.specialEventTracker.Count; i++)
			{
				if (this.specialEventTracker[i].AttemptEvent(incrementOnFailure))
				{
					flag = true;
					this.specialEventTracker.RemoveAt(i);
					break;
				}
			}
			return flag;
		}

		// Token: 0x060090C7 RID: 37063 RVA: 0x00250824 File Offset: 0x0024EA24
		private void UpdateTempResults()
		{
			for (int i = this.TemporaryResultTracker.Count - 1; i >= 0; i--)
			{
				TemporarySimGameResult temporarySimGameResult = this.TemporaryResultTracker[i];
				temporarySimGameResult.DaysElapsed++;
				if (temporarySimGameResult.DaysElapsed > temporarySimGameResult.ResultDuration)
				{
					this.RemoveSimGameEventResult(temporarySimGameResult);
					this.TemporaryResultTracker.Remove(temporarySimGameResult);
					this.AddOrRemoveTempTags(temporarySimGameResult, false);
				}
			}
			this.ScrubOrphanTags();
		}

		// Token: 0x060090C8 RID: 37064 RVA: 0x00250894 File Offset: 0x0024EA94
		private void ScrubOrphanTags()
		{
			this.ScrubOrphanTags(this.Commander);
			foreach (Pilot pilot in this.PilotRoster)
			{
				this.ScrubOrphanTags(pilot);
			}
		}

		// Token: 0x060090C9 RID: 37065 RVA: 0x002508F0 File Offset: 0x0024EAF0
		private void ScrubOrphanTags(Pilot p)
		{
			this.ScrubOrphanTags(p, "pilot_morale_low");
			this.ScrubOrphanTags(p, "pilot_morale_high");
		}

		// Token: 0x060090CA RID: 37066 RVA: 0x0025090C File Offset: 0x0024EB0C
		private void ScrubOrphanTags(Pilot p, string tag)
		{
			if (!p.pilotDef.PilotTags.Contains(tag))
			{
				return;
			}
			bool flag = true;
			if (this.TemporaryResultTracker != null)
			{
				for (int i = 0; i < this.TemporaryResultTracker.Count; i++)
				{
					TemporarySimGameResult temporarySimGameResult = this.TemporaryResultTracker[i];
					if (temporarySimGameResult.AddedTags.Contains(tag) && temporarySimGameResult.TargetPilot != null && !(temporarySimGameResult.TargetPilot.GUID != p.GUID))
					{
						flag = false;
						break;
					}
				}
			}
			if (flag)
			{
				this.RemoveOrphanTags(p.pilotDef.PilotTags, tag);
			}
		}

		// Token: 0x060090CB RID: 37067 RVA: 0x002509A1 File Offset: 0x0024EBA1
		private void RemoveOrphanTags(TagSet tagSet, string tag)
		{
			if (tagSet.Contains(tag))
			{
				tagSet.Remove(tag);
			}
			if (tagSet.Contains("MODIFIED_STAT_" + tag))
			{
				tagSet.Remove("MODIFIED_STAT_" + tag);
			}
		}

		// Token: 0x060090CC RID: 37068 RVA: 0x0000D184 File Offset: 0x0000B384
		private void ReportDay()
		{
		}

		// Token: 0x060090CD RID: 37069 RVA: 0x0000D184 File Offset: 0x0000B384
		public void LogReport(string val)
		{
		}

		// Token: 0x060090CE RID: 37070 RVA: 0x002509DC File Offset: 0x0024EBDC
		public bool ForceActiveDropshipRoom(DropshipMenuType dropshipMenu, string additionalParameter = null)
		{
			DropshipLocation dropshipLocation = this.RoomManager.DropShipMenuTypeToLocation(dropshipMenu);
			if (dropshipLocation != DropshipLocation.UNKNOWN)
			{
				if (!string.IsNullOrEmpty(additionalParameter))
				{
					this.SetExitConferenceRoomContextParameters(dropshipMenu, additionalParameter);
				}
				this.RoomManager.SetQueuedUIActivationID(dropshipMenu, dropshipLocation, true);
				this.SetSimRoomState(dropshipLocation);
				return true;
			}
			return false;
		}

		// Token: 0x060090CF RID: 37071 RVA: 0x00250A24 File Offset: 0x0024EC24
		public void SetConferenceRoom(bool inRoom, DropshipMenuType exitOverride = DropshipMenuType.INVALID_UNSET, string additionalParameter = null)
		{
			if (!inRoom)
			{
				bool flag = false;
				if (exitOverride != DropshipMenuType.INVALID_UNSET)
				{
					flag = this.ForceActiveDropshipRoom(exitOverride, additionalParameter);
				}
				if (!flag)
				{
					if (this.PendingMilestoneContract != null || this.CompletedContract != null)
					{
						this.SetSimRoomState(DropshipLocation.NONE);
						return;
					}
					if (this.CurDropship != DropshipType.INVALID_UNSET)
					{
						this.SetSimRoomState(DropshipLocation.SHIP);
					}
				}
				return;
			}
			if (this.CurDropship == DropshipType.INVALID_UNSET)
			{
				this.SetSimRoomState(DropshipLocation.NONE);
				return;
			}
			this.SetSimRoomState(DropshipLocation.CONFERENCE);
		}

		// Token: 0x060090D0 RID: 37072 RVA: 0x00250A90 File Offset: 0x0024EC90
		private void SetExitConferenceRoomContextParameters(DropshipMenuType exitOverride, string parameter)
		{
			if (exitOverride == DropshipMenuType.Contract)
			{
				try
				{
					ContractDisplayStyle contractDisplayStyle = (ContractDisplayStyle)Enum.Parse(typeof(ContractDisplayStyle), parameter);
					this.RoomManager.CmdCenterRoom.SetContractDisplayAutoSelect(new ContractDisplayStyle?(contractDisplayStyle));
				}
				catch
				{
					SimGameState.logger.LogWarning(string.Format("Invalid subparameter of room {0}: {1}", exitOverride, parameter));
				}
			}
		}

		// Token: 0x060090D1 RID: 37073 RVA: 0x00250B00 File Offset: 0x0024ED00
		public void SetSimRoomState(DropshipLocation state)
		{
			this.SetUIVisible(state != DropshipLocation.NONE && state != DropshipLocation.CONFERENCE && this.CurDropship > DropshipType.INVALID_UNSET);
			this.RefreshSimRoomAudio(state);
			this.CurRoomState = state;
			this.RoomManager.ChangeRoom(state);
			this.CameraController.SetScene(state);
			SimGameRoomChangedMessage simGameRoomChangedMessage = new SimGameRoomChangedMessage(state);
			this.MessageCenter.PublishMessage(simGameRoomChangedMessage);
		}

		// Token: 0x060090D2 RID: 37074 RVA: 0x00250B68 File Offset: 0x0024ED68
		private void RefreshSimRoomAudio(DropshipLocation state)
		{
			WwiseManager.PostEvent<AudioEventList_ui>(AudioEventList_ui.ui_sim_whoosh_room_change, WwiseManager.GlobalAudioObject, null, null);
			if (state <= DropshipLocation.MECH_BAY)
			{
				if (state <= DropshipLocation.ENGINEERING)
				{
					switch (state)
					{
					case DropshipLocation.SHIP:
						AudioEventManager.SetAmbience(AudioState_ambiences.sim_space);
						if (this.TravelManager.PostTransitionState == SimGameTravelStatus.WARMING_ENGINES)
						{
							AudioEventManager.SetMusicState(AudioState_Music_State.Sim_Game, AudioSwitch_Mission_Status.jump_travel, AudioState_Player_State.alive, false);
							return;
						}
						AudioEventManager.SetMusicState(AudioState_Music_State.Sim_Game, AudioSwitch_Mission_Status.argo, AudioState_Player_State.alive, false);
						return;
					case DropshipLocation.CMD_CENTER:
						if (this.CurDropship == DropshipType.Leopard)
						{
							AudioEventManager.SetAmbience(AudioState_ambiences.sim_leopard_bridge);
						}
						else
						{
							AudioEventManager.SetAmbience(AudioState_ambiences.sim_command_center);
						}
						AudioEventManager.SetMusicState(AudioState_Music_State.Sim_Game, AudioSwitch_Mission_Status.Command_Center, AudioState_Player_State.alive, false);
						return;
					case DropshipLocation.SHIP | DropshipLocation.CMD_CENTER:
						break;
					case DropshipLocation.BARRACKS:
						if (this.CurDropship == DropshipType.Leopard)
						{
							AudioEventManager.SetAmbience(AudioState_ambiences.sim_leopard_bridge);
						}
						else
						{
							AudioEventManager.SetAmbience(AudioState_ambiences.sim_barracks);
						}
						AudioEventManager.SetMusicState(AudioState_Music_State.Sim_Game, AudioSwitch_Mission_Status.Barracks, AudioState_Player_State.alive, false);
						return;
					default:
						if (state == DropshipLocation.ENGINEERING)
						{
							if (this.CurDropship == DropshipType.Leopard)
							{
								AudioEventManager.SetAmbience(AudioState_ambiences.sim_leopard_bridge);
							}
							else
							{
								AudioEventManager.SetAmbience(AudioState_ambiences.sim_engineering);
							}
							AudioEventManager.SetMusicState(AudioState_Music_State.Sim_Game, AudioSwitch_Mission_Status.Engineering, AudioState_Player_State.alive, false);
							return;
						}
						break;
					}
				}
				else
				{
					if (state == DropshipLocation.NAVIGATION)
					{
						if (this.CurDropship == DropshipType.Leopard)
						{
							AudioEventManager.SetAmbience(AudioState_ambiences.sim_leopard_bridge);
						}
						else
						{
							AudioEventManager.SetAmbience(AudioState_ambiences.sim_navigation);
						}
						AudioEventManager.SetMusicState(AudioState_Music_State.Sim_Game, AudioSwitch_Mission_Status.Navigation, AudioState_Player_State.alive, false);
						return;
					}
					if (state == DropshipLocation.MECH_BAY)
					{
						if (this.CurDropship == DropshipType.Leopard)
						{
							AudioEventManager.SetAmbience(AudioState_ambiences.sim_leopard_mechbay);
						}
						else
						{
							AudioEventManager.SetAmbience(AudioState_ambiences.sim_mech_bay);
						}
						AudioEventManager.SetMusicState(AudioState_Music_State.Sim_Game, AudioSwitch_Mission_Status.Mech_Bay, AudioState_Player_State.alive, false);
						return;
					}
				}
			}
			else if (state <= DropshipLocation.CONFERENCE)
			{
				if (state == DropshipLocation.CPT_QUARTER)
				{
					AudioEventManager.SetAmbience(AudioState_ambiences.sim_captain_quarters);
					AudioEventManager.SetMusicState(AudioState_Music_State.Sim_Game, AudioSwitch_Mission_Status.Captain_Quarters, AudioState_Player_State.alive, false);
					return;
				}
				if (state == DropshipLocation.CONFERENCE)
				{
					if (this.CurDropship == DropshipType.Leopard)
					{
						AudioEventManager.SetAmbience(AudioState_ambiences.sim_leopard_bridge);
					}
					else
					{
						AudioEventManager.SetAmbience(AudioState_ambiences.sim_conference_room);
					}
					AudioEventManager.SetMusicState(AudioState_Music_State.Sim_Game, AudioSwitch_Mission_Status.Command_Center, AudioState_Player_State.alive, false);
					return;
				}
			}
			else
			{
				if (state == DropshipLocation.HIRING)
				{
					AudioEventManager.SetAmbience(AudioState_ambiences.sim_space);
					AudioEventManager.SetMusicState(AudioState_Music_State.Sim_Game, AudioSwitch_Mission_Status.argo, AudioState_Player_State.alive, false);
					return;
				}
				if (state == DropshipLocation.SHOP)
				{
					AudioEventManager.SetAmbience(AudioState_ambiences.sim_space);
					AudioEventManager.SetMusicState(AudioState_Music_State.Sim_Game, AudioSwitch_Mission_Status.argo, AudioState_Player_State.alive, false);
					return;
				}
			}
			AudioEventManager.SetAmbience(AudioState_ambiences.None);
			AudioEventManager.SuspendMusic(true);
		}

		// Token: 0x060090D3 RID: 37075 RVA: 0x00250D57 File Offset: 0x0024EF57
		public void SetSimShip(DropshipType dropship)
		{
			this.CurDropship = dropship;
			this.SpaceController.SetShip(dropship);
			if (dropship == DropshipType.Argo)
			{
				this.ApplyArgoUpgrades();
			}
		}

		// Token: 0x060090D4 RID: 37076 RVA: 0x00250D78 File Offset: 0x0024EF78
		public void SetCharacterVisibility(SimGameState.SimGameCharacterType character, bool isVisible)
		{
			int num = this.characterList.IndexOf(character);
			this.characterStatus[num] = isVisible;
			SimGameCharacter.SetCharacter(character, isVisible);
		}

		// Token: 0x060090D5 RID: 37077 RVA: 0x00250DA8 File Offset: 0x0024EFA8
		public bool GetCharacterStatus(SimGameState.SimGameCharacterType character)
		{
			int num = this.characterList.IndexOf(character);
			return num >= 0 && this.characterStatus[num];
		}

		// Token: 0x060090D6 RID: 37078 RVA: 0x00250DD4 File Offset: 0x0024EFD4
		public void PauseTimer()
		{
			this.canTimeElapse = false;
		}

		// Token: 0x060090D7 RID: 37079 RVA: 0x00250DDD File Offset: 0x0024EFDD
		public void ResumeTimer()
		{
			this.canTimeElapse = true;
		}

		// Token: 0x060090D8 RID: 37080 RVA: 0x00250DE6 File Offset: 0x0024EFE6
		public void StopPlayMode()
		{
			this.RoomManager.ShipRoom.SetTimeMoving(false, true);
		}

		// Token: 0x060090D9 RID: 37081 RVA: 0x00250DFC File Offset: 0x0024EFFC
		public void SetCurrentSystem(StarSystem system, bool force = false, bool timeSkip = false)
		{
			if (system == null)
			{
				SimGameState.logger.LogError("Unable to travel to unknown system");
				return;
			}
			this.Context.SetObject(GameContextObjectTagEnum.TargetStarSystem, system);
			if (this.CurSystem == system && !force)
			{
				return;
			}
			if (timeSkip)
			{
				this.Starmap.FindRouteTo(system, new Action<AStar.AStarResult>(this.OnTimeSkipTravelPathFound));
				return;
			}
			StarSystem curSystem = this.CurSystem;
			this.RemoveMapsFromMapDiscardInSystem(curSystem);
			this.CurSystem = system;
			this.VisitSystem(this.CurSystem);
			SimStarSystemChangedMessage simStarSystemChangedMessage = new SimStarSystemChangedMessage(this.CurSystem, curSystem);
			this.MessageCenter.PublishMessage(simStarSystemChangedMessage);
			curSystem.OnSystemExit();
			this.CurSystem.OnSystemChange();
			this.Starmap.FindRouteTo(this.TargetSystem, new Action<AStar.AStarResult>(this.OnTargetSystemFound));
			this.SpaceController.SetPlanet(system.Def, force);
		}

		// Token: 0x060090DA RID: 37082 RVA: 0x00250ED0 File Offset: 0x0024F0D0
		private void OnTimeSkipTravelPathFound(AStar.AStarResult result)
		{
			int count = result.path.Count;
			int num = 0;
			StarSystem starSystem = null;
			for (int i = 0; i < count; i++)
			{
				StarSystemNode starSystemNode = (StarSystemNode)result.path[i];
				if (i + 1 < count)
				{
					num += starSystemNode.Cost;
				}
				else
				{
					num += starSystemNode.System.JumpDistance;
					starSystem = starSystemNode.System;
				}
			}
			this.SetCurrentSystem(starSystem, false, false);
			this.OnDayPassed(num);
		}

		// Token: 0x060090DB RID: 37083 RVA: 0x00250F48 File Offset: 0x0024F148
		private void OnTargetSystemFound(AStar.AStarResult result)
		{
			this.CurSystem.RefreshSystem();
			if (this.UXAttached)
			{
				this.RoomManager.ShipRoom.RefreshData();
			}
			this.SetSystemOwnerReputation();
			this.NearestToTarget = null;
			if (result != null && result.path.Count >= 0)
			{
				int num = 0;
				if (result.path.Count > 1)
				{
					num = 1;
				}
				this.NearestToTarget = ((StarSystemNode)result.path[num]).System;
			}
			if (this.NearestToTarget == null || this.NearestToTarget == this.CurSystem)
			{
				List<StarSystem> availableNeighborSystem = this.Starmap.GetAvailableNeighborSystem(this.CurSystem);
				if (availableNeighborSystem == null || availableNeighborSystem.Count < 1)
				{
					SimGameState.logger.LogError(string.Format("[Breadcrumbs] Cannot find a breadcrumb for cur system {0} to target system {1}!!", this.CurSystem.ID, this.TargetSystem.ID));
				}
				else
				{
					availableNeighborSystem.Shuffle<StarSystem>();
					this.NearestToTarget = availableNeighborSystem[0];
				}
			}
			if (this.NearestToTarget != null)
			{
				this.GeneratePotentialContracts(true, null, this.NearestToTarget, false);
			}
		}

		// Token: 0x060090DC RID: 37084 RVA: 0x00251050 File Offset: 0x0024F250
		public void AddSpecialEvent(SimGameForcedEvent evt, Pilot pilot)
		{
			SimGameEventTracker simGameEventTracker = new SimGameEventTracker();
			SimGameEventDef simGameEventDef = this.DataManager.SimGameEventDefs.Get(evt.EventID);
			simGameEventTracker.InitForcedEvent(evt.Scope, evt.MinDaysWait, evt.MaxDaysWait, evt.Probability, simGameEventDef, pilot, this, null, null);
			this.specialEventTracker.Add(simGameEventTracker);
		}

		// Token: 0x060090DD RID: 37085 RVA: 0x002510AC File Offset: 0x0024F2AC
		public bool DEBUG_AddSpecialEventAndPopulateAdditionalObjects(SimGameForcedEvent evt, Pilot pilot, SimGameReport.ReportEntry entry)
		{
			SimGameEventTracker simGameEventTracker = new SimGameEventTracker();
			SimGameEventDef simGameEventDef = this.DataManager.SimGameEventDefs.Get(evt.EventID);
			simGameEventTracker.InitForcedEvent(evt.Scope, evt.MinDaysWait, evt.MaxDaysWait, evt.Probability, simGameEventDef, pilot, this, null, null);
			bool flag = simGameEventTracker.DEBUG_SetAdditionalObjectsForPredefinedEvent(entry);
			if (flag)
			{
				this.specialEventTracker.Add(simGameEventTracker);
			}
			return flag;
		}

		// Token: 0x060090DE RID: 37086 RVA: 0x00251110 File Offset: 0x0024F310
		private void ReputationDayPassed()
		{
			for (int i = 0; i < this.displayedFactions.Count; i++)
			{
				FactionValue factionByName = FactionEnumeration.GetFactionByName(this.displayedFactions[i]);
				if (!factionByName.IsAuriganRestoration)
				{
					if (!this.CanNotifyOfNewFactionRep())
					{
						break;
					}
					this.TestReputationLevelChange(factionByName);
				}
			}
		}

		// Token: 0x060090DF RID: 37087 RVA: 0x0025115C File Offset: 0x0024F35C
		public bool GetRemainingTimeFunding(out int timeRemaining)
		{
			FinancesConstantsDef finances = this.Constants.Finances;
			float num = (float)this.GetExpenditures(false);
			float num2 = (float)this.Funds;
			float num3 = num / (float)finances.QuarterLength * (float)(finances.QuarterLength - this.DayRemainingInQuarter);
			float num4 = (num2 - (float)Mathf.CeilToInt(num3)) / num * (float)(finances.QuarterLength / finances.MonthLength);
			timeRemaining = Mathf.FloorToInt(num4);
			bool flag = true;
			if (num4 < 1f)
			{
				timeRemaining = Mathf.FloorToInt(num4 * (float)finances.MonthLength);
				flag = false;
			}
			return flag;
		}

		// Token: 0x060090E0 RID: 37088 RVA: 0x002511E3 File Offset: 0x0024F3E3
		public int GetShipBaseMaintenanceCost()
		{
			if (this.CurDropship == DropshipType.Argo)
			{
				return this.Constants.Finances.ArgoBaseMaintenanceCost;
			}
			return this.Constants.Finances.LeopardBaseMaintenanceCost;
		}

		// Token: 0x060090E1 RID: 37089 RVA: 0x0025120F File Offset: 0x0024F40F
		public int GetExpenditures(bool proRate = false)
		{
			return this.GetExpenditures(this.ExpenditureLevel, proRate);
		}

		// Token: 0x060090E2 RID: 37090 RVA: 0x00251220 File Offset: 0x0024F420
		public int GetExpenditures(EconomyScale expenditureLevel, bool proRate = false)
		{
			FinancesConstantsDef finances = this.Constants.Finances;
			int num = this.GetShipBaseMaintenanceCost();
			for (int i = 0; i < this.ShipUpgrades.Count; i++)
			{
				num += Mathf.CeilToInt((float)this.ShipUpgrades[i].AdditionalCost * this.Constants.CareerMode.ArgoMaintenanceMultiplier);
			}
			foreach (MechDef mechDef in this.ActiveMechs.Values)
			{
				num += finances.MechCostPerQuarter;
			}
			for (int j = 0; j < this.PilotRoster.Count; j++)
			{
				num += this.GetMechWarriorValue(this.PilotRoster[j].pilotDef);
			}
			float expenditureCostModifier = this.GetExpenditureCostModifier(expenditureLevel);
			num -= (proRate ? this.ProRateRefund : 0);
			num = Mathf.CeilToInt((float)num * expenditureCostModifier);
			return num;
		}

		// Token: 0x060090E3 RID: 37091 RVA: 0x00251328 File Offset: 0x0024F528
		private void DeductQuarterlyFunds(int quarterPassed)
		{
			int expenditures = this.GetExpenditures(false);
			this.AddFunds(-expenditures * quarterPassed, "SimGame_Monthly", false, true);
			if (!this.IsGameOverCondition(false))
			{
				this.interruptQueue.QueueFinancialReport();
			}
			this.RoomManager.RefreshDisplay();
			this.OnNewQuarterBegin();
		}

		// Token: 0x060090E4 RID: 37092 RVA: 0x00251374 File Offset: 0x0024F574
		public void AddFunds(int val, string sourceID = null, bool updateBurndown = true, bool updateFundsGained = true)
		{
			if (sourceID == null)
			{
				sourceID = "SimGameState";
			}
			this.companyStats.ModifyStat<int>(sourceID, 0, "Funds", StatCollection.StatOperation.Int_Add, val, -1, true);
			if (val > 0 && updateFundsGained)
			{
				this.companyStats.ModifyStat<int>(sourceID, 0, "FundsEverGained", StatCollection.StatOperation.Int_Add, val, -1, true);
			}
			if (updateBurndown)
			{
				this.RoomManager.RefreshDisplay();
				if (this.UXAttached && this.Funds < 0)
				{
					this.ShowNegativeBalancePopup();
				}
			}
			SimGameFundsChangeMessage simGameFundsChangeMessage = new SimGameFundsChangeMessage(this.Funds);
			this.MessageCenter.PublishMessage(simGameFundsChangeMessage);
		}

		// Token: 0x060090E5 RID: 37093 RVA: 0x002513FF File Offset: 0x0024F5FF
		public void ShowHeavyMetalLootPopup(UnityAction onClose)
		{
			this.onHeavyMetalLootPopupClosed = onClose;
			LazySingletonBehavior<UIManager>.Instance.GetOrCreatePopupModule<HeavyMetalContentReviewPopup>("", true).Initialize(new UnityAction(this.OnHeavyMetalLootPopupAccepted), new UnityAction(this.OnHeavyMetalLootPopupDeclined));
		}

		// Token: 0x060090E6 RID: 37094 RVA: 0x00251438 File Offset: 0x0024F638
		public void ShowHeavyMetalNewMechPopup(UnityAction onClose)
		{
			ImageAndTextInfoPopup orCreatePopupModule = LazySingletonBehavior<UIManager>.Instance.GetOrCreatePopupModule<ImageAndTextInfoPopup>("", true);
			CastDef castDef = this.DataManager.CastDefs.Get("castDef_DariusDefault");
			orCreatePopupModule.Initialize("A PERIPHERY OF METAL", "This is temporary text that needs a blurb made for it. This is to inform users of free content that arrives with the release of 1.8", this.DataManager.SpriteCache.GetSprite("uixTxrSpot_flashpointExample"), castDef, onClose);
		}

		// Token: 0x060090E7 RID: 37095 RVA: 0x00251491 File Offset: 0x0024F691
		private void OnHeavyMetalLootPopupAccepted()
		{
			this.interruptQueue.QueueRewardsPopup("itemCollection_HM_careerStarter");
			if (this.onHeavyMetalLootPopupClosed != null)
			{
				this.onHeavyMetalLootPopupClosed();
			}
			this.onHeavyMetalLootPopupClosed = null;
		}

		// Token: 0x060090E8 RID: 37096 RVA: 0x002514BE File Offset: 0x0024F6BE
		private void OnHeavyMetalLootPopupDeclined()
		{
			if (this.onHeavyMetalLootPopupClosed != null)
			{
				this.onHeavyMetalLootPopupClosed();
			}
			this.onHeavyMetalLootPopupClosed = null;
		}

		// Token: 0x060090E9 RID: 37097 RVA: 0x002514DC File Offset: 0x0024F6DC
		public void CompleteFlashpoint(Flashpoint fp, FlashpointEndType howItEnded, string rewardMilestoneID = null, string rewardItemCollectionID = null, Action failNotificationAction = null)
		{
			if (fp == null)
			{
				return;
			}
			fp.GatherCurrentFactionReputations();
			if (this.availableFlashpointList.Contains(fp))
			{
				this.availableFlashpointList.Remove(fp);
			}
			if (howItEnded == FlashpointEndType.Completed)
			{
				fp.OnFlashpointSucceeded(rewardMilestoneID, rewardItemCollectionID);
				this.completedFlashpoints.Add(fp.Def.Description.Id);
				return;
			}
			if (howItEnded == FlashpointEndType.Failed)
			{
				if (this.interruptQueue.FPEnterSystemIsOpen)
				{
					this.ShowFlashpointFailedPopup(failNotificationAction, fp.Def.Description.Name);
					return;
				}
				this.ShowFlashpointFailedNotif(failNotificationAction, fp.Def.Description.Name);
				return;
			}
			else
			{
				if (howItEnded == FlashpointEndType.CompletedWithoutReward)
				{
					this.completedFlashpoints.Add(fp.Def.Description.Id);
					this.ShowFlashpointCompletedWithoutRewardNotif(failNotificationAction, fp.Def.Description.Name);
					return;
				}
				if (failNotificationAction != null)
				{
					failNotificationAction();
				}
				return;
			}
		}

		// Token: 0x060090EA RID: 37098 RVA: 0x002515C0 File Offset: 0x0024F7C0
		public void ClearActiveFlashpoint()
		{
			if (this.activeFlashpoint != null)
			{
				this.SaveActiveContractName = this.ActiveFlashpoint.Def.Description.Name;
				this.activeFlashpoint.SetStatus(Flashpoint.Status.PAUSED);
				this.activeFlashpoint = null;
				this.Context.ClearObject(GameContextObjectTagEnum.TargetFlashpoint);
				if (this.CurRoomState == DropshipLocation.NAVIGATION)
				{
					this.RoomManager.NavRoom.RefreshData();
				}
				this.TriggerSaveNow(SaveReason.SIM_GAME_COMPLETED_CONTRACT, SimGameState.TriggerSaveNowOption.QUEUE_IF_NEEDED);
			}
		}

		// Token: 0x060090EB RID: 37099 RVA: 0x00251634 File Offset: 0x0024F834
		public void SetActiveFlashpoint(Flashpoint fp)
		{
			this.ClearActiveFlashpoint();
			if (fp == null || fp.CurStatus == Flashpoint.Status.WAITING_FOR_DATA)
			{
				return;
			}
			this.activeFlashpoint = fp;
			this.activeFlashpoint.GatherCurrentFactionReputations();
			this.Context.SetObject(GameContextObjectTagEnum.TargetFlashpoint, fp);
			if (fp.CurSystem == this.CurSystem && this.TravelState == SimGameTravelStatus.IN_SYSTEM)
			{
				this.activeFlashpoint.SetStatus(Flashpoint.Status.IN_PROGRESS);
			}
			else
			{
				this.activeFlashpoint.SetStatus(Flashpoint.Status.SELECTED_ENROUTE);
			}
			this.activeFlashpoint.MilestoneCheck(false);
			this.SaveActiveContractName = this.ActiveFlashpoint.Def.Description.Name;
			this.TriggerSaveNow(SaveReason.SIM_GAME_CONTRACT_ACCEPTED, SimGameState.TriggerSaveNowOption.QUEUE_IF_NEEDED);
		}

		// Token: 0x060090EC RID: 37100 RVA: 0x0000D184 File Offset: 0x0000B384
		private void OnInterruptQueueComplete()
		{
		}

		// Token: 0x060090ED RID: 37101 RVA: 0x002516D8 File Offset: 0x0024F8D8
		private void FlashpointDayPassed()
		{
			if (this.SimGameMode == SimGameState.SimGameType.CAREER && this.DataManager.ContentPackIndex.IsContentPackOwned("heavymetal", true) && !this.companyStats.ContainsStatistic("HasSeenHeavyMetalLootPopup"))
			{
				this.companyStats.AddStatistic<int>("HasSeenHeavyMetalLootPopup", 1);
				this.interruptQueue.QueueHeavyMetalLootPopupEntry();
			}
			if (!this.companyTags.Contains(this.Constants.Flashpoints.SystemUseFlashpointsTag))
			{
				if (this.SimGameMode != SimGameState.SimGameType.CAREER || this.DaysPassed < this.careerModeFlashpointStartDate)
				{
					return;
				}
				this.CompanyTags.Add(this.Constants.Flashpoints.SystemUseFlashpointsTag);
			}
			if (this.activeFlashpoint != null)
			{
				this.activeFlashpoint.OnDayPassed();
			}
			for (int i = this.availableFlashpointList.Count - 1; i >= 0; i--)
			{
				Flashpoint flashpoint = this.availableFlashpointList[i];
				if (flashpoint != this.activeFlashpoint)
				{
					flashpoint.OnDayPassed();
					if (flashpoint.CurStatus == Flashpoint.Status.TIMED_OUT || !this.MeetsRequirements(flashpoint.Def.Requirements))
					{
						this.availableFlashpointList.Remove(flashpoint);
						if (flashpoint.CurStatus != Flashpoint.Status.TIMED_OUT)
						{
							flashpoint.SetStatus(Flashpoint.Status.REMOVED);
						}
						SimGameFlashpointAddedMessage simGameFlashpointAddedMessage = new SimGameFlashpointAddedMessage(flashpoint);
						this.MessageCenter.PublishMessage(simGameFlashpointAddedMessage);
					}
				}
			}
			if (this.flashpointPool.Count > 0)
			{
				this.CheckForNewFlashpoints(true);
			}
		}

		// Token: 0x060090EE RID: 37102 RVA: 0x00251834 File Offset: 0x0024FA34
		private void CheckForNewFlashpoints(bool updateCooldown)
		{
			bool flag = false;
			if (this.inFlashpointCooldown)
			{
				if (!updateCooldown)
				{
					return;
				}
				this.flashpointCooldownDays--;
				if (this.flashpointCooldownDays <= 0)
				{
					this.inFlashpointCooldown = false;
				}
			}
			else
			{
				int num = 0;
				bool flag2 = false;
				int num2 = this.MaxGenFlashpointsPerDay;
				List<FlashpointDef> list = new List<FlashpointDef>();
				list = this.initialFlashpointPool.FindAll((FlashpointDef flash) => !this.completedFlashpoints.Contains(flash.Description.Id) && this.availableFlashpointList.Find((Flashpoint fp) => fp.Def.Description.Id == flash.Description.Id) == null);
				if (list.Count > 0)
				{
					if (this.completedFlashpoints.Count == 0)
					{
						num2 = list.Count;
					}
					flag2 = true;
				}
				while (num < num2 && this.availableFlashpointList.Count < this.MaxActiveFlashpoints)
				{
					string text = string.Empty;
					if (flag2)
					{
						list.Shuffle<FlashpointDef>();
						FlashpointDef flashpointDef = list[0];
						list.RemoveAt(0);
						if (flashpointDef != null)
						{
							text = flashpointDef.Description.Id;
						}
						if (list.Count == 0)
						{
							flag2 = false;
						}
					}
					Flashpoint flashpoint = this.GenerateFlashpoint(text, null, false);
					if (flashpoint == null)
					{
						break;
					}
					num++;
					this.availableFlashpointList.Add(flashpoint);
					flag = true;
					if (!this.companyStats.ContainsStatistic("HasSeenFlashpointNotification"))
					{
						this.companyStats.AddStatistic<int>("HasSeenFlashpointNotification", 1);
						this.interruptQueue.QueueFlashpointsExistNotificationEntry();
					}
					SimGameFlashpointAddedMessage simGameFlashpointAddedMessage = new SimGameFlashpointAddedMessage(flashpoint);
					this.MessageCenter.PublishMessage(simGameFlashpointAddedMessage);
				}
			}
			if (flag)
			{
				int num3;
				int num4;
				if (this.completedFlashpoints.Count > 0)
				{
					num3 = this.FlashpointMinCooldown;
					num4 = this.FlashpointMaxCooldown;
				}
				else
				{
					num3 = this.InitialFlashpointMinCooldown;
					num4 = this.InitialFlashpointMaxCooldown;
				}
				this.flashpointCooldownDays = this.NetworkRandom.Int(num3, num4 + 1);
				this.inFlashpointCooldown = true;
			}
		}

		// Token: 0x060090EF RID: 37103 RVA: 0x002519E0 File Offset: 0x0024FBE0
		public Flashpoint GenerateFlashpoint(string flashpointDefID = null, string starSystemDefID = null, bool showForceInjectMessage = false)
		{
			WeightedList<FlashpointDef> weightedList = new WeightedList<FlashpointDef>(WeightedListType.WeightedRandomUseOnce, null, null, 0);
			List<FlashpointDef> list = new List<FlashpointDef>();
			if (!string.IsNullOrEmpty(flashpointDefID))
			{
				FlashpointDef flashpointDef = this.flashpointPool.Find((FlashpointDef flash) => flash.Description.Id == flashpointDefID);
				if (flashpointDef == null)
				{
					Debug.LogError(string.Format("Could not find specified FlashpointDef [{0}] - aborting FlashpointGenearation.", flashpointDefID));
					return null;
				}
				if (!this.MeetsRequirements(flashpointDef.Requirements) && showForceInjectMessage)
				{
					Debug.LogError(string.Format("Not all requirements are met, but we're injecting flashpoint[{0}] anyway.", flashpointDefID));
				}
				weightedList.Add(flashpointDef, flashpointDef.Weight);
			}
			if (weightedList.Count == 0)
			{
				for (int i = 0; i < this.flashpointPool.Count; i++)
				{
					FlashpointDef flashpointDef2 = this.flashpointPool[i];
					string id = flashpointDef2.Description.Id;
					if (!this.completedFlashpoints.Contains(id))
					{
						bool flag = false;
						for (int j = 0; j < this.availableFlashpointList.Count; j++)
						{
							if (this.availableFlashpointList[j].Def.Description.Id == id)
							{
								flag = true;
								break;
							}
						}
						if (!flag && this.MeetsRequirements(flashpointDef2.Requirements) && (flashpointDef2.Tags == null || !flashpointDef2.Tags.Contains(Tags_MDDExtenstions.BLACKLISTED_TAG)))
						{
							if (this.flashpointDiscardPile.Contains(id))
							{
								list.Add(flashpointDef2);
							}
							else
							{
								weightedList.Add(flashpointDef2, flashpointDef2.Weight);
							}
						}
					}
				}
			}
			WeightedList<string> weightedList2 = new WeightedList<string>(WeightedListType.SimpleRandom, null, null, 0);
			if (starSystemDefID == "local")
			{
				weightedList2.Add(this.CurSystem.Def.CoreSystemID, 0);
			}
			else if (!string.IsNullOrEmpty(starSystemDefID))
			{
				if (this.starSystems.Find((StarSystem ss) => ss.ID == starSystemDefID) != null)
				{
					weightedList2.Add(starSystemDefID, 0);
				}
				else
				{
					Debug.LogError(string.Format("Could not find StarYstem [{0}] - looking for a valid planet instead.", starSystemDefID));
				}
			}
			if (weightedList2.Count == 0)
			{
				weightedList2.AddRange(this.starDict.Keys);
			}
			new WeightedList<FactionDef>(WeightedListType.SimpleRandom, null, null, 0);
			StarSystem starSystem = null;
			FactionDef factionDef = null;
			if (this.Context.HasObject(GameContextObjectTagEnum.TargetStarSystem))
			{
				starSystem = this.Context.GetObject(GameContextObjectTagEnum.TargetStarSystem) as StarSystem;
			}
			if (this.Context.HasObject(GameContextObjectTagEnum.FlashpointEmployer))
			{
				factionDef = this.Context.GetObject(GameContextObjectTagEnum.FlashpointEmployer) as FactionDef;
			}
			StarSystem starSystem2 = null;
			FactionDef factionDef2 = null;
			FlashpointDef flashpointDef3 = null;
			while (weightedList.ActiveListCount > 0)
			{
				FlashpointDef next = weightedList.GetNext(true);
				List<string> list2 = new List<string>(next.PotentialEmployers);
				list2.Shuffle<string>();
				factionDef2 = this.GetFactionDef(list2[0]);
				this.Context.SetObject(GameContextObjectTagEnum.FlashpointEmployer, factionDef2);
				starSystem2 = null;
				weightedList2.Reset(false);
				while (weightedList2.ActiveListCount > 0)
				{
					starSystem2 = this.starDict[weightedList2.GetNext(true)];
					for (int k = 0; k < this.AvailableFlashpoints.Count; k++)
					{
						if (this.AvailableFlashpoints[k].CurSystem.ID == starSystem2.ID)
						{
							starSystem2 = null;
							break;
						}
					}
					if (starSystem2 != null)
					{
						this.Context.SetObject(GameContextObjectTagEnum.TargetStarSystem, starSystem2);
						if (SimGameState.MeetsRequirements(next.LocationRequirements, starSystem2.Tags, starSystem2.Stats, null))
						{
							break;
						}
						if (!string.IsNullOrEmpty(starSystemDefID))
						{
							Debug.LogError(string.Format("StarSystem[{0}] does not meet requirements, but we're using it anyway", starSystemDefID));
							break;
						}
						starSystem2 = null;
					}
				}
				if (starSystem2 != null)
				{
					flashpointDef3 = next;
					break;
				}
				Debug.LogError(string.Format("Unable to find valid system for flashpoint {0}", next.Description.Name));
			}
			if (starSystem != null)
			{
				this.Context.SetObject(GameContextObjectTagEnum.TargetStarSystem, starSystem);
			}
			if (factionDef != null)
			{
				this.Context.SetObject(GameContextObjectTagEnum.FlashpointEmployer, factionDef);
			}
			if (flashpointDef3 == null)
			{
				return null;
			}
			return new Flashpoint(flashpointDef3, this, starSystem2, factionDef2.FactionValue);
		}

		// Token: 0x060090F0 RID: 37104 RVA: 0x00251DFC File Offset: 0x0024FFFC
		public Flashpoint GenerateFlashpointCommand(string flashpointDefID, string starSystemDefID)
		{
			if (flashpointDefID == "random")
			{
				flashpointDefID = "";
			}
			if (starSystemDefID == "normal")
			{
				starSystemDefID = "";
			}
			Flashpoint flashpoint = this.GenerateFlashpoint(flashpointDefID, starSystemDefID, true);
			if (string.IsNullOrEmpty(flashpointDefID))
			{
				flashpointDefID = "random";
			}
			if (string.IsNullOrEmpty(starSystemDefID))
			{
				starSystemDefID = "normal";
			}
			if (flashpoint == null)
			{
				Debug.LogError(string.Format("Unable to generate flashpoint with these parameters: flashpointDefID[{0}], starSystemDefID[{1}]", flashpointDefID, starSystemDefID));
				return null;
			}
			Debug.Log(string.Format("Generated flashpoint:[{0}] at system: [{1}]", flashpoint.Def.Description.Id, flashpoint.CurSystem.Name));
			this.availableFlashpointList.Add(flashpoint);
			if (!this.companyStats.ContainsStatistic("HasSeenFlashpointNotification"))
			{
				this.companyStats.AddStatistic<int>("HasSeenFlashpointNotification", 1);
				this.interruptQueue.QueueFlashpointsExistNotificationEntry();
			}
			SimGameFlashpointAddedMessage simGameFlashpointAddedMessage = new SimGameFlashpointAddedMessage(flashpoint);
			this.MessageCenter.PublishMessage(simGameFlashpointAddedMessage);
			return flashpoint;
		}

		// Token: 0x060090F1 RID: 37105 RVA: 0x00251EEC File Offset: 0x002500EC
		public void ShowIntroductionToFlashpointsWithFlashpointDLC(UnityAction onClose)
		{
			CastDef castDef = this.DataManager.CastDefs.Get("castDef_DariusDefault");
			LazySingletonBehavior<UIManager>.Instance.GetOrCreatePopupModule<ImageAndTextInfoPopup>("", true).Initialize("FLASHPOINTS", "Inner Sphere politics impact everything they touch, even down here in the Periphery. Flashpoints between the Great Houses are developing all over the map, and our faction reps are paying good money to resolve them. We're talking about high-risk, high-reward jobs composed of multiple combat drops, with rewards at the end to match.\n \nI'll post any active Flashpoints on the Star Map, Commander. You can review them whenever you're ready.", this.DataManager.SpriteCache.GetSprite("uixTxrSpot_flashpointExample"), castDef, onClose);
		}

		// Token: 0x060090F2 RID: 37106 RVA: 0x00251F45 File Offset: 0x00250145
		public void CheckForNewStarmapTechNotification()
		{
			if (!this.companyStats.ContainsStatistic("HasSeenNewStarmapTechNotification"))
			{
				this.companyStats.AddStatistic<int>("HasSeenNewStarmapTechNotification", 1);
				this.interruptQueue.QueueNewStarmapTechNotification();
			}
		}

		// Token: 0x060090F3 RID: 37107 RVA: 0x00251F78 File Offset: 0x00250178
		public void ShowNewStarmapTechNotification(UnityAction onClose)
		{
			CastDef castDef = this.DataManager.CastDefs.Get("castDef_ComStarDefault");
			LazySingletonBehavior<UIManager>.Instance.GetOrCreatePopupModule<MercNetUpdatePopup>("", true).Initialize("Merc Net System Update", "Announcing ComStar Starmap v1.7", "Whether for trade, settlement, or military expedition, traveling via our patented KF Drive should have no surprises. [[DM.BaseDescriptionDefs[LoreComStar], ComStar]] is proud to announce the updated MercNet Starmap interface:\n\n<color=#85DBF6FF>1 • STORE FILTERS </color> Looking for specific gear? See which systems are likely to carry what you need so you can shop with purpose and save on your travel budget.\n\n<color=#85DBF6FF>2 • BIOME FILTERS</color> Choose to see systems with specific climate patterns or features to make sure your destination is accomodating.\n\n<color=#85DBF6FF>3 • DIFFICULTY FILTERS </color> Whether you want to challenge your mercenary outfit for big rewards, or stroll through a system like a cakewalk, we have your plans covered.\n\n<color=#85DBF6FF>4 • EXTENDED COVERAGE</color> Now with <color=#85DBF6FF>50</color> more system destinations, rich with potential conflict resolution business and lucrative salvage opportunites. New star indicators let you see where you have been, and where you have yet to explore.\n\nMembers of our Platinum and Uranium level plans will find the new filter features in the upper right of your Starmap interface. If you have any questions or concerns, don't hesitate to contact your local [[DM.BaseDescriptionDefs[LoreComStar],ComStar]] representative, and we'll get you locked-in.\n\nMay the peace of Blake be with you!", this.DataManager.SpriteCache.GetSprite("uixTxrSpot_StarmapV2-Example"), castDef, onClose);
		}

		// Token: 0x060090F4 RID: 37108 RVA: 0x00251FD6 File Offset: 0x002501D6
		public void ShowCareerModeFlashpointInformation(UnityAction onClose)
		{
			this.ShowIntroductionToFlashpointsWithFlashpointDLC(onClose);
		}

		// Token: 0x060090F5 RID: 37109 RVA: 0x00251FDF File Offset: 0x002501DF
		public void FlashpointMilestoneCheck(bool force = false)
		{
			if (this.activeFlashpoint == null)
			{
				return;
			}
			if (this.activeFlashpoint.CurStatus != Flashpoint.Status.IN_PROGRESS)
			{
				return;
			}
			this.activeFlashpoint.MilestoneCheck(force);
		}

		// Token: 0x060090F6 RID: 37110 RVA: 0x00252005 File Offset: 0x00250205
		public void DEBUG_ShowIntroductionToFlashpoints()
		{
			this.interruptQueue.QueueFlashpointsExistNotificationEntry();
		}

		// Token: 0x060090F7 RID: 37111 RVA: 0x0000D184 File Offset: 0x0000B384
		public void DEBUG_AddFlashpoint(string fpName)
		{
		}

		// Token: 0x060090F8 RID: 37112 RVA: 0x00252013 File Offset: 0x00250213
		public void DEBUG_ForcePopulateFlashpoints()
		{
			this.inFlashpointCooldown = false;
			this.CheckForNewFlashpoints(false);
		}

		// Token: 0x060090F9 RID: 37113 RVA: 0x00252023 File Offset: 0x00250223
		public void DEBUG_AllowDebug(bool allowDebug)
		{
			this.AllowDebug = allowDebug;
		}

		// Token: 0x060090FA RID: 37114 RVA: 0x0025202C File Offset: 0x0025022C
		public void PlayVideo(string video)
		{
			SGVideoPlayer videoPlayer = this.GetVideoPlayer();
			videoPlayer.gameObject.SetActive(true);
			this.VideoPlayerActive = true;
			LazySingletonBehavior<UIManager>.Instance.StartCoroutine(videoPlayer.PauseGameAudio(AudioEventManager.AudioConstants.audioFadeDuration));
			videoPlayer.PlayVideo(video, Strings.CurrentCulture, new Action<string>(this.OnVideoComplete));
		}

		// Token: 0x060090FB RID: 37115 RVA: 0x00252088 File Offset: 0x00250288
		private void OnVideoComplete(string videoName)
		{
			SGVideoPlayer videoPlayer = this.GetVideoPlayer();
			videoPlayer.gameObject.SetActive(false);
			this.VideoPlayerActive = false;
			LazySingletonBehavior<UIManager>.Instance.StartCoroutine(videoPlayer.ResumeGameAudio(AudioEventManager.AudioConstants.audioFadeDuration));
			videoPlayer.Pool(false);
			if (DebugBridge.DisableCinematics)
			{
				LazySingletonBehavior<UIManager>.Instance.StartCoroutine(this.DelayVideoComplete());
				return;
			}
			this.UpdateMilestones();
		}

		// Token: 0x060090FC RID: 37116 RVA: 0x002520F1 File Offset: 0x002502F1
		private IEnumerator DelayVideoComplete()
		{
			yield return new WaitForSeconds(0.25f);
			this.UpdateMilestones();
			yield break;
		}

		// Token: 0x060090FD RID: 37117 RVA: 0x00252100 File Offset: 0x00250300
		public void StartCharacterCreation()
		{
			this.CharacterCreation.CreateCharacter(new UnityAction<Pilot>(this.OnCharacterCreationComplete), this);
		}

		// Token: 0x060090FE RID: 37118 RVA: 0x0025211C File Offset: 0x0025031C
		private void OnCharacterCreationComplete(Pilot p)
		{
			this.DataManager.PortraitManager.ClearLargePortraits();
			p.pilotDef.DataManager = this.DataManager;
			this.commander = p;
			p.pilotDef.SetHiringHallStats(true, false, true, false);
			this.commander.AddExperience(0, "Starting XP", this.Constants.Story.CommanderStartingExperience);
			this.Context.SetObject(GameContextObjectTagEnum.Commander, this.commander);
			this.SetCommanderDefaultHeraldry();
		}

		// Token: 0x060090FF RID: 37119 RVA: 0x0025219C File Offset: 0x0025039C
		private void SetCommanderDefaultHeraldry()
		{
			string text = null;
			foreach (string text2 in this.commander.pilotDef.PilotTags)
			{
				if (text2.StartsWith("commander_career"))
				{
					string text3 = text2.Replace("commander_career", "heraldrydef_career").ToLowerInvariant();
					if (this.DataManager.ResourceLocator.EntryByID(text3, BattleTechResourceType.HeraldryDef, false) != null)
					{
						text = text3;
						break;
					}
				}
			}
			if (!string.IsNullOrEmpty(text))
			{
				this.RequestItem<HeraldryDef>(text, new Action<HeraldryDef>(this.CompleteSetCommanderDefaultHeraldry), BattleTechResourceType.HeraldryDef);
			}
		}

		// Token: 0x06009100 RID: 37120 RVA: 0x00252248 File Offset: 0x00250448
		private void CompleteSetCommanderDefaultHeraldry(HeraldryDef def)
		{
			this.Player1sMercUnitHeraldryDef.textureLogoID = def.textureLogoID;
			this.Player1sMercUnitHeraldryDef.primaryMechColorID = def.primaryMechColorID;
			this.Player1sMercUnitHeraldryDef.secondaryMechColorID = def.secondaryMechColorID;
			this.Player1sMercUnitHeraldryDef.tertiaryMechColorID = def.tertiaryMechColorID;
			if (this.Player1sMercUnitHeraldryDef.Description.Name == this.Constants.Player1sMercUnitHeraldryDef.Description.Name)
			{
				this.Player1sMercUnitHeraldryDef.UpdateName(Strings.T("{0}'s Marauders", new object[]
				{
					new NonLocalizableText(this.commander.Description.Callsign, Array.Empty<object>())
				}));
			}
			this.Player1sMercUnitHeraldryDef.Refresh();
			this.RoomManager.SetHeaderCompanyCrest(this.Player1sMercUnitHeraldryDef.textureLogoID);
			this.RoomManager.SetHeaderCompanyName(this.Player1sMercUnitHeraldryDef.Description.Name);
			this.CameraController.SetColors(this.Player1sMercUnitHeraldryDef);
			this.BattleTechGame.Save(SaveReason.SIM_GAME_FIRST_SAVE);
			this.UpdateMilestones();
		}

		// Token: 0x06009101 RID: 37121 RVA: 0x0025235E File Offset: 0x0025055E
		public void OnEventTriggered(SimGameEventDef eventDef, EventScope scope, SimGameEventTracker tracker)
		{
			this.interruptQueue.QueueEventPopup(eventDef, scope, tracker);
		}

		// Token: 0x06009102 RID: 37122 RVA: 0x0025236F File Offset: 0x0025056F
		public SimGameEventResultSet OnEventOptionSelected(SimGameEventOption option, SimGameEventTracker tracker, out List<TagSet> preAlteredResults)
		{
			return tracker.OnOptionSelected(option, out preAlteredResults);
		}

		// Token: 0x06009103 RID: 37123 RVA: 0x00252379 File Offset: 0x00250579
		public void OnEventDismissed(SimGameInterruptManager.EventPopupEntry entry)
		{
			this.RefreshInjuries();
			this.interruptQueue.PopupClosed(entry);
		}

		// Token: 0x06009104 RID: 37124 RVA: 0x0025238D File Offset: 0x0025058D
		public void MarkCampaignFailed()
		{
			this.campaignFailed = true;
		}

		// Token: 0x06009105 RID: 37125 RVA: 0x00252398 File Offset: 0x00250598
		public FactionDef GetFactionDef(string factionName)
		{
			FactionDef factionDef = null;
			if (this.factions.ContainsKey(factionName))
			{
				factionDef = this.factions[factionName];
			}
			return factionDef;
		}

		// Token: 0x06009106 RID: 37126 RVA: 0x002523C3 File Offset: 0x002505C3
		private void OnStarmapStoreManifestLoaded(CSVReader manifest)
		{
			this.starmapStoreManifest = manifest;
		}

		// Token: 0x06009107 RID: 37127 RVA: 0x002523CC File Offset: 0x002505CC
		public void OnCareerModeStart()
		{
			this.CharacterCreation.CreateCharacter(new UnityAction<Pilot>(this.OnCareerModeCharacterCreationComplete), this);
		}

		// Token: 0x06009108 RID: 37128 RVA: 0x002523E8 File Offset: 0x002505E8
		public void OnCareerModeCharacterCreationComplete(Pilot p)
		{
			this.DataManager.PortraitManager.ClearLargePortraits();
			p.pilotDef.DataManager = this.DataManager;
			this.commander = p;
			p.pilotDef.SetHiringHallStats(true, false, true, false);
			this.commander.AddExperience(0, "Starting XP", this.Constants.Story.CommanderStartingExperience);
			this.Context.SetObject(GameContextObjectTagEnum.Commander, this.commander);
			this.SetCommanderDefaultHeraldry();
			this.BattleTechGame.Save(SaveReason.SIM_GAME_FIRST_SAVE);
			this.SetSimRoomState(DropshipLocation.SHIP);
		}

		// Token: 0x06009109 RID: 37129 RVA: 0x0025247A File Offset: 0x0025067A
		public void GeneratePotentialContracts(bool clearExistingContracts, Action onContractGenComplete, StarSystem systemOverride = null, bool useCoroutine = false)
		{
			SceneSingletonBehavior<UnityGameInstance>.Instance.StartCoroutine(this.StartGeneratePotentialContractsRoutine(clearExistingContracts, onContractGenComplete, systemOverride, useCoroutine));
		}

		// Token: 0x0600910A RID: 37130 RVA: 0x00252492 File Offset: 0x00250692
		private IEnumerator StartGeneratePotentialContractsRoutine(bool clearExistingContracts, Action onContractGenComplete, StarSystem systemOverride, bool useWait)
		{
			int debugCount = 0;
			bool usingBreadcrumbs = systemOverride != null;
			if (useWait)
			{
				yield return new WaitForSeconds(0.2f);
			}
			StarSystem system;
			List<Contract> contractList;
			int maxContracts;
			if (usingBreadcrumbs)
			{
				system = systemOverride;
				contractList = this.CurSystem.SystemBreadcrumbs;
				maxContracts = this.CurSystem.CurMaxBreadcrumbs;
			}
			else
			{
				system = this.CurSystem;
				contractList = this.CurSystem.SystemContracts;
				maxContracts = Mathf.CeilToInt(system.CurMaxContracts);
			}
			if (clearExistingContracts)
			{
				contractList.Clear();
			}
			SimGameState.ContractDifficultyRange difficultyRange = this.GetContractRangeDifficultyRange(system, this.SimGameMode, this.GlobalDifficulty);
			Dictionary<int, List<ContractOverride>> potentialContracts = this.GetSinglePlayerProceduralContractOverrides(difficultyRange);
			WeightedList<MapAndEncounters> playableMaps = SimGameState.GetSinglePlayerProceduralPlayableMaps(system);
			Dictionary<string, WeightedList<SimGameState.ContractParticipants>> validParticipants = this.GetValidParticipants(system);
			if (!this.HasValidMaps(system, playableMaps) || !this.HasValidContracts(difficultyRange, potentialContracts) || !this.HasValidParticipants(system, validParticipants))
			{
				if (onContractGenComplete != null)
				{
					onContractGenComplete();
				}
				yield break;
			}
			this.ClearUsedBiomeFromDiscardPile(playableMaps);
			while (contractList.Count < maxContracts && debugCount < 1000)
			{
				int num = debugCount;
				debugCount = num + 1;
				IEnumerable<int> enumerable = playableMaps.Select((MapAndEncounters map) => map.Map.Weight);
				WeightedList<MapAndEncounters> weightedList = new WeightedList<MapAndEncounters>(WeightedListType.WeightedRandom, playableMaps.ToList(), enumerable.ToList<int>(), 0);
				this.FilterActiveMaps(weightedList, contractList);
				weightedList.Reset(false);
				MapAndEncounters mapAndEncounters = weightedList.GetNext(false);
				SimGameState.MapEncounterContractData mapEncounterContractData = this.FillMapEncounterContractData(system, difficultyRange, potentialContracts, validParticipants, mapAndEncounters);
				while (!mapEncounterContractData.HasContracts && weightedList.ActiveListCount > 0)
				{
					mapAndEncounters = weightedList.GetNext(false);
					mapEncounterContractData = this.FillMapEncounterContractData(system, difficultyRange, potentialContracts, validParticipants, mapAndEncounters);
				}
				system.SetCurrentContractFactions(null, null);
				if (mapEncounterContractData == null || mapEncounterContractData.Contracts.Count == 0)
				{
					if (this.mapDiscardPile.Count > 0)
					{
						this.mapDiscardPile.Clear();
					}
					else
					{
						debugCount = 1000;
						SimGameState.logger.LogError(string.Format("[CONTRACT] Unable to find any valid contracts for available map pool. Alert designers.", Array.Empty<object>()));
					}
				}
				GameContext gameContext = new GameContext(this.Context);
				gameContext.SetObject(GameContextObjectTagEnum.TargetStarSystem, system);
				Contract contract = this.CreateProceduralContract(system, usingBreadcrumbs, mapAndEncounters, mapEncounterContractData, gameContext);
				contractList.Add(contract);
				if (useWait)
				{
					yield return new WaitForSeconds(0.2f);
				}
			}
			if (debugCount >= 1000)
			{
				SimGameState.logger.LogError("Unable to fill contract list. Please inform AJ Immediately");
			}
			if (onContractGenComplete != null)
			{
				onContractGenComplete();
			}
			yield break;
		}

		// Token: 0x0600910B RID: 37131 RVA: 0x002524BE File Offset: 0x002506BE
		private bool HasValidParticipants(StarSystem system, Dictionary<string, WeightedList<SimGameState.ContractParticipants>> validParticipants)
		{
			if (!validParticipants.Any<KeyValuePair<string, WeightedList<SimGameState.ContractParticipants>>>())
			{
				this.LogErrorNoValidTargets(system);
				return false;
			}
			return true;
		}

		// Token: 0x0600910C RID: 37132 RVA: 0x002524D2 File Offset: 0x002506D2
		private static WeightedList<MapAndEncounters> GetSinglePlayerProceduralPlayableMaps(StarSystem system)
		{
			return MetadataDatabase.Instance.GetReleasedMapsAndEncountersBySinglePlayerProceduralContractTypeAndTags(system.Def.MapRequiredTags, system.Def.MapExcludedTags, system.Def.SupportedBiomes, true).ToWeightedList(WeightedListType.SimpleRandom);
		}

		// Token: 0x0600910D RID: 37133 RVA: 0x00252508 File Offset: 0x00250708
		private bool HasValidContracts(SimGameState.ContractDifficultyRange diffRange, Dictionary<int, List<ContractOverride>> potentialContracts)
		{
			if (!potentialContracts.Any((KeyValuePair<int, List<ContractOverride>> c) => c.Value.Count > 0))
			{
				Debug.LogError(string.Format("No valid contracts queried for difficulties between {0} and {1}, with a SCOPE of {2}", diffRange.MinDifficultyClamped, diffRange.MaxDifficultyClamped, this.ContractScope));
				return false;
			}
			return true;
		}

		// Token: 0x0600910E RID: 37134 RVA: 0x0025256F File Offset: 0x0025076F
		private bool HasValidMaps(StarSystem system, WeightedList<MapAndEncounters> contractMaps)
		{
			if (!contractMaps.Any<MapAndEncounters>())
			{
				Debug.LogError(string.Format("No valid map for System {0}", system.Name));
				return false;
			}
			return true;
		}

		// Token: 0x0600910F RID: 37135 RVA: 0x00252594 File Offset: 0x00250794
		private void FilterActiveMaps(WeightedList<MapAndEncounters> activeMaps, List<Contract> currentContracts)
		{
			List<MapAndEncounters> list = new List<MapAndEncounters>();
			for (int i = activeMaps.Count - 1; i >= 0; i--)
			{
				Map_MDD map = activeMaps[i].Map;
				if (this.mapDiscardPile.Contains(map.MapID) || this.DoesActiveFlashpointUseSameMap(map.MapName) || this.IsMapUsedInContracts(currentContracts, map.MapID))
				{
					list.Add(activeMaps[i]);
					activeMaps.RemoveAt(i);
				}
			}
			if (activeMaps.Count == 0)
			{
				this.mapDiscardPile = this.mapDiscardPile.Skip(Mathf.Max(0, Mathf.CeilToInt((float)(this.mapDiscardPile.Count / 2)))).ToList<string>();
				foreach (MapAndEncounters mapAndEncounters in list)
				{
					activeMaps.Add(mapAndEncounters, 0);
				}
			}
			activeMaps.Reset(false);
		}

		// Token: 0x06009110 RID: 37136 RVA: 0x00252690 File Offset: 0x00250890
		private void ClearUsedBiomeFromDiscardPile(WeightedList<MapAndEncounters> activeMaps)
		{
			foreach (IGrouping<long, MapAndEncounters> grouping in from m in activeMaps
				group m by m.Map.BiomeSkinID)
			{
				if (grouping.All((MapAndEncounters map) => this.mapDiscardPile.Contains(map.Map.MapID)))
				{
					foreach (var <>f__AnonymousType in (from map in grouping
						select new
						{
							Index = this.mapDiscardPile.IndexOf(map.Map.MapID),
							Map = map
						} into i
						orderby i.Index descending
						select i).Skip(Math.Max(1, grouping.Count<MapAndEncounters>() - 2)))
					{
						this.mapDiscardPile.RemoveAt(<>f__AnonymousType.Index);
					}
				}
			}
		}

		// Token: 0x06009111 RID: 37137 RVA: 0x002527B8 File Offset: 0x002509B8
		private SimGameState.MapEncounterContractData FillMapEncounterContractData(StarSystem system, SimGameState.ContractDifficultyRange diffRange, Dictionary<int, List<ContractOverride>> potentialContracts, Dictionary<string, WeightedList<SimGameState.ContractParticipants>> validTargets, MapAndEncounters level)
		{
			SimGameState.MapEncounterContractData mapEncounterContractData = new SimGameState.MapEncounterContractData();
			if (level != null)
			{
				bool flag = string.Compare("mapGeneral_jumbledKarst_aDes", level.Map.MapID) == 0;
				foreach (EncounterLayer_MDD encounterLayer_MDD in level.Encounters)
				{
					int num = (int)encounterLayer_MDD.ContractTypeRow.ContractTypeID;
					if (!flag || encounterLayer_MDD.Name.CompareTo("encGeneral_ThreeWayBattle") != 0)
					{
						if (mapEncounterContractData.Encounters.ContainsKey(num))
						{
							mapEncounterContractData.AddEncounter(num, encounterLayer_MDD);
						}
						else
						{
							if (potentialContracts.ContainsKey(num))
							{
								using (List<ContractOverride>.Enumerator enumerator = potentialContracts[num].GetEnumerator())
								{
									while (enumerator.MoveNext())
									{
										ContractOverride contractOverride = enumerator.Current;
										SimGameState.PotentialContract potentialContract;
										if (this.CreatePotentialContract(system, diffRange, contractOverride, validTargets, level, num, out potentialContract))
										{
											mapEncounterContractData.AddContract(num, potentialContract, contractOverride.weight);
											mapEncounterContractData.AddEncounter(num, encounterLayer_MDD);
										}
									}
									goto IL_108;
								}
							}
							Debug.LogWarning("There were no potential contracts for this contractType " + encounterLayer_MDD.ContractTypeRow.Name);
						}
					}
					IL_108:;
				}
			}
			return mapEncounterContractData;
		}

		// Token: 0x06009112 RID: 37138 RVA: 0x002528EC File Offset: 0x00250AEC
		private Contract CreateProceduralContract(StarSystem system, bool usingBreadcrumbs, MapAndEncounters level, SimGameState.MapEncounterContractData MapEncounterContractData, GameContext gameContext)
		{
			WeightedList<SimGameState.PotentialContract> flatContracts = MapEncounterContractData.FlatContracts;
			this.FilterContracts(flatContracts);
			SimGameState.PotentialContract next = flatContracts.GetNext(true);
			int id = next.contractOverride.ContractTypeValue.ID;
			MapEncounterContractData.Encounters[id].Shuffle<EncounterLayer_MDD>();
			string encounterLayerGUID = MapEncounterContractData.Encounters[id][0].EncounterLayerGUID;
			ContractOverride contractOverride = next.contractOverride;
			FactionValue employer = next.employer;
			FactionValue target = next.target;
			FactionValue employerAlly = next.employerAlly;
			FactionValue targetAlly = next.targetAlly;
			FactionValue neutralToAll = next.NeutralToAll;
			FactionValue hostileToAll = next.HostileToAll;
			int difficulty = next.difficulty;
			Contract contract;
			if (usingBreadcrumbs)
			{
				contract = this.CreateTravelContract(level.Map.MapName, level.Map.MapPath, encounterLayerGUID, next.contractOverride.ContractTypeValue, contractOverride, gameContext, employer, target, targetAlly, employerAlly, neutralToAll, hostileToAll, false, difficulty);
			}
			else
			{
				contract = new Contract(level.Map.MapName, level.Map.MapPath, encounterLayerGUID, next.contractOverride.ContractTypeValue, this.BattleTechGame, contractOverride, gameContext, true, difficulty, 0, null);
			}
			this.mapDiscardPile.Add(level.Map.MapID);
			this.contractDiscardPile.Add(contractOverride.ID);
			this.PrepContract(contract, employer, employerAlly, target, targetAlly, neutralToAll, hostileToAll, level.Map.BiomeSkinEntry.BiomeSkin, contract.Override.travelSeed, system);
			return contract;
		}

		// Token: 0x06009113 RID: 37139 RVA: 0x00252A68 File Offset: 0x00250C68
		private void FilterContracts(WeightedList<SimGameState.PotentialContract> flatValidContracts)
		{
			List<SimGameState.PotentialContract> list = new List<SimGameState.PotentialContract>();
			for (int i = flatValidContracts.Count - 1; i >= 0; i--)
			{
				if (this.contractDiscardPile.Contains(flatValidContracts[i].contractOverride.ID))
				{
					list.Add(flatValidContracts[i]);
					flatValidContracts.RemoveAt(i);
				}
			}
			if ((float)list.Count >= (float)flatValidContracts.Count * this.Constants.Story.DiscardPileToActiveRatio || flatValidContracts.Count == 0)
			{
				this.contractDiscardPile.Clear();
				foreach (SimGameState.PotentialContract potentialContract in list)
				{
					flatValidContracts.Add(potentialContract, 0);
				}
			}
		}

		// Token: 0x06009114 RID: 37140 RVA: 0x00252B38 File Offset: 0x00250D38
		private bool CreatePotentialContract(StarSystem system, SimGameState.ContractDifficultyRange diffRange, ContractOverride contractOvr, Dictionary<string, WeightedList<SimGameState.ContractParticipants>> validTargets, MapAndEncounters level, int encounterContractTypeID, out SimGameState.PotentialContract returnContract)
		{
			returnContract = default(SimGameState.PotentialContract);
			SimGameState.ChosenContractParticipants chosenContractParticipants;
			if (this.GetValidFaction(system, validTargets, contractOvr.requirementList, out chosenContractParticipants))
			{
				int num = this.NetworkRandom.Int(diffRange.MinDifficulty, diffRange.MaxDifficulty + 1);
				system.SetCurrentContractFactions(chosenContractParticipants.Employer, chosenContractParticipants.Target);
				if (this.DoesContractMeetRequirements(system, level, contractOvr))
				{
					returnContract = new SimGameState.PotentialContract
					{
						contractOverride = contractOvr,
						difficulty = num,
						employer = chosenContractParticipants.Employer,
						target = chosenContractParticipants.Target,
						employerAlly = chosenContractParticipants.EmployersAlly,
						targetAlly = chosenContractParticipants.TargetsAlly,
						NeutralToAll = chosenContractParticipants.NeutralToAll,
						HostileToAll = chosenContractParticipants.HostileToAll
					};
					return true;
				}
			}
			return false;
		}

		// Token: 0x06009115 RID: 37141 RVA: 0x00252C0C File Offset: 0x00250E0C
		private bool DoesActiveFlashpointUseSameMap(string mapName)
		{
			return this.ActiveFlashpoint != null && this.ActiveFlashpoint.ActiveContract != null && this.ActiveFlashpoint.ActiveContract.mapName == mapName;
		}

		// Token: 0x06009116 RID: 37142 RVA: 0x00252C3C File Offset: 0x00250E3C
		private bool IsMapUsedInContracts(IEnumerable<Contract> contracts, string mapId)
		{
			using (IEnumerator<Contract> enumerator = contracts.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.mapName == mapId)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06009117 RID: 37143 RVA: 0x00252C90 File Offset: 0x00250E90
		private bool DoesContractMeetRequirements(StarSystem system, MapAndEncounters level, ContractOverride contractOvr)
		{
			for (int i = 0; i < contractOvr.requirementList.Count; i++)
			{
				RequirementDef requirementDef = new RequirementDef(contractOvr.requirementList[i]);
				TagSet tagSet;
				StatCollection statCollection;
				this.SetTagsAndStats(system, level, requirementDef.Scope, out tagSet, out statCollection);
				requirementDef.RequirementComparisons = requirementDef.RequirementComparisons.Where((ComparisonDef c) => !c.obj.StartsWith("Target") && !c.obj.StartsWith("Employer")).ToList<ComparisonDef>();
				if (!SimGameState.MeetsRequirements(requirementDef, tagSet, statCollection, null))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06009118 RID: 37144 RVA: 0x00252D1C File Offset: 0x00250F1C
		private void SetTagsAndStats(StarSystem system, MapAndEncounters level, EventScope scope, out TagSet tags, out StatCollection stats)
		{
			switch (scope)
			{
			case EventScope.Company:
				tags = this.CompanyTags;
				stats = this.CompanyStats;
				return;
			case EventScope.MechWarrior:
			case EventScope.Mech:
				break;
			case EventScope.Commander:
				tags = this.CommanderTags;
				stats = this.CommanderStats;
				return;
			case EventScope.StarSystem:
				tags = system.Tags;
				stats = system.Stats;
				return;
			default:
				if (scope == EventScope.Map)
				{
					tags = MetadataDatabase.Instance.GetTagSetForTagSetEntry(level.Map.TagSetID);
					stats = new StatCollection();
					return;
				}
				break;
			}
			throw new Exception("Contracts cannot use the scope of: " + scope);
		}

		// Token: 0x06009119 RID: 37145 RVA: 0x00252DB9 File Offset: 0x00250FB9
		private static bool IsWithinDifficultyRange(SimGameState.ContractDifficultyRange diffRange, ContractDifficulty ovrDiff)
		{
			return diffRange.MinDifficultyClamped <= ovrDiff && diffRange.MaxDifficultyClamped >= ovrDiff;
		}

		// Token: 0x0600911A RID: 37146 RVA: 0x00252DD4 File Offset: 0x00250FD4
		private Dictionary<int, List<ContractOverride>> GetContractOverrides(SimGameState.ContractDifficultyRange diffRange, int[] allowedTypes)
		{
			Func<string, ContractOverride> <>9__5;
			Func<ContractOverride, bool> <>9__6;
			return (from c in MetadataDatabase.Instance.GetContractsByDifficultyRangeAndScopeAndOwnership((int)diffRange.MinDifficultyClamped, (int)diffRange.MaxDifficultyClamped, this.ContractScope, true)
				where allowedTypes.Contains((int)c.ContractTypeRow.ContractTypeID)
				group c.ContractID by (int)c.ContractTypeRow.ContractTypeID).ToDictionary((IGrouping<int, string> c) => c.Key, delegate(IGrouping<int, string> c)
			{
				Func<string, ContractOverride> func;
				if ((func = <>9__5) == null)
				{
					func = (<>9__5 = (string ci) => this.DataManager.ContractOverrides.Get(ci));
				}
				IEnumerable<ContractOverride> enumerable = c.Select(func);
				Func<ContractOverride, bool> func2;
				if ((func2 = <>9__6) == null)
				{
					func2 = (<>9__6 = (ContractOverride ci) => SimGameState.IsWithinDifficultyRange(diffRange, this.GetDifficultyEnumFromValue(ci.difficulty)));
				}
				return enumerable.Where(func2).ToList<ContractOverride>();
			});
		}

		// Token: 0x0600911B RID: 37147 RVA: 0x00252EA8 File Offset: 0x002510A8
		private Dictionary<int, List<ContractOverride>> GetSinglePlayerProceduralContractOverrides(SimGameState.ContractDifficultyRange diffRange)
		{
			Func<string, ContractOverride> <>9__5;
			Func<ContractOverride, bool> <>9__6;
			return (from c in MetadataDatabase.Instance.GetContractsByDifficultyRangeAndScopeAndOwnership((int)diffRange.MinDifficultyClamped, (int)diffRange.MaxDifficultyClamped, this.ContractScope, true)
				where c.ContractTypeRow.IsSinglePlayerProcedural
				group c.ContractID by (int)c.ContractTypeRow.ContractTypeID).ToDictionary((IGrouping<int, string> c) => c.Key, delegate(IGrouping<int, string> c)
			{
				Func<string, ContractOverride> func;
				if ((func = <>9__5) == null)
				{
					func = (<>9__5 = (string ci) => this.DataManager.ContractOverrides.Get(ci));
				}
				IEnumerable<ContractOverride> enumerable = c.Select(func);
				Func<ContractOverride, bool> func2;
				if ((func2 = <>9__6) == null)
				{
					func2 = (<>9__6 = (ContractOverride ci) => SimGameState.IsWithinDifficultyRange(diffRange, this.GetDifficultyEnumFromValue(ci.difficulty)));
				}
				return enumerable.Where(func2).ToList<ContractOverride>();
			});
		}

		// Token: 0x0600911C RID: 37148 RVA: 0x00252F88 File Offset: 0x00251188
		private Dictionary<string, WeightedList<SimGameState.ContractParticipants>> GetValidParticipants(StarSystem system)
		{
			return (from e in system.Def.ContractEmployerIDList
				where !this.ignoredContractEmployers.Contains(e)
				select new
				{
					Employer = e,
					Participants = this.GenerateContractParticipants(this.factions[e], system.Def)
				} into e
				where e.Participants.Any<SimGameState.ContractParticipants>()
				select e).ToDictionary(e => e.Employer, t => t.Participants);
		}

		// Token: 0x0600911D RID: 37149 RVA: 0x00253044 File Offset: 0x00251244
		private WeightedList<SimGameState.ContractParticipants> GenerateContractParticipants(FactionDef employer, StarSystemDef system)
		{
			SimGameState.<>c__DisplayClass444_0 CS$<>8__locals1 = new SimGameState.<>c__DisplayClass444_0();
			CS$<>8__locals1.system = system;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.employer = employer;
			WeightedList<SimGameState.ContractParticipants> weightedList = new WeightedList<SimGameState.ContractParticipants>(WeightedListType.PureRandom, null, null, 0);
			List<string> list = CS$<>8__locals1.employer.Enemies.Where((string t) => CS$<>8__locals1.system.ContractTargetIDList.Contains(t) && !CS$<>8__locals1.<>4__this.IgnoredContractTargets.Contains(t) && !CS$<>8__locals1.<>4__this.IsFactionAlly(FactionEnumeration.GetFactionByName(t), null)).ToList<string>();
			IEnumerable<FactionValue> enumerable = FactionEnumeration.PossibleNeutralToAllList.Where((FactionValue f) => !CS$<>8__locals1.employer.FactionValue.Equals(f) && !CS$<>8__locals1.<>4__this.IgnoredContractTargets.Contains(f.Name));
			IEnumerable<FactionValue> enumerable2 = FactionEnumeration.PossibleHostileToAllList.Where((FactionValue f) => !CS$<>8__locals1.employer.FactionValue.Equals(f) && !CS$<>8__locals1.<>4__this.IgnoredContractTargets.Contains(f.Name));
			IEnumerable<FactionValue> enumerable3 = FactionEnumeration.PossibleAllyFallbackList.Where((FactionValue f) => !CS$<>8__locals1.employer.FactionValue.Equals(f) && !CS$<>8__locals1.<>4__this.IgnoredContractTargets.Contains(f.Name));
			using (List<string>.Enumerator enumerator = list.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					SimGameState.<>c__DisplayClass444_1 CS$<>8__locals2 = new SimGameState.<>c__DisplayClass444_1();
					CS$<>8__locals2.CS$<>8__locals1 = CS$<>8__locals1;
					CS$<>8__locals2.target = enumerator.Current;
					FactionDef targetFactionDef = this.factions[CS$<>8__locals2.target];
					FactionValue hostileMercenariesFactionValue = FactionEnumeration.GetHostileMercenariesFactionValue();
					FactionValue defaultHostileFaction = this.GetDefaultHostileFaction(CS$<>8__locals2.CS$<>8__locals1.employer.FactionValue, targetFactionDef.FactionValue);
					FactionValue defaultTargetAlly = enumerable3.Where((FactionValue f) => !targetFactionDef.Enemies.Contains(f.Name) && !CS$<>8__locals2.CS$<>8__locals1.employer.Allies.Contains(f.Name) && CS$<>8__locals2.target != f.Name).DefaultIfEmpty(targetFactionDef.FactionValue).GetRandomElement(this.NetworkRandom);
					FactionValue randomElement = enumerable3.Where((FactionValue f) => !CS$<>8__locals2.CS$<>8__locals1.employer.Enemies.Contains(f.Name) && !targetFactionDef.Allies.Contains(f.Name) && defaultTargetAlly != f && CS$<>8__locals2.target != f.Name).DefaultIfEmpty(CS$<>8__locals2.CS$<>8__locals1.employer.FactionValue).GetRandomElement(this.NetworkRandom);
					IEnumerable<FactionValue> enumerable4 = targetFactionDef.Allies.Select((string f) => FactionEnumeration.GetFactionByName(f));
					Func<FactionValue, bool> func;
					if ((func = CS$<>8__locals2.CS$<>8__locals1.<>9__7) == null)
					{
						func = (CS$<>8__locals2.CS$<>8__locals1.<>9__7 = (FactionValue f) => !CS$<>8__locals2.CS$<>8__locals1.employer.Allies.Contains(f.Name) && !CS$<>8__locals2.CS$<>8__locals1.<>4__this.IgnoredContractTargets.Contains(f.Name));
					}
					WeightedList<FactionValue> weightedList2 = enumerable4.Where(func).DefaultIfEmpty(defaultTargetAlly).ToWeightedList(WeightedListType.PureRandom);
					WeightedList<FactionValue> weightedList3 = (from f in CS$<>8__locals2.CS$<>8__locals1.employer.Allies
						select FactionEnumeration.GetFactionByName(f) into f
						where !targetFactionDef.Allies.Contains(f.Name) && !CS$<>8__locals2.CS$<>8__locals1.<>4__this.IgnoredContractTargets.Contains(f.Name)
						select f).DefaultIfEmpty(randomElement).ToWeightedList(WeightedListType.PureRandom);
					List<FactionValue> list2 = enumerable.Where((FactionValue f) => CS$<>8__locals2.target != f.Name && !targetFactionDef.Enemies.Contains(f.Name) && !CS$<>8__locals2.CS$<>8__locals1.employer.Enemies.Contains(f.Name)).DefaultIfEmpty(hostileMercenariesFactionValue).ToList<FactionValue>();
					List<FactionValue> list3 = enumerable2.Where((FactionValue f) => CS$<>8__locals2.target != f.Name && !targetFactionDef.Allies.Contains(f.Name) && !CS$<>8__locals2.CS$<>8__locals1.employer.Allies.Contains(f.Name)).DefaultIfEmpty(defaultHostileFaction).ToList<FactionValue>();
					weightedList.Add(new SimGameState.ContractParticipants(targetFactionDef.FactionValue, weightedList2, weightedList3, list2, list3), 0);
				}
			}
			return weightedList;
		}

		// Token: 0x0600911E RID: 37150 RVA: 0x0025335C File Offset: 0x0025155C
		private void LogErrorNoValidTargets(StarSystem system)
		{
			SimGameState.logger.LogError(string.Format("There are no valid employers or targets for the system of {0}", system.Name));
			SimGameState.logger.LogError("System Employers:");
			for (int i = 0; i < system.Def.ContractEmployerIDList.Count; i++)
			{
				SimGameState.logger.LogError(string.Format("\t{0}", system.Def.ContractEmployerIDList[i]));
				FactionDef factionDef = this.factions[system.Def.ContractEmployerIDList[i]];
				SimGameState.logger.LogError(string.Format("\t{0} Enemies:", system.Def.ContractEmployerIDList[i]));
				for (int j = 0; j < factionDef.Enemies.Length; j++)
				{
					SimGameState.logger.LogError(string.Format("\t\t{0}", factionDef.Enemies[j]));
				}
			}
			SimGameState.logger.LogError("System Targets:");
			if (system.Def.ContractTargets != null)
			{
				for (int k = 0; k < system.Def.ContractTargets.Count; k++)
				{
					SimGameState.logger.LogError(string.Format("\t{0}", system.Def.ContractTargets[k]));
				}
			}
			SimGameState.logger.LogError("Ignored Employers:");
			if (this.ignoredContractEmployers != null)
			{
				for (int l = 0; l < this.ignoredContractEmployers.Count; l++)
				{
					SimGameState.logger.LogError(string.Format("\t{0}", this.ignoredContractEmployers[l]));
				}
			}
			SimGameState.logger.LogError("Ignored Targets:");
			if (this.IgnoredContractTargets != null)
			{
				for (int m = 0; m < this.IgnoredContractTargets.Count; m++)
				{
					SimGameState.logger.LogError(string.Format("\t{0}", this.IgnoredContractTargets[m]));
				}
			}
		}

		// Token: 0x0600911F RID: 37151 RVA: 0x00253550 File Offset: 0x00251750
		private SimGameState.ContractDifficultyRange GetContractRangeDifficultyRange(StarSystem system, SimGameState.SimGameType simGameType, float globalGameDifficulty)
		{
			int num = system.Def.GetDifficulty(simGameType) + Mathf.FloorToInt(globalGameDifficulty);
			int num2;
			int num3;
			this.GetDifficultyRangeForContract(num, out num2, out num3);
			ContractDifficulty difficultyEnumFromValue = this.GetDifficultyEnumFromValue(num2);
			ContractDifficulty difficultyEnumFromValue2 = this.GetDifficultyEnumFromValue(num3);
			return new SimGameState.ContractDifficultyRange(num2, num3, difficultyEnumFromValue, difficultyEnumFromValue2);
		}

		// Token: 0x06009120 RID: 37152 RVA: 0x00253598 File Offset: 0x00251798
		public Contract AddContract(SimGameState.AddContractData contractData)
		{
			StarSystem starSystem;
			if (!string.IsNullOrEmpty(contractData.TargetSystem))
			{
				string validatedSystemString = this.GetValidatedSystemString(contractData.TargetSystem);
				if (!this.starDict.ContainsKey(validatedSystemString))
				{
					return null;
				}
				starSystem = this.starDict[validatedSystemString];
			}
			else
			{
				starSystem = this.CurSystem;
			}
			FactionValue factionValueFromString = this.GetFactionValueFromString(contractData.Target);
			FactionValue factionValueFromString2 = this.GetFactionValueFromString(contractData.Employer);
			FactionValue factionValue = this.GetFactionValueFromString(contractData.TargetAlly);
			FactionValue factionValue2 = this.GetFactionValueFromString(contractData.EmployerAlly);
			FactionValue factionValueFromString3 = this.GetFactionValueFromString(contractData.NeutralToAll);
			FactionValue factionValueFromString4 = this.GetFactionValueFromString(contractData.HostileToAll);
			if (factionValueFromString.IsInvalidUnset || factionValueFromString2.IsInvalidUnset)
			{
				return null;
			}
			factionValue = (factionValue.IsInvalidUnset ? factionValueFromString : factionValue);
			factionValue2 = (factionValue2.IsInvalidUnset ? factionValueFromString2 : factionValue2);
			ContractOverride contractOverride = this.DataManager.ContractOverrides.Get(contractData.ContractName).Copy();
			ContractTypeValue contractTypeValue = contractOverride.ContractTypeValue;
			if (contractTypeValue.IsTravelOnly)
			{
				return this.AddTravelContract(contractOverride, starSystem, factionValueFromString2);
			}
			List<MapAndEncounters> releasedMapsAndEncountersByContractTypeAndOwnership = MetadataDatabase.Instance.GetReleasedMapsAndEncountersByContractTypeAndOwnership(contractTypeValue.ID, false);
			if (releasedMapsAndEncountersByContractTypeAndOwnership == null || releasedMapsAndEncountersByContractTypeAndOwnership.Count == 0)
			{
				Debug.LogError(string.Format("There are no playable maps for this contract type[{0}]. Was your map published?", contractTypeValue.Name));
			}
			MapAndEncounters mapAndEncounters = releasedMapsAndEncountersByContractTypeAndOwnership[0];
			List<EncounterLayer_MDD> list = new List<EncounterLayer_MDD>();
			foreach (EncounterLayer_MDD encounterLayer_MDD in mapAndEncounters.Encounters)
			{
				if (encounterLayer_MDD.ContractTypeRow.ContractTypeID == (long)contractTypeValue.ID)
				{
					list.Add(encounterLayer_MDD);
				}
			}
			if (list.Count <= 0)
			{
				throw new Exception("Map does not contain any encounters of type: " + contractTypeValue.Name);
			}
			string encounterLayerGUID = list[this.NetworkRandom.Int(0, list.Count)].EncounterLayerGUID;
			GameContext gameContext = new GameContext(this.Context);
			gameContext.SetObject(GameContextObjectTagEnum.TargetStarSystem, starSystem);
			if (contractData.IsGlobal)
			{
				Contract contract = this.CreateTravelContract(mapAndEncounters.Map.MapName, mapAndEncounters.Map.MapPath, encounterLayerGUID, contractTypeValue, contractOverride, gameContext, factionValueFromString2, factionValueFromString, factionValue, factionValue2, factionValueFromString3, factionValueFromString4, contractData.IsGlobal, contractOverride.difficulty);
				this.PrepContract(contract, factionValueFromString2, factionValue2, factionValueFromString, factionValue, factionValueFromString3, factionValueFromString4, mapAndEncounters.Map.BiomeSkinEntry.BiomeSkin, contract.Override.travelSeed, starSystem);
				this.GlobalContracts.Add(contract);
				return contract;
			}
			Contract contract2 = new Contract(mapAndEncounters.Map.MapName, mapAndEncounters.Map.MapPath, encounterLayerGUID, contractTypeValue, this.BattleTechGame, contractOverride, gameContext, true, contractOverride.difficulty, 0, null);
			if (!contractData.FromSave)
			{
				ContractData contractData2 = new ContractData(contractData.ContractName, contractData.Target, contractData.Employer, contractData.TargetSystem, contractData.TargetAlly, contractData.EmployerAlly);
				contractData2.SetGuid(Guid.NewGuid().ToString());
				contract2.SetGuid(contractData2.GUID);
				this.contractBits.Add(contractData2);
			}
			if (contractData.FromSave)
			{
				contract2.SetGuid(contractData.SaveGuid);
			}
			this.PrepContract(contract2, factionValueFromString2, factionValue2, factionValueFromString, factionValue, factionValueFromString3, factionValueFromString4, mapAndEncounters.Map.BiomeSkinEntry.BiomeSkin, contract2.Override.travelSeed, starSystem);
			starSystem.SystemContracts.Add(contract2);
			return contract2;
		}

		// Token: 0x06009121 RID: 37153 RVA: 0x0025390C File Offset: 0x00251B0C
		private Contract AddFlashpointContract(Flashpoint flashpoint, SimGameState.AddContractData contractData)
		{
			ContractOverride contractOverride = this.DataManager.ContractOverrides.Get(contractData.ContractName).Copy();
			if (contractOverride.OnContractSuccessResults == null)
			{
				contractOverride.OnContractSuccessResults = new List<SimGameEventResult>();
			}
			if (contractOverride.OnContractFailureResults == null)
			{
				contractOverride.OnContractFailureResults = new List<SimGameEventResult>();
			}
			GameContext gameContext = new GameContext(this.Context);
			StarSystem curSystem = flashpoint.CurSystem;
			FactionValue factionValueFromString = this.GetFactionValueFromString(contractData.Target);
			FactionValue factionValueFromString2 = this.GetFactionValueFromString(contractData.Employer);
			FactionValue factionValue = this.GetFactionValueFromString(contractData.TargetAlly);
			FactionValue factionValue2 = this.GetFactionValueFromString(contractData.EmployerAlly);
			FactionValue factionValueFromString3 = this.GetFactionValueFromString(contractData.NeutralToAll);
			FactionValue factionValueFromString4 = this.GetFactionValueFromString(contractData.HostileToAll);
			factionValue = (factionValue.IsInvalidUnset ? factionValueFromString : factionValue);
			factionValue2 = (factionValue2.IsInvalidUnset ? factionValueFromString2 : factionValue2);
			if (!string.IsNullOrEmpty(contractData.Map) && string.IsNullOrEmpty(contractData.EncounterGuid))
			{
				ContractTypeValue contractTypeValue = contractOverride.ContractTypeValue;
				List<MapAndEncounters> releasedMapsAndEncountersByContractTypeAndOwnership = MetadataDatabase.Instance.GetReleasedMapsAndEncountersByContractTypeAndOwnership(contractTypeValue.ID, false);
				MapAndEncounters mapAndEncounters = null;
				foreach (MapAndEncounters mapAndEncounters2 in releasedMapsAndEncountersByContractTypeAndOwnership)
				{
					if (mapAndEncounters2.Map.MapName == contractData.Map)
					{
						mapAndEncounters = mapAndEncounters2;
						break;
					}
				}
				if (mapAndEncounters == null)
				{
					string text = string.Format("The specified map [{0}] was not found in the MDD. Ensure proper spelling, casing, and rebuild the mdd. The map will be chosen randomly.", contractData.Map);
					SimGameState.logger.LogError(text);
				}
				if (mapAndEncounters != null)
				{
					List<EncounterLayer_MDD> list = new List<EncounterLayer_MDD>();
					foreach (EncounterLayer_MDD encounterLayer_MDD in mapAndEncounters.Encounters)
					{
						if (encounterLayer_MDD.ContractTypeValue.ID == contractTypeValue.ID)
						{
							list.Add(encounterLayer_MDD);
						}
					}
					list.Shuffle<EncounterLayer_MDD>();
					if (list.Count > 0)
					{
						contractData.MapPath = mapAndEncounters.Map.MapPath;
						contractData.EncounterGuid = list[0].EncounterLayerGUID;
					}
				}
			}
			if (string.IsNullOrEmpty(contractData.EncounterGuid))
			{
				ContractTypeValue contractTypeValue2 = contractOverride.ContractTypeValue;
				List<MapAndEncounters> releasedMapsAndEncountersByContractTypeAndOwnership2 = MetadataDatabase.Instance.GetReleasedMapsAndEncountersByContractTypeAndOwnership(contractTypeValue2.ID, false);
				if (releasedMapsAndEncountersByContractTypeAndOwnership2 == null || releasedMapsAndEncountersByContractTypeAndOwnership2.Count == 0)
				{
					Debug.LogError(string.Format("There are no playable maps for this contract type[{0}]. Was your map published?", contractTypeValue2));
				}
				MapAndEncounters mapAndEncounters3 = releasedMapsAndEncountersByContractTypeAndOwnership2[0];
				List<EncounterLayer_MDD> list2 = new List<EncounterLayer_MDD>();
				foreach (EncounterLayer_MDD encounterLayer_MDD2 in mapAndEncounters3.Encounters)
				{
					if (encounterLayer_MDD2.ContractTypeValue.ID == contractTypeValue2.ID)
					{
						list2.Add(encounterLayer_MDD2);
					}
				}
				if (list2.Count <= 0)
				{
					throw new Exception("Map does not contain any encounters of type: " + contractTypeValue2);
				}
				contractData.Map = mapAndEncounters3.Map.MapName;
				contractData.MapPath = mapAndEncounters3.Map.MapPath;
				contractData.EncounterGuid = list2[this.NetworkRandom.Int(0, list2.Count)].EncounterLayerGUID;
			}
			if (string.IsNullOrEmpty(contractData.MapPath))
			{
				MapAndEncounters releasedMapByName = MetadataDatabase.Instance.GetReleasedMapByName(contractData.Map, true);
				if (releasedMapByName == null)
				{
					Debug.LogError(string.Format("There are no playable maps for this map name [{0}]. Was your map published?", contractData.Map));
				}
				contractData.MapPath = releasedMapByName.Map.MapPath;
			}
			Map_MDD mapByPath = MetadataDatabase.Instance.GetMapByPath(contractData.MapPath, false);
			gameContext.SetObject(GameContextObjectTagEnum.TargetStarSystem, curSystem);
			Contract contract = new Contract(contractData.Map, contractData.MapPath, contractData.EncounterGuid, contractOverride.ContractTypeValue, this.BattleTechGame, contractOverride, gameContext, true, contractData.Difficulty, 0, null);
			this.PrepContract(contract, factionValueFromString2, factionValue2, factionValueFromString, factionValue, factionValueFromString3, factionValueFromString4, mapByPath.BiomeSkinEntry.BiomeSkin, 0, curSystem);
			contract.SetCarryOverNegotationValues(false);
			flashpoint.SetActiveContract(contract, contractData.NextNodeId, contractData.OnContractFailureMilestone);
			return contract;
		}

		// Token: 0x06009122 RID: 37154 RVA: 0x00253D00 File Offset: 0x00251F00
		private Contract AddPredefinedContract2(SimGameState.AddContractData contractData)
		{
			ContractOverride contractOverride = this.DataManager.ContractOverrides.Get(contractData.ContractName).Copy();
			GameContext gameContext = new GameContext(this.Context);
			StarSystem starSystem;
			if (!string.IsNullOrEmpty(contractData.TargetSystem))
			{
				string validatedSystemString = this.GetValidatedSystemString(contractData.TargetSystem);
				if (!this.starDict.ContainsKey(validatedSystemString))
				{
					return null;
				}
				starSystem = this.starDict[validatedSystemString];
			}
			else
			{
				starSystem = this.CurSystem;
			}
			gameContext.SetObject(GameContextObjectTagEnum.TargetStarSystem, starSystem);
			FactionValue factionValueFromString = this.GetFactionValueFromString(contractData.Target);
			FactionValue factionValueFromString2 = this.GetFactionValueFromString(contractData.Employer);
			FactionValue factionValue = this.GetFactionValueFromString(contractData.TargetAlly);
			FactionValue factionValue2 = this.GetFactionValueFromString(contractData.EmployerAlly);
			FactionValue factionValueFromString3 = this.GetFactionValueFromString(contractData.NeutralToAll);
			FactionValue factionValueFromString4 = this.GetFactionValueFromString(contractData.HostileToAll);
			factionValue = (factionValue.IsInvalidUnset ? factionValueFromString : factionValue);
			factionValue2 = (factionValue2.IsInvalidUnset ? factionValueFromString2 : factionValue2);
			Map_MDD mapByPath = MetadataDatabase.Instance.GetMapByPath(contractData.MapPath, true);
			Contract contract = new Contract(contractData.Map, contractData.MapPath, contractData.EncounterGuid, contractOverride.ContractTypeValue, this.BattleTechGame, contractOverride, gameContext, true, contractData.Difficulty, 0, null);
			this.PrepContract(contract, factionValueFromString2, factionValue2, factionValueFromString, factionValue, factionValueFromString3, factionValueFromString4, mapByPath.BiomeSkinEntry.BiomeSkin, contractData.RandomSeed, starSystem);
			contract.SetCarryOverNegotationValues(contractData.CarryOverNegotiation);
			if (contractData.IsGlobal)
			{
				this.GlobalContracts.Add(contract);
			}
			else if (contract.Override.ContractTypeValue.IsTravelOnly)
			{
				starSystem.SystemBreadcrumbs.Add(contract);
			}
			else
			{
				starSystem.SystemContracts.Add(contract);
			}
			return contract;
		}

		// Token: 0x06009123 RID: 37155 RVA: 0x00253EBC File Offset: 0x002520BC
		private Contract AddTravelContract(ContractOverride ovr, StarSystem tgtSystem, FactionValue employer)
		{
			GameContext gameContext = new GameContext(this.Context);
			gameContext.SetObject(GameContextObjectTagEnum.TargetStarSystem, tgtSystem);
			ovr.travelSeed = this.NetworkRandom.Int(1, int.MaxValue);
			Contract contract = new Contract(null, null, null, ovr.ContractTypeValue, this.BattleTechGame, ovr, gameContext, true, 0, 0, null);
			FactionValue invalidUnsetFactionValue = FactionEnumeration.GetInvalidUnsetFactionValue();
			this.PrepContract(contract, employer, invalidUnsetFactionValue, invalidUnsetFactionValue, invalidUnsetFactionValue, invalidUnsetFactionValue, invalidUnsetFactionValue, Biome.BIOMESKIN.generic, ovr.travelSeed, tgtSystem);
			this.GlobalContracts.Add(contract);
			return contract;
		}

		// Token: 0x06009124 RID: 37156 RVA: 0x00253F40 File Offset: 0x00252140
		private Contract CreateTravelContract(string mapName, string mapPath, string encounterGuid, ContractTypeValue contractTypeValue, ContractOverride ovr, GameContext context, FactionValue employer, FactionValue target, FactionValue targetsAlly, FactionValue employersAlly, FactionValue neutralToAll, FactionValue hostileToAll, bool isGlobal, int difficulty)
		{
			StarSystem starSystem = context.GetObject(GameContextObjectTagEnum.TargetStarSystem) as StarSystem;
			int num = this.NetworkRandom.Int(0, int.MaxValue);
			ovr.FullRehydrate();
			ContractOverride contractOverride = new ContractOverride();
			contractOverride.CopyContractTypeData(ovr);
			contractOverride.contractName = ovr.contractName;
			contractOverride.difficulty = ovr.difficulty;
			contractOverride.longDescription = ovr.longDescription;
			contractOverride.shortDescription = ovr.shortDescription;
			contractOverride.travelOnly = true;
			contractOverride.useTravelCostPenalty = !isGlobal;
			contractOverride.disableNegotations = ovr.disableNegotations;
			contractOverride.disableAfterAction = ovr.disableAfterAction;
			contractOverride.salvagePotential = ovr.salvagePotential;
			contractOverride.contractRewardOverride = ovr.contractRewardOverride;
			contractOverride.travelSeed = num;
			contractOverride.difficultyUIModifier = ovr.difficultyUIModifier;
			int num2 = starSystem.Def.GetDifficulty(this.SimGameMode) + Mathf.FloorToInt(this.GlobalDifficulty);
			int num3;
			int num4;
			this.GetDifficultyRangeForContract(num2, out num3, out num4);
			int num5 = this.NetworkRandom.Int(num3, num4 + 1);
			SimGameEventResult simGameEventResult = new SimGameEventResult();
			SimGameResultAction simGameResultAction = new SimGameResultAction();
			int num6 = 14;
			simGameResultAction.Type = SimGameResultAction.ActionType.System_StartNonProceduralContract;
			simGameResultAction.value = mapName;
			simGameResultAction.additionalValues = new string[num6];
			simGameResultAction.additionalValues[0] = starSystem.ID;
			simGameResultAction.additionalValues[1] = mapPath;
			simGameResultAction.additionalValues[2] = encounterGuid;
			simGameResultAction.additionalValues[3] = ovr.ID;
			simGameResultAction.additionalValues[4] = isGlobal.ToString();
			simGameResultAction.additionalValues[5] = employer.Name;
			simGameResultAction.additionalValues[6] = target.Name;
			simGameResultAction.additionalValues[7] = difficulty.ToString();
			simGameResultAction.additionalValues[8] = "true";
			simGameResultAction.additionalValues[9] = targetsAlly.Name;
			simGameResultAction.additionalValues[10] = num.ToString();
			simGameResultAction.additionalValues[11] = employersAlly.Name;
			simGameResultAction.additionalValues[12] = neutralToAll.Name;
			simGameResultAction.additionalValues[13] = hostileToAll.Name;
			simGameEventResult.Actions = new SimGameResultAction[1];
			simGameEventResult.Actions[0] = simGameResultAction;
			contractOverride.OnContractSuccessResults.Add(simGameEventResult);
			return new Contract(mapName, mapPath, encounterGuid, contractTypeValue, this.BattleTechGame, contractOverride, context, true, num5, 0, null)
			{
				Override = 
				{
					travelSeed = num
				}
			};
		}

		// Token: 0x06009125 RID: 37157 RVA: 0x002541A8 File Offset: 0x002523A8
		public void PrepContract(Contract contract, FactionValue employer, FactionValue employersAlly, FactionValue target, FactionValue targetsAlly, FactionValue NeutralToAll, FactionValue HostileToAll, Biome.BIOMESKIN skin, int presetSeed, StarSystem system)
		{
			if (presetSeed != 0 && !contract.IsPriorityContract)
			{
				int num = system.Def.GetDifficulty(this.SimGameMode) + Mathf.FloorToInt(this.GlobalDifficulty);
				int num2;
				int num3;
				this.GetDifficultyRangeForContract(num, out num2, out num3);
				int num4 = new NetworkRandom
				{
					seed = presetSeed
				}.Int(num2, num3 + 1);
				contract.SetFinalDifficulty(num4);
			}
			FactionValue player1sMercUnitFactionValue = FactionEnumeration.GetPlayer1sMercUnitFactionValue();
			FactionValue player2sMercUnitFactionValue = FactionEnumeration.GetPlayer2sMercUnitFactionValue();
			contract.AddTeamFaction("bf40fd39-ccf9-47c4-94a6-061809681140", player1sMercUnitFactionValue.ID);
			contract.AddTeamFaction("757173dd-b4e1-4bb5-9bee-d78e623cc867", player2sMercUnitFactionValue.ID);
			contract.AddTeamFaction("ecc8d4f2-74b4-465d-adf6-84445e5dfc230", employer.ID);
			contract.AddTeamFaction("70af7e7f-39a8-4e81-87c2-bd01dcb01b5e", employersAlly.ID);
			contract.AddTeamFaction("be77cadd-e245-4240-a93e-b99cc98902a5", target.ID);
			contract.AddTeamFaction("31151ed6-cfc2-467e-98c4-9ae5bea784cf", targetsAlly.ID);
			contract.AddTeamFaction("61612bb3-abf9-4586-952a-0559fa9dcd75", NeutralToAll.ID);
			contract.AddTeamFaction("3c9f3a20-ab03-4bcb-8ab6-b1ef0442bbf0", HostileToAll.ID);
			contract.SetupContext();
			int finalDifficulty = contract.Override.finalDifficulty;
			int num5;
			if (contract.Override.contractRewardOverride >= 0)
			{
				num5 = contract.Override.contractRewardOverride;
			}
			else
			{
				num5 = this.CalculateContractValueByContractType(contract.ContractTypeValue, finalDifficulty, (float)this.Constants.Finances.ContractPricePerDifficulty, this.Constants.Finances.ContractPriceVariance, presetSeed);
			}
			num5 = SimGameState.RoundTo((float)num5, 1000);
			contract.SetInitialReward(num5);
			contract.SetBiomeSkin(skin);
		}

		// Token: 0x06009126 RID: 37158 RVA: 0x00254324 File Offset: 0x00252524
		public void PrepareBreadcrumb(Contract contract)
		{
			this.SetSelectedContract(contract, false, false);
			this.potentialTravelContract = contract;
			StarSystem starSystem = contract.GameContext.GetObject(GameContextObjectTagEnum.TargetStarSystem) as StarSystem;
			if (starSystem != null && starSystem.ID == this.CurSystem.ID)
			{
				this.StartBreadcrumb(0);
				this.QueueCompleteBreadcrumbProcess(false);
				return;
			}
			if (!this.Starmap.SetSelectedSystem(starSystem))
			{
				SimGameState.logger.LogError(string.Format("Cannot take contract. Path to: {0} is blocked.", starSystem.Name));
				this.potentialTravelContract = null;
				return;
			}
			this.Starmap.StarSystemRouted.AddListener(new UnityAction<StarSystem>(this.OnPathRouted));
			this.Starmap.FindRouteTo(starSystem.ID);
		}

		// Token: 0x06009127 RID: 37159 RVA: 0x002543DC File Offset: 0x002525DC
		private void OnPathRouted(StarSystem system)
		{
			this.Starmap.StarSystemRouted.RemoveListener(new UnityAction<StarSystem>(this.OnPathRouted));
			this.StartBreadcrumb(this.Starmap.ProjectedTravelCost);
			this.Starmap.SetActivePath();
			this.SetSimRoomState(DropshipLocation.SHIP);
		}

		// Token: 0x06009128 RID: 37160 RVA: 0x00254428 File Offset: 0x00252628
		public void StartBreadcrumb(int cost = 0)
		{
			this.activeBreadcrumb = this.potentialTravelContract;
			this.potentialTravelContract = null;
			if (cost > 0)
			{
				SimGameEventResult simGameEventResult = new SimGameEventResult();
				simGameEventResult.Stats = new SimGameStat[1];
				simGameEventResult.Stats[0] = new SimGameStat("Funds", -cost, false);
				this.activeBreadcrumb.Override.OnContractFailureResults.Add(simGameEventResult);
			}
		}

		// Token: 0x06009129 RID: 37161 RVA: 0x0025448D File Offset: 0x0025268D
		public void CancelBreadcrumb()
		{
			if (this.potentialTravelContract != null)
			{
				this.CancelPontentialBreadcrumb();
				return;
			}
			this.FailBreadcrumb();
		}

		// Token: 0x0600912A RID: 37162 RVA: 0x002544A4 File Offset: 0x002526A4
		public void CancelPontentialBreadcrumb()
		{
			this.potentialTravelContract = null;
			this.RoomManager.SetQueuedUIActivationID(DropshipMenuType.Contract, DropshipLocation.CMD_CENTER, true);
			this.SetSimRoomState(DropshipLocation.CMD_CENTER);
		}

		// Token: 0x0600912B RID: 37163 RVA: 0x002544C4 File Offset: 0x002526C4
		public void CreateBreakContractWarning(Action continueAction, Action cancelAction)
		{
			if (this.ActiveTravelContract == null)
			{
				return;
			}
			string text = Strings.T("Confirm");
			string text2 = Strings.T("WARNING: This action will break your current contract. Your reputation with the employer will be affected, and you will be charged for any travel costs that were paid for by the contract.");
			if (this.ActiveTravelContract.IsPriorityContract)
			{
				text2 = Strings.T("This action will put your current Priority Mission on hold. It will be waiting for you in the Command Center when you are ready to continue.");
			}
			PauseNotification.Show("Navigation Change", text2, this.GetCrewPortrait(SimGameCrew.Crew_Sumire), "", true, continueAction, text, cancelAction, "Cancel");
		}

		// Token: 0x0600912C RID: 37164 RVA: 0x00254529 File Offset: 0x00252729
		public void OnBreadcrumbCancelledByUser()
		{
			if (this.ActiveTravelContract.Override.useTravelCostPenalty)
			{
				this.FailBreadcrumb();
				return;
			}
			this.StopBreadcrumb();
		}

		// Token: 0x0600912D RID: 37165 RVA: 0x0025454C File Offset: 0x0025274C
		public bool OnBreadcrumbArrival()
		{
			if (this.ActiveTravelContract == null)
			{
				this.TriggerSaveNow(SaveReason.SIM_GAME_ARRIVED_AT_PLANET, SimGameState.TriggerSaveNowOption.QUEUE_IF_NEEDED);
				return false;
			}
			Action action = delegate
			{
				this.QueueCompleteBreadcrumbProcess(true);
			};
			this.interruptQueue.QueueTravelPauseNotification("Arrived", Strings.T("We've arrived at {0}, Commander. Ready to proceed with our current contract?", new object[] { this.CurSystem.Def.Description.Name }), this.GetCrewPortrait(SimGameCrew.Crew_Darius), "", action, "Proceed", new Action(this.OnBreadcrumbWait), "Not Yet");
			if (!this.TimeMoving)
			{
				this.interruptQueue.DisplayIfAvailable();
			}
			return true;
		}

		// Token: 0x0600912E RID: 37166 RVA: 0x002545EB File Offset: 0x002527EB
		public void OnBreadcrumbWait()
		{
			this.TriggerSaveNow(SaveReason.SIM_GAME_ARRIVED_AT_PLANET, SimGameState.TriggerSaveNowOption.QUEUE_IF_NEEDED);
			this.RoomManager.ShipRoom.RefreshData();
		}

		// Token: 0x0600912F RID: 37167 RVA: 0x00254608 File Offset: 0x00252808
		public void QueueCompleteBreadcrumbProcess(bool arrivedAtPlanet)
		{
			this.completeBreadcrumbProcessQueued = true;
			if (this.activeBreadcrumb == null)
			{
				if (arrivedAtPlanet)
				{
					this.TriggerSaveNow(SaveReason.SIM_GAME_ARRIVED_AT_PLANET, SimGameState.TriggerSaveNowOption.QUEUE_IF_NEEDED);
				}
				return;
			}
			if (!this.activeBreadcrumb.IsTutorial)
			{
				this.SaveActiveContractName = this.activeBreadcrumb.Name;
				if (this.TriggerSaveNow(SaveReason.SIM_GAME_BREADCRUMB_COMPLETE, SimGameState.TriggerSaveNowOption.QUEUE_IF_NEEDED) == SimGameState.TriggerSaveNowResult.DID_NOT_QUEUE)
				{
					SimGameState.logger.LogError("TriggerSaveNow 15083");
					return;
				}
			}
			else if (arrivedAtPlanet)
			{
				this.TriggerSaveNow(SaveReason.SIM_GAME_ARRIVED_AT_PLANET, SimGameState.TriggerSaveNowOption.QUEUE_IF_NEEDED);
			}
		}

		// Token: 0x06009130 RID: 37168 RVA: 0x0025467C File Offset: 0x0025287C
		private void FinishCompleteBreadcrumbProcess()
		{
			if (this.globalContracts.Contains(this.activeBreadcrumb))
			{
				this.globalContracts.Remove(this.activeBreadcrumb);
			}
			bool flag = false;
			if (this.activeBreadcrumb.Override.OnContractSuccessResults != null)
			{
				foreach (SimGameEventResult simGameEventResult in this.activeBreadcrumb.Override.OnContractSuccessResults)
				{
					if (simGameEventResult.Actions != null)
					{
						SimGameResultAction[] actions = simGameEventResult.Actions;
						for (int i = 0; i < actions.Length; i++)
						{
							if (actions[i].Type == SimGameResultAction.ActionType.System_StartNonProceduralContract)
							{
								flag = true;
								break;
							}
						}
					}
					if (flag)
					{
						break;
					}
				}
			}
			if (flag)
			{
				this.pendingBreadcrumb = this.activeBreadcrumb;
			}
			SimGameState.ApplySimGameEventResult(this.activeBreadcrumb.Override.OnContractSuccessResults);
			this.activeBreadcrumb = null;
			this.UpdateMilestones();
			if (this.interruptQueue.HasQueue && !this.interruptQueue.IsOpen)
			{
				if (this.interruptQueue.NextIsSave)
				{
					SimGameState.logger.LogError("CompleteBreadcrumb 15728");
				}
				this.interruptQueue.DisplayIfAvailable();
			}
		}

		// Token: 0x06009131 RID: 37169 RVA: 0x002547B8 File Offset: 0x002529B8
		public void ClearBreadcrumb()
		{
			this.activeBreadcrumb = null;
		}

		// Token: 0x06009132 RID: 37170 RVA: 0x002547C4 File Offset: 0x002529C4
		public void FailBreadcrumb()
		{
			if (this.globalContracts.Contains(this.activeBreadcrumb))
			{
				this.globalContracts.Remove(this.activeBreadcrumb);
			}
			SimGameState.ApplySimGameEventResult(this.activeBreadcrumb.Override.OnContractFailureResults);
			FactionValue teamFaction = this.activeBreadcrumb.GetTeamFaction("ecc8d4f2-74b4-465d-adf6-84445e5dfc230");
			if (teamFaction.DoesGainReputation && !this.activeBreadcrumb.IsStoryContract)
			{
				float employerRepBadFaithMod = this.Constants.Story.EmployerRepBadFaithMod;
				int num = Mathf.RoundToInt((float)this.activeBreadcrumb.GetCurrentReputationValue(this.Constants, null) * employerRepBadFaithMod);
				if (num != 0)
				{
					this.SetReputation(teamFaction, num, StatCollection.StatOperation.Int_Add, null);
				}
			}
			this.activeBreadcrumb = null;
		}

		// Token: 0x06009133 RID: 37171 RVA: 0x002547B8 File Offset: 0x002529B8
		private void StopBreadcrumb()
		{
			this.activeBreadcrumb = null;
		}

		// Token: 0x06009134 RID: 37172 RVA: 0x0025487C File Offset: 0x00252A7C
		public void ForceTakeContract(Contract c, bool breadcrumb)
		{
			if (this.CompletedContract != null)
			{
				this.PendingMilestoneContract = c;
				this.IsPendingMilestoneContractBreadcrumb = breadcrumb;
				return;
			}
			this.SetSelectedContract(c, breadcrumb, false);
			this._selectedContractForced = true;
			if (this.TimeMoving)
			{
				this.StopPlayMode();
			}
			if (c.CarryOverNegotationValues)
			{
				if (this.activeBreadcrumb != null)
				{
					c.SetNegotiatedValues(this.activeBreadcrumb.PercentageContractValue, this.activeBreadcrumb.PercentageContractSalvage);
					this.ClearBreadcrumb();
					this.OnForcedContractNegotiationComplete();
					return;
				}
				SimGameState.logger.LogError("Attempting to carry over negotiated values without a breadcrumb");
				c.SetNegotiatedValues(1f, 0f);
				return;
			}
			else
			{
				if (c.CanNegotiate)
				{
					this.RoomManager.CmdCenterRoom.NegotiateContract(c, new Action(this.OnForcedContractNegotiationComplete));
					return;
				}
				c.SetNegotiatedValues(c.Override.negotiatedSalary, c.Override.negotiatedSalvage);
				this.OnForcedContractNegotiationComplete();
				return;
			}
		}

		// Token: 0x06009135 RID: 37173 RVA: 0x0025495F File Offset: 0x00252B5F
		private void OnForcedContractNegotiationComplete()
		{
			if (!this.IsSelectedContractTravel)
			{
				this.StartLanceConfiguration();
				return;
			}
			this.PrepareBreadcrumb(this.SelectedContract);
		}

		// Token: 0x06009136 RID: 37174 RVA: 0x0025497C File Offset: 0x00252B7C
		public void StartLanceConfiguration()
		{
			Contract contract;
			if (this.HasTravelContract)
			{
				contract = this.ActiveTravelContract;
			}
			else
			{
				contract = this.SelectedContract;
			}
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			if (contract.Override != null && contract.Override.player1Team.lanceOverrideList.Count > 0)
			{
				foreach (UnitSpawnPointOverride unitSpawnPointOverride in contract.Override.player1Team.lanceOverrideList[0].unitSpawnPointOverrideList)
				{
					if (!string.IsNullOrEmpty(unitSpawnPointOverride.pilotDefId) && unitSpawnPointOverride.pilotDefId != UnitSpawnPointGameLogic.PilotDef_Commander && unitSpawnPointOverride.pilotDefId != UnitSpawnPointGameLogic.PilotDef_InheritLance && !this.DataManager.PilotDefs.Exists(unitSpawnPointOverride.pilotDefId))
					{
						list.Add(unitSpawnPointOverride.pilotDefId);
					}
					if (!string.IsNullOrEmpty(unitSpawnPointOverride.unitDefId) && unitSpawnPointOverride.unitDefId != "mechDef_None" && !this.DataManager.MechDefs.Exists(unitSpawnPointOverride.unitDefId))
					{
						list2.Add(unitSpawnPointOverride.unitDefId);
					}
				}
			}
			if (list.Count == 0 && list2.Count == 0)
			{
				this.CompleteLanceConfigurationPrep(null);
				return;
			}
			LoadRequest loadRequest = this.DataManager.CreateLoadRequest(new Action<LoadRequest>(this.CompleteLanceConfigurationPrep), false);
			foreach (string text in list)
			{
				loadRequest.AddLoadRequest<PilotDef>(BattleTechResourceType.PilotDef, text, null, false);
			}
			foreach (string text2 in list2)
			{
				loadRequest.AddLoadRequest<MechDef>(BattleTechResourceType.MechDef, text2, null, false);
			}
			loadRequest.ProcessRequests(10U);
		}

		// Token: 0x06009137 RID: 37175 RVA: 0x00254B98 File Offset: 0x00252D98
		private void CompleteLanceConfigurationPrep(LoadRequest request)
		{
			Contract contract = null;
			if (this.HasTravelContract)
			{
				contract = this.ActiveTravelContract;
			}
			else
			{
				contract = this.SelectedContract;
			}
			List<PilotDef> list = new List<PilotDef>();
			List<Pilot> list2 = new List<Pilot>();
			list.Add(this.commander.pilotDef);
			list2.Add(this.commander);
			foreach (Pilot pilot in this.PilotRoster)
			{
				list.Add(pilot.pilotDef);
				list2.Add(pilot);
			}
			this.RoomManager.CmdCenterRoom.lanceConfigBG.ShowLanceConfiguratorScreen(true);
			int num = 4;
			LanceDef.Unit[] array = new LanceDef.Unit[num];
			List<MechDef> list3 = new List<MechDef>(this.ActiveMechs.Values);
			List<MechDef> list4 = new List<MechDef>(this.ActiveMechs.Values);
			LanceConfiguration lanceConfiguration = null;
			if (contract.Override != null && contract.Override.player1Team.lanceOverrideList.Count > 0)
			{
				num = contract.Override.maxNumberOfPlayerUnits;
				array = new LanceDef.Unit[num];
				Dictionary<int, SpawnableUnit> dictionary = new Dictionary<int, SpawnableUnit>();
				List<UnitSpawnPointOverride> unitSpawnPointOverrideList = contract.Override.player1Team.lanceOverrideList[0].unitSpawnPointOverrideList;
				for (int i = 0; i < num; i++)
				{
					array[i] = new LanceDef.Unit();
					if (unitSpawnPointOverrideList[i].pilotDefId == UnitSpawnPointGameLogic.PilotDef_Commander)
					{
						if (string.IsNullOrEmpty(unitSpawnPointOverrideList[i].unitDefId) || unitSpawnPointOverrideList[i].unitDefId == "mechDef_None")
						{
							MechDef availableMechFromList = this.GetAvailableMechFromList(ref list3);
							if (availableMechFromList != null)
							{
								array[i].unitType = UnitType.Mech;
								array[i].unitId = availableMechFromList.Description.Id;
								array[i].unitSimGameID = availableMechFromList.GUID;
								array[i].pilotId = this.Commander.pilotDef.Description.Id;
								array[i].locked = true;
								dictionary.Add(i, new SpawnableUnit("bf40fd39-ccf9-47c4-94a6-061809681140", availableMechFromList, this.Commander.pilotDef));
							}
						}
						else
						{
							array[i].unitType = UnitType.Mech;
							array[i].unitId = unitSpawnPointOverrideList[i].unitDefId;
							array[i].pilotId = this.Commander.pilotDef.Description.Id;
							array[i].locked = true;
							array[i].unitSimGameID = this.GenerateSimGameUID();
							list4.Add(this.DataManager.MechDefs.Get(unitSpawnPointOverrideList[i].unitDefId));
							dictionary.Add(i, new SpawnableUnit("bf40fd39-ccf9-47c4-94a6-061809681140", unitSpawnPointOverrideList[i].unitDefId, UnitType.Mech, this.Commander.pilotDef));
						}
					}
					else if (!string.IsNullOrEmpty(unitSpawnPointOverrideList[i].unitDefId) && !(unitSpawnPointOverrideList[i].unitDefId == "mechDef_None"))
					{
						array[i].unitType = UnitType.Mech;
						array[i].unitId = unitSpawnPointOverrideList[i].unitDefId;
						array[i].locked = true;
						array[i].unitSimGameID = this.GenerateSimGameUID();
						array[i].pilotId = unitSpawnPointOverrideList[i].pilotDefId;
						MechDef mechDef = this.DataManager.MechDefs.Get(unitSpawnPointOverrideList[i].unitDefId);
						mechDef.SetGuid(array[i].unitSimGameID);
						list4.Add(mechDef);
						PilotDef pilotDef = null;
						if (!string.IsNullOrEmpty(unitSpawnPointOverrideList[i].pilotDefId) && !unitSpawnPointOverrideList[i].pilotDefId.Equals("pilotDef_InheritLance", StringComparison.OrdinalIgnoreCase))
						{
							pilotDef = this.DataManager.PilotDefs.Get(unitSpawnPointOverrideList[i].pilotDefId);
							pilotDef.SetGuid(this.GenerateSimGameUID());
							list.Add(pilotDef);
							Pilot pilot2 = new Pilot(null, string.Format("Override_{0}", unitSpawnPointOverrideList[i].pilotDefId), false);
							pilot2.FromPilotDef(pilotDef);
							pilot2.SetGuid(this.GenerateSimGameUID());
							list2.Add(pilot2);
						}
						dictionary.Add(i, new SpawnableUnit("bf40fd39-ccf9-47c4-94a6-061809681140", mechDef, pilotDef));
					}
				}
				if (dictionary.Count > 0)
				{
					lanceConfiguration = new LanceConfiguration();
					lanceConfiguration.AddUnits(dictionary.Values);
				}
				else if (!contract.CanLanceConfigure)
				{
					int num2 = 0;
					int num3 = 0;
					int num4 = 0;
					list3 = new List<MechDef>(this.ActiveMechs.Values);
					this.FilterUnavailableMechsFromList(ref list3);
					if (list3.Count <= 0)
					{
						Debug.LogError("Don't have enough fieldable mechs to complete mission");
						return;
					}
					if (this.commander.CanPilot)
					{
						dictionary.Add(num2, new SpawnableUnit("bf40fd39-ccf9-47c4-94a6-061809681140", list3[num3], this.commander.ToPilotDef(false)));
						num3++;
						num4++;
					}
					for (int j = num4; j < num; j++)
					{
						if (num3 >= list3.Count)
						{
							break;
						}
						while (num2 < this.PilotRoster.Count && !this.PilotRoster[num2].CanPilot)
						{
							num2++;
						}
						if (num2 >= this.PilotRoster.Count)
						{
							break;
						}
						dictionary.Add(num4, new SpawnableUnit("bf40fd39-ccf9-47c4-94a6-061809681140", list3[num3], this.PilotRoster[num2].ToPilotDef(false)));
						num2++;
						num3++;
						num4++;
					}
					lanceConfiguration = new LanceConfiguration();
					lanceConfiguration.AddUnits(dictionary.Values);
				}
			}
			if (!contract.CanLanceConfigure)
			{
				this.OnContractReady(lanceConfiguration);
			}
			else
			{
				LanceConfiguration lastLance = this.GetLastLance();
				this.RoomManager.CmdCenterRoom.lanceConfigBG.ShowLanceConfiguratorScreen(true);
				this.RoomManager.CmdCenterRoom.lanceConfigBG.LC.SetData(this, contract, "bf40fd39-ccf9-47c4-94a6-061809681140", list4, list2, true, false, -1, num, false, lanceConfiguration, lastLance, "", new UnityAction(this.OnLanceConfiguratorAccept), new UnityAction(this.OnLanceConfigurationCancelled));
			}
			this.RoomManager.SetOptionsHamburgerButtonActive(false);
		}

		// Token: 0x06009138 RID: 37176 RVA: 0x00255200 File Offset: 0x00253400
		private void OnLanceConfiguratorBack()
		{
			this.RoomManager.CmdCenterRoom.lanceConfigBG.ShowLanceConfiguratorScreen(false);
			this.RoomManager.SetOptionsHamburgerButtonActive(true);
		}

		// Token: 0x06009139 RID: 37177 RVA: 0x00255224 File Offset: 0x00253424
		private void ContractReadyLanceConfiguratorCloseDown()
		{
			this.RoomManager.CmdCenterRoom.lanceConfigBG.ShowLanceConfiguratorScreen(false);
		}

		// Token: 0x0600913A RID: 37178 RVA: 0x0025523C File Offset: 0x0025343C
		private void OnLanceConfiguratorAccept()
		{
			LanceConfiguration lanceConfiguration = this.RoomManager.CmdCenterRoom.lanceConfigBG.LC.CreateLanceConfiguration();
			this.SaveLastLance(lanceConfiguration);
			this.OnContractReady(lanceConfiguration);
		}

		// Token: 0x0600913B RID: 37179 RVA: 0x00255274 File Offset: 0x00253474
		public void ShowDifficultMissionPopup()
		{
			this.interruptQueue.QueuePauseNotification("Difficult Mission", "Careful, Commander. This drop looks like it might require more firepower than that. I recommend that we field some heavier 'Mechs, or hold off on this mission until we can find some.", this.GetCrewPortrait(SimGameCrew.Crew_Darius), "", new Action(this.RoomManager.CmdCenterRoom.lanceConfigBG.LC.ContinueConfirmClicked), "CONFIRM", null, "BACK");
		}

		// Token: 0x0600913C RID: 37180 RVA: 0x002552D0 File Offset: 0x002534D0
		public void ShowDifficultIronmanMissionPopup()
		{
			this.interruptQueue.QueuePauseNotification("Difficult Mission", "Careful, Commander. This drop looks like it might require more firepower than that. I recommend that we field some heavier 'Mechs, or hold off on this mission until we can find some.\r\n\r\n<color=#F79B26>Failing a Priority Mission will result in campaign loss.</color>", this.GetCrewPortrait(SimGameCrew.Crew_Darius), "", new Action(this.RoomManager.CmdCenterRoom.lanceConfigBG.LC.ContinueConfirmClicked), "CONFIRM", null, "BACK");
		}

		// Token: 0x0600913D RID: 37181 RVA: 0x0025532C File Offset: 0x0025352C
		private void OnLanceConfigurationCancelled()
		{
			if (this.SelectedContract.IsPriorityContract)
			{
				GenericPopupBuilder.Create(GenericPopupType.Warning, "This action will put your current Priority Mission on hold. It will be waiting for you in the Command Center when you are ready to continue.").AddButton("Cancel", null, true, null).AddButton("Confirm", new Action(this.CancelStoryOrConsecutiveLanceConfiguration), true, null)
					.CancelOnEscape()
					.AddFader(new UIColorRef?(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PopupBackfill), 0f, true)
					.Render();
				return;
			}
			if (this.pendingBreadcrumb != null)
			{
				this.activeBreadcrumb = this.pendingBreadcrumb;
				this.pendingBreadcrumb = null;
				if (this.IsSelectedContractForced && this.SelectedContract != null)
				{
					if (this.CurSystem.SystemContracts.Contains(this.SelectedContract))
					{
						this.CurSystem.SystemContracts.Remove(this.SelectedContract);
					}
					else if (this.GlobalContracts.Contains(this.SelectedContract))
					{
						this.GlobalContracts.Remove(this.SelectedContract);
					}
				}
				this.StopPlayMode();
			}
			this.OnLanceConfiguratorBack();
			this.RoomManager.CmdCenterRoom.StartNegotiation(this.IsSelectedContractTravel);
		}

		// Token: 0x0600913E RID: 37182 RVA: 0x00255444 File Offset: 0x00253644
		public void CancelStoryOrConsecutiveLanceConfiguration()
		{
			this.pendingBreadcrumb = null;
			this.globalContracts.Remove(this.SelectedContract);
			this.CurSystem.SystemContracts.Remove(this.SelectedContract);
			SimGameState.ApplySimGameEventResult(this.SelectedContract.Override.OnContractFailureResults);
			this.OnLanceConfiguratorBack();
		}

		// Token: 0x0600913F RID: 37183 RVA: 0x002554A0 File Offset: 0x002536A0
		private void OnContractReady(LanceConfiguration config)
		{
			Contract contract = null;
			if (this.HasTravelContract)
			{
				contract = this.ActiveTravelContract;
			}
			else
			{
				contract = this.SelectedContract;
			}
			if (this.interruptQueue.HasQueue || this.interruptQueue.IsOpen)
			{
				SimGameInterruptManager.InterruptType highestPriority = this.interruptQueue.GetHighestPriority();
				string text = string.Format("Going into combat and skipping intterupt queue with priority {0}. Will trigger auto save? {1}", highestPriority, !this._selectedContractForced);
				if (highestPriority == SimGameInterruptManager.InterruptType.AutoSave)
				{
					SimGameState.logger.LogWarning(text);
					this.interruptQueue.ClearAll();
				}
				else
				{
					SimGameState.logger.LogError(text);
				}
			}
			if (!this._selectedContractForced && !contract.IsFlashpointContract)
			{
				this.SaveActiveContractName = contract.Name;
				this.TriggerSaveNow(SaveReason.SIM_GAME_CONTRACT_ACCEPTED, SimGameState.TriggerSaveNowOption.DONT_QUEUE);
			}
			if (this._selectedContractForced || contract.Override.disableLanceConfiguration)
			{
				using (Dictionary<string, List<SpawnableUnit>>.KeyCollection.Enumerator enumerator = config.Lances.Keys.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						string text2 = enumerator.Current;
						if (!string.IsNullOrEmpty(text2))
						{
							SpawnableUnit[] lanceUnits = config.GetLanceUnits(text2);
							contract.Lances.AddUnits(lanceUnits);
						}
					}
					goto IL_14C;
				}
			}
			this.FillContractLance();
			IL_14C:
			this.ContractReadyLanceConfiguratorCloseDown();
			if (this._selectedContractSimulated)
			{
				if (this.Constants.Debug.SimAutoWin)
				{
					this.SimulateContract_OLD(contract, SimGameBattleSimulator.Results.Complete_Success);
					return;
				}
				this.SimulateContract(contract, false);
				return;
			}
			else
			{
				if (!this._selectedContractForced || !this.DebugMode)
				{
					this.StartContract(contract);
					return;
				}
				this.SetSimRoomState(DropshipLocation.NONE);
				this.interruptQueue.QueuePauseNotification(string.Format("(DEBUG MODE) Starting: {0}", contract.Override.ID), "Start Mission or Auto Succeed?", null, "", delegate
				{
					this.StartContract(contract);
				}, "Start", delegate
				{
					this.DEBUG_AutoCompleteMission(contract);
				}, "AutoComplete");
				return;
			}
		}

		// Token: 0x06009140 RID: 37184 RVA: 0x002556C0 File Offset: 0x002538C0
		private void DEBUG_AutoCompleteMission(Contract contract)
		{
			this.interruptQueue.QueuePauseNotification(string.Format("(DEBUG MODE) AutoComplete: {0}", contract.Override.ID), "Simulate or Auto Succeed?", null, "", delegate
			{
				this.SimulateContract(contract, false);
			}, "Simulate", delegate
			{
				this.SimulateContract(contract, true);
			}, "Auto WIN");
		}

		// Token: 0x06009141 RID: 37185 RVA: 0x00255734 File Offset: 0x00253934
		public void FillContractLance()
		{
			Contract selectedContract = this.SelectedContract;
			LanceConfiguration lanceConfiguration = this.RoomManager.CmdCenterRoom.lanceConfigBG.LC.CreateLanceConfiguration();
			this.SaveLastLance(lanceConfiguration);
			foreach (string text in lanceConfiguration.Lances.Keys)
			{
				if (!string.IsNullOrEmpty(text))
				{
					SpawnableUnit[] lanceUnits = lanceConfiguration.GetLanceUnits(text);
					selectedContract.Lances.AddUnits(lanceUnits);
				}
			}
		}

		// Token: 0x06009142 RID: 37186 RVA: 0x002557CC File Offset: 0x002539CC
		private MechDef GetAvailableMechFromList(ref List<MechDef> mechList)
		{
			for (int i = 0; i < mechList.Count; i++)
			{
				MechDef mechDef = mechList[i];
				if (MechValidationRules.ValidateMechCanBeFielded(this, mechDef))
				{
					mechList.Remove(mechDef);
					return mechDef;
				}
				Debug.LogWarning("StartLanceConfiguration had a mech that could not be fielded: " + mechDef.Name);
			}
			return null;
		}

		// Token: 0x06009143 RID: 37187 RVA: 0x00255820 File Offset: 0x00253A20
		private void FilterUnavailableMechsFromList(ref List<MechDef> mechList)
		{
			for (int i = mechList.Count - 1; i >= 0; i--)
			{
				MechDef mechDef = mechList[i];
				if (!MechValidationRules.ValidateMechCanBeFielded(this, mechDef))
				{
					mechList.Remove(mechDef);
				}
			}
		}

		// Token: 0x06009144 RID: 37188 RVA: 0x0025585C File Offset: 0x00253A5C
		private LanceConfiguration GetLastLance()
		{
			LanceConfiguration lanceConfiguration = new LanceConfiguration();
			if (this.LastUsedMechs != null && this.LastUsedPilots != null)
			{
				for (int i = 0; i < 4; i++)
				{
					string text = "";
					if (this.LastUsedMechs.Count > i)
					{
						text = this.LastUsedMechs[i];
					}
					string text2 = "";
					if (this.LastUsedPilots.Count > i)
					{
						text2 = this.LastUsedPilots[i];
					}
					MechDef mechByID = this.GetMechByID(text);
					Pilot pilot = this.GetPilot(text2);
					PilotDef pilotDef = ((pilot != null) ? pilot.pilotDef : null);
					if (mechByID != null || pilotDef != null)
					{
						lanceConfiguration.AddUnit("bf40fd39-ccf9-47c4-94a6-061809681140", mechByID, pilotDef);
					}
				}
			}
			return lanceConfiguration;
		}

		// Token: 0x06009145 RID: 37189 RVA: 0x00255910 File Offset: 0x00253B10
		private void SaveLastLance(LanceConfiguration config)
		{
			this.LastUsedMechs = new List<string>();
			this.LastUsedPilots = new List<string>();
			SpawnableUnit[] lanceUnits = config.GetLanceUnits("bf40fd39-ccf9-47c4-94a6-061809681140");
			for (int i = 0; i < lanceUnits.Length; i++)
			{
				string text = "";
				string text2 = "";
				SpawnableUnit spawnableUnit = lanceUnits[i];
				if (spawnableUnit.Unit != null && spawnableUnit.Pilot != null)
				{
					text = spawnableUnit.Unit.GUID;
					text2 = spawnableUnit.Pilot.Description.Id;
				}
				if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(text2))
				{
					this.LastUsedMechs.Add(text);
					this.LastUsedPilots.Add(text2);
				}
			}
		}

		// Token: 0x06009146 RID: 37190 RVA: 0x002559B8 File Offset: 0x00253BB8
		public void StartContract(Contract contract)
		{
			this.activeBreadcrumb = null;
			this.pendingBreadcrumb = null;
			contract.SetPlayerOneMorale(this.Morale);
			contract.Accept(false);
			LazySingletonBehavior<UIManager>.Instance.SetReferenceResolution(BTCanvasScale.MIN);
			AudioEventManager.PlayLoadingMusic();
			LoadingCurtain.ExecuteWhenVisible(delegate
			{
				LazySingletonBehavior<UIManager>.Instance.StartCoroutine(this.DelaySFXAudioPause());
				UnityGameInstance.BattleTechGame.LaunchContract(contract);
			});
			LoadingCurtain.ShowUntil(() => LevelLoader.InterstitialLoaded, false, GameTipManager.GameTipType.Combat, 1.5f);
			this.DetatchUX();
		}

		// Token: 0x06009147 RID: 37191 RVA: 0x00255A54 File Offset: 0x00253C54
		public IEnumerator DelaySFXAudioPause()
		{
			yield return new WaitForSeconds(5f);
			AudioEventManager.PauseSFXAudio();
			yield break;
		}

		// Token: 0x06009148 RID: 37192 RVA: 0x00255A5C File Offset: 0x00253C5C
		public void SimulateContract(Contract contract, bool takeNoDamage = false)
		{
			this.DetatchUX();
			this.SimulatedContract = contract;
			SimGameBattleSimulator.BeginSimulation(contract, this.DataManager, takeNoDamage);
		}

		// Token: 0x06009149 RID: 37193 RVA: 0x00255A78 File Offset: 0x00253C78
		public void SimulateContract_OLD(Contract contract, SimGameBattleSimulator.Results result)
		{
			this.DetatchUX();
			this.SimulatedContract = contract;
			SimGameBattleSimulator.BeginSimulation_OLD(contract, result, this.DataManager);
		}

		// Token: 0x0600914A RID: 37194 RVA: 0x00255A95 File Offset: 0x00253C95
		public void OnCompleteContract(Contract contract)
		{
			this.CompletedContract = contract;
		}

		// Token: 0x0600914B RID: 37195 RVA: 0x00255AA0 File Offset: 0x00253CA0
		private void ResolveCompleteContract()
		{
			bool isFlashpointContract = this.CompletedContract.IsFlashpointContract;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("You recieved:", Array.Empty<object>()).AppendLine();
			if (!this.CompletedContract.ResultsResolved)
			{
				SimGameContractCompleteMessage simGameContractCompleteMessage = new SimGameContractCompleteMessage(this.CompletedContract);
				this.MessageCenter.PublishMessage(simGameContractCompleteMessage);
				this.LogReport("Completed A Mission!");
				if (this.activeFlashpoint != null && this.activeFlashpoint.ActiveContract == this.CompletedContract)
				{
					this.activeFlashpoint.CompleteActiveContract(this.CompletedContract.State == Contract.ContractState.Complete);
				}
				else if (this.CurSystem.SystemContracts.Contains(this.CompletedContract))
				{
					this.CurSystem.SystemContracts.Remove(this.CompletedContract);
				}
				else if (this.GlobalContracts.Contains(this.CompletedContract))
				{
					this.GlobalContracts.Remove(this.CompletedContract);
				}
				if (this.CompletedContract.State == Contract.ContractState.Complete)
				{
					SimGameState.ApplySimGameEventResult(this.CompletedContract.Override.OnContractSuccessResults);
				}
				else
				{
					SimGameState.ApplySimGameEventResult(this.CompletedContract.Override.OnContractFailureResults);
				}
				bool flag;
				if (this.CompletedContract.State == Contract.ContractState.Complete)
				{
					flag = this.CanIgnoreMissionResults(this.CompletedContract.Override.OnContractSuccessResults);
				}
				else
				{
					flag = this.CanIgnoreMissionResults(this.CompletedContract.Override.OnContractFailureResults);
				}
				if (flag)
				{
					if (this.CurSystem.SystemContracts.Contains(this.CompletedContract))
					{
						this.CurSystem.SystemContracts.Remove(this.CompletedContract);
					}
					this.RoomManager.RefreshDisplay();
					this.CompletedContract = null;
					this.SimulatedContract = null;
					this.realTimeElapsed = 0f;
					this.UpdateMilestones();
					if (this.activeFlashpoint != null && this.activeFlashpoint.CurStatus == Flashpoint.Status.IN_PROGRESS)
					{
						this.activeFlashpoint.MilestoneCheck(false);
					}
					return;
				}
				this.CompanyStats.ModifyStat<int>("Mission", 0, this.Constants.Story.SystemMissionCompleteCountStat, StatCollection.StatOperation.Int_Add, 1, -1, true);
				this.CompanyStats.ModifyStat<int>("Mission", 0, "COMPANY_MissionsAttempted", StatCollection.StatOperation.Int_Add, 1, -1, true);
				if (this.CompletedContract.State == Contract.ContractState.Complete)
				{
					this.CompanyStats.ModifyStat<int>("Mission", 0, "COMPANY_MissionAggregateDifficulty", StatCollection.StatOperation.Int_Add, this.CompletedContract.Override.finalDifficulty, -1, true);
				}
				if (this.CompletedContract.State == Contract.ContractState.Complete)
				{
					this.CompanyStats.ModifyStat<int>("Mission", 0, "COMPANY_MissionsSucceeded", StatCollection.StatOperation.Int_Add, 1, -1, true);
				}
				else if (this.CompletedContract.IsGoodFaithEffort)
				{
					this.CompanyStats.ModifyStat<int>("Mission", 0, "COMPANY_MissionsGoodFaith", StatCollection.StatOperation.Int_Add, 1, -1, true);
				}
				else
				{
					this.CompanyStats.ModifyStat<int>("Mission", 0, "COMPANY_MissionFailures", StatCollection.StatOperation.Int_Add, 1, -1, true);
				}
				this.CompletedContract.ResultsResolved = true;
			}
			if (this.UpdateMilestones())
			{
				return;
			}
			if (this.CompletedContract.MoneyResults != 0)
			{
				this.AddFunds(this.CompletedContract.MoneyResults, null, true, true);
				stringBuilder.AppendFormat("• {0}{1} C-Bills", (this.CompletedContract.MoneyResults > -1) ? "+" : "-", this.CompletedContract.MoneyResults).AppendLine();
			}
			FactionValue factionValue = this.CompletedContract.GetTeamFaction("ecc8d4f2-74b4-465d-adf6-84445e5dfc230");
			if (this.CompletedContract.EmployerReputationResults != 0 && factionValue.DoesGainReputation)
			{
				this.SetReputation(factionValue, this.CompletedContract.EmployerReputationResults, StatCollection.StatOperation.Int_Add, null);
			}
			stringBuilder.AppendFormat("• (EMPLOYER) {0}{1} {2} Reputation", (this.CompletedContract.EmployerReputationResults >= 0) ? "" : "-", this.CompletedContract.EmployerReputationResults, factionValue.FriendlyName).AppendLine();
			factionValue = this.CompletedContract.GetTeamFaction("be77cadd-e245-4240-a93e-b99cc98902a5");
			if (this.CompletedContract.TargetReputationResults != 0 && factionValue.DoesGainReputation)
			{
				this.SetReputation(factionValue, this.CompletedContract.TargetReputationResults, StatCollection.StatOperation.Int_Add, null);
			}
			stringBuilder.AppendFormat("• (TARGET) {0}{1} {2} Reputation", (this.CompletedContract.TargetReputationResults >= 0) ? "" : "-", this.CompletedContract.TargetReputationResults, factionValue.FriendlyName).AppendLine();
			if (this.CompletedContract.MercenaryReviewboardReputationResults != 0)
			{
				this.SetReputation(FactionEnumeration.GetMercenaryReviewBoardFactionValue(), this.CompletedContract.MercenaryReviewboardReputationResults, StatCollection.StatOperation.Int_Add, null);
			}
			stringBuilder.AppendFormat("• {0}{1} {2} Reputation", (this.CompletedContract.MercenaryReviewboardReputationResults > -1) ? "" : "-", this.CompletedContract.TargetReputationResults, FactionEnumeration.GetMercenaryReviewBoardFactionValue().FriendlyName).AppendLine();
			if (this.CompletedContract.State != Contract.ContractState.Complete)
			{
				if (this.CompletedContract.KilledPilots.Count == this.CompletedContract.PlayerUnitResults.Count)
				{
					this.AddMorale(-this.Constants.Story.CatastropheMoraleModifier, "Team Wipe");
				}
				else if (!this.CompletedContract.IsGoodFaithEffort)
				{
					this.AddMorale(-this.Constants.Story.BadFaithMoraleModifier, "Bad Faith Failure");
				}
			}
			if (this.CompletedContract.SalvageResults != null)
			{
				foreach (SalvageDef salvageDef in this.CompletedContract.SalvageResults)
				{
					switch (salvageDef.Type)
					{
					case SalvageDef.SalvageType.COMPONENT:
					{
						for (int i = 0; i < salvageDef.Count; i++)
						{
							this.AddItemStat(salvageDef.Description.Id, salvageDef.ComponentType.ToString() + "Def", salvageDef.Damaged);
						}
						break;
					}
					case SalvageDef.SalvageType.MECH_PART:
					{
						for (int j = 0; j < salvageDef.Count; j++)
						{
							this.AddMechPart(salvageDef.Description.Id);
						}
						break;
					}
					case SalvageDef.SalvageType.CHASSIS:
					{
						MechDef mechDef = new MechDef(this.DataManager.ChassisDefs.Get(salvageDef.Description.Id).Description, salvageDef.Description.Id, new MechComponentRef[0], this.DataManager);
						this.CreateMechPlacementPopup(mechDef);
						break;
					}
					}
					stringBuilder.AppendFormat("• {0}", salvageDef.Description.Name).AppendLine();
				}
			}
			List<Pilot> list = new List<Pilot>(this.PilotRoster);
			list.Add(this.commander);
			foreach (UnitResult unitResult in this.CompletedContract.PlayerUnitResults)
			{
				foreach (Pilot pilot in list)
				{
					if (unitResult.pilot.pilotDef.Description.Id == pilot.pilotDef.Description.Id)
					{
						PilotDef pilotDef = unitResult.pilot.ToPilotDef(true);
						pilotDef.SetUnspentExperience(pilotDef.ExperienceUnspent + this.CompletedContract.ExperienceEarned);
						pilot.FromPilotDef(pilotDef);
						stringBuilder.AppendFormat("• {0} gained {1} experience", pilotDef.Description.Name, this.CompletedContract.ExperienceEarned).AppendLine();
						this.CompanyStats.ModifyStat<int>("Mission", 0, "COMPANY_MechKills", StatCollection.StatOperation.Int_Add, pilot.MechsKilled, -1, true);
						this.CompanyStats.ModifyStat<int>("Mission", 0, "COMPANY_OtherKills", StatCollection.StatOperation.Int_Add, pilot.OthersKilled, -1, true);
						pilot.pilotDef.AddMissionsPilotedCount(1);
						if (unitResult.pilot.HasEjected)
						{
							pilot.pilotDef.AddMissionsEjectedCount(1);
							break;
						}
						break;
					}
				}
				List<int> list2 = new List<int>();
				Dictionary<int, MechDef> dictionary = new Dictionary<int, MechDef>();
				foreach (int num in this.ActiveMechs.Keys)
				{
					MechDef mechDef2 = this.ActiveMechs[num];
					if (unitResult.mech.GUID == mechDef2.GUID)
					{
						list2.Add(num);
						if (unitResult.mechLost)
						{
							this.interruptQueue.QueuePauseNotification("Lost Mech", Strings.T("{0} has been destroyed and could not be recovered.", new object[] { unitResult.mech.Name }), this.GetCrewPortrait(SimGameCrew.Crew_Darius), "", null, "Continue", null, null);
							stringBuilder.AppendFormat("• {0} Mech was destroyed", unitResult.mech.Name);
						}
						else
						{
							dictionary.Add(num, unitResult.mech);
						}
					}
				}
				foreach (int num2 in list2)
				{
					this.ActiveMechs.Remove(num2);
				}
				foreach (int num3 in dictionary.Keys)
				{
					this.ActiveMechs.Add(num3, dictionary[num3]);
					this.RestoreMechPostCombat(this.ActiveMechs[num3]);
				}
			}
			for (int k = 0; k < this.CompletedContract.KilledPilots.Count; k++)
			{
				Pilot deadPilot = this.CompletedContract.KilledPilots[k];
				Pilot pilot2 = this.PilotRoster.Find((Pilot x) => x.pilotDef.Description.Id == deadPilot.pilotDef.Description.Id);
				ReportMechwarriorKilledMessage reportMechwarriorKilledMessage = new ReportMechwarriorKilledMessage(pilot2, this.CompletedContract.Name);
				this.MessageCenter.PublishMessage(reportMechwarriorKilledMessage);
				if (!this.KillPilot(pilot2, false, null, null))
				{
					if (pilot2 != this.commander)
					{
						SimGameState.logger.LogWarning(string.Format("Killed pilot {0} ({1}) does not exist in pilot roster", deadPilot.Name, deadPilot.GUID));
					}
				}
				else
				{
					stringBuilder.AppendFormat("{0} has died", pilot2.Name).AppendLine();
				}
			}
			this.RefreshInjuries();
			this.RoomManager.RefreshDisplay();
			Action action = delegate
			{
				LazySingletonBehavior<UIManager>.Instance.ResetFader(UIManagerRootType.UIRoot);
				if (this.PendingMilestoneContract == null)
				{
					if (this.CurRoomState != DropshipLocation.SHIP)
					{
						this.SetSimRoomState(DropshipLocation.SHIP);
					}
					else
					{
						this.RoomManager.ShipRoom.EnterRoom();
					}
					if (this.CompanyTags.Contains(this.Constants.Story.SystemUseTimeTag))
					{
						this.VersionUpdateCheck();
					}
					this.UpdateMilestones();
					if (this.activeFlashpoint != null && this.activeFlashpoint.CurStatus == Flashpoint.Status.IN_PROGRESS)
					{
						this.activeFlashpoint.MilestoneCheck(false);
					}
				}
				if (this.CompanyTags.Contains(this.Constants.Story.AARCompleteNotificationsDisabled))
				{
					return;
				}
				List<MechDef> fieldableActiveMechs = this.GetFieldableActiveMechs();
				int num4 = this.DaysPassed - this.CompanyStats.GetValue<int>("COMPANY_NotificationViewed_BattleMechRepairsNeeded");
				if (this.Constants.Story.NotifMechRepairsNeededRecurrence > 0 && fieldableActiveMechs.Count < this.ActiveMechs.Count && num4 > this.Constants.Story.NotifMechRepairsNeededRecurrence)
				{
					this.ShowMechRepairsNeededNotif();
				}
				int num5 = 0;
				foreach (Pilot pilot3 in this.PilotRoster)
				{
					int[] array = new int[] { pilot3.Gunnery, pilot3.Tactics, pilot3.Guts, pilot3.Piloting };
					int num6 = 0;
					foreach (int num7 in array)
					{
						if (num7 > num6 && num7 < 10)
						{
							num6 = num7;
						}
					}
					if (num6 > 0 && pilot3.UnspentXP > this.GetLevelCost(num6 + 1))
					{
						num5++;
					}
				}
				if (num5 >= 2)
				{
					if (this.activeFlashpoint != null && this.activeFlashpoint.Def != null)
					{
						if (!isFlashpointContract)
						{
							this.ShowMechWarriorTrainingNotif();
							return;
						}
						if (this.activeFlashpoint.Def.AllowRefitTime)
						{
							this.ShowMechWarriorTrainingNotif();
							return;
						}
					}
					else
					{
						this.ShowMechWarriorTrainingNotif();
					}
				}
			};
			string name = this.CompletedContract.Name;
			if (!this.CompletedContract.Override.disableAfterAction)
			{
				this.CompletedContract = null;
				action();
			}
			else
			{
				this.CompletedContract = null;
				action();
			}
			this.SimulatedContract = null;
			this.CurSystem.CompletedContract();
			if (this.CurSystem.InitialContractsFetched)
			{
				this.GeneratePotentialContracts(false, null, null, false);
			}
			else
			{
				this.CurSystem.GenerateInitialContracts(null);
			}
			if (this.NearestToTarget != null)
			{
				this.GeneratePotentialContracts(false, null, this.NearestToTarget, false);
			}
			this.realTimeElapsed = 0f;
			if (this.PendingMilestoneContract == null && !isFlashpointContract)
			{
				this.SaveActiveContractName = name;
				this.TriggerSaveNow(SaveReason.SIM_GAME_COMPLETED_CONTRACT, SimGameState.TriggerSaveNowOption.QUEUE_IF_NEEDED);
			}
		}

		// Token: 0x0600914C RID: 37196 RVA: 0x0025667C File Offset: 0x0025487C
		private bool CanIgnoreMissionResults(List<SimGameEventResult> resultList)
		{
			if (resultList != null)
			{
				for (int i = 0; i < resultList.Count; i++)
				{
					SimGameResultAction[] actions = resultList[i].Actions;
					if (actions != null)
					{
						SimGameResultAction[] array = actions;
						for (int j = 0; j < array.Length; j++)
						{
							if (array[j].Type == SimGameResultAction.ActionType.Contract_IgnoreMissionResults)
							{
								return true;
							}
						}
					}
				}
			}
			return false;
		}

		// Token: 0x0600914D RID: 37197 RVA: 0x002566CC File Offset: 0x002548CC
		[Obsolete("Used for Old Restoration System")]
		private void OnContractExpired(Contract contract)
		{
			if (contract == null || contract.Override == null || contract.Override.OnContractFailureResults == null)
			{
				return;
			}
			SimGameState.ApplySimGameEventResult(contract.Override.OnContractFailureResults);
		}

		// Token: 0x0600914E RID: 37198 RVA: 0x002566F8 File Offset: 0x002548F8
		private void GetDifficultyRangeForContract(int baseDiff, out int minDiff, out int maxDiff)
		{
			int contractDifficultyVariance = this.Constants.Story.ContractDifficultyVariance;
			minDiff = Mathf.Max(1, baseDiff - contractDifficultyVariance);
			maxDiff = Mathf.Max(1, baseDiff + contractDifficultyVariance);
		}

		// Token: 0x0600914F RID: 37199 RVA: 0x0025672C File Offset: 0x0025492C
		private ContractDifficulty GetDifficultyEnumFromValue(int value)
		{
			if (value >= 7)
			{
				return ContractDifficulty.Hard;
			}
			if (value >= 4)
			{
				return ContractDifficulty.Medium;
			}
			return ContractDifficulty.Easy;
		}

		// Token: 0x06009150 RID: 37200 RVA: 0x0025673C File Offset: 0x0025493C
		private FactionValue GetFactionValueFromString(string factionID)
		{
			FactionValue factionValue = FactionEnumeration.GetInvalidUnsetFactionValue();
			if (!string.IsNullOrEmpty(factionID))
			{
				factionValue = FactionEnumeration.GetFactionByName(factionID);
			}
			return factionValue;
		}

		// Token: 0x06009151 RID: 37201 RVA: 0x00256760 File Offset: 0x00254960
		private void AddOrRemoveGreatHousesToList(bool add, List<string> listToModify, IEnumerable<string> whiteList = null)
		{
			if (whiteList == null)
			{
				whiteList = new List<string>();
			}
			foreach (FactionValue factionValue in FactionEnumeration.FactionList.FindAll((FactionValue faction) => faction.IsGreatHouse))
			{
				if (add)
				{
					if (whiteList.Contains(factionValue.Name))
					{
						listToModify.Add(factionValue.Name);
					}
				}
				else if (listToModify.Contains(factionValue.Name))
				{
					listToModify.Remove(factionValue.Name);
				}
			}
		}

		// Token: 0x06009152 RID: 37202 RVA: 0x00256814 File Offset: 0x00254A14
		public void GetContractTypeIllustration(Contract contract, Action<Sprite> callback)
		{
			ContractTypeValue contractTypeValue = contract.ContractTypeValue;
			if (contract.Override.contractDisplayStyle == ContractDisplayStyle.BaseCampaignStory)
			{
				contractTypeValue = ContractTypeEnumeration.GetContractTypeByInt(17);
			}
			else if (contract.Override.contractDisplayStyle == ContractDisplayStyle.BaseCampaignRestoration)
			{
				contractTypeValue = ContractTypeEnumeration.GetContractTypeByInt(42);
			}
			if (string.IsNullOrEmpty(contractTypeValue.Illustration))
			{
				SimGameState.logger.LogError("Cannot find Contract Type Illustration for type: " + contractTypeValue.Name);
			}
			string illustration = contractTypeValue.Illustration;
			this.RequestItem<Sprite>(illustration, callback, BattleTechResourceType.Sprite);
		}

		// Token: 0x06009153 RID: 37203 RVA: 0x00256890 File Offset: 0x00254A90
		public void GetContractTypeIcon(Contract contract, Action<SVGAsset> callback)
		{
			ContractTypeValue contractTypeValue = contract.ContractTypeValue;
			if (contract.Override.contractDisplayStyle == ContractDisplayStyle.BaseCampaignStory)
			{
				contractTypeValue = ContractTypeEnumeration.GetContractTypeByInt(17);
			}
			else if (contract.Override.contractDisplayStyle == ContractDisplayStyle.BaseCampaignRestoration)
			{
				contractTypeValue = ContractTypeEnumeration.GetContractTypeByInt(42);
			}
			if (string.IsNullOrEmpty(contractTypeValue.Icon))
			{
				SimGameState.logger.LogError("Cannot find Contract Type Icon for type: " + contractTypeValue.Name);
			}
			string icon = contractTypeValue.Icon;
			this.RequestItem<SVGAsset>(icon, callback, BattleTechResourceType.SVGAsset);
		}

		// Token: 0x06009154 RID: 37204 RVA: 0x0025690C File Offset: 0x00254B0C
		private bool GetValidFaction(StarSystem system, Dictionary<string, WeightedList<SimGameState.ContractParticipants>> targetList, List<RequirementDef> defList, out SimGameState.ChosenContractParticipants chosenContractParticipants)
		{
			chosenContractParticipants = new SimGameState.ChosenContractParticipants();
			HashSet<string> hashSet = (from t in targetList.Values.SelectMany((WeightedList<SimGameState.ContractParticipants> t) => t)
				select t.Target.Name).ToHashSet<string>();
			HashSet<string> hashSet2 = targetList.Keys.ToHashSet<string>();
			HashSet<string> hashSet3 = targetList.Values.SelectMany((WeightedList<SimGameState.ContractParticipants> t) => t).SelectMany((SimGameState.ContractParticipants t) => t.NeutralToAll.Select((FactionValue f) => f.Name)).ToHashSet<string>();
			HashSet<string> hashSet4 = targetList.Values.SelectMany((WeightedList<SimGameState.ContractParticipants> t) => t).SelectMany((SimGameState.ContractParticipants t) => t.HostileToAll.Select((FactionValue f) => f.Name)).ToHashSet<string>();
			IEnumerable<ComparisonDef> enumerable = defList.SelectMany((RequirementDef r) => r.RequirementComparisons);
			List<ComparisonDef> list;
			List<ComparisonDef> list2;
			List<ComparisonDef> list3;
			List<ComparisonDef> list4;
			this.FilterEmployerTargetComparisons(enumerable, out list, out list2, out list3, out list4);
			List<string> potentialEmployers = this.GetPotentialFactions(system, hashSet2, list);
			if (!potentialEmployers.Any<string>())
			{
				return false;
			}
			List<string> potentialTargets = this.GetPotentialFactions(system, hashSet, list2);
			if (!potentialTargets.Any<string>())
			{
				return false;
			}
			List<string> potentialNeutrals = this.GetPotentialFactions(system, hashSet3, list3);
			List<string> potentialHostiles = this.GetPotentialFactions(system, hashSet4, list4);
			Func<SimGameState.ContractParticipants, bool> <>9__16;
			var enumerable2 = from employerTargets in targetList.Where((KeyValuePair<string, WeightedList<SimGameState.ContractParticipants>> employerTargets) => potentialEmployers.Contains(employerTargets.Key)).Select(delegate(KeyValuePair<string, WeightedList<SimGameState.ContractParticipants>> employerTargets)
				{
					string key = employerTargets.Key;
					IEnumerable<SimGameState.ContractParticipants> value = employerTargets.Value;
					Func<SimGameState.ContractParticipants, bool> func;
					if ((func = <>9__16) == null)
					{
						func = (<>9__16 = (SimGameState.ContractParticipants sourceTargets) => potentialTargets.Contains(sourceTargets.Target.Name));
					}
					return new
					{
						Employer = key,
						Participants = value.Where(func).ToWeightedList(WeightedListType.PureRandom)
					};
				})
				where employerTargets.Participants.Any<SimGameState.ContractParticipants>()
				select employerTargets;
			if (!enumerable2.Any())
			{
				return false;
			}
			int num = this.NetworkRandom.Int(0, enumerable2.Count());
			var <>f__AnonymousType = enumerable2.ElementAt(num);
			chosenContractParticipants.Employer = FactionEnumeration.GetFactionByName(<>f__AnonymousType.Employer);
			SimGameState.ContractParticipants next = <>f__AnonymousType.Participants.GetNext(true);
			chosenContractParticipants.Target = next.Target;
			FactionValue currentEmployerAlly = next.EmployerAllies.GetNext(true);
			FactionValue currentTargetAlly = next.TargetAllies.GetNext(true);
			potentialHostiles.RemoveAll((string f) => f == currentEmployerAlly.Name || f == currentTargetAlly.Name);
			potentialNeutrals.RemoveAll((string f) => f == currentEmployerAlly.Name || f == currentTargetAlly.Name);
			chosenContractParticipants.EmployersAlly = currentEmployerAlly;
			chosenContractParticipants.TargetsAlly = currentTargetAlly;
			WeightedList<FactionValue> weightedList = next.HostileToAll.Where((FactionValue f) => potentialHostiles.Contains(f.Name)).ToWeightedList(WeightedListType.PureRandom);
			if (!weightedList.Any<FactionValue>())
			{
				return false;
			}
			FactionValue currentHostileToAll = weightedList.GetNext(true);
			WeightedList<FactionValue> weightedList2 = next.NeutralToAll.Where((FactionValue f) => currentHostileToAll.Equals(f) && potentialNeutrals.Contains(f.Name)).ToWeightedList(WeightedListType.PureRandom);
			if (weightedList2.Any<FactionValue>())
			{
				chosenContractParticipants.NeutralToAll = weightedList2.GetNext(true);
			}
			else
			{
				chosenContractParticipants.NeutralToAll = FactionEnumeration.GetHostileMercenariesFactionValue();
			}
			chosenContractParticipants.HostileToAll = currentHostileToAll;
			return true;
		}

		// Token: 0x06009155 RID: 37205 RVA: 0x00256C74 File Offset: 0x00254E74
		private void FilterEmployerTargetComparisons(IEnumerable<ComparisonDef> comparisons, out List<ComparisonDef> employer, out List<ComparisonDef> target, out List<ComparisonDef> neutralToAll, out List<ComparisonDef> hostileToAll)
		{
			employer = new List<ComparisonDef>();
			target = new List<ComparisonDef>();
			neutralToAll = new List<ComparisonDef>();
			hostileToAll = new List<ComparisonDef>();
			foreach (ComparisonDef comparisonDef in comparisons)
			{
				if (comparisonDef.obj.StartsWith("Employer"))
				{
					employer.Add(comparisonDef);
				}
				else if (comparisonDef.obj.StartsWith("Target"))
				{
					target.Add(comparisonDef);
				}
				else if (comparisonDef.obj.StartsWith("NeutralToAll"))
				{
					neutralToAll.Add(comparisonDef);
				}
				else if (comparisonDef.obj.StartsWith("HostileToAll"))
				{
					hostileToAll.Add(comparisonDef);
				}
			}
		}

		// Token: 0x06009156 RID: 37206 RVA: 0x00256D44 File Offset: 0x00254F44
		private List<string> GetPotentialFactions(StarSystem system, IEnumerable<string> sourceFactions, IEnumerable<ComparisonDef> comparisons)
		{
			List<string> list = new List<string>();
			SimGameState.FilteredComparisonResults whiteListComparisons = this.GetWhiteListComparisons(comparisons);
			SimGameState.FilteredComparisonResults BlackListResult = this.GetBlackListComparisons(comparisons);
			if (whiteListComparisons.IsEmpty)
			{
				list.AddRange(sourceFactions);
			}
			else
			{
				FactionValue invalidUnset = FactionEnumeration.GetInvalidUnsetFactionValue();
				list = whiteListComparisons.Factions.Where((string f) => f != invalidUnset.Name && sourceFactions.Contains(f)).ToList<string>();
				if (whiteListComparisons.Strings.Contains("IsGreatHouse"))
				{
					this.AddOrRemoveGreatHousesToList(true, list, sourceFactions);
				}
				if (whiteListComparisons.Strings.Contains("IsOwner") && sourceFactions.Contains(system.OwnerValue.Name))
				{
					list.Add(system.OwnerValue.Name);
				}
			}
			if (!BlackListResult.IsEmpty)
			{
				list.RemoveAll((string e) => BlackListResult.Factions.Contains(e));
				if (BlackListResult.Strings.Contains("IsGreatHouse"))
				{
					this.AddOrRemoveGreatHousesToList(false, list, null);
				}
				if (BlackListResult.Strings.Contains("IsOwner"))
				{
					list.RemoveAll((string e) => e == system.OwnerValue.Name);
				}
			}
			return list;
		}

		// Token: 0x06009157 RID: 37207 RVA: 0x00256EB2 File Offset: 0x002550B2
		private SimGameState.FilteredComparisonResults GetBlackListComparisons(IEnumerable<ComparisonDef> comparisons)
		{
			return this.GetFilteredComparisons(comparisons, new Func<ComparisonDef, bool>(this.IsBlackList));
		}

		// Token: 0x06009158 RID: 37208 RVA: 0x00256EC7 File Offset: 0x002550C7
		private SimGameState.FilteredComparisonResults GetWhiteListComparisons(IEnumerable<ComparisonDef> comparisons)
		{
			return this.GetFilteredComparisons(comparisons, new Func<ComparisonDef, bool>(this.IsWhiteList));
		}

		// Token: 0x06009159 RID: 37209 RVA: 0x00256EDC File Offset: 0x002550DC
		private SimGameState.FilteredComparisonResults GetFilteredComparisons(IEnumerable<ComparisonDef> comparisons, Func<ComparisonDef, bool> ComparisonFunc)
		{
			IEnumerable<string> enumerable = from c in comparisons
				where ComparisonFunc(c)
				select c.obj.Split(new char[] { '.' }) into s
				where this.IsProperLength(s)
				select s[1];
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			foreach (string text in enumerable)
			{
				list.Add(text);
				if (text == "IsOwner")
				{
					list2.Add(FactionEnumeration.GetInvalidUnsetFactionValue().Name);
				}
				else
				{
					list2.Add(this.GetFactionValueFromString(text).Name);
				}
			}
			return new SimGameState.FilteredComparisonResults(list, list2);
		}

		// Token: 0x0600915A RID: 37210 RVA: 0x00256FEC File Offset: 0x002551EC
		private bool RequiresGreatHouses(string comparisonString)
		{
			return string.Compare(comparisonString, "IsGreatHouse") == 0;
		}

		// Token: 0x0600915B RID: 37211 RVA: 0x00256FFC File Offset: 0x002551FC
		private bool RequiresSystemOwner(string comparisonString)
		{
			return string.Compare(comparisonString, "IsOwner") == 0;
		}

		// Token: 0x0600915C RID: 37212 RVA: 0x0025700C File Offset: 0x0025520C
		private bool IsWhiteList(ComparisonDef def)
		{
			return def.op == Operator.GreaterThanOrEqual || (def.op == Operator.Equal && def.val > 0f) || def.op == Operator.GreaterThan || (def.op == Operator.NotEqual && def.val == 0f);
		}

		// Token: 0x0600915D RID: 37213 RVA: 0x0025705A File Offset: 0x0025525A
		private bool IsBlackList(ComparisonDef def)
		{
			return def.op == Operator.LessThan || (def.op == Operator.NotEqual && def.val < 1f) || (def.op == Operator.Equal && def.val == 0f);
		}

		// Token: 0x0600915E RID: 37214 RVA: 0x00257094 File Offset: 0x00255294
		private bool IsProperLength(string[] subitem)
		{
			return subitem != null && subitem.Length >= 2;
		}

		// Token: 0x0600915F RID: 37215 RVA: 0x002570A4 File Offset: 0x002552A4
		private FactionValue GetDefaultHostileFaction(FactionValue employer, FactionValue target)
		{
			if (!employer.IsAuriganPirates && !target.IsAuriganPirates)
			{
				return FactionEnumeration.GetAuriganPiratesFactionValue();
			}
			return FactionEnumeration.GetHostileMercenariesFactionValue();
		}

		// Token: 0x06009160 RID: 37216 RVA: 0x002570C4 File Offset: 0x002552C4
		private float GetModifiedDifficultyScale(int diff)
		{
			float num = 0f;
			for (float num2 = 1f; num2 <= (float)diff; num2 += 1f)
			{
				num += 1f / (num2 * 0.4f);
			}
			return num - 1.5f;
		}

		// Token: 0x06009161 RID: 37217 RVA: 0x00257108 File Offset: 0x00255308
		private ContractDifficulty GetDifficultyCategory(int actualDifficulty)
		{
			ContractDifficulty contractDifficulty;
			if (actualDifficulty <= 3)
			{
				contractDifficulty = ContractDifficulty.Easy;
			}
			else if (actualDifficulty >= 7)
			{
				contractDifficulty = ContractDifficulty.Hard;
			}
			else
			{
				contractDifficulty = ContractDifficulty.Medium;
			}
			return contractDifficulty;
		}

		// Token: 0x06009162 RID: 37218 RVA: 0x00257128 File Offset: 0x00255328
		public int CalculateContractValueByContractType(ContractTypeValue contractTypeValue, int diff, float multiplier, float baseVariance, int presetSeed)
		{
			int num = ((diff > 0) ? diff : 1);
			float modifiedDifficultyScale = this.GetModifiedDifficultyScale(num);
			float contractRewardMultiplier = contractTypeValue.ContractRewardMultiplier;
			float num2 = multiplier * contractRewardMultiplier;
			float num3 = modifiedDifficultyScale * num2;
			NetworkRandom networkRandom;
			if (presetSeed == 0)
			{
				networkRandom = this.NetworkRandom;
			}
			else
			{
				networkRandom = new NetworkRandom();
				networkRandom.seed = presetSeed;
			}
			float num4 = networkRandom.Float(-baseVariance, baseVariance);
			float num5 = num3 * num4;
			return Mathf.RoundToInt(num3 + num5);
		}

		// Token: 0x06009163 RID: 37219 RVA: 0x0025718C File Offset: 0x0025538C
		public int CalculateContractValue(int diff, float multiplier, float baseVariance)
		{
			int num = ((diff > 0) ? diff : 1);
			float num2 = this.GetModifiedDifficultyScale(num) * multiplier;
			float num3 = this.NetworkRandom.Float(-baseVariance, baseVariance);
			float num4 = num2 * num3;
			return Mathf.RoundToInt(num2 + num4);
		}

		// Token: 0x06009164 RID: 37220 RVA: 0x002571C8 File Offset: 0x002553C8
		public bool ContractUserMeetsReputation(Contract c)
		{
			if (c.IsFlashpointContract || c.IsFlashpointCampaignContract)
			{
				return true;
			}
			SimGameState.SimGameType simGameMode = this.SimGameMode;
			if (simGameMode == SimGameState.SimGameType.CAREER)
			{
				return this.ContractUserMeetsReputation_Career(c);
			}
			return this.ContractUserMeetsReputation_Campaign(c);
		}

		// Token: 0x06009165 RID: 37221 RVA: 0x00257204 File Offset: 0x00255404
		public bool ContractUserMeetsReputation_Campaign(Contract c)
		{
			int num = Mathf.Min(c.Override.finalDifficulty + c.Override.difficultyUIModifier, (int)this.Constants.Story.GlobalContractDifficultyMax);
			FactionValue teamFaction = c.GetTeamFaction("ecc8d4f2-74b4-465d-adf6-84445e5dfc230");
			if (!teamFaction.DoesGainReputation || c.Override.contractDisplayStyle == ContractDisplayStyle.BaseCampaignStory || c.Override.contractDisplayStyle == ContractDisplayStyle.BaseCampaignRestoration || c.Override.contractDisplayStyle == ContractDisplayStyle.BaseFlashpoint)
			{
				return true;
			}
			switch (this.GetReputation(teamFaction))
			{
			case SimGameReputation.LOATHED:
				return (float)num <= this.Constants.Story.LoathedMaxContractDifficulty + this.GlobalDifficulty;
			case SimGameReputation.HATED:
				return (float)num <= this.Constants.Story.HatedMaxContractDifficulty + this.GlobalDifficulty;
			case SimGameReputation.DISLIKED:
				return (float)num <= this.Constants.Story.DislikedMaxContractDifficulty + this.GlobalDifficulty;
			case SimGameReputation.INDIFFERENT:
				return (float)num <= this.Constants.Story.IndifferentMaxContractDifficulty + this.GlobalDifficulty;
			case SimGameReputation.LIKED:
				return (float)num <= this.Constants.Story.LikedMaxContractDifficulty + this.GlobalDifficulty;
			case SimGameReputation.FRIENDLY:
				return (float)num <= this.Constants.Story.FriendlyMaxContractDifficulty + this.GlobalDifficulty;
			default:
				return (float)num <= this.Constants.Story.HonoredMaxContractDifficulty + this.GlobalDifficulty;
			}
		}

		// Token: 0x06009166 RID: 37222 RVA: 0x00257388 File Offset: 0x00255588
		public bool ContractUserMeetsReputation_Career(Contract c)
		{
			int num = Mathf.Min(c.Override.finalDifficulty + c.Override.difficultyUIModifier, (int)this.Constants.Story.GlobalContractDifficultyMax);
			FactionValue teamFaction = c.GetTeamFaction("ecc8d4f2-74b4-465d-adf6-84445e5dfc230");
			if (!teamFaction.DoesGainReputation || c.Override.contractDisplayStyle == ContractDisplayStyle.BaseCampaignStory || c.Override.contractDisplayStyle == ContractDisplayStyle.BaseCampaignRestoration || c.Override.contractDisplayStyle == ContractDisplayStyle.BaseFlashpoint)
			{
				return true;
			}
			switch (this.GetReputation(teamFaction))
			{
			case SimGameReputation.LOATHED:
				return (float)num <= this.Constants.CareerMode.LoathedMaxContractDifficulty;
			case SimGameReputation.HATED:
				return (float)num <= this.Constants.CareerMode.HatedMaxContractDifficulty;
			case SimGameReputation.DISLIKED:
				return (float)num <= this.Constants.CareerMode.DislikedMaxContractDifficulty;
			case SimGameReputation.INDIFFERENT:
				return (float)num <= this.Constants.CareerMode.IndifferentMaxContractDifficulty;
			case SimGameReputation.LIKED:
				return (float)num <= this.Constants.CareerMode.LikedMaxContractDifficulty;
			case SimGameReputation.FRIENDLY:
				return (float)num <= this.Constants.CareerMode.FriendlyMaxContractDifficulty;
			default:
				return (float)num <= this.Constants.CareerMode.HonoredMaxContractDifficulty;
			}
		}

		// Token: 0x06009167 RID: 37223 RVA: 0x002574D8 File Offset: 0x002556D8
		public SimGameReputation GetContractReputationReq(Contract c)
		{
			int difficulty = c.Override.difficulty;
			if ((float)difficulty >= this.Constants.Story.IndifferentMaxContractDifficulty)
			{
				return SimGameReputation.LIKED;
			}
			if ((float)difficulty >= this.Constants.Story.DislikedMaxContractDifficulty)
			{
				return SimGameReputation.INDIFFERENT;
			}
			return SimGameReputation.DISLIKED;
		}

		// Token: 0x06009168 RID: 37224 RVA: 0x00257520 File Offset: 0x00255720
		public bool IsContractOurArrivedAtTravelContract(Contract c)
		{
			if (!this.HasTravelContract)
			{
				return false;
			}
			if (c == null)
			{
				return false;
			}
			if (c.Name != this.ActiveTravelContract.Name)
			{
				return false;
			}
			StarSystem starSystem = c.GameContext.GetObject(GameContextObjectTagEnum.TargetStarSystem) as StarSystem;
			return this.CurSystem == starSystem && this.TravelState == SimGameTravelStatus.IN_SYSTEM;
		}

		// Token: 0x06009169 RID: 37225 RVA: 0x00257580 File Offset: 0x00255780
		public List<Contract> GetAllCurrentlySelectableContracts(bool includeArrivedAtTravelContract = true)
		{
			List<Contract> list = new List<Contract>();
			if (this.ActiveFlashpoint != null && this.ActiveFlashpoint.ActiveContract != null && this.ActiveFlashpoint.CurSystem.ID == this.CurSystem.ID)
			{
				list.Add(this.ActiveFlashpoint.ActiveContract);
			}
			list.AddRange(this.GlobalContracts);
			list.AddRange(this.CurSystem.SystemContracts);
			list.AddRange(this.CurSystem.SystemBreadcrumbs);
			if (this.HasTravelContract && this.CurSystem.ID == this.ActiveTravelContract.TargetSystem && this.TravelState == SimGameTravelStatus.IN_SYSTEM && (!list.Contains(this.ActiveTravelContract) && includeArrivedAtTravelContract))
			{
				list.Add(this.ActiveTravelContract);
			}
			return list;
		}

		// Token: 0x0600916A RID: 37226 RVA: 0x00257658 File Offset: 0x00255858
		public SimGameState.AddContractData ParseFlashpointContractActionData(string actionValue, string[] additionalValues)
		{
			return new SimGameState.AddContractData
			{
				Map = actionValue,
				MapPath = additionalValues[0],
				EncounterGuid = additionalValues[1],
				ContractName = additionalValues[2],
				Employer = additionalValues[3],
				Target = additionalValues[4],
				Difficulty = int.Parse(additionalValues[5]),
				NextNodeId = additionalValues.ElementAtOrDefault(6),
				EmployerAlly = additionalValues.ElementAtOrDefault(7),
				TargetAlly = additionalValues.ElementAtOrDefault(8),
				OnContractFailureMilestone = additionalValues.ElementAtOrDefault(9),
				NeutralToAll = additionalValues.ElementAtOrDefault(10),
				HostileToAll = additionalValues.ElementAtOrDefault(11)
			};
		}

		// Token: 0x0600916B RID: 37227 RVA: 0x002576FD File Offset: 0x002558FD
		public SimGameState.AddContractData ParseContractActionData(string actionValue, string[] additionalValues)
		{
			return new SimGameState.AddContractData
			{
				ContractName = actionValue,
				Target = additionalValues[0],
				Employer = additionalValues[1],
				TargetSystem = additionalValues.ElementAtOrDefault(2),
				TargetAlly = additionalValues.ElementAtOrDefault(3)
			};
		}

		// Token: 0x0600916C RID: 37228 RVA: 0x00257738 File Offset: 0x00255938
		public SimGameState.AddContractData ParseNonProceduralContractActionData(string actionValue, string[] additionalValues)
		{
			return new SimGameState.AddContractData
			{
				Map = actionValue,
				TargetSystem = additionalValues.ElementAt(0),
				MapPath = additionalValues[1],
				EncounterGuid = additionalValues[2],
				ContractName = additionalValues[3],
				IsGlobal = string.Equals(bool.TrueString, additionalValues[4], StringComparison.OrdinalIgnoreCase),
				Employer = additionalValues[5],
				Target = additionalValues[6],
				Difficulty = int.Parse(additionalValues[7]),
				CarryOverNegotiation = string.Equals(bool.TrueString, additionalValues[8], StringComparison.OrdinalIgnoreCase),
				TargetAlly = additionalValues.ElementAtOrDefault(9),
				RandomSeed = ((additionalValues.ElementAtOrDefault(10) != null) ? int.Parse(additionalValues[10]) : 0),
				EmployerAlly = additionalValues.ElementAtOrDefault(11),
				NeutralToAll = additionalValues.ElementAtOrDefault(12),
				HostileToAll = additionalValues.ElementAtOrDefault(13)
			};
		}

		// Token: 0x0600916D RID: 37229 RVA: 0x00257818 File Offset: 0x00255A18
		public bool GenerateProceduralContractsSampleCSV()
		{
			Dictionary<string, StarSystem>.ValueCollection values = this.StarSystemDictionary.Values;
			List<SimGameState.ValidFactionResult> list = new List<SimGameState.ValidFactionResult>();
			IEnumerable<ContractOverride> enumerable = from c in MetadataDatabase.Instance.GetSinglePlayerProceduralContractsByType()
				where this.DataManager.ContractOverrides.Exists(c.ContractID)
				select this.DataManager.ContractOverrides.Get(c.ContractID);
			using (Dictionary<string, StarSystem>.ValueCollection.Enumerator enumerator = values.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					SimGameState.<>c__DisplayClass525_0 CS$<>8__locals1 = new SimGameState.<>c__DisplayClass525_0();
					CS$<>8__locals1.<>4__this = this;
					CS$<>8__locals1.system = enumerator.Current;
					Dictionary<string, WeightedList<SimGameState.ContractParticipants>> dictionary = (from e in CS$<>8__locals1.system.Def.ContractEmployerIDList
						where !this.ignoredContractEmployers.Contains(e)
						select new
						{
							Employer = e,
							Targets = CS$<>8__locals1.<>4__this.GenerateContractParticipants(CS$<>8__locals1.<>4__this.factions[e], CS$<>8__locals1.system.Def)
						} into e
						where e.Targets.Any<SimGameState.ContractParticipants>()
						select e).ToDictionary(e => e.Employer, t => t.Targets.ToWeightedList(WeightedListType.PureRandom));
					List<MapAndEncounters> playableMaps = MetadataDatabase.Instance.GetReleasedMapsAndEncountersBySinglePlayerProceduralContractTypeAndTags(CS$<>8__locals1.system.Def.MapRequiredTags, CS$<>8__locals1.system.Def.MapExcludedTags, CS$<>8__locals1.system.Def.SupportedBiomes, true);
					using (IEnumerator<ContractOverride> enumerator2 = enumerable.Where((ContractOverride c) => (from e in playableMaps.SelectMany((MapAndEncounters p) => p.Encounters)
						select e.ContractTypeRow.ContractTypeID).Contains((long)c.ContractTypeValue.ID)).GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							ContractOverride contract = enumerator2.Current;
							SimGameState.ChosenContractParticipants chosenContractParticipants;
							bool validFaction = this.GetValidFaction(CS$<>8__locals1.system, dictionary, contract.requirementList, out chosenContractParticipants);
							CS$<>8__locals1.system.SetCurrentContractFactions(chosenContractParticipants.Employer, chosenContractParticipants.Target);
							List<MapAndEncounters> list2 = playableMaps.Where((MapAndEncounters p) => p.Encounters.Select((EncounterLayer_MDD e) => e.ContractTypeRow.ContractTypeID).Contains((long)contract.ContractTypeValue.ID) && CS$<>8__locals1.<>4__this.DoesContractMeetRequirements(CS$<>8__locals1.system, p, contract)).ToList<MapAndEncounters>();
							CS$<>8__locals1.system.SetCurrentContractFactions(null, null);
							string text;
							if (!list2.Any<MapAndEncounters>())
							{
								text = "NONE";
							}
							else
							{
								text = list2.Select((MapAndEncounters p) => p.Map.FriendlyName).Aggregate((string current, string next) => current + "-" + next);
							}
							string text2 = text;
							list.Add(new SimGameState.ValidFactionResult
							{
								System = CS$<>8__locals1.system.Name,
								Maps = text2,
								Contract = contract.ID,
								Difficulty = contract.difficulty,
								Scope = contract.scope.ToString(),
								Valid = (validFaction && text2 != "NONE"),
								Employer = chosenContractParticipants.Employer,
								Target = chosenContractParticipants.Target,
								EmployerAlly = chosenContractParticipants.EmployersAlly,
								TargetAlly = chosenContractParticipants.TargetsAlly,
								Neutral = chosenContractParticipants.NeutralToAll,
								Hostile = chosenContractParticipants.HostileToAll
							});
						}
					}
				}
			}
			List<string> list3 = list.Select((SimGameState.ValidFactionResult o) => o.ToString()).ToList<string>();
			list3.Insert(0, SimGameState.ValidFactionResult.GetStringHeader());
			File.WriteAllLines(Application.temporaryCachePath + "/ProceduralContractsSample.txt", list3.ToArray());
			return false;
		}

		// Token: 0x0600916E RID: 37230 RVA: 0x00257C4C File Offset: 0x00255E4C
		public StatCollection GetStatsByScope(EventScope scope)
		{
			switch (scope)
			{
			case EventScope.Company:
				return this.CompanyStats;
			case EventScope.MechWarrior:
				return ((Pilot)this.Context.GetObject(GameContextObjectTagEnum.TargetMechWarrior)).StatCollection;
			case EventScope.Mech:
				return ((MechDef)this.Context.GetObject(GameContextObjectTagEnum.TargetUnit)).Stats;
			case EventScope.Commander:
				return this.CommanderStats;
			case EventScope.StarSystem:
				return this.CurSystem.Stats;
			case EventScope.SecondaryMechWarrior:
			case EventScope.SecondaryMech:
			case EventScope.AllMechWarriors:
			case EventScope.AllMechs:
			case EventScope.TertiaryMechWarrior:
				break;
			case EventScope.RandomMech:
				return ((MechDef)this.Context.GetObject(GameContextObjectTagEnum.RandomUnit)).Stats;
			default:
				if (scope == EventScope.Flashpoint)
				{
					if (this.ActiveFlashpoint == null)
					{
						return null;
					}
					return this.ActiveFlashpoint.Stats;
				}
				break;
			}
			return null;
		}

		// Token: 0x0600916F RID: 37231 RVA: 0x00257D0C File Offset: 0x00255F0C
		public TagSet GetTagsByScope(EventScope scope)
		{
			switch (scope)
			{
			case EventScope.Company:
				return this.CompanyTags;
			case EventScope.MechWarrior:
				return ((Pilot)this.Context.GetObject(GameContextObjectTagEnum.TargetMechWarrior)).pilotDef.PilotTags;
			case EventScope.Mech:
				return ((MechDef)this.Context.GetObject(GameContextObjectTagEnum.TargetUnit)).MechTags;
			case EventScope.Commander:
				return this.CommanderTags;
			case EventScope.StarSystem:
				return this.CurSystem.Tags;
			case EventScope.SecondaryMechWarrior:
			case EventScope.SecondaryMech:
			case EventScope.AllMechWarriors:
			case EventScope.AllMechs:
			case EventScope.TertiaryMechWarrior:
				break;
			case EventScope.RandomMech:
				return ((MechDef)this.Context.GetObject(GameContextObjectTagEnum.RandomUnit)).MechTags;
			default:
				if (scope == EventScope.Flashpoint)
				{
					if (this.ActiveFlashpoint == null)
					{
						return null;
					}
					return this.ActiveFlashpoint.Tags;
				}
				break;
			}
			return null;
		}

		// Token: 0x06009170 RID: 37232 RVA: 0x00257DD0 File Offset: 0x00255FD0
		public bool IsFactionAlly(FactionValue faction, List<string> allyListOverride = null)
		{
			List<string> list = allyListOverride;
			if (list == null)
			{
				list = this.AlliedFactions;
			}
			return list.Contains(faction.Name);
		}

		// Token: 0x06009171 RID: 37233 RVA: 0x00257DF8 File Offset: 0x00255FF8
		public bool IsFactionEnemy(FactionValue faction, List<string> allyListOverride = null)
		{
			if (this.IsFactionAlly(faction, allyListOverride))
			{
				return false;
			}
			List<string> list = allyListOverride;
			if (list == null)
			{
				list = this.AlliedFactions;
			}
			for (int i = 0; i < list.Count; i++)
			{
				FactionDef factionDef = this.GetFactionDef(list[i]);
				if (factionDef.Enemies != null && factionDef.Enemies.Contains(faction.Name))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06009172 RID: 37234 RVA: 0x00257E5C File Offset: 0x0025605C
		public List<string> GetAllEnemies(List<string> allyListOverride = null)
		{
			List<string> list = allyListOverride;
			if (list == null)
			{
				list = this.AlliedFactions;
			}
			List<string> list2 = new List<string>();
			for (int i = 0; i < list.Count; i++)
			{
				FactionDef factionDef = this.GetFactionDef(list[i]);
				if (factionDef.Enemies != null)
				{
					for (int j = 0; j < factionDef.Enemies.Length; j++)
					{
						FactionValue factionByName = FactionEnumeration.GetFactionByName(factionDef.Enemies[j]);
						if (factionByName.DoesGainReputation && !list2.Contains(factionByName.Name))
						{
							list2.Add(factionByName.Name);
						}
					}
				}
			}
			return list2;
		}

		// Token: 0x06009173 RID: 37235 RVA: 0x00257EF0 File Offset: 0x002560F0
		public string GetFactionStoreLocationsString(FactionValue theFaction)
		{
			StringBuilder stringBuilder = new StringBuilder();
			List<StarSystem> list = this.FactionStoreStarSystemsDictionary[theFaction.Name];
			for (int i = 0; i < list.Count; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(" and ");
				}
				stringBuilder.Append(list[i].Name);
			}
			return stringBuilder.ToString();
		}

		// Token: 0x06009174 RID: 37236 RVA: 0x00257F4F File Offset: 0x0025614F
		public bool IsSystemFactionStore(StarSystem system, FactionValue theFaction)
		{
			return this.FactionStoreStarSystemsDictionary[theFaction.Name].Contains(system);
		}

		// Token: 0x06009175 RID: 37237 RVA: 0x00257F68 File Offset: 0x00256168
		public bool IsSystemBlackMarket(StarSystem system)
		{
			return system.Tags.Contains("planet_other_blackmarket");
		}

		// Token: 0x06009176 RID: 37238 RVA: 0x00257F7C File Offset: 0x0025617C
		public bool CanFactionBeAllied(FactionValue faction)
		{
			return faction.CanAlly && faction.DoesGainReputation && this.GetAllianceBrokenCooldown(faction) <= 0 && !this.IsFactionAlly(faction, null) && (float)this.GetRawReputation(faction) >= this.Constants.Story.AllyReputationThreshold && !this.IsFactionEnemy(faction, null);
		}

		// Token: 0x06009177 RID: 37239 RVA: 0x00257FE0 File Offset: 0x002561E0
		public int GetAllianceBrokenCooldown(FactionValue faction)
		{
			string text = string.Format("{0}.{1}", "AllianceBroken", faction.Name);
			if (this.CompanyStats.ContainsStatistic(text))
			{
				return Mathf.Max(0, this.Constants.Story.AllianceBreakCooldown - (this.DaysPassed - this.CompanyStats.GetValue<int>(text)));
			}
			return 0;
		}

		// Token: 0x06009178 RID: 37240 RVA: 0x00258040 File Offset: 0x00256240
		public int GetFactionRepNotifcationValue(FactionValue faction)
		{
			string text = string.Format("{0}.{1}", "REP_NOTIFICATION_STATE", faction.Name);
			if (this.CompanyStats.ContainsStatistic(text))
			{
				return this.CompanyStats.GetValue<int>(text);
			}
			return 0;
		}

		// Token: 0x06009179 RID: 37241 RVA: 0x00258080 File Offset: 0x00256280
		public bool CanNotifyOfNewFactionRep()
		{
			int num = 0;
			if (this.CompanyStats.ContainsStatistic("LAST_REP_NOTIFICATION_DAY"))
			{
				num = this.CompanyStats.GetValue<int>("LAST_REP_NOTIFICATION_DAY");
			}
			return this.daysPassed - num >= this.Constants.Story.DaysBetweenFactionRepUpdateNotifications;
		}

		// Token: 0x0600917A RID: 37242 RVA: 0x002580D0 File Offset: 0x002562D0
		public void TestReputationLevelChange(FactionValue faction)
		{
			SimGameState.FactionReputationStateForNotifications factionReputationStateForNotifications = SimGameState.FactionReputationStateForNotifications.Neutral;
			string text = string.Format("{0}.{1}", "REP_NOTIFICATION_STATE", faction.Name);
			if (this.CompanyStats.ContainsStatistic(text))
			{
				factionReputationStateForNotifications = this.CompanyStats.GetValue<SimGameState.FactionReputationStateForNotifications>(text);
			}
			else
			{
				this.CompanyStats.AddStatistic<SimGameState.FactionReputationStateForNotifications>(text, SimGameState.FactionReputationStateForNotifications.Neutral);
			}
			int rawReputation = this.GetRawReputation(faction);
			if (rawReputation < this.Constants.Story.LowRepNotificationValue)
			{
				if (factionReputationStateForNotifications != SimGameState.FactionReputationStateForNotifications.ShownNegative && this.ShowFactionReputationLevelNotification(faction, SimGameState.FactionReputationStateForNotifications.ShownNegative))
				{
					this.CompanyStats.ModifyStat<SimGameState.FactionReputationStateForNotifications>("ReputationDayPassed", 0, text, StatCollection.StatOperation.Set, SimGameState.FactionReputationStateForNotifications.ShownNegative, -1, true);
					return;
				}
			}
			else if (this.CanFactionBeAllied(faction))
			{
				if (factionReputationStateForNotifications != SimGameState.FactionReputationStateForNotifications.ShownAllied && this.ShowFactionReputationLevelNotification(faction, SimGameState.FactionReputationStateForNotifications.ShownAllied))
				{
					this.CompanyStats.ModifyStat<SimGameState.FactionReputationStateForNotifications>("ReputationDayPassed", 0, text, StatCollection.StatOperation.Set, SimGameState.FactionReputationStateForNotifications.ShownAllied, -1, true);
					return;
				}
			}
			else if (rawReputation > this.Constants.Story.HighRepNotificationValue)
			{
				if (factionReputationStateForNotifications != SimGameState.FactionReputationStateForNotifications.ShownPositive && factionReputationStateForNotifications != SimGameState.FactionReputationStateForNotifications.ShownAllied && this.ShowFactionReputationLevelNotification(faction, SimGameState.FactionReputationStateForNotifications.ShownPositive))
				{
					this.CompanyStats.ModifyStat<SimGameState.FactionReputationStateForNotifications>("ReputationDayPassed", 0, text, StatCollection.StatOperation.Set, SimGameState.FactionReputationStateForNotifications.ShownPositive, -1, true);
					return;
				}
			}
			else if (rawReputation < 0)
			{
				if (factionReputationStateForNotifications == SimGameState.FactionReputationStateForNotifications.ShownPositive || factionReputationStateForNotifications == SimGameState.FactionReputationStateForNotifications.ShownAllied)
				{
					this.CompanyStats.ModifyStat<SimGameState.FactionReputationStateForNotifications>("ReputationDayPassed", 0, text, StatCollection.StatOperation.Set, SimGameState.FactionReputationStateForNotifications.Neutral, -1, true);
					return;
				}
			}
			else if (rawReputation > 0 && factionReputationStateForNotifications == SimGameState.FactionReputationStateForNotifications.ShownNegative)
			{
				this.CompanyStats.ModifyStat<SimGameState.FactionReputationStateForNotifications>("ReputationDayPassed", 0, text, StatCollection.StatOperation.Set, SimGameState.FactionReputationStateForNotifications.Neutral, -1, true);
			}
		}

		// Token: 0x0600917B RID: 37243 RVA: 0x0025821C File Offset: 0x0025641C
		public bool ShowFactionReputationLevelNotification(FactionValue faction, SimGameState.FactionReputationStateForNotifications state)
		{
			FactionDef factionDef = this.GetFactionDef(faction.Name);
			Sprite sprite = null;
			if (factionDef != null)
			{
				sprite = factionDef.GetSprite();
				CastDef defaultFactionRepresentative = factionDef.DefaultFactionRepresentative;
				if (defaultFactionRepresentative != null)
				{
					sprite = defaultFactionRepresentative.defaultEmotePortrait.LoadPortrait(false);
				}
				else
				{
					sprite = factionDef.GetSprite();
				}
			}
			if (factionDef == null)
			{
				return false;
			}
			switch (state)
			{
			case SimGameState.FactionReputationStateForNotifications.ShownNegative:
			{
				string text = Strings.T("Reputation Warning: {0}", new object[] { factionDef.Name });
				string text2 = Strings.T("Your actions against our interests are earning you a powerful enemy, Commander. Continue to impede {0} operations in this region, and you will pay the cost.\n<i><color=#A1A1A1>You can view your faction reputation status in the CPT QUARTERS.</color></i>", new object[] { factionDef.Demonym });
				this.interruptQueue.QueuePauseNotification(text, text2, sprite, "", null, "Continue", null, null);
				break;
			}
			case SimGameState.FactionReputationStateForNotifications.Neutral:
				return false;
			case SimGameState.FactionReputationStateForNotifications.ShownPositive:
			{
				string text3 = Strings.T("Reputation Update: {0}", new object[] { factionDef.Name });
				string text4 = Strings.T("My superiors thank you for your willingness to take on challenging tasks. You are making an invaluable contribution to the advancement of {0} interests in this region.\n<i><color=#A1A1A1>You can view your faction reputation status in the CPT QUARTERS.</color></i>", new object[] { factionDef.Demonym });
				this.interruptQueue.QueuePauseNotification(text3, text4, sprite, "", null, "Continue", null, null);
				break;
			}
			case SimGameState.FactionReputationStateForNotifications.ShownAllied:
			{
				string text5 = Strings.T("Alliance Offer: {0}", new object[] { factionDef.Name });
				string text6 = Strings.T("Your service on our behalf has been exemplary, Commander. My government wishes me to convey their gratitude. Perhaps we should formalize the bonds between us with an alliance.\n<i><color=#A1A1A1>You can view your faction reputation status in the CPT QUARTERS.</color></i>");
				this.interruptQueue.QueuePauseNotification(text5, text6, sprite, "", null, "Continue", null, null);
				break;
			}
			}
			if (this.CompanyStats.ContainsStatistic("LAST_REP_NOTIFICATION_DAY"))
			{
				this.CompanyStats.ModifyStat<int>("ReputationDayPassed", 0, "LAST_REP_NOTIFICATION_DAY", StatCollection.StatOperation.Set, this.daysPassed, -1, true);
			}
			else
			{
				this.CompanyStats.AddStatistic<int>("LAST_REP_NOTIFICATION_DAY", this.daysPassed);
			}
			return true;
		}

		// Token: 0x0600917C RID: 37244 RVA: 0x002583C4 File Offset: 0x002565C4
		public bool AddAllyFaction(FactionValue faction, bool checkRequirement = true)
		{
			if (checkRequirement && !this.CanFactionBeAllied(faction))
			{
				return false;
			}
			if (!faction.DoesGainReputation)
			{
				return false;
			}
			if (this.IsFactionAlly(faction, null))
			{
				return false;
			}
			string text = "ALLIED_FACTION_" + faction.Name;
			this.companyTags.Add(text);
			this.AlliedFactions.Add(faction.Name);
			this.SetFactionValidators(true);
			SimGameAllyFactionAddedMessage simGameAllyFactionAddedMessage = new SimGameAllyFactionAddedMessage(faction.Name);
			this.MessageCenter.PublishMessage(simGameAllyFactionAddedMessage);
			return true;
		}

		// Token: 0x0600917D RID: 37245 RVA: 0x00258444 File Offset: 0x00256644
		public void RemoveAllyFaction(FactionValue faction)
		{
			if (!this.IsFactionAlly(faction, null))
			{
				return;
			}
			string text = string.Format("{0}.{1}", "AllianceBroken", faction.Name);
			this.AlliedFactions.Remove(faction.Name);
			string text2 = "ALLIED_FACTION_" + faction.Name;
			this.companyTags.Remove(text2);
			FactionDef factionDef = this.GetFactionDef(faction.Name);
			for (int i = 0; i < factionDef.Enemies.Length; i++)
			{
				string text3 = factionDef.Enemies[i];
				bool flag = false;
				foreach (string text4 in this.AlliedFactions)
				{
					if (this.GetFactionDef(text4).Enemies.Contains(text3))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					string text5 = "ENEMY_FACTION_" + text3;
					this.companyTags.Remove(text5);
				}
			}
			this.SetCompanyStat(text, this.DaysPassed, false);
			this.SetReputation(faction, this.Constants.Story.BreakAllianceReputationChange, StatCollection.StatOperation.Int_Add, null);
			this.SetFactionValidators(true);
		}

		// Token: 0x0600917E RID: 37246 RVA: 0x0025857C File Offset: 0x0025677C
		public void SetFactionValidators(bool forceValidate = false)
		{
			List<string> allEnemies = this.GetAllEnemies(null);
			foreach (string text in allEnemies)
			{
				FactionValue factionByName = FactionEnumeration.GetFactionByName(text);
				if (factionByName.DoesGainReputation)
				{
					string repID = this.GetRepID("Reputation", factionByName);
					Statistic statistic = this.CompanyStats.GetStatistic(repID);
					bool flag = false;
					bool flag2 = false;
					if (statistic != null)
					{
						if (this.IsFactionAlly(factionByName, null))
						{
							statistic.SetValidator<int>(new Statistic.Validator<int>(this.ReputationAllyValidator<int>));
							flag = true;
						}
						else if (allEnemies.Contains(factionByName.Name))
						{
							statistic.SetValidator<int>(new Statistic.Validator<int>(this.ReputationEnemyValidator<int>));
							flag2 = true;
						}
						else
						{
							statistic.SetValidator<int>(new Statistic.Validator<int>(this.ReputationNormalValidator<int>));
						}
						if (forceValidate)
						{
							statistic.SetValue<int>(statistic.Value<int>(), true);
							string text2 = factionByName.ToString();
							string text3 = "ALLIED_FACTION_" + text2;
							string text4 = "ENEMY_FACTION_" + text2;
							if (flag && !this.companyTags.Contains(text3))
							{
								this.companyTags.Add(text3);
							}
							else if (!flag && this.companyTags.Contains(text3))
							{
								this.companyTags.Remove(text3);
							}
							if (flag2 && !this.companyTags.Contains(text4))
							{
								this.companyTags.Add(text4);
							}
							else if (!flag2 && this.companyTags.Contains(text4))
							{
								this.companyTags.Remove(text4);
							}
						}
					}
				}
			}
		}

		// Token: 0x0600917F RID: 37247 RVA: 0x00258734 File Offset: 0x00256934
		public int GetFinalReputationChange(FactionValue faction, int valueChange)
		{
			if (!faction.DoesGainReputation)
			{
				return 0;
			}
			int rawReputation = this.GetRawReputation(faction);
			string repID = this.GetRepID("Reputation", faction);
			Statistic statistic = this.CompanyStats.GetStatistic(repID);
			if (statistic == null)
			{
				return valueChange;
			}
			Statistic.Validator<int> validator = statistic.GetValidator<int>();
			if (validator == null)
			{
				return valueChange;
			}
			int num = rawReputation + valueChange;
			validator(ref num);
			if (rawReputation + valueChange == num)
			{
				return valueChange;
			}
			return num - rawReputation;
		}

		// Token: 0x06009180 RID: 37248 RVA: 0x0025879C File Offset: 0x0025699C
		public int GetRawReputation(FactionValue faction)
		{
			string repID = this.GetRepID("Reputation", faction);
			return this.companyStats.GetValue<int>(repID);
		}

		// Token: 0x06009181 RID: 37249 RVA: 0x002587C2 File Offset: 0x002569C2
		public SimGameReputation GetReputation(FactionValue faction)
		{
			if (!faction.DoesGainReputation)
			{
				return SimGameReputation.INDIFFERENT;
			}
			return this.GetReputation((float)this.GetRawReputation(faction));
		}

		// Token: 0x06009182 RID: 37250 RVA: 0x002587DC File Offset: 0x002569DC
		public SimGameReputation GetReputation(float reputation)
		{
			if (reputation <= this.Constants.Story.LoathedReputation)
			{
				return SimGameReputation.LOATHED;
			}
			if (reputation <= this.Constants.Story.HatedReputation)
			{
				return SimGameReputation.HATED;
			}
			if (reputation <= this.Constants.Story.DislikedReputation)
			{
				return SimGameReputation.DISLIKED;
			}
			if (reputation >= this.Constants.Story.HonoredReputation)
			{
				return SimGameReputation.HONORED;
			}
			if (reputation >= this.Constants.Story.FriendlyReputation)
			{
				return SimGameReputation.FRIENDLY;
			}
			if (reputation >= this.Constants.Story.LikedReputation)
			{
				return SimGameReputation.LIKED;
			}
			return SimGameReputation.INDIFFERENT;
		}

		// Token: 0x06009183 RID: 37251 RVA: 0x0025886A File Offset: 0x00256A6A
		public float GetReputationPaymentAdjustment(FactionValue faction)
		{
			return this.GetReputationPaymentAdjustment(this.GetReputation(faction));
		}

		// Token: 0x06009184 RID: 37252 RVA: 0x0025887C File Offset: 0x00256A7C
		public float GetReputationPaymentAdjustment(SimGameReputation rep)
		{
			switch (rep)
			{
			case SimGameReputation.LOATHED:
				return this.Constants.Story.LoathedRepPaymentAdjustment;
			case SimGameReputation.HATED:
				return this.Constants.Story.HatedRepPaymentAdjustment;
			case SimGameReputation.DISLIKED:
				return this.Constants.Story.DislikedRepPaymentAdjustment;
			case SimGameReputation.LIKED:
				return this.Constants.Story.LikedRepPaymentAdjustment;
			case SimGameReputation.FRIENDLY:
				return this.Constants.Story.FriendlyRepPaymentAdjustment;
			case SimGameReputation.HONORED:
				return this.Constants.Story.HonoredRepPaymentAdjustment;
			}
			return 0f;
		}

		// Token: 0x06009185 RID: 37253 RVA: 0x0025891B File Offset: 0x00256B1B
		public float GetReputationShopAdjustment(FactionValue faction)
		{
			return this.GetReputationShopAdjustment(this.GetReputation(faction));
		}

		// Token: 0x06009186 RID: 37254 RVA: 0x0025892C File Offset: 0x00256B2C
		public float GetReputationShopAdjustment(SimGameReputation rep)
		{
			switch (rep)
			{
			case SimGameReputation.LOATHED:
				return 10f;
			case SimGameReputation.HATED:
				return this.Constants.Story.HatedRepShopAdjustment;
			case SimGameReputation.DISLIKED:
				return this.Constants.Story.DislikedRepShopAdjustment;
			case SimGameReputation.LIKED:
				return this.Constants.Story.LikedRepShopAdjustment;
			case SimGameReputation.FRIENDLY:
				return this.Constants.Story.FriendlyRepShopAdjustment;
			case SimGameReputation.HONORED:
				return this.Constants.Story.HonoredRepShopAdjustment;
			}
			return this.Constants.Story.IndifferentRepShopAdjustment;
		}

		// Token: 0x06009187 RID: 37255 RVA: 0x002589CC File Offset: 0x00256BCC
		public float GetReputationPercentage(float rep)
		{
			float num;
			float num2;
			switch (this.GetReputation(rep))
			{
			case SimGameReputation.LOATHED:
				num = -1f * this.Constants.Story.MaxReputation;
				num2 = this.Constants.Story.LoathedReputation;
				break;
			case SimGameReputation.HATED:
				num = this.Constants.Story.LoathedReputation;
				num2 = this.Constants.Story.HatedReputation;
				break;
			case SimGameReputation.DISLIKED:
				num = this.Constants.Story.HatedReputation;
				num2 = this.Constants.Story.DislikedReputation;
				break;
			case SimGameReputation.INDIFFERENT:
				num = this.Constants.Story.DislikedReputation;
				num2 = this.Constants.Story.LikedReputation;
				break;
			case SimGameReputation.LIKED:
				num = this.Constants.Story.LikedReputation;
				num2 = this.Constants.Story.FriendlyReputation;
				break;
			case SimGameReputation.FRIENDLY:
				num = this.Constants.Story.FriendlyReputation;
				num2 = this.Constants.Story.HonoredReputation;
				break;
			case SimGameReputation.HONORED:
				num = this.Constants.Story.HonoredReputation;
				num2 = this.Constants.Story.MaxReputation;
				break;
			default:
				return 0f;
			}
			float num3 = num2 - num;
			return (rep - num) / num3;
		}

		// Token: 0x06009188 RID: 37256 RVA: 0x00258B34 File Offset: 0x00256D34
		public void SetSystemOwnerReputation()
		{
			if (this.CurSystem != null)
			{
				FactionValue ownerFactionValue = FactionEnumeration.GetOwnerFactionValue();
				this.SetReputation(ownerFactionValue, this.CurSystem.OwnerReputation, StatCollection.StatOperation.Set, null);
			}
		}

		// Token: 0x06009189 RID: 37257 RVA: 0x00258B64 File Offset: 0x00256D64
		public void SetReputation(FactionValue factionValue, int val, StatCollection.StatOperation op = StatCollection.StatOperation.Int_Add, string sourceID = null)
		{
			int num = this.ClampNewRepValue(this.GetRepID("Reputation", factionValue), val, StatCollection.StatOperation.Int_Add, factionValue.IsMercenaryReviewBoard);
			this.SetFactionVal(factionValue, "Reputation", num, op, sourceID);
			if (this.CurSystem != null && !factionValue.IsOwner && factionValue.ID == this.CurSystem.OwnerValue.ID)
			{
				this.SetSystemOwnerReputation();
			}
		}

		// Token: 0x0600918A RID: 37258 RVA: 0x00258BD0 File Offset: 0x00256DD0
		public void AddReputation(FactionValue faction, int val, bool modifyEnemies, string sourceID = null)
		{
			int num = this.ClampNewRepValue(this.GetRepID("Reputation", faction), val, StatCollection.StatOperation.Int_Add, faction.IsMercenaryReviewBoard);
			this.SetFactionVal(faction, "Reputation", num, StatCollection.StatOperation.Int_Add, sourceID);
			if (modifyEnemies && val > 0)
			{
				FactionDef factionDef = this.GetFactionDef(faction.Name);
				for (int i = 0; i < factionDef.Enemies.Length; i++)
				{
					FactionValue factionByName = FactionEnumeration.GetFactionByName(factionDef.Enemies[i]);
					if (factionByName.DoesGainReputation)
					{
						int num2 = Mathf.RoundToInt((float)val * this.Constants.Story.FactionEnemyRepChangeMod);
						int num3 = this.ClampNewRepValue(this.GetRepID("Reputation", factionByName), num2, StatCollection.StatOperation.Int_Add, faction.IsMercenaryReviewBoard);
						this.SetFactionVal(factionByName, "Reputation", num3, StatCollection.StatOperation.Int_Add, sourceID);
					}
				}
			}
			if (this.CurSystem != null && !faction.IsOwner && faction.Equals(this.CurSystem.OwnerValue))
			{
				this.SetSystemOwnerReputation();
			}
		}

		// Token: 0x0600918B RID: 37259 RVA: 0x00258CC6 File Offset: 0x00256EC6
		private void SetInfluence(FactionValue faction, float val, StatCollection.StatOperation op = StatCollection.StatOperation.Int_Add, string sourceID = null)
		{
			this.SetFactionVal(faction, "Influence", val, op, sourceID);
		}

		// Token: 0x0600918C RID: 37260 RVA: 0x00258CE0 File Offset: 0x00256EE0
		private void SetFactionVal(FactionValue factionValue, string header, object val, StatCollection.StatOperation op = StatCollection.StatOperation.Int_Add, string sourceID = null)
		{
			if (!factionValue.IsRealFaction && !factionValue.IsOwner)
			{
				return;
			}
			if (sourceID == null)
			{
				sourceID = "SimGameState";
			}
			string repID = this.GetRepID(header, factionValue);
			if (factionValue.IsMercenaryReviewBoard && header == "Reputation")
			{
				int num = Convert.ToInt32(val);
				if (this.companyStats.ContainsStatistic(repID))
				{
					this.companyStats.ModifyStat<int>(sourceID, 0, repID, op, num, -1, true);
					return;
				}
				if (num < 0)
				{
					num = 0;
				}
				this.companyStats.AddStatistic<int>(repID, num, new Statistic.Validator<int>(this.MinimumZeroValidator<int>));
				return;
			}
			else if (val.GetType() == typeof(int))
			{
				if (this.companyStats.ContainsStatistic(repID))
				{
					this.companyStats.ModifyStat<int>(sourceID, 0, repID, op, (int)val, -1, true);
					return;
				}
				this.companyStats.AddStatistic<int>(repID, (int)val);
				return;
			}
			else
			{
				if (this.companyStats.ContainsStatistic(repID))
				{
					this.companyStats.ModifyStat<float>(sourceID, 0, repID, op, (float)val, -1, true);
					return;
				}
				this.companyStats.AddStatistic<float>(repID, (float)val);
				return;
			}
		}

		// Token: 0x0600918D RID: 37261 RVA: 0x00258E04 File Offset: 0x00257004
		private int ClampNewRepValue(string repId, int val, StatCollection.StatOperation op, bool isMRBRep)
		{
			int num = Mathf.RoundToInt(-this.Constants.Story.MaxReputation);
			int num2 = Mathf.RoundToInt(this.Constants.Story.MaxReputation);
			if (isMRBRep)
			{
				num = 0;
				num2 = Mathf.RoundToInt(this.Constants.Story.MRBRepMaxCap);
			}
			int num3 = 0;
			if (this.companyStats.ContainsStatistic(repId))
			{
				num3 = this.companyStats.GetValue<int>(repId);
			}
			if (op == StatCollection.StatOperation.Set)
			{
				val = Mathf.Clamp(val, num, num2);
			}
			else
			{
				int num4 = num3 + val;
				if (num4 < num || num4 > num2)
				{
					int num5 = ((num4 < 0) ? (num4 - num) : (num4 - num2));
					val -= num5;
				}
			}
			return val;
		}

		// Token: 0x0600918E RID: 37262 RVA: 0x00258EA8 File Offset: 0x002570A8
		public bool MeetsRequirements(RequirementDef[] requirements)
		{
			if (requirements == null)
			{
				return true;
			}
			foreach (RequirementDef requirementDef in requirements)
			{
				if (!this.MeetsRequirements(requirementDef, null))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x0600918F RID: 37263 RVA: 0x00258EDC File Offset: 0x002570DC
		public bool MeetsRequirements(RequirementDef requirement, SimGameReport.ReportEntry log = null)
		{
			List<TagSet> list = new List<TagSet>();
			List<StatCollection> list2 = new List<StatCollection>();
			if (requirement == null)
			{
				if (log != null)
				{
					log.Log("Null requirement means we pass.");
				}
				return true;
			}
			if (log != null)
			{
				log.Log("Checking requirements for requirement ID: " + requirement.Scope);
			}
			EventScope scope = requirement.Scope;
			if (scope - EventScope.AllMechWarriors <= 1 || scope == EventScope.RandomMech)
			{
				string text = "Invalid Requirement Scope: " + requirement.Scope;
				if (log != null)
				{
					log.Log(text);
				}
				return false;
			}
			list.Add(this.GetTagsByScope(requirement.Scope));
			list2.Add(this.GetStatsByScope(requirement.Scope));
			for (int i = 0; i < list.Count; i++)
			{
				if (!SimGameState.MeetsRequirements(requirement, list[i], list2[i], log))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06009190 RID: 37264 RVA: 0x00258FAF File Offset: 0x002571AF
		public static bool MeetsRequirements(TagSet reqTags, TagSet exTags, List<ComparisonDef> compDefs, TagSet curTags, StatCollection stats, SimGameReport.ReportEntry log = null)
		{
			return SimGameState.MeetsTagRequirements(reqTags, exTags, curTags, log) && SimGameState.MeetsStatRequirements(compDefs, stats, log);
		}

		// Token: 0x06009191 RID: 37265 RVA: 0x00258FCC File Offset: 0x002571CC
		public static bool MeetsTagRequirements(TagSet reqTags, TagSet exTags, TagSet curTags, SimGameReport.ReportEntry log = null)
		{
			if (reqTags == null)
			{
				reqTags = new TagSet();
			}
			if (exTags == null)
			{
				exTags = new TagSet();
			}
			if (curTags == null)
			{
				curTags = new TagSet();
			}
			if (log != null)
			{
				log.Push("Tag Check");
				log.Log("Testing Tags: " + curTags.ToString());
				if (reqTags != null)
				{
					log.Log("Req Tags: " + reqTags.ToString());
				}
				if (exTags != null)
				{
					log.Log("Ex Tags: " + exTags.ToString());
				}
			}
			if (reqTags != null && reqTags.Count > 0 && !curTags.ContainsAll(reqTags))
			{
				if (log != null)
				{
					log.Log("TagSet doesn't not have all required tags: " + reqTags.ToString());
					log.Pop();
				}
				return false;
			}
			if (exTags != null && exTags.Count > 0 && curTags.ContainsAny(exTags, true))
			{
				if (log != null)
				{
					log.Log("TagSet contains an excluded tag: " + exTags.ToString());
					log.Pop();
				}
				return false;
			}
			if (log != null)
			{
				log.Log("Successful Tag Check");
				log.Pop();
			}
			return true;
		}

		// Token: 0x06009192 RID: 37266 RVA: 0x002590D4 File Offset: 0x002572D4
		public static bool MeetsStatRequirements(List<ComparisonDef> compDefs, StatCollection stats, SimGameReport.ReportEntry log = null)
		{
			if (log != null)
			{
				log.Push("Stat Check");
			}
			if (compDefs != null)
			{
				for (int i = 0; i < compDefs.Count; i++)
				{
					float num = 0f;
					if (stats.ContainsStatistic(compDefs[i].obj))
					{
						num = Convert.ToSingle(stats.GetStatistic(compDefs[i].obj).CurrentValue.objVal);
					}
					if (log != null)
					{
						log.Log(string.Format("Comparing {0} ({1}) {2} {3}", new object[]
						{
							compDefs[i].obj,
							num,
							compDefs[i].op,
							compDefs[i].val
						}));
					}
					if (!compDefs[i].Compare(num))
					{
						if (log != null)
						{
							log.Log(string.Format("Failed stat check: {0}", compDefs[i].obj));
							log.Pop();
						}
						return false;
					}
				}
			}
			if (log != null)
			{
				log.Log("Successful Stat Check");
				log.Pop();
			}
			return true;
		}

		// Token: 0x06009193 RID: 37267 RVA: 0x002591F1 File Offset: 0x002573F1
		public static bool MeetsRequirements(RequirementDef requirements, TagSet curTags, StatCollection stats, SimGameReport.ReportEntry log = null)
		{
			return requirements == null || SimGameState.MeetsRequirements(requirements.RequirementTags, requirements.ExclusionTags, requirements.RequirementComparisons, curTags, stats, log);
		}

		// Token: 0x06009194 RID: 37268 RVA: 0x00259214 File Offset: 0x00257414
		public SimGameEventResultSet GetResultSet(SimGameEventResultSet[] resultSet)
		{
			List<int> list = new List<int>();
			for (int i = 0; i < resultSet.Length; i++)
			{
				list.Add(resultSet[i].Weight);
			}
			int weightedResult = SimGameState.GetWeightedResult(list, this.NetworkRandom.Float(0f, 1f));
			return resultSet[weightedResult];
		}

		// Token: 0x06009195 RID: 37269 RVA: 0x00259264 File Offset: 0x00257464
		public static int GetWeightedResult(List<int> weights, float randomRoll)
		{
			int num = 0;
			foreach (int num2 in weights)
			{
				num += num2;
			}
			int num3 = Mathf.RoundToInt(randomRoll * (float)SimGameState.WeightMod * (float)num);
			int num4 = 0;
			for (int i = 0; i < weights.Count; i++)
			{
				num4 += weights[i] * SimGameState.WeightMod;
				if (num4 >= num3)
				{
					return i;
				}
			}
			throw new MissingMemberException(string.Format("Random roll of {0} is unable to find result set.", randomRoll));
		}

		// Token: 0x06009196 RID: 37270 RVA: 0x00259308 File Offset: 0x00257508
		public void RemoveSimGameEventResult(TemporarySimGameResult result)
		{
			Pilot pilot = null;
			StatCollection statCollection;
			TagSet tagSet;
			switch (result.Scope)
			{
			case EventScope.Company:
				statCollection = this.CompanyStats;
				tagSet = this.CompanyTags;
				break;
			case EventScope.MechWarrior:
			case EventScope.SecondaryMechWarrior:
			case EventScope.AllMechWarriors:
			case EventScope.TertiaryMechWarrior:
			case EventScope.DeadMechWarrior:
			{
				if (result.TargetPilot == null)
				{
					SimGameState.logger.LogError(string.Format("Unable to resolve scope of {0} without a valid pilot ref", result.Scope));
					return;
				}
				Pilot targetPilot = result.TargetPilot;
				if (result.Scope != EventScope.DeadMechWarrior)
				{
					using (IEnumerator<Pilot> enumerator = this.PilotRoster.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							Pilot pilot2 = enumerator.Current;
							if (pilot2.GUID == targetPilot.GUID)
							{
								if (pilot2 != targetPilot)
								{
									SimGameState.logger.LogWarning("pilot roster same guid different reference");
								}
								pilot = pilot2;
								break;
							}
						}
						goto IL_154;
					}
				}
				foreach (Pilot pilot3 in this.Graveyard)
				{
					if (pilot3.GUID == targetPilot.GUID)
					{
						if (pilot3 != targetPilot)
						{
							SimGameState.logger.LogWarning("graveyard same guid different reference");
						}
						pilot = pilot3;
						break;
					}
				}
				IL_154:
				if (pilot == null)
				{
					SimGameState.logger.LogWarning(string.Format("p is still null. unable to resolve scope of {0} without a valid pilot ref", result.Scope));
					return;
				}
				statCollection = pilot.StatCollection;
				tagSet = pilot.pilotDef.PilotTags;
				break;
			}
			case EventScope.Mech:
			case EventScope.SecondaryMech:
			case EventScope.AllMechs:
			case EventScope.RandomMech:
			{
				MechDef targetMechDef = result.TargetMechDef;
				statCollection = targetMechDef.Stats;
				tagSet = targetMechDef.MechTags;
				break;
			}
			case EventScope.Commander:
				statCollection = this.CommanderStats;
				tagSet = this.CommanderTags;
				break;
			case EventScope.StarSystem:
				statCollection = this.CurSystem.Stats;
				tagSet = this.CurSystem.Tags;
				break;
			default:
				SimGameState.logger.LogError("Invalid scope: " + result.Scope);
				return;
			}
			if (result.AddedTags != null)
			{
				foreach (string text in result.AddedTags)
				{
					if (tagSet.Contains(text))
					{
						tagSet.Remove(text);
					}
				}
			}
			if (result.RemovedTags != null)
			{
				foreach (string text2 in result.RemovedTags)
				{
					if (!tagSet.Contains(text2))
					{
						tagSet.Add(text2);
					}
				}
			}
			if (result.Stats != null)
			{
				foreach (SimGameStat simGameStat in result.Stats)
				{
					if (!simGameStat.set)
					{
						SimGameStat simGameStat2;
						if (simGameStat.Type == typeof(int))
						{
							int num = simGameStat.ToInt();
							simGameStat2 = new SimGameStat(simGameStat.name, -num, false);
						}
						else
						{
							if (!(simGameStat.Type == typeof(float)))
							{
								goto IL_319;
							}
							float num2 = simGameStat.ToSingle();
							simGameStat2 = new SimGameStat(simGameStat.name, -num2, false);
						}
						SimGameState.SetSimGameStat(simGameStat2, statCollection);
					}
					IL_319:;
				}
			}
			if (pilot != null && pilot.GUID != this.commander.GUID)
			{
				bool flag = true;
				for (int j = 0; j < this.PilotRoster.Count; j++)
				{
					if (this.PilotRoster[j].GUID == pilot.GUID)
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					return;
				}
			}
			SimGameEventResult[] array = new SimGameEventResult[] { result };
			List<ResultDescriptionEntry> list = this.BuildSimGameResults(array, this.Context, new SimGameStatDescDef.DescriptionTense?(SimGameStatDescDef.DescriptionTense.TemporalEnd), pilot);
			if (list != null)
			{
				foreach (ResultDescriptionEntry resultDescriptionEntry in list)
				{
					this.RoomManager.ShipRoom.AddEventToast(resultDescriptionEntry.Text);
				}
			}
		}

		// Token: 0x06009197 RID: 37271 RVA: 0x0025974C File Offset: 0x0025794C
		public static bool ApplySimGameEventResult(List<SimGameEventResult> resultList)
		{
			bool flag = false;
			SimGameState simulation = SceneSingletonBehavior<UnityGameInstance>.Instance.Game.Simulation;
			foreach (SimGameEventResult simGameEventResult in resultList)
			{
				List<object> list = new List<object>();
				EventScope scope = simGameEventResult.Scope;
				if (scope == EventScope.Company)
				{
					goto IL_146;
				}
				switch (scope)
				{
				case EventScope.Commander:
					goto IL_F0;
				case EventScope.StarSystem:
				case EventScope.SecondaryMechWarrior:
				case EventScope.SecondaryMech:
				case EventScope.TertiaryMechWarrior:
				case EventScope.Map:
					goto IL_153;
				case EventScope.AllMechWarriors:
					break;
				case EventScope.AllMechs:
				{
					using (Dictionary<int, MechDef>.ValueCollection.Enumerator enumerator2 = simulation.ActiveMechs.Values.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							MechDef mechDef = enumerator2.Current;
							list.Add(mechDef);
						}
						goto IL_153;
					}
					break;
				}
				case EventScope.RandomMech:
					goto IL_137;
				case EventScope.DeadMechWarrior:
				{
					using (IEnumerator<Pilot> enumerator3 = simulation.Graveyard.GetEnumerator())
					{
						while (enumerator3.MoveNext())
						{
							Pilot pilot = enumerator3.Current;
							list.Add(pilot);
						}
						goto IL_153;
					}
					goto IL_137;
				}
				case EventScope.Flashpoint:
					goto IL_146;
				default:
					goto IL_153;
				}
				using (IEnumerator<Pilot> enumerator3 = simulation.PilotRoster.GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						Pilot pilot2 = enumerator3.Current;
						list.Add(pilot2);
					}
					goto IL_153;
				}
				IL_F0:
				list.Add(simulation.commander);
				goto IL_153;
				IL_137:
				list.Add(simulation.GetRandomActiveMech());
				IL_153:
				flag |= SimGameState.ApplySimGameEventResult(simGameEventResult, list, null);
				continue;
				IL_146:
				list.Add(0);
				goto IL_153;
			}
			return flag;
		}

		// Token: 0x06009198 RID: 37272 RVA: 0x0025993C File Offset: 0x00257B3C
		public static bool ApplySimGameEventResult(SimGameEventResult result, List<object> objects, SimGameEventTracker tracker = null)
		{
			SimGameState simulation = SceneSingletonBehavior<UnityGameInstance>.Instance.Game.Simulation;
			bool flag = false;
			bool flag2 = false;
			SimGameReport.ReportEntry reportEntry = null;
			if (tracker != null)
			{
				tracker.RecordLog(string.Format("Applying results of scope {0}", result.Scope), SimGameLogLevel.CRITICAL);
				tracker.RecordLog("Added Tags: " + result.AddedTags.ToString(), SimGameLogLevel.CRITICAL);
				tracker.RecordLog("Removed Tags: " + result.RemovedTags.ToString(), SimGameLogLevel.CRITICAL);
				reportEntry = tracker.GetReportEntry(SimGameLogLevel.VERBOSE);
			}
			int i = 0;
			while (i < objects.Count)
			{
				object obj = objects[i];
				Pilot pilot = null;
				MechDef mechDef = null;
				TagSet tagSet;
				switch (result.Scope)
				{
				case EventScope.Company:
				{
					StatCollection statCollection = simulation.CompanyStats;
					tagSet = simulation.CompanyTags;
					flag2 = true;
					goto IL_1A8;
				}
				case EventScope.MechWarrior:
				case EventScope.SecondaryMechWarrior:
				case EventScope.AllMechWarriors:
				case EventScope.TertiaryMechWarrior:
				case EventScope.DeadMechWarrior:
				{
					pilot = (Pilot)obj;
					StatCollection statCollection = pilot.StatCollection;
					tagSet = pilot.pilotDef.PilotTags;
					goto IL_1A8;
				}
				case EventScope.Mech:
				case EventScope.SecondaryMech:
				case EventScope.AllMechs:
				case EventScope.RandomMech:
				{
					mechDef = (MechDef)obj;
					StatCollection statCollection = mechDef.Stats;
					tagSet = mechDef.MechTags;
					goto IL_1A8;
				}
				case EventScope.Commander:
				{
					StatCollection statCollection = simulation.CommanderStats;
					tagSet = simulation.CommanderTags;
					goto IL_1A8;
				}
				case EventScope.StarSystem:
				{
					StatCollection statCollection = simulation.CurSystem.Stats;
					tagSet = simulation.CurSystem.Tags;
					goto IL_1A8;
				}
				case EventScope.Map:
					goto IL_187;
				case EventScope.Flashpoint:
					if (simulation.ActiveFlashpoint != null)
					{
						StatCollection statCollection = simulation.ActiveFlashpoint.Stats;
						tagSet = simulation.ActiveFlashpoint.Tags;
						goto IL_1A8;
					}
					break;
				default:
					goto IL_187;
				}
				IL_371:
				i++;
				continue;
				IL_1A8:
				if (result.Requirements != null && !simulation.MeetsRequirements(result.Requirements, reportEntry))
				{
					goto IL_371;
				}
				if (result.AddedTags != null)
				{
					foreach (string text in result.AddedTags)
					{
						if (!tagSet.Contains(text))
						{
							tagSet.Add(text);
						}
					}
				}
				if (result.RemovedTags != null)
				{
					foreach (string text2 in result.RemovedTags)
					{
						if (tagSet.Contains(text2))
						{
							tagSet.Remove(text2);
						}
					}
				}
				if (result.Actions != null)
				{
					foreach (SimGameResultAction simGameResultAction in result.Actions)
					{
						flag |= SimGameState.ApplyEventAction(simGameResultAction, obj);
					}
				}
				if (result.Stats != null)
				{
					foreach (SimGameStat simGameStat in result.Stats)
					{
						if (tracker != null)
						{
							tracker.RecordLog(string.Format("Modifying stat {0} of type {1} by {2}", simGameStat.name, simGameStat.typeString, simGameStat.value), SimGameLogLevel.CRITICAL);
						}
						StatCollection statCollection;
						SimGameState.SetSimGameStat(simGameStat, statCollection);
					}
				}
				if (result.ForceEvents != null)
				{
					foreach (SimGameForcedEvent simGameForcedEvent in result.ForceEvents)
					{
						simulation.AddSpecialEvent(simGameForcedEvent, simGameForcedEvent.RetainPilot ? pilot : null);
					}
				}
				if (result.TemporaryResult && result.ResultDuration > 0)
				{
					TemporarySimGameResult temporarySimGameResult;
					if (pilot != null)
					{
						temporarySimGameResult = new TemporarySimGameResult(pilot, result);
					}
					else
					{
						temporarySimGameResult = new TemporarySimGameResult(mechDef, result);
					}
					simulation.AddOrRemoveTempTags(temporarySimGameResult, true);
					simulation.TemporaryResultTracker.Add(temporarySimGameResult);
					goto IL_371;
				}
				goto IL_371;
				IL_187:
				SimGameState.logger.LogError("Invalid scope: " + result.Scope);
				return false;
			}
			if (flag2 && simulation.UXAttached)
			{
				simulation.RoomManager.RefreshDisplay();
			}
			simulation.RoomManager.RefreshTimeline(true);
			return flag;
		}

		// Token: 0x06009199 RID: 37273 RVA: 0x00259D0C File Offset: 0x00257F0C
		public static bool ApplyEventAction(SimGameResultAction action, object additionalObject)
		{
			SimGameState simulation = UnityGameInstance.BattleTechGame.Simulation;
			bool flag = false;
			if (simulation == null)
			{
				return false;
			}
			switch (action.Type)
			{
			case SimGameResultAction.ActionType.MechWarrior_AddRoster:
				simulation.AddPilotToRoster(action.value);
				return flag;
			case SimGameResultAction.ActionType.MechWarrior_AddHiring:
			{
				string id = simulation.CurSystem.ID;
				simulation.AddPilotToHiringHall(action.value, id);
				return flag;
			}
			case SimGameResultAction.ActionType.MechWarrior_Kill:
			{
				flag = true;
				string text = null;
				if (action.additionalValues != null && action.additionalValues.Length >= 1)
				{
					text = action.additionalValues[0];
				}
				if (additionalObject != null && additionalObject.GetType() == typeof(Pilot))
				{
					simulation.KillPilot((Pilot)additionalObject, true, null, text);
					return flag;
				}
				simulation.KillPilot(action.value, true, text);
				return flag;
			}
			case SimGameResultAction.ActionType.MechWarrior_Fire:
				flag = true;
				if (additionalObject != null && additionalObject.GetType() == typeof(Pilot))
				{
					simulation.DismissPilot((Pilot)additionalObject);
					return flag;
				}
				simulation.DismissPilot(action.value);
				return flag;
			case SimGameResultAction.ActionType.MechWarrior_Callsign:
				if (additionalObject != null && !(additionalObject.GetType() != typeof(Pilot)) && !string.IsNullOrEmpty(action.value))
				{
					Pilot pilot = (Pilot)additionalObject;
					pilot.Description.SetCallsign(action.value);
					pilot.Description.SetName(action.value);
					return flag;
				}
				return flag;
			case SimGameResultAction.ActionType.MechWarrior_HealAll:
				foreach (Pilot pilot2 in simulation.PilotRoster)
				{
					pilot2.ClearInjuries("Result", 0, "Result");
					pilot2.ForceRefreshDef();
				}
				simulation.commander.ClearInjuries("Result", 0, "Result");
				simulation.commander.ForceRefreshDef();
				simulation.RefreshInjuries();
				return flag;
			case SimGameResultAction.ActionType.MechWarrior_SetTimeout:
			{
				if (additionalObject == null || additionalObject.GetType() != typeof(Pilot) || string.IsNullOrEmpty(action.value))
				{
					return flag;
				}
				Pilot pilot3 = (Pilot)additionalObject;
				int num = 0;
				if (!int.TryParse(action.value, out num))
				{
					SimGameState.logger.LogError("Invalid time value for action MechWarrior_SetTimeout: " + action.value);
					return flag;
				}
				pilot3.pilotDef.SetTimeoutTime(num);
				simulation.RefreshInjuries();
				return flag;
			}
			case SimGameResultAction.ActionType.Mech_RepairAll:
			{
				using (Dictionary<int, MechDef>.ValueCollection.Enumerator enumerator2 = simulation.ActiveMechs.Values.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						MechDef mechDef = enumerator2.Current;
						if (mechDef != null)
						{
							simulation.Mech_InstantRepairAll(mechDef);
						}
					}
					return flag;
				}
				break;
			}
			case SimGameResultAction.ActionType.Mech_Damage_Location:
				break;
			case SimGameResultAction.ActionType.Mech_AddRoster:
				flag = true;
				simulation.AddMechByID(action.value, true);
				return flag;
			case SimGameResultAction.ActionType.Contract_IgnoreMissionResults:
			case SimGameResultAction.ActionType.Equipment_AddRandom_POSTLAUNCH:
			case SimGameResultAction.ActionType.System_AddStartingRoster:
				return flag;
			case SimGameResultAction.ActionType.Company_TravelTime:
			{
				int travelTime = simulation.TravelTime;
				simulation.SetTravelTime(travelTime + action.GetIntValue(), null);
				return flag;
			}
			case SimGameResultAction.ActionType.Company_TravelTo:
				flag = true;
				simulation.TravelToSystemByString(action.value, false);
				return flag;
			case SimGameResultAction.ActionType.Ship_SetRepairState:
			{
				int num2 = 0;
				int.TryParse(action.value, out num2);
				if (action.additionalValues != null)
				{
					string[] array = action.additionalValues;
					int i = 0;
					while (i < array.Length)
					{
						string text2 = array[i];
						ArgoController.RepairStateLocations repairStateLocations;
						try
						{
							repairStateLocations = (ArgoController.RepairStateLocations)Enum.Parse(typeof(ArgoController.RepairStateLocations), text2);
						}
						catch
						{
							goto IL_63D;
						}
						goto IL_633;
						IL_63D:
						i++;
						continue;
						IL_633:
						simulation.SetArgoLocationRepairState(repairStateLocations, num2);
						goto IL_63D;
					}
					return flag;
				}
				return flag;
			}
			case SimGameResultAction.ActionType.StarSystem_SetActiveDef:
			{
				if (!simulation.StarSystemDictionary.ContainsKey(action.value))
				{
					return flag;
				}
				StarSystem starSystem = simulation.StarSystemDictionary[action.value];
				if (action.additionalValues == null || action.additionalValues.Length < 1)
				{
					return flag;
				}
				simulation.SetStarSystemDef(starSystem, action.additionalValues[0]);
				if (starSystem == simulation.CurSystem)
				{
					simulation.RoomManager.ShipRoom.RefreshData();
					return flag;
				}
				return flag;
			}
			case SimGameResultAction.ActionType.System_PlayVideo:
				simulation.PlayVideo(action.value);
				return true;
			case SimGameResultAction.ActionType.System_StartContract:
			{
				SimGameState.AddContractData addContractData = simulation.ParseContractActionData(action.value, action.additionalValues);
				if (!string.IsNullOrEmpty(addContractData.TargetSystem))
				{
					simulation.TravelToSystemByString(addContractData.TargetSystem, false);
				}
				Contract contract = simulation.AddContract(addContractData);
				simulation.RoomManager.CmdCenterRoom.ClearHoldForContract();
				if (simulation.TimeMoving)
				{
					simulation.PauseTimer();
				}
				simulation.ForceTakeContract(contract, false);
				return true;
			}
			case SimGameResultAction.ActionType.System_AddContract:
			{
				SimGameState.AddContractData addContractData2 = simulation.ParseContractActionData(action.value, action.additionalValues);
				addContractData2.IsGlobal = addContractData2.TargetSystem != simulation.CurSystem.ID;
				simulation.AddContract(addContractData2);
				simulation.RoomManager.CmdCenterRoom.ClearHoldForContract();
				return flag;
			}
			case SimGameResultAction.ActionType.System_StartNonProceduralContract:
			{
				SimGameState.AddContractData addContractData3 = simulation.ParseNonProceduralContractActionData(action.value, action.additionalValues);
				bool flag2 = simulation.GetValidatedSystemString(addContractData3.TargetSystem) == simulation.CurSystem.ID;
				Contract contract2 = simulation.AddPredefinedContract2(addContractData3);
				simulation.ForceTakeContract(contract2, !flag2);
				return true;
			}
			case SimGameResultAction.ActionType.System_CreateCommander:
				simulation.StartCharacterCreation();
				return true;
			case SimGameResultAction.ActionType.System_SimGameCharacterVisible:
			{
				bool flag3 = false;
				if (bool.TryParse(action.value, out flag3) && action.additionalValues != null)
				{
					foreach (string text3 in action.additionalValues)
					{
						try
						{
							SimGameState.SimGameCharacterType simGameCharacterType = (SimGameState.SimGameCharacterType)Enum.Parse(typeof(SimGameState.SimGameCharacterType), text3, true);
							simulation.SetCharacterVisibility(simGameCharacterType, flag3);
						}
						catch
						{
							SimGameState.logger.LogWarning(string.Format("Attempted to affect unknown simgamecharacters visibility: {0}", text3));
						}
					}
					return flag;
				}
				return flag;
			}
			case SimGameResultAction.ActionType.System_UpdateMilestones:
				simulation.QueueUpdateMilestoneCheck();
				return flag;
			case SimGameResultAction.ActionType.System_SetContractScope:
			{
				ContractScope contractScope;
				try
				{
					contractScope = (ContractScope)Enum.Parse(typeof(ContractScope), action.value);
				}
				catch
				{
					contractScope = ContractScope.UNKNOWN;
				}
				if (contractScope == ContractScope.UNKNOWN || simulation.ContractScope == contractScope)
				{
					return flag;
				}
				simulation.ContractScope = contractScope;
				simulation.CurSystem.ResetContracts();
				if (simulation.NearestToTarget != null)
				{
					simulation.GeneratePotentialContracts(true, null, simulation.NearestToTarget, false);
					return flag;
				}
				return flag;
			}
			case SimGameResultAction.ActionType.System_SetDropship:
			{
				if (string.IsNullOrEmpty(action.value))
				{
					return flag;
				}
				DropshipType dropshipType = DropshipType.INVALID_UNSET;
				if (action.value.ToLower().Contains("leopard"))
				{
					dropshipType = DropshipType.Leopard;
				}
				else if (action.value.ToLower().Contains("argo"))
				{
					dropshipType = DropshipType.Argo;
				}
				simulation.SetSimShip(dropshipType);
				simulation.HasSimShipBeenSet = true;
				if (simulation.CurDropship != DropshipType.INVALID_UNSET && simulation.CurRoomState == DropshipLocation.NONE)
				{
					simulation.SetSimRoomState(DropshipLocation.SHIP);
					simulation.RefreshTravelStatus();
					return flag;
				}
				if (simulation.CurDropship == DropshipType.INVALID_UNSET && simulation.CurRoomState == DropshipLocation.SHIP)
				{
					simulation.SetSimRoomState(DropshipLocation.NONE);
					return flag;
				}
				return flag;
			}
			case SimGameResultAction.ActionType.System_SetTargetBreadcrumbSystem:
			{
				string validatedSystemString = simulation.GetValidatedSystemString(action.value);
				StarSystem systemById = simulation.GetSystemById(validatedSystemString);
				if (systemById != null)
				{
					simulation.TargetSystem = systemById;
					return flag;
				}
				return flag;
			}
			case SimGameResultAction.ActionType.System_SetObjective:
			{
				string text4 = "";
				string text5 = "";
				if (action.additionalValues != null)
				{
					if (action.additionalValues.Length != 0)
					{
						text4 = action.additionalValues[0];
					}
					if (action.additionalValues.Length > 1)
					{
						text5 = action.additionalValues[1];
					}
				}
				simulation.SetCurrentObjective(action.value.ToLower() == "true", text4, text5);
				return flag;
			}
			case SimGameResultAction.ActionType.System_StartConversation:
			{
				flag = true;
				string text6 = null;
				string text7 = null;
				DropshipMenuType dropshipMenuType = DropshipMenuType.INVALID_UNSET;
				string text8 = null;
				if (action.additionalValues != null)
				{
					if (action.additionalValues.Length != 0)
					{
						text6 = action.additionalValues[0];
					}
					if (action.additionalValues.Length > 1)
					{
						text7 = action.additionalValues[1];
					}
					if (action.additionalValues.Length > 2)
					{
						try
						{
							dropshipMenuType = (DropshipMenuType)Enum.Parse(typeof(DropshipMenuType), action.additionalValues[2], true);
						}
						catch
						{
							dropshipMenuType = DropshipMenuType.INVALID_UNSET;
						}
					}
					if (action.additionalValues.Length > 3)
					{
						text8 = action.additionalValues[3];
					}
				}
				simulation.interruptQueue.QueueConversation(simulation.DataManager.SimGameConversations.Get(action.value), text6, text7, null, true, dropshipMenuType, text8);
				return flag;
			}
			case SimGameResultAction.ActionType.System_PauseNotification:
			{
				simulation.SetTimeMoving(false, true);
				flag = true;
				CastDef castDef = null;
				if (action.additionalValues.Length > 1)
				{
					castDef = simulation.GetCastDef(action.additionalValues[1], false);
				}
				if (castDef != null)
				{
					simulation.interruptQueue.QueuePauseNotification(action.additionalValues[0], action.value, castDef.defaultEmotePortrait.LoadPortrait(false), "", new Action(simulation.OnActionPopupClosed), "Continue", null, null);
					return flag;
				}
				simulation.interruptQueue.QueueGenericPopup(action.additionalValues[0], action.value, Array.Empty<GenericPopupButtonSettings>()).AddButton("Continue", new Action(simulation.OnActionPopupClosed), true, null);
				return flag;
			}
			case SimGameResultAction.ActionType.System_StartCredits:
				simulation.SetCreditsModule(LazySingletonBehavior<UIManager>.Instance.GetOrCreateUIModule<CreditsModule>("", true));
				simulation.Credits.exitAction = new Action(simulation.OnCreditsClosed);
				return flag;
			case SimGameResultAction.ActionType.System_ResetContracts:
				simulation.CurSystem.ResetContracts();
				if (simulation.NearestToTarget != null)
				{
					simulation.GeneratePotentialContracts(true, null, simulation.NearestToTarget, false);
					return flag;
				}
				return flag;
			case SimGameResultAction.ActionType.System_ShowCampaignResults:
			{
				string text9 = (string.IsNullOrEmpty(action.value) ? null : action.value);
				string text10 = null;
				string text11 = null;
				if (action.additionalValues != null)
				{
					if (action.additionalValues.Length != 0 && !string.IsNullOrEmpty(action.additionalValues[0]))
					{
						text10 = action.additionalValues[0];
					}
					if (action.additionalValues.Length > 1 && !string.IsNullOrEmpty(action.additionalValues[1]))
					{
						text11 = action.additionalValues[1];
					}
				}
				simulation.interruptQueue.QueueWinOutcome(SGCampaignOutcomeScreen.ScreenMode.VICTORY, text10, text9, text11);
				SimGameCampaignCompleteMessage simGameCampaignCompleteMessage = new SimGameCampaignCompleteMessage(simulation.IsDekkerAlive(), simulation.IsIronmanCampaign);
				simulation.MessageCenter.PublishMessage(simGameCampaignCompleteMessage);
				return flag;
			}
			case SimGameResultAction.ActionType.System_ShowTitleCard:
			{
				bool flag4 = false;
				bool.TryParse(action.value, out flag4);
				string text12 = "";
				if (action.additionalValues != null && action.additionalValues.Length != 0)
				{
					text12 = Strings.T(action.additionalValues[0]);
				}
				string text13 = null;
				if (action.additionalValues != null && action.additionalValues.Length > 1)
				{
					text13 = Strings.T(action.additionalValues[1]);
				}
				float? num3 = null;
				float num4;
				if (action.additionalValues != null && action.additionalValues.Length > 2 && float.TryParse(action.additionalValues[2], out num4))
				{
					num3 = new float?(num4);
				}
				float? num5 = null;
				float num6;
				if (action.additionalValues != null && action.additionalValues.Length > 3 && float.TryParse(action.additionalValues[3], out num6))
				{
					num5 = new float?(num6);
				}
				simulation.SetTitleCard(text12, text13, flag4, num3, num5);
				return flag;
			}
			case SimGameResultAction.ActionType.StarSystem_SetCurBreadcrumbOverride:
			{
				string text14 = "";
				int num7 = 0;
				if (action.additionalValues != null && action.additionalValues.Length != 0)
				{
					text14 = simulation.GetValidatedSystemString(action.additionalValues[0]);
				}
				StarSystem starSystem2;
				if (simulation.starDict.ContainsKey(text14))
				{
					starSystem2 = simulation.starDict[text14];
				}
				else
				{
					starSystem2 = simulation.CurSystem;
				}
				int.TryParse(action.value, out num7);
				starSystem2.SetCurBreadcrumbOverride(num7);
				return flag;
			}
			case SimGameResultAction.ActionType.Ship_AddUpgrade:
			{
				string value = action.value;
				if (simulation.DataManager.ShipUpgradeDefs.Exists(value))
				{
					ShipModuleUpgrade shipModuleUpgrade = simulation.DataManager.ShipUpgradeDefs.Get(value);
					simulation.AddArgoUpgrade(shipModuleUpgrade);
					return flag;
				}
				return flag;
			}
			case SimGameResultAction.ActionType.System_ToggleIgnoredContractTargets:
			case SimGameResultAction.ActionType.System_ToggleIgnoredContractEmployers:
			{
				bool flag5 = false;
				bool.TryParse(action.value, out flag5);
				List<string> list;
				if (action.Type == SimGameResultAction.ActionType.System_ToggleIgnoredContractTargets)
				{
					list = simulation.ignoredContractTargets;
				}
				else
				{
					list = simulation.ignoredContractEmployers;
				}
				if (action.additionalValues != null)
				{
					foreach (string text15 in action.additionalValues)
					{
						FactionValue factionValueFromString = simulation.GetFactionValueFromString(text15);
						if (!factionValueFromString.IsInvalidUnset)
						{
							if (flag5 && !list.Contains(factionValueFromString.Name))
							{
								list.Add(factionValueFromString.Name);
							}
							else if (!flag5 && list.Contains(factionValueFromString.Name))
							{
								list.Remove(factionValueFromString.Name);
							}
						}
					}
					return flag;
				}
				return flag;
			}
			case SimGameResultAction.ActionType.System_AddDisplayedFaction:
			{
				FactionValue factionValueFromString2 = simulation.GetFactionValueFromString(action.value);
				if (!simulation.displayedFactions.Contains(factionValueFromString2.Name))
				{
					simulation.displayedFactions.Add(factionValueFromString2.Name);
					return flag;
				}
				return flag;
			}
			case SimGameResultAction.ActionType.System_RemoveDisplayedFaction:
				simulation.displayedFactions.Remove(simulation.GetFactionValueFromString(action.value).Name);
				return flag;
			case SimGameResultAction.ActionType.System_ShowSummaryOverlay:
				simulation.SetSummaryScreen(action.value, action.additionalValues[0]);
				return flag;
			case SimGameResultAction.ActionType.Flashpoint_SetNextMilestone:
				if (simulation.activeFlashpoint != null)
				{
					return simulation.activeFlashpoint.SetNextMilestone(action.value);
				}
				return flag;
			case SimGameResultAction.ActionType.Flashpoint_StartContract:
			case SimGameResultAction.ActionType.Flashpoint_AddContract:
				if (simulation.activeFlashpoint != null)
				{
					SimGameState.AddContractData addContractData4 = simulation.ParseFlashpointContractActionData(action.value, action.additionalValues);
					Contract contract3 = simulation.AddFlashpointContract(simulation.activeFlashpoint, addContractData4);
					simulation.RoomManager.CmdCenterRoom.ClearHoldForContract();
					if (action.Type == SimGameResultAction.ActionType.Flashpoint_StartContract)
					{
						simulation.ForceTakeContract(contract3, false);
					}
					else
					{
						simulation.SaveActiveContractName = contract3.Name;
						simulation.TriggerSaveNow(SaveReason.SIM_GAME_CONTRACT_ACCEPTED, SimGameState.TriggerSaveNowOption.QUEUE_IF_NEEDED);
					}
					return true;
				}
				return flag;
			case SimGameResultAction.ActionType.Flashpoint_FailFlashpoint:
			case SimGameResultAction.ActionType.Flashpoint_CompleteFlashpoint:
				if (simulation.activeFlashpoint != null)
				{
					string text16 = null;
					string text17 = null;
					FlashpointEndType flashpointEndType = FlashpointEndType.Completed;
					if (action.Type == SimGameResultAction.ActionType.Flashpoint_FailFlashpoint)
					{
						flashpointEndType = FlashpointEndType.Failed;
					}
					else if (action.Type == SimGameResultAction.ActionType.Flashpoint_CompleteFlashpoint)
					{
						text17 = action.value;
						if (action.additionalValues != null && action.additionalValues.Length != 0)
						{
							text16 = action.additionalValues[0];
						}
						if (action.additionalValues != null && action.additionalValues.Length > 1)
						{
							try
							{
								flashpointEndType = (FlashpointEndType)Enum.Parse(typeof(FlashpointEndType), action.additionalValues[1]);
								goto IL_FCC;
							}
							catch
							{
								flashpointEndType = FlashpointEndType.Completed;
								goto IL_FCC;
							}
						}
						flashpointEndType = FlashpointEndType.Completed;
					}
					IL_FCC:
					simulation.CompleteFlashpoint(simulation.activeFlashpoint, flashpointEndType, text17, text16, null);
					simulation.SetActiveFlashpoint(null);
					return flag;
				}
				return flag;
			case SimGameResultAction.ActionType.System_WhitelistRonin:
				if (!string.IsNullOrEmpty(action.value))
				{
					simulation.WhitelistedRonin.Add(action.value);
					return flag;
				}
				return flag;
			case SimGameResultAction.ActionType.System_CmdCenter_HoldForNewContract:
				simulation.RoomManager.CmdCenterRoom.AddHoldForContract();
				return flag;
			case SimGameResultAction.ActionType.System_ShowRewards:
			{
				simulation.SetTimeMoving(false, true);
				flag = true;
				string value2 = action.value;
				simulation.interruptQueue.QueueRewardsPopup(value2);
				return flag;
			}
			case SimGameResultAction.ActionType.System_SetFactionAlly:
			{
				bool flag6 = false;
				if (bool.TryParse(action.value, out flag6) && action.additionalValues != null)
				{
					for (int j = 0; j < action.additionalValues.Length; j++)
					{
						try
						{
							FactionValue factionByName = FactionEnumeration.GetFactionByName(action.additionalValues[j]);
							if (factionByName != null)
							{
								if (flag6 && !simulation.IsFactionAlly(factionByName, null))
								{
									simulation.AddAllyFaction(factionByName, true);
								}
								else if (!flag6 && simulation.IsFactionAlly(factionByName, null))
								{
									simulation.RemoveAllyFaction(factionByName);
								}
							}
							else
							{
								SimGameState.logger.LogError(string.Format("Could not parse [{0}] as valid faction. Skipping", action.additionalValues[j]));
							}
						}
						catch
						{
						}
					}
					return flag;
				}
				return flag;
			}
			case SimGameResultAction.ActionType.System_ForceDropshipRoom:
			{
				DropshipMenuType dropshipMenuType2 = DropshipMenuType.INVALID_UNSET;
				if (!string.IsNullOrEmpty(action.value))
				{
					try
					{
						dropshipMenuType2 = (DropshipMenuType)Enum.Parse(typeof(DropshipMenuType), action.value, true);
					}
					catch
					{
						dropshipMenuType2 = DropshipMenuType.INVALID_UNSET;
					}
				}
				string text18 = ((action.additionalValues != null && action.additionalValues.Length != 0) ? action.additionalValues[0] : null);
				simulation.ForceActiveDropshipRoom(dropshipMenuType2, text18);
				return flag;
			}
			case SimGameResultAction.ActionType.MechWarrior_Heal:
				if (additionalObject != null && !(additionalObject.GetType() != typeof(Pilot)))
				{
					Pilot pilot4 = (Pilot)additionalObject;
					pilot4.ClearInjuries("Result", 0, "Result");
					pilot4.ForceRefreshDef();
					simulation.RefreshInjuries();
					return flag;
				}
				return flag;
			case SimGameResultAction.ActionType.MechWarrior_AddInjuries:
			case SimGameResultAction.ActionType.MechWarrior_SubtractInjuries:
			{
				if (additionalObject == null || additionalObject.GetType() != typeof(Pilot) || string.IsNullOrEmpty(action.value))
				{
					return flag;
				}
				Pilot pilot5 = (Pilot)additionalObject;
				int num8 = 0;
				int.TryParse(action.value, out num8);
				if (num8 != 0)
				{
					if (action.Type == SimGameResultAction.ActionType.MechWarrior_SubtractInjuries)
					{
						num8 *= -1;
					}
					pilot5.ModifyInjuries(num8, "Result", 0, "Result");
					pilot5.ForceRefreshDef();
					simulation.RefreshInjuries();
					return flag;
				}
				return flag;
			}
			case SimGameResultAction.ActionType.MechWarrior_AddInjury:
			case SimGameResultAction.ActionType.MechWarrior_SubtractInjury:
				if (additionalObject != null && !(additionalObject.GetType() != typeof(Pilot)))
				{
					Pilot pilot6 = (Pilot)additionalObject;
					int num9 = ((action.Type == SimGameResultAction.ActionType.MechWarrior_SubtractInjury) ? (-1) : 1);
					pilot6.ModifyInjuries(num9, "Result", 0, "Result");
					pilot6.ForceRefreshDef();
					simulation.RefreshInjuries();
					return flag;
				}
				return flag;
			case SimGameResultAction.ActionType.System_AddFlashpoint:
			{
				string value3 = action.value;
				string text19 = ((action.additionalValues != null && action.additionalValues.Length != 0) ? action.additionalValues[0] : null);
				if (!string.IsNullOrEmpty(text19) && !text19.StartsWith("starsystemdef_"))
				{
					text19 = "starsystemdef_" + text19;
				}
				simulation.GenerateFlashpointCommand(value3, text19);
				return flag;
			}
			default:
				return flag;
			}
			if (additionalObject != null && !(additionalObject.GetType() != typeof(MechDef)))
			{
				MechDef mechDef2 = (MechDef)additionalObject;
				int intValue = action.GetIntValue();
				if (action.additionalValues != null)
				{
					string[] array = action.additionalValues;
					int i = 0;
					while (i < array.Length)
					{
						string text20 = array[i];
						ChassisLocations chassisLocations;
						try
						{
							chassisLocations = (ChassisLocations)Enum.Parse(typeof(ChassisLocations), text20);
						}
						catch
						{
							goto IL_51A;
						}
						goto IL_504;
						IL_51A:
						i++;
						continue;
						IL_504:
						simulation.Mech_InstantDamageStructure(mechDef2, intValue, chassisLocations);
						simulation.Mech_InstantStripArmor(mechDef2, chassisLocations);
						goto IL_51A;
					}
				}
			}
			return flag;
		}

		// Token: 0x0600919A RID: 37274 RVA: 0x0025AF58 File Offset: 0x00259158
		private void OnCreditsClosed()
		{
			this.Credits = null;
			this.UpdateMilestones();
		}

		// Token: 0x0600919B RID: 37275 RVA: 0x0025AF68 File Offset: 0x00259168
		private void OnActionPopupClosed()
		{
			this.UpdateMilestones();
		}

		// Token: 0x0600919C RID: 37276 RVA: 0x0025AF74 File Offset: 0x00259174
		public static void ApplySimGameResult(SimGameEventResult result, StatCollection stats, TagSet tags)
		{
			if (result.AddedTags != null)
			{
				tags.AddRange(result.AddedTags);
			}
			if (result.RemovedTags != null)
			{
				tags.RemoveRange(result.RemovedTags);
			}
			if (result.Stats != null)
			{
				for (int i = 0; i < result.Stats.Length; i++)
				{
					SimGameStat simGameStat = result.Stats[i];
					float num = simGameStat.ToSingle();
					if (stats.ContainsStatistic(simGameStat.name))
					{
						float num2 = stats.GetStatistic(simGameStat.name).Value<float>();
						if (simGameStat.set)
						{
							num2 = num;
						}
						else
						{
							num2 += num;
						}
						stats.Set<float>(simGameStat.name, num2);
					}
					else
					{
						stats.AddStatistic<float>(simGameStat.name, num);
					}
				}
			}
		}

		// Token: 0x0600919D RID: 37277 RVA: 0x0025B02C File Offset: 0x0025922C
		public static void SetSimGameStat(SimGameStat stat, StatCollection statCol)
		{
			SimGameState simulation = SceneSingletonBehavior<UnityGameInstance>.Instance.Game.Simulation;
			string name = stat.name;
			Type type = null;
			string text = null;
			if (!string.IsNullOrEmpty(stat.valueConstant))
			{
				SimGameSubstitution constant = simulation.SimGameSubtitutions.GetConstant(stat.valueConstant);
				if (constant != null)
				{
					type = constant.GetSubstitutionValue(out text);
				}
			}
			if (type == null)
			{
				type = stat.Type;
				text = stat.value;
			}
			if (type == null)
			{
				return;
			}
			if (type == typeof(int))
			{
				int num = stat.ToInt();
				if (!statCol.ContainsStatistic(name))
				{
					statCol.AddStatistic<int>(name, num);
					return;
				}
				if (stat.set)
				{
					statCol.ModifyStat<int>("SimGame.Event", 0, name, StatCollection.StatOperation.Set, num, -1, true);
					return;
				}
				statCol.ModifyStat<int>("SimGame.Event", 0, name, StatCollection.StatOperation.Int_Add, num, -1, true);
				return;
			}
			else if (type == typeof(float))
			{
				float num2 = stat.ToSingle();
				if (!statCol.ContainsStatistic(name))
				{
					statCol.AddStatistic<float>(name, num2);
					return;
				}
				if (stat.set)
				{
					statCol.ModifyStat<float>("SimGame.Event", 0, name, StatCollection.StatOperation.Set, num2, -1, true);
					return;
				}
				statCol.ModifyStat<float>("SimGame.Event", 0, name, StatCollection.StatOperation.Float_Add, num2, -1, true);
				return;
			}
			else
			{
				if (!(type == typeof(string)))
				{
					if (type == typeof(bool))
					{
						bool flag = stat.ToBool();
						if (statCol.ContainsStatistic(name))
						{
							statCol.ModifyStat<bool>("SimGame.Event", 0, name, StatCollection.StatOperation.Set, flag, -1, true);
							return;
						}
						statCol.AddStatistic<bool>(name, flag);
					}
					return;
				}
				if (statCol.ContainsStatistic(name))
				{
					statCol.ModifyStat<string>("SimGame.Event", 0, name, StatCollection.StatOperation.Set, text, -1, true);
					return;
				}
				statCol.AddStatistic<string>(name, text);
				return;
			}
		}

		// Token: 0x0600919E RID: 37278 RVA: 0x0025B1DD File Offset: 0x002593DD
		public void SetCompanyStat(SimGameStat stat)
		{
			SimGameState.SetSimGameStat(stat, this.companyStats);
		}

		// Token: 0x0600919F RID: 37279 RVA: 0x0025B1EB File Offset: 0x002593EB
		public void SetCompanyStat(string statName, int value, bool add)
		{
			if (!this.companyStats.ContainsStatistic(statName))
			{
				this.companyStats.AddStatistic<int>(statName, value);
				return;
			}
			this.companyStats.ModifyStat<int>("SimGameState", 0, statName, add ? StatCollection.StatOperation.Int_Add : StatCollection.StatOperation.Set, value, -1, true);
		}

		// Token: 0x060091A0 RID: 37280 RVA: 0x0025B227 File Offset: 0x00259427
		public void SetCompanyStat(string statName, float value, bool add)
		{
			if (!this.companyStats.ContainsStatistic(statName))
			{
				this.companyStats.AddStatistic<float>(statName, value);
				return;
			}
			this.companyStats.ModifyStat<float>("SimGameState", 0, statName, add ? StatCollection.StatOperation.Float_Add : StatCollection.StatOperation.Set, value, -1, true);
		}

		// Token: 0x060091A1 RID: 37281 RVA: 0x0025B264 File Offset: 0x00259464
		public void SetCompanyStat(string statName, bool value)
		{
			if (!this.companyStats.ContainsStatistic(statName))
			{
				this.companyStats.AddStatistic<bool>(statName, value);
				return;
			}
			this.companyStats.ModifyStat<bool>("SimGameState", 0, statName, StatCollection.StatOperation.Set, value, -1, true);
		}

		// Token: 0x060091A2 RID: 37282 RVA: 0x0025B29A File Offset: 0x0025949A
		public void SetCompanyStat(string statName, string value)
		{
			if (!this.companyStats.ContainsStatistic(statName))
			{
				this.companyStats.AddStatistic<string>(statName, value);
				return;
			}
			this.companyStats.ModifyStat<string>("SimGameState", 0, statName, StatCollection.StatOperation.Set, value, -1, true);
		}

		// Token: 0x060091A3 RID: 37283 RVA: 0x0025B2D0 File Offset: 0x002594D0
		public ConversationSpeaker GetConversationSpeaker(string id)
		{
			for (int i = 0; i < this.ConversationSpeakers.speakers.Count; i++)
			{
				if (this.ConversationSpeakers.speakers[i].id == id)
				{
					return this.ConversationSpeakers.speakers[i];
				}
			}
			return null;
		}

		// Token: 0x060091A4 RID: 37284 RVA: 0x0025B329 File Offset: 0x00259529
		public CastDef GetCastDefFromSpeakerID(string id)
		{
			return this.GetCastDef(this.GetConversationSpeaker(id));
		}

		// Token: 0x060091A5 RID: 37285 RVA: 0x0025B338 File Offset: 0x00259538
		public CastDef GetCastDef(ConversationSpeaker speaker)
		{
			if (speaker == null)
			{
				return null;
			}
			return this.GetCastDef(speaker.speaker_name, true);
		}

		// Token: 0x060091A6 RID: 37286 RVA: 0x0025B34C File Offset: 0x0025954C
		public CastDef GetCastDef(string name, bool addPrefix = false)
		{
			string text = string.Format("{0}{1}", addPrefix ? "castDef_" : "", name);
			if (this.DataManager.CastDefs.Exists(text))
			{
				return this.DataManager.CastDefs.Get(text);
			}
			return null;
		}

		// Token: 0x060091A7 RID: 37287 RVA: 0x0025B39A File Offset: 0x0025959A
		private string GetRepID(string header, FactionValue factionValue)
		{
			return string.Format("{0}.{1}", header, factionValue.Name);
		}

		// Token: 0x060091A8 RID: 37288 RVA: 0x0025B3B0 File Offset: 0x002595B0
		private void AddMorale(int val, string sourceID)
		{
			if (sourceID == null)
			{
				sourceID = "SimGameState";
			}
			if (this.companyStats.ContainsStatistic("Morale"))
			{
				this.companyStats.ModifyStat<int>(sourceID, 0, "Morale", StatCollection.StatOperation.Int_Add, val, -1, true);
			}
			else
			{
				this.companyStats.AddStatistic<int>("Morale", val, new Statistic.Validator<int>(this.MinimumZeroMaximumFiftyValidator<int>));
			}
			this.RoomManager.RefreshDisplay();
		}

		// Token: 0x060091A9 RID: 37289 RVA: 0x0025B41B File Offset: 0x0025961B
		public int GetCurrentMoraleLevel()
		{
			return this.GetMoraleLevelFromMoraleValue(this.Morale);
		}

		// Token: 0x060091AA RID: 37290 RVA: 0x0025B42C File Offset: 0x0025962C
		public int GetMoraleLevelFromMoraleValue(int moraleValue)
		{
			int num = 0;
			for (int i = 0; i < this.CombatConstants.MoraleConstants.BaselineAddFromSimGameThresholds.Length; i++)
			{
				int num2 = this.CombatConstants.MoraleConstants.BaselineAddFromSimGameThresholds[i];
				if (moraleValue >= num2)
				{
					num = i;
				}
			}
			return num;
		}

		// Token: 0x060091AB RID: 37291 RVA: 0x0025B474 File Offset: 0x00259674
		public string GetCurrentMoraleLevelDescriptor()
		{
			int currentMoraleLevel = this.GetCurrentMoraleLevel();
			return this.GetDescriptorForMoraleLevel(currentMoraleLevel);
		}

		// Token: 0x060091AC RID: 37292 RVA: 0x0025B48F File Offset: 0x0025968F
		public string GetDescriptorForMoraleLevel(int level)
		{
			if (level < 0 || level > this.Constants.Story.MoraleLevelNames.Length)
			{
				return "";
			}
			return this.Constants.Story.MoraleLevelNames[level];
		}

		// Token: 0x060091AD RID: 37293 RVA: 0x0025B4C4 File Offset: 0x002596C4
		public void SetExpenditureLevel(EconomyScale value, bool updateMorale)
		{
			if (this.companyStats.ContainsStatistic("ExpenseLevel"))
			{
				if (this.companyStats.GetValue<int>("ExpenseLevel") != (int)value)
				{
					this.companyStats.ModifyStat<int>("Expenditure Change", 0, "ExpenseLevel", StatCollection.StatOperation.Set, (int)value, -1, true);
				}
			}
			else
			{
				this.companyStats.AddStatistic<int>("ExpenseLevel", (int)value);
			}
			if (updateMorale)
			{
				this.AddMorale(this.ExpenditureMoraleValue[value], "Expenditure Change");
			}
		}

		// Token: 0x060091AE RID: 37294 RVA: 0x0025B53F File Offset: 0x0025973F
		public List<AbilityDef> GetAbilityDefFromTree(string id, int level)
		{
			if (!this.AbilityTree.ContainsKey(id))
			{
				return null;
			}
			return this.AbilityTree[id][level];
		}

		// Token: 0x060091AF RID: 37295 RVA: 0x0025B564 File Offset: 0x00259764
		public void SetCurrentObjective(bool isActive, string message = "", string starSystemId = "")
		{
			if (isActive)
			{
				this.CurrentNotification.SetDescription(message);
				this.RoomManager.AddWorkQueueEntry(this.CurrentNotification);
				this.RoomManager.NavRoom.SetNotification(true, message, starSystemId);
				return;
			}
			this.RoomManager.RemoveWorkQueueEntry(this.CurrentNotification, false);
			this.RoomManager.NavRoom.SetNotification(false, "", "");
		}

		// Token: 0x060091B0 RID: 37296 RVA: 0x0025B5D4 File Offset: 0x002597D4
		public string GetValidatedSystemString(string system)
		{
			if (string.IsNullOrEmpty(system))
			{
				return null;
			}
			if (system.StartsWith(this.Constants.Travel.StarSystemPrefix))
			{
				return system;
			}
			return string.Format("{0}{1}", this.Constants.Travel.StarSystemPrefix, system);
		}

		// Token: 0x060091B1 RID: 37297 RVA: 0x0025B620 File Offset: 0x00259820
		public StarSystem GetSystemById(string system)
		{
			if (this.starDict.ContainsKey(system))
			{
				return this.starDict[system];
			}
			return null;
		}

		// Token: 0x060091B2 RID: 37298 RVA: 0x0025B63E File Offset: 0x0025983E
		public void SetTitleCard(string title, string subTitle = null, bool updateMilestone = true, float? displayTime = 5f, float? fadeOutTime = 3f)
		{
			SceneSingletonBehavior<UnityGameInstance>.Instance.StartCoroutine(this.TitleCardRunRoutine(title, subTitle, updateMilestone, displayTime, fadeOutTime));
		}

		// Token: 0x060091B3 RID: 37299 RVA: 0x0025B658 File Offset: 0x00259858
		private IEnumerator TitleCardRunRoutine(string title, string subTitle = null, bool updateMilestone = true, float? displayTime = 5f, float? fadeOutTime = 3f)
		{
			if (subTitle == null)
			{
				subTitle = "";
			}
			float num = 0.5f;
			float num2 = ((displayTime != null) ? displayTime.Value : 5f);
			float num3 = ((fadeOutTime != null) ? fadeOutTime.Value : 3f);
			DropshipLocation loc = this.CurRoomState;
			if (loc != DropshipLocation.NONE)
			{
				this.SetSimRoomState(DropshipLocation.NONE);
			}
			LazySingletonBehavior<UIManager>.Instance.FaderController.SetFaderColor(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.FadeToBlack, UIManagerFader.FadePosition.FadeInBack, UIManagerRootType.UIRoot, true);
			bool waitingForTitle = true;
			this.TitleOverlay.Show(title, subTitle, this.Context, true, true, num, num2, num3, new float?(0.02f), delegate
			{
				waitingForTitle = false;
			});
			while (waitingForTitle)
			{
				yield return null;
			}
			yield return new WaitForSeconds(0.5f);
			LazySingletonBehavior<UIManager>.Instance.FaderController.ScreenFade(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.FadeToClear, 1f, UIManagerFader.FadePosition.FadeInBack, UIManagerRootType.UIRoot, true);
			if (loc != DropshipLocation.NONE)
			{
				this.SetSimRoomState(loc);
			}
			if (updateMilestone)
			{
				this.UpdateMilestones();
			}
			yield break;
		}

		// Token: 0x060091B4 RID: 37300 RVA: 0x0025B68C File Offset: 0x0025988C
		public void SetSummaryScreen(string header, string body)
		{
			this.SummaryOverlay.Show(header, body, new Action(this.OnSummaryScreenComplete));
		}

		// Token: 0x060091B5 RID: 37301 RVA: 0x0025AF68 File Offset: 0x00259168
		private void OnSummaryScreenComplete()
		{
			this.UpdateMilestones();
		}

		// Token: 0x060091B6 RID: 37302 RVA: 0x0025B6A7 File Offset: 0x002598A7
		public Sprite GetCrewPortrait(SimGameCrew crew)
		{
			return this._crewDefs[crew].defaultEmotePortrait.LoadPortrait(false);
		}

		// Token: 0x060091B7 RID: 37303 RVA: 0x0025B6C0 File Offset: 0x002598C0
		public int GetCurrentMRBLevel()
		{
			int rawReputation = this.GetRawReputation(FactionEnumeration.GetMercenaryReviewBoardFactionValue());
			return this.GetMRBLevelFromRep(rawReputation);
		}

		// Token: 0x060091B8 RID: 37304 RVA: 0x0025B6E0 File Offset: 0x002598E0
		public int GetMaxMRBLevel()
		{
			return this.Constants.Story.MRBRepCap.Length;
		}

		// Token: 0x060091B9 RID: 37305 RVA: 0x0025B6F4 File Offset: 0x002598F4
		public int GetMRBLevelFromRep(int rep)
		{
			float[] mrbrepCap = this.Constants.Story.MRBRepCap;
			for (int i = mrbrepCap.Length - 1; i >= 0; i--)
			{
				if ((float)rep >= mrbrepCap[i])
				{
					return i + 1;
				}
			}
			return 0;
		}

		// Token: 0x060091BA RID: 37306 RVA: 0x0025B730 File Offset: 0x00259930
		public int GetMoraleHiringLevelIndex()
		{
			for (int i = 0; i < this.Constants.Story.MoraleHiringThresholds.Length; i++)
			{
				if ((float)this.Morale < this.Constants.Story.MoraleHiringThresholds[i])
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x060091BB RID: 37307 RVA: 0x0025B778 File Offset: 0x00259978
		public float GetMRBReputationMod()
		{
			return this.GetMRBReputationMod(this.GetCurrentMRBLevel());
		}

		// Token: 0x060091BC RID: 37308 RVA: 0x0025B786 File Offset: 0x00259986
		public float GetMRBReputationMod(int mrbLevel)
		{
			mrbLevel = Mathf.Clamp(mrbLevel, 0, this.Constants.Story.MRBRepMod.Length - 1);
			return this.Constants.Story.MRBRepMod[mrbLevel];
		}

		// Token: 0x060091BD RID: 37309 RVA: 0x0025B7B8 File Offset: 0x002599B8
		public void RequestItem<ItemType>(string id, Action<ItemType> callback, BattleTechResourceType resourceType) where ItemType : class
		{
			if (callback == null)
			{
				return;
			}
			if (resourceType == BattleTechResourceType.Sprite && id.EndsWith(".png"))
			{
				id = id.Substring(0, id.Length - 4);
			}
			if (this.DataManager.ResourceLocator.EntryByID(id, resourceType, false) == null)
			{
				callback(default(ItemType));
				return;
			}
			LoadRequest loadRequest = this.DataManager.CreateLoadRequest(null, false);
			loadRequest.AddLoadRequest<ItemType>(resourceType, id, delegate(string loadedId, ItemType obj)
			{
				callback(obj);
			}, false);
			loadRequest.ProcessRequests(10U);
		}

		// Token: 0x060091BE RID: 37310 RVA: 0x0025B851 File Offset: 0x00259A51
		public int GetInSystemTransitTime(int jumpDistance = 0)
		{
			return Mathf.CeilToInt((float)((jumpDistance > 0) ? jumpDistance : this.Constants.Travel.DefaultSystemTravelTime) * this.CompanyStats.GetValue<float>(this.Constants.Story.DriveTravelID));
		}

		// Token: 0x060091BF RID: 37311 RVA: 0x0025B88C File Offset: 0x00259A8C
		public int GetArgoLocationRepairState(ArgoController.RepairStateLocations loc)
		{
			if (this.argoLocationRepairStates.ContainsKey(loc))
			{
				return this.argoLocationRepairStates[loc];
			}
			return 0;
		}

		// Token: 0x060091C0 RID: 37312 RVA: 0x0025B8AA File Offset: 0x00259AAA
		public void SetArgoLocationRepairState(ArgoController.RepairStateLocations loc, int val)
		{
			val = Mathf.Clamp(val, 0, 2);
			this.argoLocationRepairStates[loc] = val;
			this.SpaceController.argo.SetDamageStates(loc, val);
		}

		// Token: 0x060091C1 RID: 37313 RVA: 0x0025B8D8 File Offset: 0x00259AD8
		public void SetStarSystemDef(StarSystem system, string newDef)
		{
			if (!this.DataManager.SystemDefs.Exists(newDef))
			{
				return;
			}
			StarSystemDef starSystemDef = this.DataManager.SystemDefs.Get(newDef);
			if (system.Def.Description.Id == starSystemDef.Description.Id)
			{
				return;
			}
			system.SetNewStarSystemDef(starSystemDef, true);
			this.Starmap.Screen.RefreshBorders();
			if (system == this.CurSystem && this.UXAttached)
			{
				this.CurSystem.RefreshSystem();
				this.SetSystemOwnerReputation();
				this.RoomManager.RefreshDisplay();
				this.CurSystem.ResetContracts();
				if (this.NearestToTarget != null)
				{
					this.GeneratePotentialContracts(true, null, this.NearestToTarget, false);
				}
			}
		}

		// Token: 0x060091C2 RID: 37314 RVA: 0x0025B998 File Offset: 0x00259B98
		public float GetExpenditureCostModifier(EconomyScale level)
		{
			float num = 1f;
			FinancesConstantsDef finances = this.Constants.Finances;
			switch (level)
			{
			case EconomyScale.Spartan:
				num = finances.SpartanCostModifier;
				break;
			case EconomyScale.Restrictive:
				num = finances.RestrictedCostModifier;
				break;
			case EconomyScale.Generous:
				num = finances.GenerousCostModifier;
				break;
			case EconomyScale.Extravagant:
				num = finances.ExtravagantCostModifier;
				break;
			}
			return num;
		}

		// Token: 0x060091C3 RID: 37315 RVA: 0x0025B9F9 File Offset: 0x00259BF9
		public List<ResultDescriptionEntry> BuildSimGameResults(List<SimGameEventResult> resultsList, GameContext context, SimGameStatDescDef.DescriptionTense? tenseOverride = null, Pilot pilotOverride = null)
		{
			return this.BuildSimGameResults(resultsList.ToArray(), context, tenseOverride, pilotOverride);
		}

		// Token: 0x060091C4 RID: 37316 RVA: 0x0025BA0C File Offset: 0x00259C0C
		public List<ResultDescriptionEntry> BuildSimGameResults(SimGameEventResult[] resultsList, GameContext context, SimGameStatDescDef.DescriptionTense? tenseOverride = null, Pilot pilotOverride = null)
		{
			List<ResultDescriptionEntry> list = new List<ResultDescriptionEntry>();
			if (resultsList != null)
			{
				TagDataStructFetcher tagDataStructFetcher = this.Context.GetObject(GameContextObjectTagEnum.TagDataStructFetcher) as TagDataStructFetcher;
				foreach (SimGameEventResult simGameEventResult in resultsList)
				{
					GameContext gameContext = new GameContext(context);
					TagSet tagSet = null;
					Pilot pilot = null;
					MechDef mechDef = null;
					StarSystem starSystem = null;
					if (pilotOverride != null)
					{
						pilot = pilotOverride;
						gameContext.SetObject(GameContextObjectTagEnum.ResultMechWarrior, pilot);
						tagSet = pilot.pilotDef.PilotTags;
					}
					else
					{
						switch (simGameEventResult.Scope)
						{
						case EventScope.Company:
							tagSet = this.companyTags;
							break;
						case EventScope.MechWarrior:
							pilot = gameContext.GetObject(GameContextObjectTagEnum.TargetMechWarrior) as Pilot;
							gameContext.SetObject(GameContextObjectTagEnum.ResultMechWarrior, pilot);
							tagSet = pilot.pilotDef.PilotTags;
							break;
						case EventScope.Mech:
							mechDef = gameContext.GetObject(GameContextObjectTagEnum.TargetUnit) as MechDef;
							gameContext.SetObject(GameContextObjectTagEnum.ResultMech, mechDef);
							tagSet = mechDef.MechTags;
							break;
						case EventScope.Commander:
							pilot = this.Commander;
							gameContext.SetObject(GameContextObjectTagEnum.ResultMechWarrior, this.Commander);
							tagSet = this.commander.pilotDef.PilotTags;
							break;
						case EventScope.StarSystem:
							starSystem = gameContext.GetObject(GameContextObjectTagEnum.TargetStarSystem) as StarSystem;
							gameContext.SetObject(GameContextObjectTagEnum.ResultSystem, starSystem);
							tagSet = starSystem.Tags;
							break;
						case EventScope.SecondaryMechWarrior:
							pilot = gameContext.GetObject(GameContextObjectTagEnum.SecondaryMechWarrior) as Pilot;
							gameContext.SetObject(GameContextObjectTagEnum.ResultMechWarrior, pilot);
							tagSet = pilot.pilotDef.PilotTags;
							break;
						case EventScope.SecondaryMech:
							mechDef = gameContext.GetObject(GameContextObjectTagEnum.SecondaryUnit) as MechDef;
							gameContext.SetObject(GameContextObjectTagEnum.ResultMech, mechDef);
							tagSet = mechDef.MechTags;
							break;
						case EventScope.TertiaryMechWarrior:
							pilot = gameContext.GetObject(GameContextObjectTagEnum.TertiaryMechWarrior) as Pilot;
							gameContext.SetObject(GameContextObjectTagEnum.ResultMechWarrior, pilot);
							tagSet = pilot.pilotDef.PilotTags;
							break;
						case EventScope.RandomMech:
							mechDef = gameContext.GetObject(GameContextObjectTagEnum.RandomUnit) as MechDef;
							gameContext.SetObject(GameContextObjectTagEnum.ResultMech, mechDef);
							tagSet = mechDef.MechTags;
							break;
						}
					}
					if (simGameEventResult.TemporaryResult)
					{
						gameContext.SetObject(GameContextObjectTagEnum.ResultDuration, simGameEventResult.ResultDuration);
					}
					if (simGameEventResult.AddedTags != null && tagSet != null)
					{
						List<string> list2 = new List<string>();
						foreach (string text in simGameEventResult.AddedTags)
						{
							TagDataStruct item = tagDataStructFetcher.GetItem(text, false);
							if (item != null && item.IsVisible && !string.IsNullOrEmpty(item.FriendlyName))
							{
								list2.Add(item.ToToolTipString().ToString(true));
							}
						}
						if (list2.Count > 0)
						{
							string text2;
							if (pilot != null)
							{
								if (tenseOverride != null && tenseOverride.Value == SimGameStatDescDef.DescriptionTense.TemporalEnd)
								{
									text2 = Interpolator.Interpolate(this.Constants.Story.TagAddedMWTempEnd, gameContext, true);
								}
								else if (simGameEventResult.TemporaryResult)
								{
									text2 = Interpolator.Interpolate(this.Constants.Story.TagAddedMWTemp, gameContext, true);
								}
								else
								{
									text2 = Interpolator.Interpolate(this.Constants.Story.TagAddedMW, gameContext, true);
								}
							}
							else if (mechDef != null)
							{
								if (tenseOverride != null && tenseOverride.Value == SimGameStatDescDef.DescriptionTense.TemporalEnd)
								{
									text2 = Interpolator.Interpolate(this.Constants.Story.TagAddedMechTempEnd, gameContext, true);
								}
								else if (simGameEventResult.TemporaryResult)
								{
									text2 = Interpolator.Interpolate(this.Constants.Story.TagAddedMechTemp, gameContext, true);
								}
								else
								{
									text2 = Interpolator.Interpolate(this.Constants.Story.TagAddedMech, gameContext, true);
								}
							}
							else if (starSystem != null)
							{
								if (tenseOverride != null && tenseOverride.Value == SimGameStatDescDef.DescriptionTense.TemporalEnd)
								{
									text2 = Interpolator.Interpolate(this.Constants.Story.TagAddedSystemTempEnd, gameContext, true);
								}
								else if (simGameEventResult.TemporaryResult)
								{
									text2 = Interpolator.Interpolate(this.Constants.Story.TagAddedSystemTemp, gameContext, true);
								}
								else
								{
									text2 = Interpolator.Interpolate(this.Constants.Story.TagAddedSystem, gameContext, true);
								}
							}
							else if (tenseOverride != null && tenseOverride.Value == SimGameStatDescDef.DescriptionTense.TemporalEnd)
							{
								text2 = Interpolator.Interpolate(this.Constants.Story.TagAddedCompanyTempEnd, gameContext, true);
							}
							else if (simGameEventResult.TemporaryResult)
							{
								text2 = Interpolator.Interpolate(this.Constants.Story.TagAddedCompanyTemp, gameContext, true);
							}
							else
							{
								text2 = Strings.T(this.Constants.Story.TagAddedCompany);
							}
							foreach (string text3 in list2)
							{
								list.Add(new ResultDescriptionEntry(new Text("{1} {2} {3}{0}", new object[]
								{
									Environment.NewLine,
									"•",
									text2,
									text3
								}), gameContext, ""));
							}
						}
					}
					if (simGameEventResult.RemovedTags != null && tagSet != null)
					{
						List<string> list3 = new List<string>();
						foreach (string text4 in simGameEventResult.RemovedTags)
						{
							TagDataStruct item2 = tagDataStructFetcher.GetItem(text4, false);
							if (item2 != null && item2.IsVisible && !string.IsNullOrEmpty(item2.FriendlyName))
							{
								list3.Add(item2.ToToolTipString().ToString(true));
							}
						}
						if (list3.Count > 0)
						{
							string text5;
							if (pilot != null)
							{
								if (simGameEventResult.TemporaryResult)
								{
									text5 = Interpolator.Interpolate(this.Constants.Story.TagRemovedMWTemp, gameContext, true);
								}
								else
								{
									text5 = Interpolator.Interpolate(this.Constants.Story.TagRemovedMW, gameContext, true);
								}
							}
							else if (mechDef != null)
							{
								if (simGameEventResult.TemporaryResult)
								{
									text5 = Interpolator.Interpolate(this.Constants.Story.TagRemovedMechTemp, gameContext, true);
								}
								else
								{
									text5 = Interpolator.Interpolate(this.Constants.Story.TagRemovedMech, gameContext, true);
								}
							}
							else if (starSystem != null)
							{
								if (simGameEventResult.TemporaryResult)
								{
									text5 = Interpolator.Interpolate(this.Constants.Story.TagRemovedSystemTemp, gameContext, true);
								}
								else
								{
									text5 = Interpolator.Interpolate(this.Constants.Story.TagRemovedSystem, gameContext, true);
								}
							}
							else if (simGameEventResult.TemporaryResult)
							{
								text5 = Interpolator.Interpolate(this.Constants.Story.TagRemovedCompanyTemp, gameContext, true);
							}
							else
							{
								text5 = Strings.T(this.Constants.Story.TagRemovedCompany);
							}
							foreach (string text6 in list3)
							{
								list.Add(new ResultDescriptionEntry(string.Format("{1} {2} {3}{0}", new object[]
								{
									Environment.NewLine,
									"•",
									text5,
									text6
								}), gameContext, ""));
							}
						}
					}
					if (simGameEventResult.Stats != null)
					{
						SimGameStatDescDef.DescriptionTense descriptionTense = SimGameStatDescDef.DescriptionTense.Default;
						if (tenseOverride != null)
						{
							descriptionTense = tenseOverride.Value;
						}
						else if (simGameEventResult.TemporaryResult)
						{
							descriptionTense = SimGameStatDescDef.DescriptionTense.Temporal;
						}
						List<ResultDescriptionEntry> list4 = this.BuildSimGameStatsResults(simGameEventResult.Stats, gameContext, descriptionTense, "•");
						if (list4 != null && list4.Count > 0)
						{
							list.AddRange(list4);
						}
					}
					if (simGameEventResult.Actions != null)
					{
						List<ResultDescriptionEntry> list5 = this.BuildSimGameActionString(simGameEventResult.Actions, gameContext, SimGameStatDescDef.DescriptionTense.Default, "•");
						if (list5 != null && list5.Count > 0)
						{
							list.AddRange(list5);
						}
					}
				}
			}
			return list;
		}

		// Token: 0x060091C5 RID: 37317 RVA: 0x0025C1FC File Offset: 0x0025A3FC
		public List<ResultDescriptionEntry> BuildSimGameActionString(SimGameResultAction[] actions, GameContext context, SimGameStatDescDef.DescriptionTense tense, string prefix = "•")
		{
			List<ResultDescriptionEntry> list = new List<ResultDescriptionEntry>();
			foreach (SimGameResultAction simGameResultAction in actions)
			{
				string text = "SimGameStatDesc_SimGameResultAction_" + simGameResultAction.Type;
				if (this.DataManager.SimGameStatDescDefs.Exists(text))
				{
					SimGameStatDescDef simGameStatDescDef = this.DataManager.SimGameStatDescDefs.Get(text);
					if (simGameStatDescDef != null && !simGameStatDescDef.hidden)
					{
						GameContext gameContext = new GameContext(context);
						gameContext.SetObject(GameContextObjectTagEnum.ResultValue, simGameResultAction.value);
						if (simGameResultAction.additionalValues != null)
						{
							for (int j = 0; j < simGameResultAction.additionalValues.Length; j++)
							{
								string text2 = simGameResultAction.additionalValues[j];
								string text3 = string.Format("faction_{0}", text2);
								string text4;
								if (!text2.StartsWith("starsystemdef_"))
								{
									text4 = "starsystemdef_" + text2;
								}
								else
								{
									text4 = text2;
								}
								if (this.DataManager.Factions.Exists(text3))
								{
									FactionDef factionDef = this.DataManager.Factions.Get(text3);
									gameContext.SetObject(GameContextObjectTagEnum.ResultFaction, factionDef);
								}
								else if (this.starDict.ContainsKey(text4))
								{
									gameContext.SetObject(GameContextObjectTagEnum.ResultSystem, this.starDict[text4]);
								}
							}
						}
						bool flag = false;
						string text5;
						if (bool.TryParse(simGameResultAction.value, out flag))
						{
							if (flag)
							{
								text5 = Interpolator.Interpolate(simGameStatDescDef.GetResultString(SimGameStatDescDef.DescriptionType.Positive, tense), gameContext, true);
							}
							else
							{
								text5 = Interpolator.Interpolate(simGameStatDescDef.GetResultString(SimGameStatDescDef.DescriptionType.Negative, tense), gameContext, true);
							}
						}
						else
						{
							text5 = Interpolator.Interpolate(simGameStatDescDef.GetResultString(SimGameStatDescDef.DescriptionType.Set, tense), gameContext, true);
						}
						list.Add(new ResultDescriptionEntry(string.Format("{0} {1}{2}", prefix, text5, Environment.NewLine), gameContext, ""));
					}
				}
			}
			return list;
		}

		// Token: 0x060091C6 RID: 37318 RVA: 0x0025C3D0 File Offset: 0x0025A5D0
		public List<ResultDescriptionEntry> BuildSimGameStatsResults(SimGameStat[] stats, GameContext context, SimGameStatDescDef.DescriptionTense tense, string prefix = "•")
		{
			List<ResultDescriptionEntry> list = new List<ResultDescriptionEntry>();
			foreach (SimGameStat simGameStat in stats)
			{
				if (!string.IsNullOrEmpty(simGameStat.name) && simGameStat.value != null)
				{
					SimGameStatDescDef simGameStatDescDef = null;
					GameContext gameContext = new GameContext(context);
					if (this.DataManager.SimGameStatDescDefs.Exists("SimGameStatDesc_" + simGameStat.name))
					{
						simGameStatDescDef = this.DataManager.GetStatDescDef(simGameStat);
					}
					else
					{
						int num = simGameStat.name.IndexOf('.');
						if (num >= 0)
						{
							string text = simGameStat.name.Substring(0, num);
							if (this.DataManager.SimGameStatDescDefs.Exists("SimGameStatDesc_" + text))
							{
								simGameStatDescDef = this.DataManager.SimGameStatDescDefs.Get("SimGameStatDesc_" + text);
								string[] array = simGameStat.name.Split(new char[] { '.' });
								BattleTechResourceType? battleTechResourceType = null;
								object obj = null;
								string text2;
								if (array.Length < 3)
								{
									if (text == "Reputation")
									{
										text2 = "faction_" + array[1];
										battleTechResourceType = new BattleTechResourceType?(BattleTechResourceType.FactionDef);
									}
									else
									{
										text2 = null;
										battleTechResourceType = null;
									}
								}
								else
								{
									string text3 = array[1];
									text2 = array[2];
									try
									{
										battleTechResourceType = new BattleTechResourceType?((BattleTechResourceType)Enum.Parse(typeof(BattleTechResourceType), text3));
									}
									catch
									{
										battleTechResourceType = null;
									}
								}
								if (battleTechResourceType != null)
								{
									obj = this.DataManager.Get(battleTechResourceType.Value, text2);
								}
								if (obj != null)
								{
									gameContext.SetObject(GameContextObjectTagEnum.ResultObject, obj);
								}
								else
								{
									simGameStatDescDef = null;
								}
							}
						}
					}
					if (simGameStatDescDef != null)
					{
						if (!simGameStatDescDef.hidden)
						{
							gameContext.SetObject(GameContextObjectTagEnum.ResultValue, Mathf.Abs(simGameStat.ToSingle()));
							if (simGameStat.set)
							{
								string text4;
								if (simGameStat.Type == typeof(bool))
								{
									if (simGameStat.ToBool())
									{
										text4 = Interpolator.Interpolate(simGameStatDescDef.GetResultString(SimGameStatDescDef.DescriptionType.Positive, tense), gameContext, true);
									}
									else
									{
										text4 = Interpolator.Interpolate(simGameStatDescDef.GetResultString(SimGameStatDescDef.DescriptionType.Negative, tense), gameContext, true);
									}
								}
								else
								{
									text4 = Interpolator.Interpolate(simGameStatDescDef.GetResultString(SimGameStatDescDef.DescriptionType.Set, tense), gameContext, true);
								}
								if (!string.IsNullOrEmpty(text4))
								{
									list.Add(new ResultDescriptionEntry(new Text("{0} {1}\n", new object[] { prefix, text4 }), gameContext, simGameStat.name));
								}
							}
							else if (simGameStat.Type == typeof(int) || simGameStat.Type == typeof(float))
							{
								string text5 = null;
								if (simGameStat.ToSingle() > 0f)
								{
									text5 = Interpolator.Interpolate(simGameStatDescDef.GetResultString(SimGameStatDescDef.DescriptionType.Positive, tense), gameContext, true);
								}
								else if (simGameStat.ToSingle() < 0f)
								{
									text5 = Interpolator.Interpolate(simGameStatDescDef.GetResultString(SimGameStatDescDef.DescriptionType.Negative, tense), gameContext, true);
								}
								if (!string.IsNullOrEmpty(text5))
								{
									list.Add(new ResultDescriptionEntry(new Text("{0} {1}\n", new object[] { prefix, text5 }), gameContext, simGameStat.name));
								}
							}
						}
					}
					else
					{
						string tooltipString = this.DataManager.GetTooltipString(simGameStat, null);
						list.Add(new ResultDescriptionEntry(new Text("{0} {1} {2}\n", new object[] { prefix, tooltipString, simGameStat.value }), gameContext, simGameStat.name));
					}
				}
			}
			return list;
		}

		// Token: 0x060091C7 RID: 37319 RVA: 0x0025C768 File Offset: 0x0025A968
		public static BattleTechResourceType ComponentTypeToBattleTechResourceType(ComponentType type)
		{
			switch (type)
			{
			case ComponentType.Weapon:
				return BattleTechResourceType.WeaponDef;
			case ComponentType.AmmunitionBox:
				return BattleTechResourceType.AmmunitionBoxDef;
			case ComponentType.HeatSink:
				return BattleTechResourceType.HeatSinkDef;
			case ComponentType.JumpJet:
				return BattleTechResourceType.JumpJetDef;
			case ComponentType.Upgrade:
				return BattleTechResourceType.UpgradeDef;
			default:
				SimGameState.logger.LogError(string.Format("Cannot convert component type {0} into a BattleTechResourceType. Defaulting to AssetBundle", type));
				return BattleTechResourceType.AssetBundle;
			}
		}

		// Token: 0x060091C8 RID: 37320 RVA: 0x0025C7BC File Offset: 0x0025A9BC
		public static BattleTechResourceType ShopTypeToBattleTechResourceType(ShopItemType type)
		{
			switch (type)
			{
			case ShopItemType.Weapon:
				return BattleTechResourceType.WeaponDef;
			case ShopItemType.AmmunitionBox:
				return BattleTechResourceType.AmmunitionBoxDef;
			case ShopItemType.HeatSink:
				return BattleTechResourceType.HeatSinkDef;
			case ShopItemType.JumpJet:
				return BattleTechResourceType.JumpJetDef;
			case ShopItemType.MechPart:
			case ShopItemType.Mech:
				return BattleTechResourceType.MechDef;
			case ShopItemType.Upgrade:
				return BattleTechResourceType.UpgradeDef;
			default:
				SimGameState.logger.LogError(string.Format("Cannot convert shop type {0} into a BattleTechResourceType. Defaulting to AssetBundle", type));
				return BattleTechResourceType.AssetBundle;
			}
		}

		// Token: 0x060091C9 RID: 37321 RVA: 0x0025C81B File Offset: 0x0025AA1B
		public static string GetCBillString(int value)
		{
			return string.Format("{0}{1:n0}", '¢', value);
		}

		// Token: 0x060091CA RID: 37322 RVA: 0x0025C837 File Offset: 0x0025AA37
		public int GetScaledCBillValue(float maxPotentialValue, float curVal)
		{
			return Mathf.FloorToInt(curVal + maxPotentialValue * this.Constants.Finances.ContractFloorSalaryMultiplier);
		}

		// Token: 0x060091CB RID: 37323 RVA: 0x0025C852 File Offset: 0x0025AA52
		public static int RoundTo(float value, int roundVal)
		{
			if (roundVal <= 0)
			{
				return Mathf.RoundToInt(value);
			}
			return Mathf.RoundToInt(value / (float)roundVal) * roundVal;
		}

		// Token: 0x060091CC RID: 37324 RVA: 0x0025C86C File Offset: 0x0025AA6C
		public int GetCompanyModifiedInt(string statName, out Color colorRef)
		{
			colorRef = LazySingletonBehavior<UIManager>.Instance.UIColorRefs.medGray;
			if (!this.companyStats.ContainsStatistic(statName))
			{
				return 0;
			}
			int value = this.companyStats.GetValue<int>(statName);
			int num = 0;
			foreach (TemporarySimGameResult temporarySimGameResult in this.TemporaryResultTracker)
			{
				if (temporarySimGameResult.Scope == EventScope.Company && temporarySimGameResult.Stats != null)
				{
					foreach (SimGameStat simGameStat in temporarySimGameResult.Stats)
					{
						if (simGameStat.name == statName)
						{
							num += simGameStat.ToInt();
						}
					}
				}
			}
			if (num > 0)
			{
				colorRef = LazySingletonBehavior<UIManager>.Instance.UIColorRefs.green;
			}
			else if (num < 0)
			{
				colorRef = LazySingletonBehavior<UIManager>.Instance.UIColorRefs.red;
			}
			return value;
		}

		// Token: 0x060091CD RID: 37325 RVA: 0x0025C970 File Offset: 0x0025AB70
		public BaseDescriptionDef GetSimGameCharacterTooltipDesc(SimGameState.SimGameCharacterType c)
		{
			string text = "TooltipSimGameCharacter" + c.ToString();
			if (this.DataManager.Exists(BattleTechResourceType.BaseDescriptionDef, text))
			{
				return this.DataManager.BaseDescriptionDefs.Get(text);
			}
			return null;
		}

		// Token: 0x060091CE RID: 37326 RVA: 0x0025C9B8 File Offset: 0x0025ABB8
		public int GetNormalizedDifficulty(StarSystemDef def)
		{
			int num = def.GetDifficulty(this.SimGameMode);
			SimGameState.SimGameType simGameMode = this.SimGameMode;
			if (simGameMode != SimGameState.SimGameType.CAREER)
			{
				num = Mathf.Clamp(num + Mathf.FloorToInt(this.GlobalDifficulty), 1, 10);
			}
			return num;
		}

		// Token: 0x060091CF RID: 37327 RVA: 0x0025C9F4 File Offset: 0x0025ABF4
		public string GetFlashpointLengthString(FlashpointDef.EngagementLength engagementLength)
		{
			switch (engagementLength)
			{
			case FlashpointDef.EngagementLength.SHORT:
				return this.Constants.Flashpoints.ShortEngagementLengthString;
			case FlashpointDef.EngagementLength.MEDIUM:
				return this.Constants.Flashpoints.MediumEngagementLengthString;
			case FlashpointDef.EngagementLength.LONG:
				return this.Constants.Flashpoints.LongEngagementLengthString;
			case FlashpointDef.EngagementLength.CAMPAIGN:
				return this.Constants.Flashpoints.CampaignEngagementLengthString;
			default:
				return "";
			}
		}

		// Token: 0x060091D0 RID: 37328 RVA: 0x0025CA64 File Offset: 0x0025AC64
		public bool ReputationNormalValidator<T>(ref int newValue)
		{
			int num = Mathf.RoundToInt(this.Constants.Story.MaxReputation);
			newValue = Mathf.Clamp(newValue, -num, num);
			return true;
		}

		// Token: 0x060091D1 RID: 37329 RVA: 0x0025CA94 File Offset: 0x0025AC94
		public bool ReputationAllyValidator<T>(ref int newValue)
		{
			int num = Mathf.RoundToInt(this.Constants.Story.MaxReputation);
			newValue = Mathf.Clamp(newValue, 0, num);
			return true;
		}

		// Token: 0x060091D2 RID: 37330 RVA: 0x0025CAC4 File Offset: 0x0025ACC4
		public bool ReputationEnemyValidator<T>(ref int newValue)
		{
			int num = Mathf.RoundToInt(this.Constants.Story.MaxReputation);
			newValue = Mathf.Clamp(newValue, -num, 0);
			return true;
		}

		// Token: 0x060091D3 RID: 37331 RVA: 0x0025CAF4 File Offset: 0x0025ACF4
		public bool MinimumZeroValidator<T>(ref int newValue)
		{
			newValue = Mathf.Max(newValue, 0);
			return true;
		}

		// Token: 0x060091D4 RID: 37332 RVA: 0x0025CB01 File Offset: 0x0025AD01
		public bool MaximumZeroValidator<T>(ref int newValue)
		{
			newValue = Mathf.Min(newValue, 0);
			return true;
		}

		// Token: 0x060091D5 RID: 37333 RVA: 0x0025CB0E File Offset: 0x0025AD0E
		public bool MinimumOneValidator<T>(ref int newValue)
		{
			newValue = Mathf.Max(newValue, 1);
			return true;
		}

		// Token: 0x060091D6 RID: 37334 RVA: 0x0025CB1B File Offset: 0x0025AD1B
		public bool MinimumZeroMaximumFiftyValidator<T>(ref int newValue)
		{
			newValue = Mathf.Clamp(newValue, 0, 50);
			return true;
		}

		// Token: 0x060091D7 RID: 37335 RVA: 0x0025CB2A File Offset: 0x0025AD2A
		public bool ThousandRangeValidator<T>(ref int newValue)
		{
			newValue = Mathf.Clamp(newValue, -1000, 1000);
			return true;
		}

		// Token: 0x060091D8 RID: 37336 RVA: 0x00207690 File Offset: 0x00205890
		public bool OneToTenRangeValidtor<T>(ref int newValue)
		{
			newValue = Mathf.Clamp(newValue, 1, 10);
			return true;
		}

		// Token: 0x060091D9 RID: 37337 RVA: 0x0025CB40 File Offset: 0x0025AD40
		public bool OneToTenRangeValidtor<T>(ref float newValue)
		{
			newValue = Mathf.Clamp(newValue, 1f, 10f);
			return true;
		}

		// Token: 0x060091DA RID: 37338 RVA: 0x0025CB58 File Offset: 0x0025AD58
		private bool IsTempResultSameTargetType(TemporarySimGameResult a, TemporarySimGameResult b)
		{
			switch (a.Scope)
			{
			case EventScope.Company:
				return b.Scope == EventScope.Company;
			case EventScope.MechWarrior:
			case EventScope.SecondaryMechWarrior:
			case EventScope.AllMechWarriors:
			case EventScope.TertiaryMechWarrior:
			case EventScope.DeadMechWarrior:
				return b.TargetPilot != null;
			case EventScope.Mech:
			case EventScope.SecondaryMech:
			case EventScope.AllMechs:
			case EventScope.RandomMech:
				return b.TargetMechDef != null;
			case EventScope.Commander:
				return b.Scope == EventScope.Commander;
			case EventScope.StarSystem:
				return b.Scope == EventScope.StarSystem;
			default:
				SimGameState.logger.LogError("Invalid scope: " + a.Scope);
				return false;
			}
		}

		// Token: 0x060091DB RID: 37339 RVA: 0x0025CBF8 File Offset: 0x0025ADF8
		private bool IsTempResultSameTarget(TemporarySimGameResult a, TemporarySimGameResult b)
		{
			switch (a.Scope)
			{
			case EventScope.Company:
				return true;
			case EventScope.MechWarrior:
			case EventScope.SecondaryMechWarrior:
			case EventScope.AllMechWarriors:
			case EventScope.TertiaryMechWarrior:
			case EventScope.DeadMechWarrior:
				return a != null && b != null && a.TargetPilot.GUID == b.TargetPilot.GUID;
			case EventScope.Mech:
			case EventScope.SecondaryMech:
			case EventScope.AllMechs:
			case EventScope.RandomMech:
				return a != null && b != null && a.TargetMechDef.GUID == b.TargetMechDef.GUID;
			case EventScope.Commander:
				return true;
			case EventScope.StarSystem:
				return true;
			default:
				SimGameState.logger.LogError("Invalid scope: " + a.Scope);
				return true;
			}
		}

		// Token: 0x060091DC RID: 37340 RVA: 0x0025CCB4 File Offset: 0x0025AEB4
		private void AddOrRemoveTempTags(TemporarySimGameResult res, bool add)
		{
			Pilot pilot = null;
			TagSet tagSet;
			switch (res.Scope)
			{
			case EventScope.Company:
			{
				StatCollection statCollection = this.CompanyStats;
				tagSet = this.CompanyTags;
				break;
			}
			case EventScope.MechWarrior:
			case EventScope.SecondaryMechWarrior:
			case EventScope.AllMechWarriors:
			case EventScope.TertiaryMechWarrior:
			case EventScope.DeadMechWarrior:
			{
				if (res.TargetPilot == null)
				{
					SimGameState.logger.LogError(string.Format("Unable to resolve scope of {0} without a valid pilot ref", res.Scope));
					return;
				}
				Pilot targetPilot = res.TargetPilot;
				if (res.Scope != EventScope.DeadMechWarrior)
				{
					using (IEnumerator<Pilot> enumerator = this.PilotRoster.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							Pilot pilot2 = enumerator.Current;
							if (pilot2.GUID == targetPilot.GUID)
							{
								if (pilot2 != targetPilot)
								{
									SimGameState.logger.LogWarning("pilot roster same guid different reference");
								}
								pilot = pilot2;
								break;
							}
						}
						goto IL_154;
					}
				}
				foreach (Pilot pilot3 in this.Graveyard)
				{
					if (pilot3.GUID == targetPilot.GUID)
					{
						if (pilot3 != targetPilot)
						{
							SimGameState.logger.LogWarning("graveyard same guid different reference");
						}
						pilot = pilot3;
						break;
					}
				}
				IL_154:
				if (pilot == null)
				{
					SimGameState.logger.LogWarning(string.Format("p is still null. unable to resolve scope of {0} without a valid pilot ref", res.Scope));
					return;
				}
				StatCollection statCollection2 = pilot.StatCollection;
				tagSet = pilot.pilotDef.PilotTags;
				break;
			}
			case EventScope.Mech:
			case EventScope.SecondaryMech:
			case EventScope.AllMechs:
			case EventScope.RandomMech:
			{
				MechDef targetMechDef = res.TargetMechDef;
				StatCollection stats = targetMechDef.Stats;
				tagSet = targetMechDef.MechTags;
				break;
			}
			case EventScope.Commander:
			{
				StatCollection commanderStats = this.CommanderStats;
				tagSet = this.CommanderTags;
				break;
			}
			case EventScope.StarSystem:
			{
				StatCollection stats2 = this.CurSystem.Stats;
				tagSet = this.CurSystem.Tags;
				break;
			}
			default:
				SimGameState.logger.LogError("Invalid scope: " + res.Scope);
				return;
			}
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			if (res.AddedTags != null)
			{
				list.AddRange(res.AddedTags);
			}
			if (res.Stats != null)
			{
				foreach (SimGameStat simGameStat in res.Stats)
				{
					if (!list2.Contains(simGameStat.name))
					{
						list2.Add(simGameStat.name);
					}
				}
			}
			List<string> list3 = new List<string>();
			for (int j = 0; j < this.TemporaryResultTracker.Count; j++)
			{
				TemporarySimGameResult temporarySimGameResult = this.TemporaryResultTracker[j];
				if (this.IsTempResultSameTargetType(res, temporarySimGameResult) && this.IsTempResultSameTarget(res, temporarySimGameResult))
				{
					for (int k = list.Count - 1; k >= 0; k--)
					{
						if (temporarySimGameResult.AddedTags.Contains(list[k]))
						{
							list.RemoveAt(k);
						}
					}
					for (int l = list2.Count - 1; l >= 0; l--)
					{
						if (temporarySimGameResult.Stats != null)
						{
							bool flag = false;
							for (int m = 0; m < temporarySimGameResult.Stats.Length; m++)
							{
								if (temporarySimGameResult.Stats[m].name == list2[l])
								{
									list2.RemoveAt(l);
									flag = true;
									break;
								}
							}
							if (flag)
							{
								break;
							}
						}
					}
				}
			}
			foreach (string text in list)
			{
				list3.Add("MODIFIED_TAG_" + text);
			}
			foreach (string text2 in list2)
			{
				list3.Add("MODIFIED_STAT_" + text2);
			}
			foreach (string text3 in list3)
			{
				if (add)
				{
					if (!tagSet.Contains(text3))
					{
						tagSet.Add(text3);
					}
				}
				else if (tagSet.Contains(text3))
				{
					tagSet.Remove(text3);
				}
			}
		}

		// Token: 0x060091DD RID: 37341 RVA: 0x0025D11C File Offset: 0x0025B31C
		public int GetRawCareerModeDifficulty()
		{
			return this.Constants.CareerMode.StartingGlobalContractDifficulty + this.GetCurrentMRBLevel() * this.Constants.CareerMode.GlobalContractDifficultyIncreasePerMRBRating;
		}

		// Token: 0x060091DE RID: 37342 RVA: 0x0025D148 File Offset: 0x0025B348
		public int GetCurrentCareerModeRankLevel()
		{
			int num = Mathf.FloorToInt(this.GetCurrentCareerModeScore());
			for (int i = 0; i < this.Constants.CareerMode.CareerRankScoreMinimums.Length; i++)
			{
				if (num < this.Constants.CareerMode.CareerRankScoreMinimums[i])
				{
					return Mathf.Max(0, i - 1);
				}
			}
			return this.Constants.CareerMode.CareerRankScoreMinimums.Length - 1;
		}

		// Token: 0x060091DF RID: 37343 RVA: 0x0025D1B4 File Offset: 0x0025B3B4
		public string GetCurrentCareerModeRankLevelString()
		{
			int num = this.GetCurrentCareerModeRankLevel();
			num = Mathf.Min(num, this.Constants.CareerMode.CareerRankNames.Length - 1);
			return this.Constants.CareerMode.CareerRankNames[num];
		}

		// Token: 0x060091E0 RID: 37344 RVA: 0x0025D1F8 File Offset: 0x0025B3F8
		public string GetCareerModeRewardString()
		{
			int num = this.GetCurrentCareerModeRankLevel();
			num = Mathf.Min(num, this.Constants.CareerMode.CareerRankRewards.Length - 1);
			return this.Constants.CareerMode.CareerRankRewards[num];
		}

		// Token: 0x060091E1 RID: 37345 RVA: 0x0025D239 File Offset: 0x0025B439
		public Sprite GetCareerModeRankSprite()
		{
			this.GetCurrentCareerModeRankLevel();
			return null;
		}

		// Token: 0x060091E2 RID: 37346 RVA: 0x0025D243 File Offset: 0x0025B443
		public bool IsCareerMode()
		{
			return this.SimGameMode == SimGameState.SimGameType.CAREER;
		}

		// Token: 0x060091E3 RID: 37347 RVA: 0x0025D24E File Offset: 0x0025B44E
		public int GetCareerModeDaysRemaining()
		{
			return Mathf.Max(0, this.Constants.CareerMode.GameLength - this.DaysPassed);
		}

		// Token: 0x060091E4 RID: 37348 RVA: 0x0025D26D File Offset: 0x0025B46D
		public bool IsCareerModeComplete()
		{
			return this.DaysPassed > this.Constants.CareerMode.GameLength;
		}

		// Token: 0x060091E5 RID: 37349 RVA: 0x0025D287 File Offset: 0x0025B487
		public bool IsCareerModeLocked()
		{
			return this.careerModeLocked;
		}

		// Token: 0x060091E6 RID: 37350 RVA: 0x0025D290 File Offset: 0x0025B490
		public void OnCareerModeCompleted()
		{
			if (this.IsCareerModeLocked())
			{
				return;
			}
			this.LockCareerModeScores();
			this.interruptQueue.QueueCareerModeEndScreen();
			string careerModeRewardString = this.GetCareerModeRewardString();
			this.interruptQueue.QueueRewardsPopup(careerModeRewardString);
			CareerModeCompleteMessage careerModeCompleteMessage = new CareerModeCompleteMessage(this.GetCurrentCareerModeScore(), this.IsIronmanCampaign);
			this.MessageCenter.PublishMessage(careerModeCompleteMessage);
		}

		// Token: 0x060091E7 RID: 37351 RVA: 0x0025D2EA File Offset: 0x0025B4EA
		public List<string> GetAllCareerAllies()
		{
			if (this.IsCareerModeComplete())
			{
				return this.CareerModeEndAlliedFactions;
			}
			return this.AlliedFactions;
		}

		// Token: 0x060091E8 RID: 37352 RVA: 0x0025D304 File Offset: 0x0025B504
		public bool IsCareerFactionAlly(FactionValue faction)
		{
			List<string> list = (this.IsCareerModeComplete() ? this.CareerModeEndAlliedFactions : this.AlliedFactions);
			return this.IsFactionAlly(faction, list);
		}

		// Token: 0x060091E9 RID: 37353 RVA: 0x0025D330 File Offset: 0x0025B530
		public bool IsCareerFactionEnemy(FactionValue faction)
		{
			List<string> list = (this.IsCareerModeComplete() ? this.CareerModeEndAlliedFactions : this.AlliedFactions);
			return this.IsFactionEnemy(faction, list);
		}

		// Token: 0x060091EA RID: 37354 RVA: 0x0025D35C File Offset: 0x0025B55C
		public List<string> GetAllCareerEnemies()
		{
			List<string> list = (this.IsCareerModeComplete() ? this.CareerModeEndAlliedFactions : this.AlliedFactions);
			return this.GetAllEnemies(list);
		}

		// Token: 0x060091EB RID: 37355 RVA: 0x0025D388 File Offset: 0x0025B588
		public void DisplayAllCareerScoreInfo()
		{
			Debug.Log("======================== SCORE OUTPUT ================================");
			float careerModeOverallDifficultyMod = this.GetCareerModeOverallDifficultyMod();
			float num = this.GetCurrentCareerModeScore() / careerModeOverallDifficultyMod;
			float currentCareerModeScore = this.GetCurrentCareerModeScore();
			this.GetMaxPossibleCareerModeScore();
			Debug.Log(string.Format("RawTotalScore:{0}   Mod:{1}  Final TotalScore:{2}", num, careerModeOverallDifficultyMod, currentCareerModeScore));
			this.DisplayCareerScoreInfo(SimGameState.CareerModeScoreTypes.CBillScore);
			this.DisplayCareerScoreInfo(SimGameState.CareerModeScoreTypes.ContractScore);
			this.DisplayCareerScoreInfo(SimGameState.CareerModeScoreTypes.ChassisScore);
			this.DisplayCareerScoreInfo(SimGameState.CareerModeScoreTypes.WeightScore);
			this.DisplayCareerScoreInfo(SimGameState.CareerModeScoreTypes.AllChassis);
			this.DisplayCareerScoreInfo(SimGameState.CareerModeScoreTypes.PilotExperience);
			this.DisplayCareerScoreInfo(SimGameState.CareerModeScoreTypes.SystemsVisited);
			this.DisplayCareerScoreInfo(SimGameState.CareerModeScoreTypes.AllSystems);
			this.DisplayCareerScoreInfo(SimGameState.CareerModeScoreTypes.PositiveReputation);
			this.DisplayCareerScoreInfo(SimGameState.CareerModeScoreTypes.NegativeReputation);
			this.DisplayCareerScoreInfo(SimGameState.CareerModeScoreTypes.MaxedReputation);
			this.DisplayCareerScoreInfo(SimGameState.CareerModeScoreTypes.ArgoUpgrade);
			this.DisplayCareerScoreInfo(SimGameState.CareerModeScoreTypes.AllArgoUpgrade);
			this.DisplayCareerScoreInfo(SimGameState.CareerModeScoreTypes.MoraleScore);
			this.DisplayCareerScoreInfo(SimGameState.CareerModeScoreTypes.MRBScore);
			this.DisplayCareerScoreInfo(SimGameState.CareerModeScoreTypes.MaxMRBScore);
		}

		// Token: 0x060091EC RID: 37356 RVA: 0x0025D458 File Offset: 0x0025B658
		public void DisplayCareerScoreInfo(SimGameState.CareerModeScoreTypes type)
		{
			float num = this.careerModeScoreCalculators[type]();
			float careerModeOverallDifficultyMod = this.GetCareerModeOverallDifficultyMod();
			float num2 = this.MultiplyByCareerDifficultyMod(Mathf.Min(this.careerModeScoreCalculators[type](), this.GetMaxPossibleCareerModeScore(type)));
			float maxModifiedCareerModeScore = this.GetMaxModifiedCareerModeScore(type);
			Debug.Log(string.Format("DEBUG: Score type: {0}  RawValue: {1}  Mod: {2}  FinalValue: {3}  MAXValue: {4}", new object[]
			{
				type.ToString(),
				num,
				careerModeOverallDifficultyMod,
				num2,
				maxModifiedCareerModeScore
			}));
		}

		// Token: 0x060091ED RID: 37357 RVA: 0x0025D4F4 File Offset: 0x0025B6F4
		public void PopulateCareerModeScoringDictionaries()
		{
			this.careerModeTargetValues.Clear();
			this.careerModePerUnitValues.Clear();
			this.careerModeTargetValues.Add(SimGameState.CareerModeScoreTypes.AllArgoUpgrade, this.Constants.CareerMode.AllArgoUpgradeTarget);
			this.careerModeTargetValues.Add(SimGameState.CareerModeScoreTypes.AllChassis, this.Constants.CareerMode.AllChassisTarget);
			this.careerModeTargetValues.Add(SimGameState.CareerModeScoreTypes.AllSystems, this.Constants.CareerMode.AllSystemsTarget);
			this.careerModeTargetValues.Add(SimGameState.CareerModeScoreTypes.ArgoUpgrade, this.Constants.CareerMode.ArgoUpgradeTarget);
			this.careerModeTargetValues.Add(SimGameState.CareerModeScoreTypes.CBillScore, this.Constants.CareerMode.CBillScoreTarget);
			this.careerModeTargetValues.Add(SimGameState.CareerModeScoreTypes.ChassisScore, this.Constants.CareerMode.ChassisScoreTarget);
			this.careerModeTargetValues.Add(SimGameState.CareerModeScoreTypes.ContractScore, this.Constants.CareerMode.ContractScoreTarget);
			this.careerModeTargetValues.Add(SimGameState.CareerModeScoreTypes.MaxedReputation, this.Constants.CareerMode.MaxedReputationTarget);
			this.careerModeTargetValues.Add(SimGameState.CareerModeScoreTypes.MaxMRBScore, this.Constants.CareerMode.MaxMRBScoreTarget);
			this.careerModeTargetValues.Add(SimGameState.CareerModeScoreTypes.MoraleScore, this.Constants.CareerMode.MoraleScoreTarget);
			this.careerModeTargetValues.Add(SimGameState.CareerModeScoreTypes.MRBScore, this.Constants.CareerMode.MRBScoreTarget);
			this.careerModeTargetValues.Add(SimGameState.CareerModeScoreTypes.NegativeReputation, this.Constants.CareerMode.NegativeReputationTarget);
			this.careerModeTargetValues.Add(SimGameState.CareerModeScoreTypes.PilotExperience, this.Constants.CareerMode.PilotExperienceTarget);
			this.careerModeTargetValues.Add(SimGameState.CareerModeScoreTypes.PositiveReputation, this.Constants.CareerMode.PositiveReputationTarget);
			this.careerModeTargetValues.Add(SimGameState.CareerModeScoreTypes.SystemsVisited, this.Constants.CareerMode.SystemsVisitedTarget);
			this.careerModeTargetValues.Add(SimGameState.CareerModeScoreTypes.WeightScore, this.Constants.CareerMode.WeightScoreTarget);
			this.careerModePerUnitValues.Add(SimGameState.CareerModeScoreTypes.AllArgoUpgrade, this.Constants.CareerMode.AllArgoUpgradeScorePerUnit);
			this.careerModePerUnitValues.Add(SimGameState.CareerModeScoreTypes.AllChassis, this.Constants.CareerMode.AllChassisScorePerUnit);
			this.careerModePerUnitValues.Add(SimGameState.CareerModeScoreTypes.AllSystems, this.Constants.CareerMode.AllSystemsScorePerUnit);
			this.careerModePerUnitValues.Add(SimGameState.CareerModeScoreTypes.ArgoUpgrade, this.Constants.CareerMode.ArgoUpgradeScorePerUnit);
			this.careerModePerUnitValues.Add(SimGameState.CareerModeScoreTypes.CBillScore, this.Constants.CareerMode.CBillPointsPerUnit);
			this.careerModePerUnitValues.Add(SimGameState.CareerModeScoreTypes.ChassisScore, this.Constants.CareerMode.ChassisPointsPerUnit);
			this.careerModePerUnitValues.Add(SimGameState.CareerModeScoreTypes.ContractScore, this.Constants.CareerMode.ContractPointsPerUnit);
			this.careerModePerUnitValues.Add(SimGameState.CareerModeScoreTypes.MaxedReputation, this.Constants.CareerMode.MaxedReputationScorePerUnit);
			this.careerModePerUnitValues.Add(SimGameState.CareerModeScoreTypes.MaxMRBScore, this.Constants.CareerMode.MaxMRBScorePerUnit);
			this.careerModePerUnitValues.Add(SimGameState.CareerModeScoreTypes.MoraleScore, this.Constants.CareerMode.MoraleScorePerUnit);
			this.careerModePerUnitValues.Add(SimGameState.CareerModeScoreTypes.MRBScore, this.Constants.CareerMode.MRBScorePerUnit);
			this.careerModePerUnitValues.Add(SimGameState.CareerModeScoreTypes.NegativeReputation, this.Constants.CareerMode.NegativeReputationScorePerUnit);
			this.careerModePerUnitValues.Add(SimGameState.CareerModeScoreTypes.PilotExperience, this.Constants.CareerMode.PilotExperienceScorePerUnit);
			this.careerModePerUnitValues.Add(SimGameState.CareerModeScoreTypes.PositiveReputation, this.Constants.CareerMode.PositiveReputationScorePerUnit);
			this.careerModePerUnitValues.Add(SimGameState.CareerModeScoreTypes.SystemsVisited, this.Constants.CareerMode.SystemsVisitedScorePerUnit);
			this.careerModePerUnitValues.Add(SimGameState.CareerModeScoreTypes.WeightScore, this.Constants.CareerMode.WeightScorePerUnit);
			this.careerModeScoreCalculators.Add(SimGameState.CareerModeScoreTypes.AllArgoUpgrade, new SimGameState.CareerModeScoreCalulation(this.CareerMode_GetAllArgoUpgradeScore));
			this.careerModeScoreCalculators.Add(SimGameState.CareerModeScoreTypes.AllChassis, new SimGameState.CareerModeScoreCalulation(this.CareerMode_GetAllChassisScore));
			this.careerModeScoreCalculators.Add(SimGameState.CareerModeScoreTypes.AllSystems, new SimGameState.CareerModeScoreCalulation(this.CareerMode_GetAllSystemsScore));
			this.careerModeScoreCalculators.Add(SimGameState.CareerModeScoreTypes.ArgoUpgrade, new SimGameState.CareerModeScoreCalulation(this.CareerMode_GetArgoUpgradeScore));
			this.careerModeScoreCalculators.Add(SimGameState.CareerModeScoreTypes.CBillScore, new SimGameState.CareerModeScoreCalulation(this.CareerMode_GetCBillScore));
			this.careerModeScoreCalculators.Add(SimGameState.CareerModeScoreTypes.ChassisScore, new SimGameState.CareerModeScoreCalulation(this.CareerMode_GetChassisPoints));
			this.careerModeScoreCalculators.Add(SimGameState.CareerModeScoreTypes.ContractScore, new SimGameState.CareerModeScoreCalulation(this.CareerMode_ContractScore));
			this.careerModeScoreCalculators.Add(SimGameState.CareerModeScoreTypes.MaxedReputation, new SimGameState.CareerModeScoreCalulation(this.CareerMode_GetMaxedReputationScore));
			this.careerModeScoreCalculators.Add(SimGameState.CareerModeScoreTypes.MaxMRBScore, new SimGameState.CareerModeScoreCalulation(this.CareerMode_GetMaxMRBScore));
			this.careerModeScoreCalculators.Add(SimGameState.CareerModeScoreTypes.MoraleScore, new SimGameState.CareerModeScoreCalulation(this.CareerMode_GetMoraleScore));
			this.careerModeScoreCalculators.Add(SimGameState.CareerModeScoreTypes.MRBScore, new SimGameState.CareerModeScoreCalulation(this.CareerMode_GetMRBScore));
			this.careerModeScoreCalculators.Add(SimGameState.CareerModeScoreTypes.NegativeReputation, new SimGameState.CareerModeScoreCalulation(this.CareerMode_GetNegativeReputationScore));
			this.careerModeScoreCalculators.Add(SimGameState.CareerModeScoreTypes.PilotExperience, new SimGameState.CareerModeScoreCalulation(this.CareerMode_GetPilotExperienceScore));
			this.careerModeScoreCalculators.Add(SimGameState.CareerModeScoreTypes.PositiveReputation, new SimGameState.CareerModeScoreCalulation(this.CareerMode_GetPositiveReputationScore));
			this.careerModeScoreCalculators.Add(SimGameState.CareerModeScoreTypes.SystemsVisited, new SimGameState.CareerModeScoreCalulation(this.CareerMode_GetSystemsVisitedScore));
			this.careerModeScoreCalculators.Add(SimGameState.CareerModeScoreTypes.WeightScore, new SimGameState.CareerModeScoreCalulation(this.CareerMode_GetWeightScore));
		}

		// Token: 0x060091EE RID: 37358 RVA: 0x0025DA2C File Offset: 0x0025BC2C
		private void LockCareerModeScores()
		{
			if (this.careerModeLocked)
			{
				return;
			}
			foreach (object obj in Enum.GetValues(typeof(SimGameState.CareerModeScoreTypes)))
			{
				SimGameState.CareerModeScoreTypes careerModeScoreTypes = (SimGameState.CareerModeScoreTypes)obj;
				float num = this.MultiplyByCareerDifficultyMod(Mathf.Min(this.careerModeScoreCalculators[careerModeScoreTypes](), this.GetMaxPossibleCareerModeScore(careerModeScoreTypes)));
				string text = "CAREER_MODE_FINAL_" + careerModeScoreTypes.ToString();
				this.SetCompanyStat(text, num, false);
			}
			FactionValue mercenaryReviewBoardFactionValue = FactionEnumeration.GetMercenaryReviewBoardFactionValue();
			string text2 = string.Format("{0}{1}.{2}", "CAREER_MODE_FINAL_", "Reputation", mercenaryReviewBoardFactionValue.Name);
			int num2 = this.GetRawReputation(mercenaryReviewBoardFactionValue);
			this.SetCompanyStat(text2, num2, false);
			text2 = string.Format("{0}{1}", "CAREER_MODE_FINAL_", "Morale");
			num2 = this.Morale;
			this.SetCompanyStat(text2, num2, false);
			text2 = string.Format("{0}{1}", "CAREER_MODE_FINAL_", "COMPANY_Career_FinalDiffMod");
			float careerModeOverallDifficultyMod = this.GetCareerModeOverallDifficultyMod();
			this.SetCompanyStat(text2, careerModeOverallDifficultyMod, false);
			this.CareerModeEndAlliedFactions.Clear();
			this.CareerModeEndAlliedFactions.AddRange(this.AlliedFactions);
			this.careerModeLocked = true;
		}

		// Token: 0x060091EF RID: 37359 RVA: 0x0025DB84 File Offset: 0x0025BD84
		public float GetMaxPossibleCareerModeScore()
		{
			float num = 0f;
			foreach (object obj in Enum.GetValues(typeof(SimGameState.CareerModeScoreTypes)))
			{
				SimGameState.CareerModeScoreTypes careerModeScoreTypes = (SimGameState.CareerModeScoreTypes)obj;
				num += this.GetMaxPossibleCareerModeScore(careerModeScoreTypes);
			}
			return num;
		}

		// Token: 0x060091F0 RID: 37360 RVA: 0x0025DBF0 File Offset: 0x0025BDF0
		public float GetMaxModifiedCareerModeScore()
		{
			return this.MultiplyByCareerDifficultyMod(this.GetMaxPossibleCareerModeScore());
		}

		// Token: 0x060091F1 RID: 37361 RVA: 0x0025DBFE File Offset: 0x0025BDFE
		public float GetMaxPossibleCareerModeScore(SimGameState.CareerModeScoreTypes type)
		{
			return this.careerModePerUnitValues[type] * this.careerModeTargetValues[type];
		}

		// Token: 0x060091F2 RID: 37362 RVA: 0x0025DC19 File Offset: 0x0025BE19
		public float GetMaxModifiedCareerModeScore(SimGameState.CareerModeScoreTypes type)
		{
			return this.MultiplyByCareerDifficultyMod(this.GetMaxPossibleCareerModeScore(type));
		}

		// Token: 0x060091F3 RID: 37363 RVA: 0x0025DC28 File Offset: 0x0025BE28
		public float GetCurrentCareerModeScore()
		{
			float num = 0f;
			foreach (object obj in Enum.GetValues(typeof(SimGameState.CareerModeScoreTypes)))
			{
				SimGameState.CareerModeScoreTypes careerModeScoreTypes = (SimGameState.CareerModeScoreTypes)obj;
				num += this.GetCareerModeScore(careerModeScoreTypes);
			}
			return num;
		}

		// Token: 0x060091F4 RID: 37364 RVA: 0x0025DC94 File Offset: 0x0025BE94
		public float GetCareerModeScore(SimGameState.CareerModeScoreTypes type)
		{
			if (this.IsCareerModeComplete())
			{
				string text = "CAREER_MODE_FINAL_" + type.ToString();
				return this.companyStats.GetValue<float>(text);
			}
			return this.MultiplyByCareerDifficultyMod(Mathf.Min(this.careerModeScoreCalculators[type](), this.GetMaxPossibleCareerModeScore(type)));
		}

		// Token: 0x060091F5 RID: 37365 RVA: 0x0025DCF4 File Offset: 0x0025BEF4
		public float GetCareerModeOverallDifficultyMod()
		{
			if (this.IsCareerModeLocked())
			{
				string text = string.Format("{0}{1}", "CAREER_MODE_FINAL_", "COMPANY_Career_FinalDiffMod");
				return this.companyStats.GetValue<float>(text);
			}
			return this.difficultySettings.GetRawCareerModifier();
		}

		// Token: 0x060091F6 RID: 37366 RVA: 0x0025DD36 File Offset: 0x0025BF36
		public float GetClampedCareerModeDifficultyMod()
		{
			return this.difficultySettings.GetClampCareerModifier();
		}

		// Token: 0x060091F7 RID: 37367 RVA: 0x0025DD43 File Offset: 0x0025BF43
		public float GetCareerModeMaximumOverallDifficultyMod()
		{
			return this.difficultySettings.GetMaxPossibleCareerModifier();
		}

		// Token: 0x060091F8 RID: 37368 RVA: 0x0025DD50 File Offset: 0x0025BF50
		public float MultiplyByCareerDifficultyMod(float value)
		{
			return value * this.GetClampedCareerModeDifficultyMod();
		}

		// Token: 0x060091F9 RID: 37369 RVA: 0x0025DD5C File Offset: 0x0025BF5C
		public int GetCareerMRBRating()
		{
			FactionValue mercenaryReviewBoardFactionValue = FactionEnumeration.GetMercenaryReviewBoardFactionValue();
			if (this.IsCareerModeComplete())
			{
				string text = string.Format("{0}{1}.{2}", "CAREER_MODE_FINAL_", "Reputation", mercenaryReviewBoardFactionValue.Name);
				return this.companyStats.GetValue<int>(text);
			}
			return this.GetRawReputation(mercenaryReviewBoardFactionValue);
		}

		// Token: 0x060091FA RID: 37370 RVA: 0x0025DDA8 File Offset: 0x0025BFA8
		public int GetCareerMorale()
		{
			if (this.IsCareerModeComplete())
			{
				string text = string.Format("{0}{1}", "CAREER_MODE_FINAL_", "Morale");
				return this.companyStats.GetValue<int>(text);
			}
			return this.Morale;
		}

		// Token: 0x060091FB RID: 37371 RVA: 0x0025DDE5 File Offset: 0x0025BFE5
		private float CareerMode_GetCBillScore()
		{
			return (float)Mathf.FloorToInt((float)this.CompanyStats.GetValue<int>("FundsEverGained") * this.Constants.CareerMode.CBillPointsPerUnit);
		}

		// Token: 0x060091FC RID: 37372 RVA: 0x0025DE0F File Offset: 0x0025C00F
		private float CareerMode_ContractScore()
		{
			return (float)Mathf.FloorToInt((float)this.CompanyStats.GetValue<int>("COMPANY_MissionAggregateDifficulty") * this.Constants.CareerMode.ContractPointsPerUnit);
		}

		// Token: 0x060091FD RID: 37373 RVA: 0x0025DE39 File Offset: 0x0025C039
		private float CareerMode_GetAllArgoUpgradeScore()
		{
			return (float)Mathf.FloorToInt((float)((this.DataManager.ResourceLocator.AllEntriesOfResource(BattleTechResourceType.ShipModuleUpgrade, false).Length == this.ShipUpgrades.Count) ? 1 : 0) * this.Constants.CareerMode.AllArgoUpgradeScorePerUnit);
		}

		// Token: 0x060091FE RID: 37374 RVA: 0x0025DE79 File Offset: 0x0025C079
		private float CareerMode_GetArgoUpgradeScore()
		{
			return (float)Mathf.FloorToInt((float)this.ShipUpgrades.Count * this.Constants.CareerMode.ArgoUpgradeScorePerUnit);
		}

		// Token: 0x060091FF RID: 37375 RVA: 0x0025DEA0 File Offset: 0x0025C0A0
		private bool DidCompleteWeightClass(WeightClass wc, List<string> allChassis = null)
		{
			List<string> list = new List<string>(this.allAcquirableMechs[wc]);
			if (allChassis == null)
			{
				allChassis = this.GetAllUniqueOwnedChassis();
			}
			for (int i = list.Count - 1; i >= 0; i--)
			{
				if (allChassis.Contains(list[i]))
				{
					list.RemoveAt(i);
				}
			}
			return list.Count == 0;
		}

		// Token: 0x06009200 RID: 37376 RVA: 0x0025DEFC File Offset: 0x0025C0FC
		private float CareerMode_GetAllChassisScore()
		{
			float num = 1f;
			this.GetAllUniqueOwnedChassis();
			foreach (WeightClass weightClass in this.allAcquirableMechs.Keys)
			{
				if (!this.DidCompleteWeightClass(weightClass, null))
				{
					num = 0f;
					break;
				}
			}
			return (float)Mathf.FloorToInt(num * this.Constants.CareerMode.AllChassisScorePerUnit);
		}

		// Token: 0x06009201 RID: 37377 RVA: 0x0025DF84 File Offset: 0x0025C184
		private float CareerMode_GetWeightScore()
		{
			float num = 0f;
			foreach (WeightClass weightClass in this.allAcquirableMechs.Keys)
			{
				if (this.DidCompleteWeightClass(weightClass, null))
				{
					num += 1f;
				}
			}
			return (float)Mathf.FloorToInt(num * this.Constants.CareerMode.WeightScorePerUnit);
		}

		// Token: 0x06009202 RID: 37378 RVA: 0x0025E008 File Offset: 0x0025C208
		private float CareerMode_GetChassisPoints()
		{
			return (float)Mathf.FloorToInt((float)this.GetAllUniqueOwnedChassis().Count * this.Constants.CareerMode.ChassisPointsPerUnit);
		}

		// Token: 0x06009203 RID: 37379 RVA: 0x0025E02D File Offset: 0x0025C22D
		private float CareerMode_GetAllSystemsScore()
		{
			return (float)Mathf.FloorToInt((float)(((float)this.VisitedStarSystems.Count >= this.Constants.CareerMode.SystemsVisitedTarget) ? 1 : 0) * this.Constants.CareerMode.AllSystemsScorePerUnit);
		}

		// Token: 0x06009204 RID: 37380 RVA: 0x0025E069 File Offset: 0x0025C269
		private float CareerMode_GetSystemsVisitedScore()
		{
			return (float)Mathf.FloorToInt((float)this.VisitedStarSystems.Count * this.Constants.CareerMode.SystemsVisitedScorePerUnit);
		}

		// Token: 0x06009205 RID: 37381 RVA: 0x0025E090 File Offset: 0x0025C290
		private float GetIndividualRepScore(bool positive, float perUnitMod)
		{
			float num = 0f;
			foreach (FactionValue factionValue in FactionEnumeration.FactionList.FindAll((FactionValue faction) => faction.IsCareerScoringFaction))
			{
				int rawReputation = this.GetRawReputation(factionValue);
				if ((positive && rawReputation > 0) || (!positive && rawReputation < 0))
				{
					num += (float)Mathf.Abs(rawReputation);
				}
			}
			return (float)Mathf.FloorToInt(num * perUnitMod);
		}

		// Token: 0x06009206 RID: 37382 RVA: 0x0025E130 File Offset: 0x0025C330
		private float CareerMode_GetPositiveReputationScore()
		{
			return this.GetIndividualRepScore(true, this.Constants.CareerMode.PositiveReputationScorePerUnit);
		}

		// Token: 0x06009207 RID: 37383 RVA: 0x0025E149 File Offset: 0x0025C349
		private float CareerMode_GetNegativeReputationScore()
		{
			return this.GetIndividualRepScore(false, this.Constants.CareerMode.NegativeReputationScorePerUnit);
		}

		// Token: 0x06009208 RID: 37384 RVA: 0x0025E164 File Offset: 0x0025C364
		private float CareerMode_GetMaxedReputationScore()
		{
			float num = 0f;
			foreach (FactionValue factionValue in FactionEnumeration.FactionList.FindAll((FactionValue faction) => faction.IsCareerScoringFaction))
			{
				if ((float)Mathf.Abs(this.GetRawReputation(factionValue)) == this.Constants.Story.MaxReputation)
				{
					num += 1f;
				}
			}
			return (float)Mathf.FloorToInt(num * this.Constants.CareerMode.MaxedReputationScorePerUnit);
		}

		// Token: 0x06009209 RID: 37385 RVA: 0x0025E218 File Offset: 0x0025C418
		private float CareerMode_GetMaxMRBScore()
		{
			return (float)Mathf.FloorToInt((float)((this.GetCurrentMRBLevel() >= this.GetMaxMRBLevel()) ? 1 : 0) * this.Constants.CareerMode.MaxMRBScorePerUnit);
		}

		// Token: 0x0600920A RID: 37386 RVA: 0x0025E244 File Offset: 0x0025C444
		private float CareerMode_GetMoraleScore()
		{
			return (float)Mathf.FloorToInt((float)this.Morale * this.Constants.CareerMode.MoraleScorePerUnit);
		}

		// Token: 0x0600920B RID: 37387 RVA: 0x0025E264 File Offset: 0x0025C464
		private float CareerMode_GetMRBScore()
		{
			FactionValue mercenaryReviewBoardFactionValue = FactionEnumeration.GetMercenaryReviewBoardFactionValue();
			return (float)Mathf.FloorToInt((float)this.GetRawReputation(mercenaryReviewBoardFactionValue) * this.Constants.CareerMode.MRBScorePerUnit);
		}

		// Token: 0x0600920C RID: 37388 RVA: 0x0025E298 File Offset: 0x0025C498
		private float CareerMode_GetPilotExperienceScore()
		{
			float num = 0f;
			for (int i = 0; i < this.PilotRoster.Count; i++)
			{
				num += (float)this.PilotRoster[i].TotalXP;
			}
			return (float)Mathf.FloorToInt(num * this.Constants.CareerMode.PilotExperienceScorePerUnit);
		}

		// Token: 0x0600920D RID: 37389 RVA: 0x0024EFD4 File Offset: 0x0024D1D4
		public SimGameInterruptManager GetInterruptQueue()
		{
			return this.interruptQueue;
		}

		// Token: 0x0600920E RID: 37390 RVA: 0x0025E2EE File Offset: 0x0025C4EE
		public void MemorialWallShortcut()
		{
			this.RoomManager.SetQueuedUIActivationID(DropshipMenuType.MemorialWall, DropshipLocation.UNKNOWN, false);
			this.RoomManager.ForceShipRoomChangeOfRoom(DropshipLocation.BARRACKS);
		}

		// Token: 0x0600920F RID: 37391 RVA: 0x0025E30C File Offset: 0x0025C50C
		public void ClearMapDiscardPile()
		{
			if (this.mapDiscardPile != null)
			{
				this.mapDiscardPile.Clear();
			}
		}

		// Token: 0x06009210 RID: 37392 RVA: 0x0025E324 File Offset: 0x0025C524
		public void RemoveMapsFromMapDiscardInSystem(StarSystem oldSystem)
		{
			if (oldSystem == null)
			{
				return;
			}
			foreach (Contract contract in oldSystem.SystemContracts)
			{
				if (oldSystem.ID == contract.TargetSystem)
				{
					this.mapDiscardPile.Remove(contract.mapName);
				}
			}
		}

		// Token: 0x06009211 RID: 37393 RVA: 0x0025E39C File Offset: 0x0025C59C
		private bool HasInitStateBits(SimGameState.InitStates initStateMask)
		{
			return this.HasInitStateBits(this.initState, initStateMask);
		}

		// Token: 0x06009212 RID: 37394 RVA: 0x0025E3AB File Offset: 0x0025C5AB
		private bool HasInitStateBits(SimGameState.InitStates bits, SimGameState.InitStates bitMask)
		{
			return (bits & bitMask) == bitMask;
		}

		// Token: 0x06009213 RID: 37395 RVA: 0x0025E3B3 File Offset: 0x0025C5B3
		private void SetInitStateBits(SimGameState.InitStates mask)
		{
			this.initState |= mask;
			this.OnStateBitsChanged();
		}

		// Token: 0x06009214 RID: 37396 RVA: 0x0025E3C9 File Offset: 0x0025C5C9
		private void RemoveInitStateBits(SimGameState.InitStates mask)
		{
			this.initState = ~mask & this.initState;
			this.OnStateBitsChanged();
		}

		// Token: 0x06009215 RID: 37397 RVA: 0x0025E3E0 File Offset: 0x0025C5E0
		private void OnStateBitsChanged()
		{
			SimGameState.InitStates initStates = this.previousInitState;
			this.previousInitState = this.initState;
			if (!this.HasInitStateBits(initStates, SimGameState.InitStates.REQUEST_AUTO_HEADLESS_STATE_ON_READY) && this.HasInitStateBits(SimGameState.InitStates.REQUEST_AUTO_HEADLESS_STATE_ON_READY))
			{
				bool flag = this._OnHeadlessComplete();
				if (flag)
				{
					this.SetInitStateBits(SimGameState.InitStates.HEADLESS_ON_READY_SUCCESS);
				}
				try
				{
					this.MessageCenter.PublishMessage(new SimGameHeadless(flag));
				}
				catch (Exception ex)
				{
					SimGameState.logger.LogError("[OnHeadlessStateComplete] Listener threw an exception! " + ex.ToString());
				}
			}
			if (!this.HasInitStateBits(initStates, SimGameState.InitStates.UX_SYSTEMS_CREATED) && this.HasInitStateBits(SimGameState.InitStates.UX_SYSTEMS_CREATED))
			{
				this._OnAttachUXComplete();
				this.SetInitStateBits(SimGameState.InitStates.UX_ATTACHED_PREVIOUSLY);
				this.RemoveInitStateBits(SimGameState.InitStates.ASYNC_ATTACHING_UX_STATE);
				try
				{
					this.MessageCenter.PublishMessage(new SimGameUXAttached(true));
				}
				catch (Exception ex2)
				{
					SimGameState.logger.LogError("[OnHeadAttachedStateComplete] Listener threw an exception! " + ex2.ToString());
				}
			}
			if (this.HasInitStateBits(SimGameState.InitStates.REQUEST_DEFS_LOAD) && this.HasInitStateBits(SimGameState.InitStates.INITIALIZED))
			{
				this.RespondToDefsLoadRequest();
			}
			if (this.HasInitStateBits(SimGameState.InitStates.REQUEST_ATTACH_UX_STATE) && this.HasInitStateBits(SimGameState.InitStates.HEADLESS_STATE))
			{
				this.RespondToHeadAttachRequest();
			}
		}

		// Token: 0x17001981 RID: 6529
		// (get) Token: 0x06009216 RID: 37398 RVA: 0x0025E504 File Offset: 0x0025C704
		public bool IsFromSave
		{
			get
			{
				return this.HasInitStateBits(SimGameState.InitStates.FROM_SAVE);
			}
		}

		// Token: 0x06009217 RID: 37399 RVA: 0x0025E510 File Offset: 0x0025C710
		private void _OnInit(GameInstance game, SimGameDifficulty difficulty)
		{
			this.BattleTechGame = game;
			this.MessageCenter.AddSubscriber(MessageCenterMessageType.SimGameHeadless, new ReceiveMessageCenterMessage(this.OnHeadlessCompleteListner));
			this.MessageCenter.AddSubscriber(MessageCenterMessageType.SimGameUXAttached, new ReceiveMessageCenterMessage(this.OnHeadAttachedStateCompleteListener));
			Array values = Enum.GetValues(typeof(MilitaryAlphabet));
			this.DebugSeed = string.Format("{0}_{1}_{2}", this.COLORS[this.NetworkRandom.Int(0, this.COLORS.Length)], values.GetValue(this.NetworkRandom.Int(0, values.Length)), this.ANIMALS[this.NetworkRandom.Int(0, this.ANIMALS.Length)]);
			this.DebugSeedFolder = string.Format("{0}_{1}", this.DebugSeed, DateTime.Now.ToString("yyyyMMddHHmmss"));
			this.ContractScope = ContractScope.STANDARD;
			this.Constants = SimGameConstants.GetInstance(this.BattleTechGame);
			this.CombatConstants = CombatGameConstants.GetInstance(this.BattleTechGame);
			this.constantOverrides = new SimGameConstantOverride();
			if (difficulty != null)
			{
				this.difficultySettings = difficulty;
			}
			else
			{
				this.difficultySettings = new SimGameDifficulty();
			}
			this.ExpenditureMoraleValue = new Dictionary<EconomyScale, int>();
			this.ExpenditureMoraleValue.Add(EconomyScale.Spartan, this.Constants.Story.SpartanMoraleModifier);
			this.ExpenditureMoraleValue.Add(EconomyScale.Restrictive, this.Constants.Story.RestrictedMoraleModifier);
			this.ExpenditureMoraleValue.Add(EconomyScale.Normal, this.Constants.Story.NormalMoraleModifier);
			this.ExpenditureMoraleValue.Add(EconomyScale.Generous, this.Constants.Story.GenerousMoraleModifier);
			this.ExpenditureMoraleValue.Add(EconomyScale.Extravagant, this.Constants.Story.ExtravagantMoraleModifier);
			this.HabitableTags.Clear();
			this.HabitableTags.AddRange(this.Constants.Travel.HabitablePlanetTags);
			this.AlreadyClickedConversationResponses = new List<string>();
			this.PopulateCareerModeScoringDictionaries();
		}

		// Token: 0x06009218 RID: 37400 RVA: 0x0025E710 File Offset: 0x0025C910
		private void _OnFirstPlayInit(SimGameState.SimGameType gameType, bool allowDebug)
		{
			bool flag = false;
			List<string> list = new List<string>();
			this.InitManagers(null);
			this.InstanceGUID = ((gameType != SimGameState.SimGameType.KAMEA_CAMPAIGN) ? "non_campaign_instance_" : "campaign_instance_") + Guid.NewGuid().ToString();
			this.AllowDebug = allowDebug;
			this.SimGameMode = gameType;
			this.MedTechs = new List<TechDef>();
			this.MechTechs = new List<TechDef>();
			this.MechLabQueue = new List<WorkOrderEntry>();
			this.constantOverrides.Initialize(this);
			StoryConstantsDef story = this.Constants.Story;
			switch (gameType)
			{
			case SimGameState.SimGameType.CAREER:
				this.SetCampaignStartDate(this.Constants.CareerMode.GetCampaignStartDate());
				goto IL_C9;
			}
			this.SetCampaignStartDate(this.Constants.Story.GetCampaignStartDate());
			IL_C9:
			this.companyEventTracker.Init(SimGameState.CompanyTrackerScope, story.CompanyEventStartingChance, story.CompanyEventIncreaseRate, SimGameEventDef.SimEventType.NORMAL, this);
			this.mechWarriorEventTracker.Init(SimGameState.MechWarriorTrackerScope, story.PersonalEventStartingChance, story.PersonalEventIncreaseRate, SimGameEventDef.SimEventType.NORMAL, this);
			this.moraleEventTracker.Init(SimGameState.MoraleTrackerScope, story.MoraleEventStartingChance, story.MoraleEventIncreaseRate, SimGameEventDef.SimEventType.MORALE, this);
			this.deadEventTracker.Init(SimGameState.DeadTrackerScope, story.DeadEventStartingChance, story.DeadEventIncreaseRate, SimGameEventDef.SimEventType.FUNERAL, this);
			this.Context = new SimGameContext(this.BattleTechGame.GlobalGameContext);
			this.Context.SetObject(GameContextObjectTagEnum.Company, this);
			if (gameType != SimGameState.SimGameType.KAMEA_CAMPAIGN)
			{
				List<string> list2 = new List<string>();
				if (gameType != SimGameState.SimGameType.CAREER)
				{
					if (gameType == SimGameState.SimGameType.NONE)
					{
						list2.AddRange(this.Constants.Debug.NonCampaignStartingTags);
						this.CompanyTags.Add(this.Constants.Flashpoints.SystemUseFlashpointsTag);
					}
				}
				else
				{
					list2.AddRange(this.Constants.CareerMode.CareerStartingTags);
					this.careerModeFlashpointStartDate = this.NetworkRandom.Int(this.Constants.CareerMode.FlashpointStartRange[0], this.Constants.CareerMode.FlashpointStartRange[1] + 1);
					flag = true;
					list.AddRange(this.Constants.CareerMode.ConversationCharacters);
				}
				foreach (string text in list2)
				{
					this.CompanyTags.Add(text);
				}
				this.CompanyTags.Add(this.Constants.Story.SystemUseEventsTag);
				this.CompanyTags.Add(this.Constants.Story.SystemUseTimeTag);
			}
			else
			{
				this.CompanyTags.Add(this.Constants.Story.SystemUseMilestoneTag);
			}
			if (this.Constants.Story.CampaignCommanderUpdateTags != null)
			{
				foreach (string text2 in this.Constants.Story.CampaignCommanderUpdateTags)
				{
					this.CompanyTags.Add(text2);
				}
			}
			this.CompanyTags.Add(string.Format("{0}{1}", "SYSTEM_GAMEMODE_", this.SimGameMode));
			this.InitCompanyStats();
			this.DayRemainingInQuarter = this.Constants.Finances.QuarterLength;
			this.SetExpenditureLevel(EconomyScale.Normal, false);
			foreach (object obj in Enum.GetValues(typeof(SimGameState.SimGameCharacterType)))
			{
				SimGameState.SimGameCharacterType simGameCharacterType = (SimGameState.SimGameCharacterType)obj;
				this.characterList.Add(simGameCharacterType);
				if (flag)
				{
					bool flag2 = false;
					if (list.Contains(simGameCharacterType.ToString()))
					{
						flag2 = true;
					}
					this.characterStatus.Add(flag2);
				}
				else
				{
					this.characterStatus.Add(true);
				}
			}
			this.AddCachedFactionsToDisplayList();
			ReportSimGameInitMessage reportSimGameInitMessage = new ReportSimGameInitMessage(false);
			this.MessageCenter.PublishMessage(reportSimGameInitMessage);
		}

		// Token: 0x06009219 RID: 37401 RVA: 0x0025EB20 File Offset: 0x0025CD20
		public void AddCachedFactionsToDisplayList()
		{
			this.displayedFactions.Clear();
			foreach (FactionValue factionValue in FactionEnumeration.GetStartingDisplayFactionList(this.IsCareerMode()))
			{
				this.displayedFactions.Add(factionValue.Name);
			}
		}

		// Token: 0x0600921A RID: 37402 RVA: 0x0025EB90 File Offset: 0x0025CD90
		private void _OnInitFromSave(GameInstanceSave save)
		{
			this.InitManagers(save);
			this.InstanceGUID = save.InstanceGUID;
			this.save = save;
			if (save.SaveReason == SaveReason.SIM_GAME_FIRST_SAVE)
			{
				this.SetInitStateBits(SimGameState.InitStates.UPDATE_MILESTONE_ON_SAVE_LOADED);
			}
			ReportSimGameInitMessage reportSimGameInitMessage = new ReportSimGameInitMessage(true);
			this.MessageCenter.PublishMessage(reportSimGameInitMessage);
		}

		// Token: 0x0600921B RID: 37403 RVA: 0x0025EBDF File Offset: 0x0025CDDF
		private void _OnBeginDefsLoad()
		{
			this.RequestDataManagerResources();
		}

		// Token: 0x0600921C RID: 37404 RVA: 0x0025EBE7 File Offset: 0x0025CDE7
		private void _OnDefsLoadComplete()
		{
			this.InitializeDataFromDefs();
			if (!this.HasInitStateBits(SimGameState.InitStates.FROM_SAVE))
			{
				this.FirstTimeInitializeDataFromDefs();
			}
		}

		// Token: 0x0600921D RID: 37405 RVA: 0x0025EC00 File Offset: 0x0025CE00
		private bool _OnHeadlessComplete()
		{
			if (!this.HandleSaveHydrate())
			{
				return false;
			}
			this.InitializeFactionStoreDictionary();
			if (this.AllowDebug)
			{
				this.MaxActiveFlashpoints = this.Constants.Debug.MaxActiveFlashpoints;
				this.InitialFlashpointMinCooldown = this.Constants.Debug.InitialFlashpointMinCooldown;
				this.InitialFlashpointMaxCooldown = this.Constants.Debug.InitialFlashpointMaxCooldown;
				this.FlashpointMinCooldown = this.Constants.Debug.FlashpointMinCooldown;
				this.FlashpointMaxCooldown = this.Constants.Debug.FlashpointMaxCooldown;
				this.MaxGenFlashpointsPerDay = this.Constants.Debug.MaxGenFlashpointsPerDay;
			}
			else
			{
				this.MaxActiveFlashpoints = this.Constants.Flashpoints.MaxActiveFlashpoints;
				this.InitialFlashpointMinCooldown = this.Constants.Flashpoints.InitialFlashpointMinCooldown;
				this.InitialFlashpointMaxCooldown = this.Constants.Flashpoints.InitialFlashpointMaxCooldown;
				this.FlashpointMinCooldown = this.Constants.Flashpoints.FlashpointMinCooldown;
				this.FlashpointMaxCooldown = this.Constants.Flashpoints.FlashpointMaxCooldown;
				this.MaxGenFlashpointsPerDay = this.Constants.Flashpoints.MaxGenFlashpointsPerDay;
			}
			this.ReportDay();
			return true;
		}

		// Token: 0x0600921E RID: 37406 RVA: 0x0025ED3C File Offset: 0x0025CF3C
		private void _OnBeginAttachUX()
		{
			this.DataManager.Clear(false, false, true, true, false);
			LevelLoader.LoadScene("SimGame", null);
			ActiveOrDefaultSettings.CloudSettings.CustomUnitsAndLances.UnMountMemoryStore(this.DataManager);
		}

		// Token: 0x0600921F RID: 37407 RVA: 0x0025ED70 File Offset: 0x0025CF70
		private void _OnAttachUXComplete()
		{
			this.RoomManager.SetHeaderCompanyName(this.Player1sMercUnitHeraldryDef.Description.Name);
			this.RoomManager.SetHeaderCompanyCrest(this.Player1sMercUnitHeraldryDef.textureLogoID);
			this.CameraController.SetColors(this.Player1sMercUnitHeraldryDef);
			if (this.IsFromSave)
			{
				foreach (WorkOrderEntry workOrderEntry in this.MechLabQueue)
				{
					if (workOrderEntry != null)
					{
						this.RoomManager.AddWorkQueueEntry(workOrderEntry);
					}
				}
				this.RefreshInjuries();
			}
			this.CurRoomState = DropshipLocation.UNKNOWN;
			this.RoomManager.OnSimGameReady();
			bool flag = this.HasInitStateBits(SimGameState.InitStates.FROM_SAVE);
			bool flag2 = !this.HasInitStateBits(SimGameState.InitStates.UX_ATTACHED_PREVIOUSLY);
			if (flag2)
			{
				this.SetCurrentSystem(this.CurSystem, !flag, false);
				this.SetSimRoomState(DropshipLocation.NONE);
			}
			if (flag2 && !flag)
			{
				this.AddMorale(this.Constants.Story.StartingMorale, "Start Game");
				this.AddFunds(this.Constants.Story.StartingCBills, null, true, false);
				this.SetSimShip(DropshipType.INVALID_UNSET);
				switch (this.SimGameMode)
				{
				case SimGameState.SimGameType.KAMEA_CAMPAIGN:
					this.SetSimRoomState(DropshipLocation.NONE);
					break;
				case SimGameState.SimGameType.CAREER:
					this.SetSimShip(DropshipType.Argo);
					this.SetSimRoomState(DropshipLocation.NONE);
					break;
				case SimGameState.SimGameType.NONE:
					this.SetSimShip(DropshipType.Argo);
					this.SetSimRoomState(DropshipLocation.SHIP);
					break;
				}
				this.HasSimShipBeenSet = true;
				if (this.CurDropship != DropshipType.INVALID_UNSET)
				{
					for (int i = 0; i < this.characterList.Count; i++)
					{
						this.SetCharacterVisibility(this.characterList[i], this.characterStatus[i]);
					}
				}
			}
			else
			{
				if (this.IsFromSave && this.savedTravelData != null)
				{
					if (this.savedTravelData.HasPathData)
					{
						this.Starmap.SetActivePathFromLoad(this.savedTravelData.Path, this.savedTravelData.TravelIndex);
					}
					if (this.savedTravelData.HasTravelStatus)
					{
						this.TravelManager.ForceTravelState(this.savedTravelData.TravelStatus);
					}
				}
				this.savedTravelData = null;
				this.SetSimRoomState(DropshipLocation.NONE);
				this.SetSimShip(this.CurDropship);
				this.HasSimShipBeenSet = true;
				foreach (KeyValuePair<ArgoController.RepairStateLocations, int> keyValuePair in this.argoLocationRepairStates)
				{
					this.SpaceController.argo.SetDamageStates(keyValuePair.Key, keyValuePair.Value);
				}
				if (this.SpaceController.currentShip != DropshipType.INVALID_UNSET && this.CompletedContract == null)
				{
					this.SetSimRoomState(DropshipLocation.SHIP);
				}
				else if (this.CompletedContract != null)
				{
					this.TravelManager.ForceTravelState(SimGameTravelStatus.IN_SYSTEM);
				}
				for (int j = 0; j < this.characterList.Count; j++)
				{
					this.SetCharacterVisibility(this.characterList[j], this.characterStatus[j]);
				}
			}
			if (this.FinancialReportNotification != null)
			{
				this.RoomManager.RemoveWorkQueueEntry(this.FinancialReportNotification, false);
			}
			this.FinancialReportNotification = new WorkOrderEntry_Notification(WorkOrderType.FinancialReport, Strings.T("Financial Report"), Strings.T("Financial Report"), "");
			this.FinancialReportNotification.SetCost(this.DayRemainingInQuarter);
			this.FinancialReportItem = this.RoomManager.AddWorkQueueEntry(this.FinancialReportNotification);
			this.RoomManager.RefreshTimeline(false);
			if (this.BattleTechGame == null)
			{
				Debug.LogError("[_OnAttachUXComplete] BattleTechGame NULL");
			}
			if (this.CurSystem == null)
			{
				Debug.LogError("[_OnAttachUXComplete] CurSystem NULL");
			}
			if (this.CameraController == null)
			{
				Debug.LogError("[_OnAttachUXComplete] CameraController NULL");
			}
			if (this.Starmap == null)
			{
				Debug.LogError("[_OnAttachUXComplete] Starmap NULL");
			}
			if (!this.RoomManager.CheckReady())
			{
				Debug.LogError("[_OnAttachUXComplete] RoomManager not ready!");
			}
			bool flag3 = this.HasInitStateBits(SimGameState.InitStates.UPDATE_MILESTONE_ON_SAVE_LOADED);
			this.RemoveInitStateBits(SimGameState.InitStates.UPDATE_MILESTONE_ON_SAVE_LOADED);
			if (flag2)
			{
				if (this.SimGameMode == SimGameState.SimGameType.CAREER)
				{
					this.OnCareerModeStart();
				}
				else if (this.AllowDebug)
				{
					this.DebugWidget.Show();
				}
				else
				{
					flag3 = true;
				}
			}
			if (this.IsFromSave)
			{
				this.RemoveInitStateBits(SimGameState.InitStates.FROM_SAVE);
				this.VersionUpdateCheck();
			}
			if (flag3)
			{
				this.UpdateMilestones();
			}
		}

		// Token: 0x06009220 RID: 37408 RVA: 0x0025F1DC File Offset: 0x0025D3DC
		private void _OnBeginDetatchUX()
		{
			if (this.HasInitStateBits(SimGameState.InitStates.ASYNC_ATTACHING_UX_STATE))
			{
				SimGameState.logger.LogError("Can't detatch head during the attaching head state.");
				return;
			}
			if (this.HasInitStateBits(SimGameState.InitStates.REQUEST_ATTACH_UX_STATE))
			{
				SimGameState.logger.Log("Detatch cancels request attach head");
				this.RemoveInitStateBits(SimGameState.InitStates.REQUEST_ATTACH_UX_STATE);
				return;
			}
			if (!this.HasInitStateBits(SimGameState.InitStates.UX_SYSTEMS_CREATED))
			{
				SimGameState.logger.Log("Head already not attached");
				return;
			}
			this.RemoveInitStateBits(SimGameState.InitStates.UX_SYSTEMS_CREATED);
			this.interruptQueue.ClearAll();
			this.RoomManager.OnSimGameHidden();
			ActiveOrDefaultSettings.CloudSettings.CustomUnitsAndLances.MountMemoryStore(this.DataManager);
			LazySingletonBehavior<UIManager>.Instance.SetUIRootInteractable(true);
			LazySingletonBehavior<UIManager>.Instance.PoolAllModules();
			this.Starmap = null;
			this.BattleSimPanel = null;
			this.DialogPanel = null;
			this.DebugWidget = null;
			this.TitleOverlay = null;
			this.HasSimShipBeenSet = false;
		}

		// Token: 0x06009221 RID: 37409 RVA: 0x0025F2B8 File Offset: 0x0025D4B8
		private void _OnDestroy()
		{
			SimGameDestroyedMessage simGameDestroyedMessage = new SimGameDestroyedMessage();
			this.MessageCenter.PublishMessage(simGameDestroyedMessage);
			this.MessageCenter.RemoveSubscriber(MessageCenterMessageType.SimGameHeadless, new ReceiveMessageCenterMessage(this.OnHeadlessCompleteListner));
			this.MessageCenter.RemoveSubscriber(MessageCenterMessageType.SimGameUXAttached, new ReceiveMessageCenterMessage(this.OnHeadAttachedStateCompleteListener));
			this.ClearStaticSimGameResources();
		}

		// Token: 0x06009222 RID: 37410 RVA: 0x0025F315 File Offset: 0x0025D515
		private void ClearStaticSimGameResources()
		{
			SimGameCharacter.ClearStaticData();
			SimGameCameraController.ClearStaticData();
			SimGameState_Debug.ClearStaticData();
			SimGameConstants.ClearGameState();
		}

		// Token: 0x06009223 RID: 37411 RVA: 0x0025F32C File Offset: 0x0025D52C
		private void RequestDataManagerResources()
		{
			LoadRequest loadRequest = this.DataManager.CreateLoadRequest(new Action<LoadRequest>(this.RespondToDefsLoadComplete), true);
			loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.SimGameEventDef, new bool?(true));
			loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.StarSystemDef, new bool?(false));
			loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.ContractOverride, new bool?(true));
			loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.SimGameStringList, new bool?(false));
			loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.LifepathNodeDef, new bool?(false));
			loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.ShopDef, new bool?(false));
			loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.FactionDef, new bool?(true));
			loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.FlashpointDef, new bool?(true));
			foreach (string text in this.Constants.Story.StartingMechWarriors)
			{
				loadRequest.AddBlindLoadRequest(BattleTechResourceType.PilotDef, text, new bool?(false));
			}
			foreach (string text2 in this.Constants.Story.StartingMechWarriorPortraits)
			{
				loadRequest.AddBlindLoadRequest(BattleTechResourceType.PortraitSettings, text2, new bool?(false));
			}
			loadRequest.AddBlindLoadRequest(BattleTechResourceType.PilotDef, this.Constants.Story.DefaultCommanderID, new bool?(false));
			loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.MechDef, new bool?(true));
			loadRequest.AddBlindLoadRequest(BattleTechResourceType.Sprite, "uixTxrIcon_atlas", new bool?(false));
			loadRequest.AddBlindLoadRequest(BattleTechResourceType.Sprite, "uixTxrIcon_atlas", new bool?(false));
			loadRequest.AddBlindLoadRequest(BattleTechResourceType.Sprite, "uixTxrIcon_atlas", new bool?(false));
			loadRequest.AddBlindLoadRequest(BattleTechResourceType.Sprite, "uixTxrIcon_atlas", new bool?(false));
			loadRequest.AddBlindLoadRequest(BattleTechResourceType.Sprite, "uixTxrIcon_inOrbit", new bool?(false));
			loadRequest.AddBlindLoadRequest(BattleTechResourceType.Sprite, "uixTxrIcon_roomArgo", new bool?(false));
			loadRequest.AddBlindLoadRequest(BattleTechResourceType.Sprite, "uixTxrIcon_atlas", new bool?(false));
			loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.HeraldryDef, new bool?(false));
			loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.SimGameConversations, new bool?(false));
			loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.CastDef, new bool?(false));
			loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.SimGameSpeakers, new bool?(false));
			loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.BackgroundDef, new bool?(false));
			loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.SimGameMilestoneDef, new bool?(false));
			loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.SimGameStatDescDef, new bool?(false));
			loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.AbilityDef, new bool?(false));
			loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.ShipModuleUpgrade, new bool?(false));
			loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.SimGameSubstitutionListDef, new bool?(false));
			loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.PortraitSettings, new bool?(false));
			loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.AudioEventDef, new bool?(false));
			loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.WeaponDef, new bool?(false));
			loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.AmmunitionDef, new bool?(false));
			loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.AmmunitionBoxDef, new bool?(false));
			loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.HeatSinkDef, new bool?(false));
			loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.UpgradeDef, new bool?(false));
			loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.BaseDescriptionDef, new bool?(false));
			loadRequest.AddBlindLoadRequest(BattleTechResourceType.SimGameDifficultySettingList, "DifficultySettings", new bool?(false));
			loadRequest.AddBlindLoadRequest(BattleTechResourceType.SimGameDifficultySettingList, "CareerDifficultySettings", new bool?(false));
			foreach (object obj in Enum.GetValues(typeof(SimGameCrew)))
			{
				string text3 = string.Format("{0}{1}{2}", "castDef_", ((SimGameCrew)obj).ToString().Substring("Crew_".Length), "Default");
				loadRequest.AddBlindLoadRequest(BattleTechResourceType.CastDef, text3, new bool?(false));
			}
			loadRequest.AddBlindLoadRequest(BattleTechResourceType.MechDef, this.Constants.Story.StartingPlayerMech, new bool?(false));
			loadRequest.AddBlindLoadRequest(BattleTechResourceType.GenderedOptionsListDef, this.Constants.Pilot.PilotPortraits, new bool?(false));
			loadRequest.AddBlindLoadRequest(BattleTechResourceType.GenderedOptionsListDef, this.Constants.Pilot.PilotVoices, new bool?(false));
			loadRequest.AddBlindLoadRequest(BattleTechResourceType.MechDef, this.Constants.Story.StartingPlayerMech, new bool?(false));
			loadRequest.AddBlindLoadRequest(BattleTechResourceType.GenderedOptionsListDef, this.Constants.Pilot.PilotPortraits, new bool?(false));
			loadRequest.AddBlindLoadRequest(BattleTechResourceType.GenderedOptionsListDef, this.Constants.Pilot.PilotVoices, new bool?(false));
			foreach (PilotDef_MDD pilotDef_MDD in MetadataDatabase.Instance.GetRoninPilotDefs(true))
			{
				loadRequest.AddBlindLoadRequest(BattleTechResourceType.PilotDef, pilotDef_MDD.PilotDefID, new bool?(false));
			}
			loadRequest.AddBlindLoadRequest(BattleTechResourceType.SVGAsset, "uixSvgIcon_mwrank_Ronin", new bool?(false));
			loadRequest.AddBlindLoadRequest(BattleTechResourceType.SVGAsset, "uixSvgIcon_mwrank_KSBacker", new bool?(false));
			loadRequest.AddBlindLoadRequest(BattleTechResourceType.SVGAsset, "uixSvgIcon_mwrank_Commander", new bool?(false));
			for (int j = 0; j < 5; j++)
			{
				loadRequest.AddBlindLoadRequest(BattleTechResourceType.SVGAsset, string.Format("{0}{1}{2}", "uixSvgIcon_mwrank_", "Rank", j + 1), new bool?(false));
			}
			loadRequest.AddBlindLoadRequest(BattleTechResourceType.SVGAsset, "uixSvgIcon_generic_MechPart", new bool?(false));
			VersionManifestAddendum addendumByName = this.DataManager.ResourceLocator.GetAddendumByName(this.CONVERSATION_TEXTURE_ADDENDUM);
			foreach (VersionManifestEntry versionManifestEntry in this.DataManager.ResourceLocator.AllEntriesOfResourceFromAddendum(BattleTechResourceType.Texture2D, addendumByName, false))
			{
				loadRequest.AddBlindLoadRequest(BattleTechResourceType.Texture2D, versionManifestEntry.Id, new bool?(false));
			}
			VersionManifestAddendum addendumByName2 = this.DataManager.ResourceLocator.GetAddendumByName(this.PLAYER_CREST_ADDENDUM);
			foreach (VersionManifestEntry versionManifestEntry2 in this.DataManager.ResourceLocator.AllEntriesOfResourceFromAddendum(BattleTechResourceType.Sprite, addendumByName2, false))
			{
				loadRequest.AddBlindLoadRequest(BattleTechResourceType.Sprite, versionManifestEntry2.Id, new bool?(false));
			}
			this.Player1sMercUnitHeraldryDef = this.Constants.Player1sMercUnitHeraldryDef;
			this.Player1sMercUnitHeraldryDef.RequestResources(this.DataManager, null);
			foreach (object obj2 in Enum.GetValues(typeof(SimGameState.SimGameCharacterType)))
			{
				SimGameState.SimGameCharacterType simGameCharacterType = (SimGameState.SimGameCharacterType)obj2;
				if (simGameCharacterType != SimGameState.SimGameCharacterType.UNSET)
				{
					string text4 = "TooltipSimGameCharacter" + simGameCharacterType.ToString();
					if (this.DataManager.ResourceLocator.EntryByID(text4, BattleTechResourceType.BackgroundDef, false) != null)
					{
						loadRequest.AddBlindLoadRequest(BattleTechResourceType.BaseDescriptionDef, text4, new bool?(false));
					}
				}
			}
			loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.ItemCollectionDef, new bool?(true));
			loadRequest.AddBlindLoadRequest(BattleTechResourceType.SimpleText, "careerModeAllLightChassis", new bool?(false));
			loadRequest.AddBlindLoadRequest(BattleTechResourceType.SimpleText, "careerModeAllMediumChassis", new bool?(false));
			loadRequest.AddBlindLoadRequest(BattleTechResourceType.SimpleText, "careerModeAllHeavyChassis", new bool?(false));
			loadRequest.AddBlindLoadRequest(BattleTechResourceType.SimpleText, "careerModeAllAssaultChassis", new bool?(false));
			loadRequest.AddBlindLoadRequest(BattleTechResourceType.Sprite, "uixTxrSpot_flashpointExample", new bool?(false));
			loadRequest.AddBlindLoadRequest(BattleTechResourceType.Sprite, "uixTxrSpot_StarmapV2-Example", new bool?(false));
			loadRequest.ProcessRequests(10U);
		}

		// Token: 0x06009224 RID: 37412 RVA: 0x0025FA24 File Offset: 0x0025DC24
		private void InitCompanyStats()
		{
			this.CompanyStats.AddStatistic<int>(this.Constants.Story.SystemMissionCompleteCountStat, 0);
			this.companyStats.AddStatistic<int>("Funds", 0);
			this.companyStats.AddStatistic<int>("FundsEverGained", 0);
			this.companyStats.AddStatistic<int>("TaskDuration", 0);
			this.companyStats.AddStatistic<int>("TravelTime", 99);
			this.companyStats.AddStatistic<SimGameTravelStatus>("Travel", SimGameTravelStatus.IN_SYSTEM);
			this.companyStats.AddStatistic<int>("UpgradeValue", this.Constants.Story.DailyUpgradeValue);
			this.companyStats.AddStatistic<int>("MechTechSkill", this.Constants.Story.StartingMechTechSkill);
			this.companyStats.AddStatistic<int>("MedTechSkill", this.Constants.Story.StartingMedTechSkill);
			this.companyStats.AddStatistic<int>("ExperiencePerDay", this.Constants.Story.StartingPassiveXPGain);
			this.companyStats.AddStatistic<int>("ExperiencePerDayCap", this.Constants.Story.StartingPassiveXPGainCap);
			SimGameState.SimGameType simGameMode = this.SimGameMode;
			float num;
			if (simGameMode == SimGameState.SimGameType.CAREER)
			{
				num = (float)this.Constants.CareerMode.StartingGlobalContractDifficulty;
			}
			else
			{
				num = this.Constants.Story.StartingGlobalContractDifficulty;
			}
			this.companyStats.AddStatistic<float>("Difficulty", num);
			this.companyStats.AddStatistic<string>("ShipType", DropshipType.INVALID_UNSET.ToString());
			this.companyStats.AddStatistic<int>(this.Constants.Story.MechBayPodsID, this.Constants.Story.StartingMechPods);
			this.companyStats.AddStatistic<int>(this.Constants.Story.BarracksPodsID, this.Constants.Story.StartingBarrackPods);
			this.companyStats.AddStatistic<float>(this.Constants.Story.DriveTravelID, this.Constants.Story.StartingDriveMod);
			this.companyStats.AddStatistic<int>("COMPANY_MechKills", 0);
			this.companyStats.AddStatistic<int>("COMPANY_OtherKills", 0);
			this.companyStats.AddStatistic<int>("COMPANY_MissionsAttempted", 0);
			this.companyStats.AddStatistic<int>("COMPANY_MissionsSucceeded", 0);
			this.companyStats.AddStatistic<int>("COMPANY_MissionsGoodFaith", 0);
			this.companyStats.AddStatistic<int>("COMPANY_MissionFailures", 0);
			this.companyStats.AddStatistic<int>("COMPANY_MissionAggregateDifficulty", 0);
			this.companyStats.AddStatistic<int>("COMPANY_MonthlyStartingFunds", 0);
			this.companyStats.AddStatistic<int>("COMPANY_MonthlyStartingMorale", 0);
			this.companyStats.AddStatistic<int>("COMPANY_MechWarriorsHired", 0);
			this.companyStats.AddStatistic<int>("COMPANY_MechWarriorsFired", 0);
			this.companyStats.AddStatistic<int>("COMPANY_MechsAdded", 0);
			this.companyStats.AddStatistic<int>("COMPANY_NotificationViewed_LowFunds", -this.Constants.Story.NotifLowFundsRecurrence);
			this.companyStats.AddStatistic<int>("COMPANY_NotificationViewed_ArgoUpgradeNeeded", -this.Constants.Story.NotifArgoUpgradesRecurrence);
			this.companyStats.AddStatistic<int>("COMPANY_NotificationViewed_BattleMechRepairsNeeded", -this.Constants.Story.NotifMechRepairsNeededRecurrence);
			this.InitCompanyStatValidators();
			this.OnNewQuarterBegin();
		}

		// Token: 0x06009225 RID: 37413 RVA: 0x0025FD80 File Offset: 0x0025DF80
		private void InitCompanyStatValidators()
		{
			this.companyStats.SetValidator<int>("TaskDuration", new Statistic.Validator<int>(this.MinimumZeroValidator<int>));
			this.companyStats.SetValidator<int>("TravelTime", new Statistic.Validator<int>(this.MinimumZeroValidator<int>));
			this.companyStats.SetValidator<int>("MechTechSkill", new Statistic.Validator<int>(this.MinimumOneValidator<int>));
			this.companyStats.SetValidator<int>("MedTechSkill", new Statistic.Validator<int>(this.MinimumOneValidator<int>));
			this.companyStats.SetValidator<int>("UpgradeValue", new Statistic.Validator<int>(this.MinimumOneValidator<int>));
			this.companyStats.SetValidator<float>("Difficulty", new Statistic.Validator<float>(this.OneToTenRangeValidtor<float>));
			if (this.companyStats.ContainsStatistic("Morale"))
			{
				this.companyStats.SetValidator<int>("Morale", new Statistic.Validator<int>(this.MinimumZeroMaximumFiftyValidator<int>));
			}
			FactionValue mercenaryReviewBoardFactionValue = FactionEnumeration.GetMercenaryReviewBoardFactionValue();
			string repID = this.GetRepID("Reputation", mercenaryReviewBoardFactionValue);
			if (this.companyStats.ContainsStatistic(repID))
			{
				this.companyStats.SetValidator<int>(repID, new Statistic.Validator<int>(this.MinimumZeroValidator<int>));
			}
		}

		// Token: 0x06009226 RID: 37414 RVA: 0x0025FE9C File Offset: 0x0025E09C
		private void InitManagers(GameInstanceSave save = null)
		{
			this.ConversationManager = new SimGameConversationManager(this);
			this.RoomManager = new SGRoomManager(this);
			this.ItemCollectionResultGen = new ItemCollectionResultGenerator(this);
			SimGameTravelStatus simGameTravelStatus = SimGameTravelStatus.UNKNOWN;
			if (save != null)
			{
				simGameTravelStatus = (SimGameTravelStatus)save.SimGameSave.TravelStatus;
			}
			if (simGameTravelStatus != SimGameTravelStatus.UNKNOWN)
			{
				this.TravelManager = new SGTravelManager(simGameTravelStatus);
			}
			else
			{
				this.TravelManager = new SGTravelManager();
			}
			this.TravelManager.InitializeStateMachine(this);
		}

		// Token: 0x06009227 RID: 37415 RVA: 0x0025FF08 File Offset: 0x0025E108
		private bool HandleSaveHydrate()
		{
			if (this.save == null)
			{
				return true;
			}
			try
			{
				if (DebugBridge.TestToolsEnabled && SaveGameStructure.ForceSimGameRehydrateFailure)
				{
					return false;
				}
				this.Rehydrate(this.save);
				if (this.save.SimGameSave.PreviouslyAttachedHeadState)
				{
					this.SetInitStateBits(SimGameState.InitStates.UX_ATTACHED_PREVIOUSLY);
				}
				return true;
			}
			catch (Exception ex)
			{
				Debug.LogError("Failed to Rehydrate!\n" + ex.ToString());
			}
			finally
			{
				this.save = null;
			}
			return false;
		}

		// Token: 0x06009228 RID: 37416 RVA: 0x0025FF9C File Offset: 0x0025E19C
		private void InitializeFactionStoreDictionary()
		{
			this.factStoreDict.Clear();
			foreach (string text in this.factions.Keys)
			{
				string text2 = this.factions[text].factionStorePlanetTag;
				if (string.IsNullOrEmpty(text2))
				{
					text2 = "INVALID";
				}
				this.factStoreDict.Add(text, new List<StarSystem>());
				foreach (StarSystem starSystem in this.starSystems)
				{
					if (starSystem.Def.Tags.Contains(text2))
					{
						this.factStoreDict[text].Add(starSystem);
					}
				}
			}
		}

		// Token: 0x06009229 RID: 37417 RVA: 0x00260094 File Offset: 0x0025E294
		private void InitializeDataFromDefs()
		{
			this.Player1sMercUnitHeraldryDef.Refresh();
			this.RequestItem<CSVReader>("starmapStoreManifest", new Action<CSVReader>(this.OnStarmapStoreManifestLoaded), BattleTechResourceType.CSV);
			foreach (string text in this.DataManager.SystemDefs.Keys)
			{
				StarSystemDef starSystemDef = this.DataManager.SystemDefs.Get(text);
				if (starSystemDef.StartingSystemModes.Contains(this.SimGameMode))
				{
					StarSystem starSystem = new StarSystem(starSystemDef, this);
					this.starSystems.Add(starSystem);
					this.starDict.Add(starSystemDef.CoreSystemID, starSystem);
				}
			}
			foreach (string text2 in this.DataManager.Factions.Keys)
			{
				FactionDef factionDef = this.DataManager.Factions.Get(text2);
				this.factions.Add(factionDef.FactionValue.Name, factionDef);
				this.factionMissionResultStatements.AddFaction(factionDef);
			}
			this.AddCachedFactionsToDisplayList();
			this.InitializeFactionStoreDictionary();
			foreach (string text3 in this.DataManager.SimGameStringLists.Keys)
			{
				this.stringsLists.Add(this.DataManager.SimGameStringLists.Get(text3));
			}
			foreach (string text4 in this.DataManager.LifepathNodeDefs.Keys)
			{
				this.lifenodes.Add(this.DataManager.LifepathNodeDefs.Get(text4));
			}
			using (IEnumerator<string> enumerator = this.DataManager.SimGameSpeakers.Keys.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					string text5 = enumerator.Current;
					this.ConversationSpeakers = this.DataManager.SimGameSpeakers.Get(text5);
				}
			}
			foreach (object obj in Enum.GetValues(typeof(SimGameCrew)))
			{
				SimGameCrew simGameCrew = (SimGameCrew)obj;
				string text6 = string.Format("{0}{1}{2}", "castDef_", simGameCrew.ToString().Substring("Crew_".Length), "Default");
				this._crewDefs.Add(simGameCrew, this.DataManager.CastDefs.Get(text6));
			}
			foreach (object obj2 in Enum.GetValues(typeof(SimGameState.SimGameCharacterType)))
			{
				SimGameState.SimGameCharacterType simGameCharacterType = (SimGameState.SimGameCharacterType)obj2;
				string text7 = simGameCharacterType.ToString();
				if (this.Constants.Story.CrewConversationNames.Contains(text7))
				{
					this._conversationList.Add(simGameCharacterType, this.Constants.Story.CrewConversationList[this.Constants.Story.CrewConversationNames.IndexOf(text7)]);
				}
			}
			foreach (string text8 in this.DataManager.SimGameMilestones.Keys)
			{
				this.milestones.Add(this.DataManager.SimGameMilestones.Get(text8));
			}
			using (IEnumerator<string> enumerator = this.DataManager.SimGameSubstitutionDefLists.Keys.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					string text9 = enumerator.Current;
					this.SimGameSubtitutions = this.DataManager.SimGameSubstitutionDefLists.Get(text9);
				}
			}
			foreach (ContractTypeValue contractTypeValue in ContractTypeEnumeration.ContractTypeValueList)
			{
				string text10 = string.Format("{0}{1}", "ContractType", contractTypeValue.Name);
				if (this.DataManager.BaseDescriptionDefs.Exists(text10))
				{
					this._contractTypeDescriptions.Add((long)contractTypeValue.ID, this.DataManager.BaseDescriptionDefs.Get(text10));
				}
			}
			if (this.DataManager.BaseDescriptionDefs.Exists("ContractTypePriority"))
			{
				this.PriorityMissionDescription = this.DataManager.BaseDescriptionDefs.Get("ContractTypePriority");
			}
			else
			{
				SimGameState.logger.LogError("Unable to find def: ContractTypePriority");
			}
			foreach (PilotDef_MDD pilotDef_MDD in MetadataDatabase.Instance.GetRoninPilotDefs(true))
			{
				this.RoninPilots.Add(this.DataManager.PilotDefs.Get(pilotDef_MDD.PilotDefID));
			}
			this.PilotGenerator = new PilotGenerator(this, this.lifenodes, this.stringsLists);
			this.InitAbilityTree();
			if (!this.difficultySettings.Initialized || this.HasInitStateBits(SimGameState.InitStates.FROM_SAVE))
			{
				this.difficultySettings.Initialize(this, this.DataManager.SimGameDifficultySettingLists.Get("DifficultySettings"), !this.HasInitStateBits(SimGameState.InitStates.FROM_SAVE));
			}
			else
			{
				this.difficultySettings.SetSimState(this);
				if (!this.HasInitStateBits(SimGameState.InitStates.FROM_SAVE))
				{
					this.difficultySettings.ApplyAllSettings(true);
					this.difficultySettings.RefreshCareerScoreMultiplier();
				}
			}
			if (!this.companyStats.ContainsStatistic("SkipPrologue"))
			{
				this.companyStats.AddStatistic<int>("SkipPrologue", this.Constants.Story.SkipTutorial ? 1 : 0);
			}
			else
			{
				this.companyStats.ModifyStat<int>("", 0, "SkipPrologue", StatCollection.StatOperation.Set, this.Constants.Story.SkipTutorial ? 1 : 0, -1, true);
			}
			foreach (string text11 in this.DataManager.FlashpointDefs.Keys)
			{
				FlashpointDef flashpointDef = this.DataManager.FlashpointDefs.Get(text11);
				if (flashpointDef.PublishState == SimGameEventDef.EventPublishState.PUBLISHED)
				{
					this.flashpointPool.Add(flashpointDef);
					if (flashpointDef.Tags.Contains(this.Constants.Flashpoints.InitialFlashpointTag))
					{
						this.initialFlashpointPool.Add(flashpointDef);
					}
				}
			}
			this.allAcquirableMechs.Clear();
			this.allAcquirableMechs.Add(WeightClass.LIGHT, this.DataManager.SimpleTexts.Get("careerModeAllLightChassis").GetAllLines());
			this.allAcquirableMechs.Add(WeightClass.MEDIUM, this.DataManager.SimpleTexts.Get("careerModeAllMediumChassis").GetAllLines());
			this.allAcquirableMechs.Add(WeightClass.HEAVY, this.DataManager.SimpleTexts.Get("careerModeAllHeavyChassis").GetAllLines());
			this.allAcquirableMechs.Add(WeightClass.ASSAULT, this.DataManager.SimpleTexts.Get("careerModeAllAssaultChassis").GetAllLines());
		}

		// Token: 0x0600922A RID: 37418 RVA: 0x00260868 File Offset: 0x0025EA68
		private void FirstTimeInitializeDataFromDefs()
		{
			string[] array;
			switch (this.SimGameMode)
			{
			case SimGameState.SimGameType.KAMEA_CAMPAIGN:
				array = this.Constants.Story.StartingArgoUpgrades;
				goto IL_56;
			case SimGameState.SimGameType.CAREER:
				array = this.Constants.CareerMode.StartingArgoUpgrades;
				goto IL_56;
			}
			array = this.Constants.Debug.StartingArgoUpgrades;
			IL_56:
			if (array != null)
			{
				foreach (string text in array)
				{
					ShipModuleUpgrade shipModuleUpgrade = this.DataManager.ShipUpgradeDefs.Get(text);
					this.AddArgoUpgrade(shipModuleUpgrade);
				}
			}
			SimGameState.SimGameType simGameMode = this.SimGameMode;
			if (simGameMode != SimGameState.SimGameType.KAMEA_CAMPAIGN && simGameMode == SimGameState.SimGameType.CAREER)
			{
				List<PilotDef> list = new List<PilotDef>();
				while (this.PilotRoster.Count < this.Constants.CareerMode.StartingMechWarriorCount)
				{
					PilotDef pilotDef = this.PilotGenerator.GeneratePilots(1, this.Constants.CareerMode.StartingGlobalContractDifficulty, 0f, out list)[0];
					if (this.CanPilotBeCareerModeStarter(pilotDef))
					{
						pilotDef.SetDayOfHire(this.DaysPassed);
						this.AddPilotToRoster(pilotDef, false, true);
					}
				}
			}
			else
			{
				for (int j = 0; j < this.Constants.Story.StartingMechWarriors.Length; j++)
				{
					PilotDef pilotDef2 = this.DataManager.PilotDefs.Get(this.Constants.Story.StartingMechWarriors[j]);
					Pilot pilot = new Pilot(pilotDef2, this.GenerateSimGameUID(), true);
					pilot.pilotDef.PortraitSettings = this.DataManager.PortraitSettings.Get(this.Constants.Story.StartingMechWarriorPortraits[j]);
					this.PilotRoster.Add(pilot, 0);
					if (pilotDef2.IsRonin)
					{
						this.usedRoninIDs.Add(pilotDef2.Description.Id);
					}
				}
			}
			this.commander = new Pilot(this.DataManager.PilotDefs.Get(this.Constants.Story.DefaultCommanderID), this.GenerateSimGameUID(), true);
			this.commander.pilotDef.SetHiringHallStats(true, false, true, false);
			this.commander.AddExperience(0, "Starting XP", this.Constants.Story.CommanderStartingExperience);
			this.Context.SetObject(GameContextObjectTagEnum.Commander, this.commander);
			switch (this.SimGameMode)
			{
			case SimGameState.SimGameType.CAREER:
				this.AddCareerMechs();
				goto IL_277;
			case SimGameState.SimGameType.NONE:
				this.AddDebugStartingMechs();
				goto IL_277;
			}
			this.AddKameaCampaignStartingMechs();
			IL_277:
			if (this.IsCareerMode())
			{
				this.AddCareerModeIgnoredContractTargets(this.IgnoredContractTargets);
			}
			this.InitStartingPlanet_TEMP();
		}

		// Token: 0x0600922B RID: 37419 RVA: 0x00260B08 File Offset: 0x0025ED08
		private void AddCareerModeIgnoredContractTargets(List<string> ignoredContractTargetList)
		{
			List<FactionValue> list = FactionEnumeration.FactionList.FindAll((FactionValue faction) => faction.IsCareerIgnoredContractTarget);
			for (int i = 0; i < list.Count; i++)
			{
				FactionValue factionValue = list[i];
				if (!this.ignoredContractTargets.Contains(factionValue.Name))
				{
					this.ignoredContractTargets.Add(factionValue.Name);
				}
			}
		}

		// Token: 0x0600922C RID: 37420 RVA: 0x00260B7C File Offset: 0x0025ED7C
		private void AddCareerMechs()
		{
			if (this.Constants.CareerMode.StartWithRandomMechs)
			{
				this.AddRandomStartingMechs();
				return;
			}
			this.AddCareerPresetMechs();
		}

		// Token: 0x0600922D RID: 37421 RVA: 0x00260BA0 File Offset: 0x0025EDA0
		private void AddDebugStartingMechs()
		{
			List<string> list = new List<string>();
			list.Add(this.Constants.Story.StartingPlayerMech);
			list.AddRange(this.Constants.Debug.StartingDebugLance);
			this.AddMechs(list);
		}

		// Token: 0x0600922E RID: 37422 RVA: 0x00260BE8 File Offset: 0x0025EDE8
		private void AddKameaCampaignStartingMechs()
		{
			List<string> list = new List<string>();
			list.Add(this.Constants.Story.StartingPlayerMech);
			list.AddRange(this.Constants.Story.StartingLance);
			this.AddMechs(list);
		}

		// Token: 0x0600922F RID: 37423 RVA: 0x00260C30 File Offset: 0x0025EE30
		private void AddCareerPresetMechs()
		{
			List<string> list = new List<string>();
			list.Add(this.Constants.CareerMode.StartingPlayerMech);
			list.AddRange(this.Constants.CareerMode.StartingLance);
			this.AddMechs(list);
		}

		// Token: 0x06009230 RID: 37424 RVA: 0x00260C78 File Offset: 0x0025EE78
		private void AddRandomStartingMechs()
		{
			List<MechDef> list = new List<MechDef>();
			int num = 10;
			float minimumStartingWeight = this.Constants.CareerMode.MinimumStartingWeight;
			float maximumStartingWeight = this.Constants.CareerMode.MaximumStartingWeight;
			bool flag = false;
			do
			{
				list.Clear();
				num--;
				for (int i = 0; i < this.Constants.CareerMode.StartingRandomMechLists.Length; i++)
				{
					string text = this.Constants.CareerMode.StartingRandomMechLists[i];
					ItemCollectionDef itemCollectionDef = this.DataManager.ItemCollectionDefs.Get(text);
					ItemCollectionResult itemCollectionResult = this.ItemCollectionResultGen.GenerateItemCollection(itemCollectionDef, 1, delegate(ItemCollectionResult x)
					{
					}, null);
					if (itemCollectionResult == null)
					{
						SimGameState.logger.LogError(string.Format("Item Collection {0} uses Reference Type which is not expected.", text));
						flag = true;
						break;
					}
					list.Add(new MechDef(this.DataManager.MechDefs.Get(itemCollectionResult.items[0].ID), this.GenerateSimGameUID(), true));
				}
			}
			while (!MechValidationRules.LanceTonnageWithinRange(list, minimumStartingWeight, maximumStartingWeight) && num >= 0);
			if (!flag)
			{
				this.AddMechs(list);
				if (num < 0)
				{
					SimGameState.logger.LogError("Attempted to get random starting mechs but failed to keep it under minimun and maximun tonnage");
					return;
				}
			}
			else
			{
				this.AddCareerPresetMechs();
			}
		}

		// Token: 0x06009231 RID: 37425 RVA: 0x00260DC8 File Offset: 0x0025EFC8
		private void AddMechs(List<string> startingMechs)
		{
			for (int i = 0; i < startingMechs.Count; i++)
			{
				string text = startingMechs[i];
				MechDef mechDef = new MechDef(this.DataManager.MechDefs.Get(text), this.GenerateSimGameUID(), true);
				this.AddMech(i, mechDef, true, true, false, null);
			}
		}

		// Token: 0x06009232 RID: 37426 RVA: 0x00260E18 File Offset: 0x0025F018
		private void AddMechs(List<MechDef> startingMechs)
		{
			for (int i = 0; i < startingMechs.Count; i++)
			{
				this.AddMech(i, startingMechs[i], true, true, false, null);
			}
		}

		// Token: 0x06009233 RID: 37427 RVA: 0x00260E48 File Offset: 0x0025F048
		private void InitAbilityTree()
		{
			this.AbilityTree = new Dictionary<string, Dictionary<int, List<AbilityDef>>>();
			this.AbilityTree.Add("Piloting", new Dictionary<int, List<AbilityDef>>());
			for (int i = 0; i < this.Constants.Progression.PilotingSkills.Length; i++)
			{
				this.AbilityTree["Piloting"].Add(i, new List<AbilityDef>());
				string[] array = this.Constants.Progression.PilotingSkills[i];
				for (int j = 0; j < array.Length; j++)
				{
					this.AbilityTree["Piloting"][i].Add(this.DataManager.AbilityDefs.Get(array[j]));
				}
			}
			this.AbilityTree.Add("Gunnery", new Dictionary<int, List<AbilityDef>>());
			for (int k = 0; k < this.Constants.Progression.GunnerySkills.Length; k++)
			{
				this.AbilityTree["Gunnery"].Add(k, new List<AbilityDef>());
				string[] array2 = this.Constants.Progression.GunnerySkills[k];
				for (int l = 0; l < array2.Length; l++)
				{
					this.AbilityTree["Gunnery"][k].Add(this.DataManager.AbilityDefs.Get(array2[l]));
				}
			}
			this.AbilityTree.Add("Tactics", new Dictionary<int, List<AbilityDef>>());
			for (int m = 0; m < this.Constants.Progression.TacticsSkills.Length; m++)
			{
				this.AbilityTree["Tactics"].Add(m, new List<AbilityDef>());
				string[] array3 = this.Constants.Progression.TacticsSkills[m];
				for (int n = 0; n < array3.Length; n++)
				{
					this.AbilityTree["Tactics"][m].Add(this.DataManager.AbilityDefs.Get(array3[n]));
				}
			}
			this.AbilityTree.Add("Guts", new Dictionary<int, List<AbilityDef>>());
			for (int num = 0; num < this.Constants.Progression.GutsSkills.Length; num++)
			{
				this.AbilityTree["Guts"].Add(num, new List<AbilityDef>());
				string[] array4 = this.Constants.Progression.GutsSkills[num];
				for (int num2 = 0; num2 < array4.Length; num2++)
				{
					this.AbilityTree["Guts"][num].Add(this.DataManager.AbilityDefs.Get(array4[num2]));
				}
			}
		}

		// Token: 0x06009234 RID: 37428 RVA: 0x00261104 File Offset: 0x0025F304
		private void InitStartingPlanet_TEMP()
		{
			foreach (FactionDef factionDef in this.factions.Values)
			{
				this.SetReputation(factionDef.FactionValue, factionDef.GetStartingReputation(this.SimGameMode), StatCollection.StatOperation.Int_Add, null);
				this.SetInfluence(factionDef.FactionValue, factionDef.Presence, StatCollection.StatOperation.Set, null);
			}
			this.SetFactionValidators(false);
			List<StarSystem> list = new List<StarSystem>();
			for (int i = 0; i < this.starSystems.Count; i++)
			{
				if (this.starSystems[i].Def.Tags.Contains("planet_faction_directorate"))
				{
					list.Add(this.starSystems[i]);
				}
			}
			string text = this.Constants.Story.DefaultStartingSystem;
			if (this.SimGameMode == SimGameState.SimGameType.CAREER)
			{
				List<string> list2 = new List<string>(this.Constants.CareerMode.StartingSystems);
				list2.Shuffle<string>();
				text = list2[0];
			}
			else if (this.SimGameMode == SimGameState.SimGameType.KAMEA_CAMPAIGN)
			{
				text = this.Constants.Story.PrologueStartingSystem;
			}
			this.CurSystem = this.starDict[string.Format("{0}{1}", this.Constants.Travel.StarSystemPrefix, text)];
			this.TargetSystem = this.starDict[string.Format("{0}{1}", this.Constants.Travel.StarSystemPrefix, this.Constants.Story.StartingTargetSystem)];
			this.VisitSystem(this.CurSystem);
		}

		// Token: 0x06009235 RID: 37429 RVA: 0x002612AC File Offset: 0x0025F4AC
		private void RespondToDefsLoadRequest()
		{
			this.RemoveInitStateBits(SimGameState.InitStates.REQUEST_DEFS_LOAD);
			this.SetInitStateBits(SimGameState.InitStates.ASYNC_LOADING_DEFS);
			this._OnBeginDefsLoad();
		}

		// Token: 0x06009236 RID: 37430 RVA: 0x002612CA File Offset: 0x0025F4CA
		private void RespondToDefsLoadComplete(LoadRequest request)
		{
			this.RemoveInitStateBits(SimGameState.InitStates.ASYNC_LOADING_DEFS);
			this._OnDefsLoadComplete();
			this.SetInitStateBits(SimGameState.InitStates.DEFS_LOADED);
		}

		// Token: 0x06009237 RID: 37431 RVA: 0x002612E4 File Offset: 0x0025F4E4
		private void RespondToHeadAttachRequest()
		{
			this.RemoveInitStateBits(SimGameState.InitStates.REQUEST_ATTACH_UX_STATE);
			this.SetInitStateBits(SimGameState.InitStates.ASYNC_ATTACHING_UX_STATE);
			this._OnBeginAttachUX();
		}

		// Token: 0x06009238 RID: 37432 RVA: 0x002612FF File Offset: 0x0025F4FF
		private void RespondToUXSystemsCreated()
		{
			this.SetInitStateBits(SimGameState.InitStates.UX_SYSTEMS_CREATED);
		}

		// Token: 0x06009239 RID: 37433 RVA: 0x00261308 File Offset: 0x0025F508
		private void OnHeadlessCompleteListner(MessageCenterMessage message)
		{
			if ((message as SimGameHeadless).Success)
			{
				SimGameState.logger.Log("[OnHeadlessStateComplete] success!");
				return;
			}
			SimGameState.logger.LogError("[OnHeadlessStateComplete] failure!");
		}

		// Token: 0x0600923A RID: 37434 RVA: 0x00261336 File Offset: 0x0025F536
		private void OnHeadAttachedStateCompleteListener(MessageCenterMessage message)
		{
			if ((message as SimGameUXAttached).Success)
			{
				SimGameState.logger.Log("[OnHeadAttachedStateComplete] success!");
				return;
			}
			SimGameState.logger.LogError("[OnHeadAttachedStateComplete] failure!");
		}

		// Token: 0x0600923B RID: 37435 RVA: 0x00261364 File Offset: 0x0025F564
		public int GetItemCount(DescriptionDef def, Type type, SimGameState.ItemCountType outputType)
		{
			string id = def.Id;
			return this.GetItemCount(id, type, outputType);
		}

		// Token: 0x0600923C RID: 37436 RVA: 0x00261381 File Offset: 0x0025F581
		public int GetItemCount(string id, Type type, SimGameState.ItemCountType outputType)
		{
			return this.GetItemCount(this.GetItemStatID(id, type), outputType);
		}

		// Token: 0x0600923D RID: 37437 RVA: 0x00261392 File Offset: 0x0025F592
		public int GetItemCount(string id, string type, SimGameState.ItemCountType outputType)
		{
			return this.GetItemCount(this.GetItemStatID(id, type), outputType);
		}

		// Token: 0x0600923E RID: 37438 RVA: 0x002613A4 File Offset: 0x0025F5A4
		private int GetItemCount(string id, SimGameState.ItemCountType outputType)
		{
			int num = 0;
			if ((outputType == SimGameState.ItemCountType.ALL || outputType == SimGameState.ItemCountType.UNDAMAGED_ONLY) && this.CompanyStats.ContainsStatistic(id))
			{
				num += this.CompanyStats.GetValue<int>(id);
			}
			if (outputType == SimGameState.ItemCountType.ALL || outputType == SimGameState.ItemCountType.DAMAGED_ONLY)
			{
				id += string.Format(".{0}", "DAMAGED");
				if (this.CompanyStats.ContainsStatistic(id))
				{
					num += this.CompanyStats.GetValue<int>(id);
				}
			}
			return num;
		}

		// Token: 0x0600923F RID: 37439 RVA: 0x00261413 File Offset: 0x0025F613
		public SimGameState.ItemCountType GetItemCountDamageType(MechComponentRef componentRef)
		{
			if (componentRef == null)
			{
				return SimGameState.ItemCountType.ALL;
			}
			if (componentRef.DamageLevel != ComponentDamageLevel.Functional && componentRef.DamageLevel != ComponentDamageLevel.Installing)
			{
				return SimGameState.ItemCountType.DAMAGED_ONLY;
			}
			return SimGameState.ItemCountType.UNDAMAGED_ONLY;
		}

		// Token: 0x06009240 RID: 37440 RVA: 0x00261430 File Offset: 0x0025F630
		public string GetItemStatID(DescriptionDef def, Type type)
		{
			string text = type.ToString();
			if (text.Contains("."))
			{
				text = text.Split(new char[] { '.' })[1];
			}
			return string.Format("{0}.{1}.{2}", "Item", text, def.Id);
		}

		// Token: 0x06009241 RID: 37441 RVA: 0x0026147B File Offset: 0x0025F67B
		private string GetItemStatID(string id, string type)
		{
			return string.Format("{0}.{1}.{2}", "Item", type, id);
		}

		// Token: 0x06009242 RID: 37442 RVA: 0x00261490 File Offset: 0x0025F690
		private string GetItemStatID(string id, Type type)
		{
			string text = type.ToString();
			if (text.Contains("."))
			{
				text = text.Split(new char[] { '.' })[1];
			}
			return string.Format("{0}.{1}.{2}", "Item", text, id);
		}

		// Token: 0x06009243 RID: 37443 RVA: 0x002614D8 File Offset: 0x0025F6D8
		public void AddMech(int idx, MechDef mech, bool active, bool forcePlacement, bool displayMechPopup, string mechAddedHeader = null)
		{
			if (string.IsNullOrEmpty(mech.GUID))
			{
				mech.SetGuid(this.GenerateSimGameUID());
			}
			if (!this.DataManager.ContentPackIndex.IsResourceOwned(mech.Description.Id) || !this.DataManager.ContentPackIndex.IsResourceOwned(mech.Chassis.Description.Id) || !this.DataManager.ContentPackIndex.IsResourceOwned(mech.Chassis.PrefabIdentifier))
			{
				return;
			}
			this.companyStats.ModifyStat<int>("Mission", 0, "COMPANY_MechsAdded", StatCollection.StatOperation.Int_Add, 1, -1, true);
			if (displayMechPopup)
			{
				Text text;
				if (string.IsNullOrEmpty(mechAddedHeader))
				{
					text = new Text("'Mech Chassis Complete", Array.Empty<object>());
					WwiseManager.PostEvent<AudioEventList_ui>(AudioEventList_ui.ui_sim_popup_newChassis, WwiseManager.GlobalAudioObject, null, null);
				}
				else
				{
					text = new Text(mechAddedHeader, Array.Empty<object>());
				}
				text.Append(": ", Array.Empty<object>());
				text.Append(mech.Description.UIName, Array.Empty<object>());
				this.interruptQueue.QueuePauseNotification(text.ToString(true), mech.Chassis.YangsThoughts, this.GetCrewPortrait(SimGameCrew.Crew_Yang), "notification_mechreadycomplete", delegate
				{
					int firstFreeMechBay = this.GetFirstFreeMechBay();
					if (firstFreeMechBay >= 0)
					{
						this.ActiveMechs[firstFreeMechBay] = mech;
						return;
					}
					this.CreateMechPlacementPopup(mech);
				}, "Continue", null, null);
				return;
			}
			if (!forcePlacement && this.GetFirstFreeMechBay() < 0)
			{
				this.CreateMechPlacementPopup(mech);
				return;
			}
			if (active)
			{
				if (this.ActiveMechs.ContainsKey(idx))
				{
					SimGameState.logger.LogError(string.Concat(new object[]
					{
						"SimGame.AddMech is attempting to add a mech ",
						mech.Description.Id,
						" to bay ",
						idx,
						" but that bay is already occupied! This will overwrite the mech in that slot!"
					}));
				}
				this.ActiveMechs[idx] = mech;
			}
			else
			{
				Type typeFromHandle = typeof(MechDef);
				this.AddItemStat(mech.Description.Id, typeFromHandle, false);
			}
			this.MessageCenter.PublishMessage(new SimGameMechAddedMessage(mech, 0, false));
		}

		// Token: 0x06009244 RID: 37444 RVA: 0x00261715 File Offset: 0x0025F915
		public void CreateMechPlacementPopup(MechDef m)
		{
			this.interruptQueue.QueueMechPlacementPopup(m, !this.TimeMoving);
		}

		// Token: 0x06009245 RID: 37445 RVA: 0x0026172D File Offset: 0x0025F92D
		public void CreateMechPlacementPopup(ChassisDef c)
		{
			this.interruptQueue.QueueMechPlacementPopup(c, !this.TimeMoving);
		}

		// Token: 0x06009246 RID: 37446 RVA: 0x00261748 File Offset: 0x0025F948
		public void AddMechByID(string id, bool active)
		{
			if (this.DataManager.MechDefs.Exists(id))
			{
				MechDef mechDef2 = new MechDef(this.DataManager.MechDefs.Get(id), this.GenerateSimGameUID(), true);
				this.AddMech(0, mechDef2, active, false, true, "NEW 'MECH ACQUIRED");
				return;
			}
			if (this.DataManager.ResourceLocator.EntryByID(id, BattleTechResourceType.MechDef, false) == null)
			{
				SimGameState.logger.LogWarning("Unable to Add Mech By ID. Invalid ID Of: " + id);
				return;
			}
			this.RequestItem<MechDef>(id, delegate(MechDef mechDef)
			{
				this.AddMechByID(id, active);
			}, BattleTechResourceType.MechDef);
		}

		// Token: 0x06009247 RID: 37447 RVA: 0x00261814 File Offset: 0x0025FA14
		public void RemoveMech(int idx, MechDef mech, bool active)
		{
			if (active && this.ActiveMechs.ContainsValue(mech))
			{
				this.ActiveMechs.Remove(idx);
				return;
			}
			if (!active)
			{
				Type typeFromHandle = typeof(MechDef);
				string itemStatID = this.GetItemStatID(mech.Description, typeFromHandle);
				this.RemoveItemStat(itemStatID, typeFromHandle, false);
			}
		}

		// Token: 0x06009248 RID: 37448 RVA: 0x00261868 File Offset: 0x0025FA68
		public MechDef GetRandomActiveMech()
		{
			List<int> list = new List<int>(this.ActiveMechs.Keys);
			int num = this.NetworkRandom.Int(0, list.Count);
			return this.ActiveMechs[list[num]];
		}

		// Token: 0x06009249 RID: 37449 RVA: 0x002618AB File Offset: 0x0025FAAB
		public int GetMaxActiveMechs()
		{
			return this.companyStats.GetValue<int>(this.Constants.Story.MechBayPodsID) * this.Constants.Story.MaxMechsPerPod;
		}

		// Token: 0x0600924A RID: 37450 RVA: 0x002618DC File Offset: 0x0025FADC
		public int GetCurrentMechCount(bool includeStorage)
		{
			int num = 0;
			int maxActiveMechs = this.GetMaxActiveMechs();
			for (int i = 0; i < maxActiveMechs; i++)
			{
				if (this.ActiveMechs.ContainsKey(i))
				{
					num++;
				}
			}
			if (!includeStorage)
			{
				return num;
			}
			List<ChassisDef> allInventoryMechDefs = this.GetAllInventoryMechDefs(false);
			num += allInventoryMechDefs.Count;
			return num + this.ReadyingMechs.Count;
		}

		// Token: 0x0600924B RID: 37451 RVA: 0x00261938 File Offset: 0x0025FB38
		private List<MechDef> GetFieldableActiveMechs()
		{
			List<MechDef> list = new List<MechDef>();
			foreach (MechDef mechDef in this.ActiveMechs.Values)
			{
				if (MechValidationRules.ValidateMechCanBeFielded(this, mechDef))
				{
					list.Add(mechDef);
				}
			}
			return list;
		}

		// Token: 0x0600924C RID: 37452 RVA: 0x002619A0 File Offset: 0x0025FBA0
		public int GetFirstOccupiedMechBay()
		{
			int maxActiveMechs = this.GetMaxActiveMechs();
			for (int i = 0; i < maxActiveMechs; i++)
			{
				if (this.ActiveMechs.ContainsKey(i))
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x0600924D RID: 37453 RVA: 0x002619D4 File Offset: 0x0025FBD4
		public int GetFirstFreeMechBay()
		{
			int maxActiveMechs = this.GetMaxActiveMechs();
			for (int i = 0; i < maxActiveMechs; i++)
			{
				if (!this.ActiveMechs.ContainsKey(i) && !this.ReadyingMechs.ContainsKey(i))
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x0600924E RID: 37454 RVA: 0x00261A14 File Offset: 0x0025FC14
		public void AddItemStat(string id, Type type, bool damaged)
		{
			id = this.GetItemStatID(id, type);
			if (damaged)
			{
				id += string.Format(".{0}", "DAMAGED");
			}
			if (this.companyStats.ContainsStatistic(id))
			{
				this.companyStats.ModifyStat<int>("SimGameState", 0, id, StatCollection.StatOperation.Int_Add, 1, -1, true);
				return;
			}
			this.companyStats.AddStatistic<int>(id, 1);
		}

		// Token: 0x0600924F RID: 37455 RVA: 0x00261A7C File Offset: 0x0025FC7C
		public void AddItemStat(string id, string type, bool damaged)
		{
			id = this.GetItemStatID(id, type);
			if (damaged)
			{
				id += string.Format(".{0}", "DAMAGED");
			}
			if (this.companyStats.ContainsStatistic(id))
			{
				this.companyStats.ModifyStat<int>("SimGameState", 0, id, StatCollection.StatOperation.Int_Add, 1, -1, true);
				return;
			}
			this.companyStats.AddStatistic<int>(id, 1);
		}

		// Token: 0x06009250 RID: 37456 RVA: 0x00261AE4 File Offset: 0x0025FCE4
		public void RemoveItemStat(string id, Type type, bool damaged)
		{
			id = this.GetItemStatID(id, type);
			if (damaged)
			{
				id += string.Format(".{0}", "DAMAGED");
			}
			if (this.companyStats.ContainsStatistic(id))
			{
				this.companyStats.ModifyStat<int>("SimGameState", 0, id, StatCollection.StatOperation.Int_Subtract, 1, -1, true);
			}
		}

		// Token: 0x06009251 RID: 37457 RVA: 0x00261B3C File Offset: 0x0025FD3C
		private void RemoveItemStat(string id, string type, bool damaged)
		{
			id = this.GetItemStatID(id, type);
			if (damaged)
			{
				id += string.Format(".{0}", "DAMAGED");
			}
			if (this.companyStats.ContainsStatistic(id))
			{
				this.companyStats.ModifyStat<int>("SimGameState", 0, id, StatCollection.StatOperation.Int_Subtract, 1, -1, true);
			}
		}

		// Token: 0x06009252 RID: 37458 RVA: 0x00261B94 File Offset: 0x0025FD94
		public void AddMechPart(string id)
		{
			int itemCount = this.GetItemCount(id, "MECHPART", SimGameState.ItemCountType.UNDAMAGED_ONLY);
			int defaultMechPartMax = this.Constants.Story.DefaultMechPartMax;
			if (itemCount + 1 >= defaultMechPartMax)
			{
				for (int i = 0; i < defaultMechPartMax - 1; i++)
				{
					this.RemoveItemStat(id, "MECHPART", false);
				}
				MechDef mechDef = new MechDef(this.DataManager.MechDefs.Get(id), this.GenerateSimGameUID(), this.Constants.Salvage.EquipMechOnSalvage);
				this.AddMech(0, mechDef, true, false, true, null);
				this.interruptQueue.DisplayIfAvailable();
				this.MessageCenter.PublishMessage(new SimGameMechAddedMessage(mechDef, defaultMechPartMax, true));
				return;
			}
			this.AddItemStat(id, "MECHPART", false);
		}

		// Token: 0x06009253 RID: 37459 RVA: 0x00261C48 File Offset: 0x0025FE48
		public List<string> GetAllInventoryStrings()
		{
			Dictionary<string, Statistic>.KeyCollection items = this.CompanyStats.Items;
			List<string> list = new List<string>();
			foreach (string text in items)
			{
				if (text.Contains(string.Format("{0}.", "Item")))
				{
					list.Add(text);
				}
			}
			return list;
		}

		// Token: 0x06009254 RID: 37460 RVA: 0x00261CC0 File Offset: 0x0025FEC0
		public MechComponentDef GetComponentDef(BattleTechResourceType type, string id)
		{
			if (type <= BattleTechResourceType.HeatSinkDef)
			{
				if (type == BattleTechResourceType.AmmunitionBoxDef)
				{
					return this.DataManager.AmmoBoxDefs.Get(id);
				}
				if (type == BattleTechResourceType.HeatSinkDef)
				{
					return this.DataManager.HeatSinkDefs.Get(id);
				}
			}
			else
			{
				if (type == BattleTechResourceType.JumpJetDef)
				{
					return this.DataManager.JumpJetDefs.Get(id);
				}
				if (type == BattleTechResourceType.UpgradeDef)
				{
					return this.DataManager.UpgradeDefs.Get(id);
				}
				if (type == BattleTechResourceType.WeaponDef)
				{
					return this.DataManager.WeaponDefs.Get(id);
				}
			}
			throw new Exception("Unsupported ComponentDef Type: " + type);
		}

		// Token: 0x06009255 RID: 37461 RVA: 0x00261D60 File Offset: 0x0025FF60
		public MechComponentDef GetMechComponentDefFromShopItemType(ShopItemType type, string id)
		{
			switch (type)
			{
			case ShopItemType.Weapon:
				return this.DataManager.WeaponDefs.Get(id);
			case ShopItemType.AmmunitionBox:
				return this.DataManager.AmmoBoxDefs.Get(id);
			case ShopItemType.HeatSink:
				return this.DataManager.HeatSinkDefs.Get(id);
			case ShopItemType.JumpJet:
				return this.DataManager.JumpJetDefs.Get(id);
			case ShopItemType.Upgrade:
				return this.DataManager.UpgradeDefs.Get(id);
			}
			throw new Exception("Unsupported ComponentDef Type: " + type);
		}

		// Token: 0x06009256 RID: 37462 RVA: 0x00261E00 File Offset: 0x00260000
		public List<MechComponentRef> GetAllInventoryItemDefs()
		{
			List<MechComponentRef> list = new List<MechComponentRef>();
			foreach (string text in this.GetAllInventoryStrings())
			{
				if (this.CompanyStats.GetValue<int>(text) >= 1)
				{
					string[] array = text.Split(new char[] { '.' });
					if (string.Compare(array[1], "MECHPART") != 0)
					{
						BattleTechResourceType battleTechResourceType = (BattleTechResourceType)Enum.Parse(typeof(BattleTechResourceType), array[1]);
						if (battleTechResourceType != BattleTechResourceType.MechDef && this.DataManager.Exists(battleTechResourceType, array[2]))
						{
							bool flag = array.Length > 3 && array[3].CompareTo("DAMAGED") == 0;
							MechComponentDef componentDef = this.GetComponentDef(battleTechResourceType, array[2]);
							MechComponentRef mechComponentRef = new MechComponentRef(componentDef.Description.Id, this.GenerateSimGameUID(), componentDef.ComponentType, ChassisLocations.None, -1, flag ? ComponentDamageLevel.NonFunctional : ComponentDamageLevel.Functional, false);
							mechComponentRef.SetComponentDef(componentDef);
							list.Add(mechComponentRef);
						}
					}
				}
			}
			return list;
		}

		// Token: 0x06009257 RID: 37463 RVA: 0x00261F24 File Offset: 0x00260124
		public List<ChassisDef> GetAllInventoryMechDefs(bool showMechParts = true)
		{
			List<ChassisDef> list = new List<ChassisDef>();
			foreach (string text in this.GetAllInventoryStrings())
			{
				if (this.CompanyStats.GetValue<int>(text) >= 1)
				{
					string[] array = text.Split(new char[] { '.' });
					if (string.Compare(array[1], "MECHPART") != 0 && (BattleTechResourceType)Enum.Parse(typeof(BattleTechResourceType), array[1]) == BattleTechResourceType.MechDef && this.DataManager.Exists(BattleTechResourceType.ChassisDef, array[2]))
					{
						list.Add(this.DataManager.ChassisDefs.Get(array[2]));
					}
				}
			}
			if (showMechParts)
			{
				List<ChassisDef> allInventoryMechParts = this.GetAllInventoryMechParts();
				if (allInventoryMechParts.Count > 0)
				{
					list.AddRange(allInventoryMechParts);
				}
			}
			return list;
		}

		// Token: 0x06009258 RID: 37464 RVA: 0x00262010 File Offset: 0x00260210
		private List<ChassisDef> GetAllInventoryMechParts()
		{
			List<ChassisDef> list = new List<ChassisDef>();
			foreach (string text in this.GetAllInventoryStrings())
			{
				int value = this.CompanyStats.GetValue<int>(text);
				if (value >= 1)
				{
					string[] array = text.Split(new char[] { '.' });
					if (string.Compare(array[1], "MECHPART") == 0 && this.DataManager.Exists(BattleTechResourceType.MechDef, array[2]))
					{
						ChassisDef chassisDef = new ChassisDef(this.DataManager.MechDefs.Get(array[2]).Chassis);
						chassisDef.DataManager = this.DataManager;
						chassisDef.Refresh();
						chassisDef.MechPartCount = value;
						chassisDef.MechPartMax = this.Constants.Story.DefaultMechPartMax;
						list.Add(chassisDef);
					}
				}
			}
			return list;
		}

		// Token: 0x06009259 RID: 37465 RVA: 0x00262110 File Offset: 0x00260310
		private List<string> GetAllUniqueOwnedChassis()
		{
			List<string> list = new List<string>();
			foreach (MechDef mechDef in this.ActiveMechs.Values)
			{
				if (!list.Contains(mechDef.ChassisID))
				{
					list.Add(mechDef.ChassisID);
				}
			}
			foreach (MechDef mechDef2 in this.ReadyingMechs.Values)
			{
				if (!list.Contains(mechDef2.ChassisID))
				{
					list.Add(mechDef2.ChassisID);
				}
			}
			foreach (ChassisDef chassisDef in this.GetAllInventoryMechDefs(false))
			{
				if (!list.Contains(chassisDef.Description.Id))
				{
					list.Add(chassisDef.Description.Id);
				}
			}
			return list;
		}

		// Token: 0x0600925A RID: 37466 RVA: 0x00262240 File Offset: 0x00260440
		public MechComponentRef GetMechComponentRefForUID(MechDef mech, string simGameUID, string componentID, ComponentType componentType, ComponentDamageLevel damageLevel, ChassisLocations desiredLocation, int hardpointSlot, ref bool itemWasFromInventory)
		{
			MechComponentRef mechComponentRef = null;
			if (mech != null)
			{
				foreach (MechComponentRef mechComponentRef2 in mech.Inventory)
				{
					if (mechComponentRef2.SimGameUID == simGameUID)
					{
						itemWasFromInventory = false;
						return mechComponentRef2;
					}
				}
			}
			foreach (MechComponentRef mechComponentRef3 in this.WorkOrderComponents)
			{
				if (mechComponentRef3.SimGameUID == simGameUID)
				{
					itemWasFromInventory = false;
					return mechComponentRef3;
				}
			}
			if (this.GetItemCount(componentID, SimGameState.GetTypeFromComponent(componentType), (damageLevel == ComponentDamageLevel.Functional || damageLevel == ComponentDamageLevel.Installing) ? SimGameState.ItemCountType.UNDAMAGED_ONLY : SimGameState.ItemCountType.DAMAGED_ONLY) > 0)
			{
				mechComponentRef = new MechComponentRef(componentID, simGameUID, componentType, desiredLocation, hardpointSlot, damageLevel, false);
				mechComponentRef.DataManager = this.DataManager;
				mechComponentRef.RefreshComponentDef();
				itemWasFromInventory = true;
			}
			return mechComponentRef;
		}

		// Token: 0x0600925B RID: 37467 RVA: 0x00262324 File Offset: 0x00260524
		public MechComponentRef GetWorkOrderComponent(string simGameUID)
		{
			foreach (MechComponentRef mechComponentRef in this.WorkOrderComponents)
			{
				if (mechComponentRef.SimGameUID == simGameUID)
				{
					return mechComponentRef;
				}
			}
			return null;
		}

		// Token: 0x0600925C RID: 37468 RVA: 0x00262388 File Offset: 0x00260588
		public void AddFromShopDefItem(ShopDefItem item, bool useCount = true, int cost = 0, SimGamePurchaseMessage.TransactionType transactionType = SimGamePurchaseMessage.TransactionType.Add)
		{
			int num = (useCount ? item.Count : 1);
			string id = item.ID;
			int i = 0;
			while (i < num)
			{
				Type type;
				switch (item.Type)
				{
				case ShopItemType.Chassis_DEPRECATED:
				{
					type = typeof(ChassisDef);
					ChassisDef chassisDef = this.DataManager.ChassisDefs.Get(id);
					this.CreateMechPlacementPopup(chassisDef);
					break;
				}
				case ShopItemType.Weapon:
					type = typeof(WeaponDef);
					goto IL_135;
				case ShopItemType.AmmunitionBox:
					type = typeof(AmmunitionBoxDef);
					goto IL_135;
				case ShopItemType.HeatSink:
					type = typeof(HeatSinkDef);
					goto IL_135;
				case ShopItemType.JumpJet:
					type = typeof(JumpJetDef);
					goto IL_135;
				case ShopItemType.MechPart:
					this.AddMechPart(item.ID);
					break;
				case ShopItemType.Upgrade:
					type = typeof(UpgradeDef);
					goto IL_135;
				case ShopItemType.Mech:
				{
					type = typeof(MechDef);
					MechDef mechDef = new MechDef(this.DataManager.MechDefs.Get(id), this.GenerateSimGameUID(), true);
					int firstFreeMechBay = this.GetFirstFreeMechBay();
					this.AddMech(firstFreeMechBay, mechDef, true, false, true, "'Mech Purchased");
					SimGamePurchaseMessage simGamePurchaseMessage = new SimGamePurchaseMessage(item, cost, transactionType);
					this.MessageCenter.PublishMessage(simGamePurchaseMessage);
					break;
				}
				default:
					throw new Exception("Unknown type");
				}
				IL_15B:
				i++;
				continue;
				IL_135:
				SimGamePurchaseMessage simGamePurchaseMessage2 = new SimGamePurchaseMessage(item, cost, transactionType);
				this.MessageCenter.PublishMessage(simGamePurchaseMessage2);
				this.AddItemStat(item.ID, type, false);
				goto IL_15B;
			}
		}

		// Token: 0x0600925D RID: 37469 RVA: 0x002624FC File Offset: 0x002606FC
		public int GetWorkOrderComponentReferenceCount(WorkOrderEntry_MechLab baseWorkOrder, string currentOrderID, string componentSimGameUID)
		{
			int num = 0;
			for (int i = 0; i < baseWorkOrder.SubEntryCount; i++)
			{
				WorkOrderType type = baseWorkOrder.SubEntries[i].Type;
				if (type != WorkOrderType.MechLabComponentRepair)
				{
					if (type == WorkOrderType.MechLabComponentInstall)
					{
						WorkOrderEntry_InstallComponent workOrderEntry_InstallComponent = (WorkOrderEntry_InstallComponent)baseWorkOrder.SubEntries[i];
						if (!workOrderEntry_InstallComponent.IsMechLabComplete && workOrderEntry_InstallComponent.ID != currentOrderID && workOrderEntry_InstallComponent.ComponentSimGameUID == componentSimGameUID)
						{
							num++;
						}
					}
				}
				else
				{
					WorkOrderEntry_RepairComponent workOrderEntry_RepairComponent = (WorkOrderEntry_RepairComponent)baseWorkOrder.SubEntries[i];
					if (!workOrderEntry_RepairComponent.IsMechLabComplete && workOrderEntry_RepairComponent.ID != currentOrderID && workOrderEntry_RepairComponent.ComponentSimGameUID == componentSimGameUID)
					{
						num++;
					}
				}
			}
			return num;
		}

		// Token: 0x0600925E RID: 37470 RVA: 0x002625B9 File Offset: 0x002607B9
		public bool InMechLabStore()
		{
			return this.CurRoomState == DropshipLocation.MECH_BAY && this.RoomManager.MechBayRoom.IsShopOpen();
		}

		// Token: 0x0600925F RID: 37471 RVA: 0x002625D8 File Offset: 0x002607D8
		public MechDef GetMechByID(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				return null;
			}
			foreach (MechDef mechDef in this.ActiveMechs.Values)
			{
				if (mechDef.GUID.CompareTo(id) == 0)
				{
					return mechDef;
				}
			}
			foreach (MechDef mechDef2 in this.ReadyingMechs.Values)
			{
				if (mechDef2.GUID.CompareTo(id) == 0)
				{
					return mechDef2;
				}
			}
			return null;
		}

		// Token: 0x06009260 RID: 37472 RVA: 0x0026269C File Offset: 0x0026089C
		public static Type GetTypeFromComponent(MechComponentDef def)
		{
			return SimGameState.GetTypeFromComponent(def.ComponentType);
		}

		// Token: 0x06009261 RID: 37473 RVA: 0x002626AC File Offset: 0x002608AC
		public static Type GetTypeFromComponent(ComponentType componentType)
		{
			Type type;
			switch (componentType)
			{
			case ComponentType.Weapon:
				type = typeof(WeaponDef);
				break;
			case ComponentType.AmmunitionBox:
				type = typeof(AmmunitionBoxDef);
				break;
			case ComponentType.HeatSink:
				type = typeof(HeatSinkDef);
				break;
			case ComponentType.JumpJet:
				type = typeof(JumpJetDef);
				break;
			case ComponentType.Upgrade:
				type = typeof(UpgradeDef);
				break;
			default:
				throw new Exception("Unknown type: " + componentType.ToString());
			}
			return type;
		}

		// Token: 0x06009262 RID: 37474 RVA: 0x00262738 File Offset: 0x00260938
		public WorkOrderEntry_MechLab GetWorkOrderEntryForMech(MechDef def)
		{
			if (def == null)
			{
				return null;
			}
			WorkOrderEntry_MechLab workOrderEntry_MechLab = null;
			for (int i = 0; i < this.MechLabQueue.Count; i++)
			{
				WorkOrderEntry_MechLab workOrderEntry_MechLab2 = this.MechLabQueue[i] as WorkOrderEntry_MechLab;
				if (workOrderEntry_MechLab2 != null && workOrderEntry_MechLab2.MechID == def.GUID)
				{
					workOrderEntry_MechLab = workOrderEntry_MechLab2;
					break;
				}
			}
			return workOrderEntry_MechLab;
		}

		// Token: 0x06009263 RID: 37475 RVA: 0x00262790 File Offset: 0x00260990
		public void PruneWorkOrder(ref WorkOrderEntry_MechLab baseWorkOrder)
		{
			Dictionary<string, List<WorkOrderEntry_InstallComponent>> dictionary = new Dictionary<string, List<WorkOrderEntry_InstallComponent>>();
			Dictionary<ChassisLocations, List<WorkOrderEntry_ModifyMechArmor>> dictionary2 = new Dictionary<ChassisLocations, List<WorkOrderEntry_ModifyMechArmor>>();
			Dictionary<string, List<WorkOrderEntry_RepairComponent>> dictionary3 = new Dictionary<string, List<WorkOrderEntry_RepairComponent>>();
			Dictionary<ChassisLocations, List<WorkOrderEntry_RepairMechStructure>> dictionary4 = new Dictionary<ChassisLocations, List<WorkOrderEntry_RepairMechStructure>>();
			for (int i = 0; i < baseWorkOrder.SubEntryCount; i++)
			{
				WorkOrderEntry workOrderEntry = baseWorkOrder[i];
				switch (workOrderEntry.Type)
				{
				case WorkOrderType.MechLabComponentRepair:
				{
					WorkOrderEntry_RepairComponent workOrderEntry_RepairComponent = (WorkOrderEntry_RepairComponent)workOrderEntry;
					if (!dictionary3.ContainsKey(workOrderEntry_RepairComponent.ComponentSimGameUID))
					{
						dictionary3.Add(workOrderEntry_RepairComponent.ComponentSimGameUID, new List<WorkOrderEntry_RepairComponent>());
					}
					dictionary3[workOrderEntry_RepairComponent.ComponentSimGameUID].Add(workOrderEntry_RepairComponent);
					break;
				}
				case WorkOrderType.MechLabMechRepair:
				{
					WorkOrderEntry_RepairMechStructure workOrderEntry_RepairMechStructure = (WorkOrderEntry_RepairMechStructure)workOrderEntry;
					if (!dictionary4.ContainsKey(workOrderEntry_RepairMechStructure.Location))
					{
						dictionary4.Add(workOrderEntry_RepairMechStructure.Location, new List<WorkOrderEntry_RepairMechStructure>());
					}
					dictionary4[workOrderEntry_RepairMechStructure.Location].Add(workOrderEntry_RepairMechStructure);
					break;
				}
				case WorkOrderType.MechLabComponentInstall:
				{
					WorkOrderEntry_InstallComponent workOrderEntry_InstallComponent = (WorkOrderEntry_InstallComponent)workOrderEntry;
					if (!dictionary.ContainsKey(workOrderEntry_InstallComponent.ComponentSimGameUID))
					{
						dictionary.Add(workOrderEntry_InstallComponent.ComponentSimGameUID, new List<WorkOrderEntry_InstallComponent>());
					}
					dictionary[workOrderEntry_InstallComponent.ComponentSimGameUID].Add(workOrderEntry_InstallComponent);
					break;
				}
				case WorkOrderType.MechLabModifyArmor:
				{
					WorkOrderEntry_ModifyMechArmor workOrderEntry_ModifyMechArmor = (WorkOrderEntry_ModifyMechArmor)workOrderEntry;
					if (!dictionary2.ContainsKey(workOrderEntry_ModifyMechArmor.Location))
					{
						dictionary2.Add(workOrderEntry_ModifyMechArmor.Location, new List<WorkOrderEntry_ModifyMechArmor>());
					}
					dictionary2[workOrderEntry_ModifyMechArmor.Location].Add(workOrderEntry_ModifyMechArmor);
					break;
				}
				}
			}
			foreach (ChassisLocations chassisLocations in dictionary4.Keys)
			{
				while (dictionary4[chassisLocations].Count > 1)
				{
					List<WorkOrderEntry_RepairMechStructure> list = dictionary4[chassisLocations];
					while (list.Count > 1)
					{
						list[0].Parent.RemoveSubEntry(list[0]);
						list.RemoveAt(0);
					}
				}
			}
			foreach (ChassisLocations chassisLocations2 in dictionary2.Keys)
			{
				List<WorkOrderEntry_ModifyMechArmor> list2 = dictionary2[chassisLocations2];
				while (list2.Count > 1)
				{
					list2[0].Parent.RemoveSubEntry(list2[0]);
					list2.RemoveAt(0);
				}
			}
			foreach (string text in dictionary.Keys)
			{
				List<WorkOrderEntry_InstallComponent> list3 = dictionary[text];
				ChassisLocations chassisLocations3 = list3[0].PreviousLocation;
				ChassisLocations desiredLocation = list3[list3.Count - 1].DesiredLocation;
				MechComponentRef mechComponentRef = list3[0].MechComponentRef;
				for (int j = 0; j < list3.Count; j++)
				{
					baseWorkOrder.RemoveSubEntry(list3[j].ID);
				}
				if (chassisLocations3 != desiredLocation)
				{
					if (chassisLocations3 != ChassisLocations.None && desiredLocation != ChassisLocations.None)
					{
						WorkOrderEntry_InstallComponent workOrderEntry_InstallComponent2 = this.CreateComponentInstallWorkOrder(baseWorkOrder.MechID, mechComponentRef, ChassisLocations.None, chassisLocations3);
						baseWorkOrder.AddSubEntry(workOrderEntry_InstallComponent2);
						chassisLocations3 = ChassisLocations.None;
					}
					WorkOrderEntry_InstallComponent workOrderEntry_InstallComponent3 = this.CreateComponentInstallWorkOrder(baseWorkOrder.MechID, mechComponentRef, desiredLocation, chassisLocations3);
					baseWorkOrder.AddSubEntry(workOrderEntry_InstallComponent3);
				}
			}
			foreach (string text2 in dictionary3.Keys)
			{
				while (dictionary3[text2].Count > 1)
				{
					List<WorkOrderEntry_RepairComponent> list4 = dictionary3[text2];
					while (list4.Count > 1)
					{
						list4[0].Parent.RemoveSubEntry(list4[0]);
						list4.RemoveAt(0);
					}
				}
			}
		}

		// Token: 0x06009264 RID: 37476 RVA: 0x00262B8C File Offset: 0x00260D8C
		public WorkOrderEntry_InstallComponent CreateComponentInstallWorkOrder(string mechSimGameUID, MechComponentRef mechComponent, ChassisLocations newLocation, ChassisLocations previousLocation)
		{
			string text = Strings.T("MechLab - {0} - {1}", new object[]
			{
				(newLocation != ChassisLocations.None) ? "InstallComponent" : "RemoveComponent",
				this.GenerateSimGameUID()
			});
			string text2 = mechComponent.SimGameUID;
			if (string.IsNullOrEmpty(text2))
			{
				text2 = this.GenerateSimGameUID();
			}
			int num;
			int num2;
			switch (mechComponent.Def.ComponentType)
			{
			case ComponentType.Weapon:
				switch (((WeaponDef)mechComponent.Def).Type)
				{
				case WeaponType.Autocannon:
				case WeaponType.Gauss:
				case WeaponType.MachineGun:
					num = this.Constants.MechLab.BallisticInstallTechPoints;
					num2 = this.Constants.MechLab.BallisticInstallCost;
					goto IL_20A;
				case WeaponType.Laser:
				case WeaponType.PPC:
				case WeaponType.COIL:
					num = this.Constants.MechLab.EnergyInstallTechPoints;
					num2 = this.Constants.MechLab.EnergyInstallCost;
					goto IL_20A;
				case WeaponType.LRM:
				case WeaponType.SRM:
					num = this.Constants.MechLab.MissileInstallTechPoints;
					num2 = this.Constants.MechLab.MissileInstallCost;
					goto IL_20A;
				case WeaponType.Flamer:
					num = this.Constants.MechLab.APInstallTechPoints;
					num2 = this.Constants.MechLab.APInstallCost;
					goto IL_20A;
				}
				num = this.Constants.MechLab.OtherInstallTechPoints;
				num2 = this.Constants.MechLab.OtherInstallCost;
				break;
			case ComponentType.AmmunitionBox:
				num = this.Constants.MechLab.AmmoInstallTechPoints;
				num2 = this.Constants.MechLab.AmmoInstallCost;
				break;
			case ComponentType.HeatSink:
				num = this.Constants.MechLab.HeatSinkInstallTechPoints;
				num2 = this.Constants.MechLab.HeatSinkInstallCost;
				break;
			case ComponentType.JumpJet:
				num = this.Constants.MechLab.JumpJetInstallTechPoints;
				num2 = this.Constants.MechLab.JumpJetInstallCost;
				break;
			default:
				num = this.Constants.MechLab.OtherInstallTechPoints;
				num2 = this.Constants.MechLab.OtherInstallCost;
				break;
			}
			IL_20A:
			if (newLocation == ChassisLocations.None)
			{
				num = this.Constants.MechLab.UninstallTechPoints;
			}
			int num3 = num * mechComponent.Def.InventorySize;
			int num4 = num2 * mechComponent.Def.InventorySize;
			string text3 = "";
			if (mechComponent.DamageLevel == ComponentDamageLevel.Destroyed)
			{
				text3 = " Destroyed";
				num4 = 0;
			}
			string text4 = ((newLocation != ChassisLocations.None) ? Strings.T("Install") : Strings.T("Remove"));
			string text5 = ((newLocation != ChassisLocations.None) ? newLocation.ToString() : "");
			return new WorkOrderEntry_InstallComponent(text, Strings.T("{0}{1} {2} Component - {3}", new object[]
			{
				text4,
				text3,
				text5,
				mechComponent.Def.Description.Name
			}), mechSimGameUID, mechComponent, num3, text2, newLocation, previousLocation, mechComponent.HardpointSlot, num4, "");
		}

		// Token: 0x06009265 RID: 37477 RVA: 0x00262E70 File Offset: 0x00261070
		public WorkOrderEntry_RepairComponent CreateComponentRepairWorkOrder(MechComponentRef mechComponent, bool isOnMech)
		{
			string text = Strings.T("MechBay - RepairComponent - {0}", new object[] { this.GenerateSimGameUID() });
			string text2 = mechComponent.SimGameUID;
			if (string.IsNullOrEmpty(text2))
			{
				text2 = this.GenerateSimGameUID();
			}
			int num = this.Constants.MechLab.ComponentRepairTechPoints * mechComponent.Def.InventorySize;
			int num2 = this.Constants.MechLab.ComponentRepairCost * mechComponent.Def.InventorySize;
			if (mechComponent.IsFixed)
			{
				num = 0;
				num2 = 0;
			}
			return new WorkOrderEntry_RepairComponent(text, Strings.T("Repair Component - {0}", new object[] { mechComponent.Def.Description.Name }), num, mechComponent.ComponentDefID, mechComponent.ComponentDefType, text2, mechComponent.DamageLevel, num2, "");
		}

		// Token: 0x06009266 RID: 37478 RVA: 0x00262F34 File Offset: 0x00261134
		public WorkOrderEntry_ModifyMechArmor CreateMechArmorModifyWorkOrder(string mechSimGameUID, ChassisLocations location, int armorDiff, int frontArmor, int rearArmor)
		{
			string text = Strings.T("MechLab - ModifyArmor - {0}", new object[] { this.GenerateSimGameUID() });
			int num = this.Constants.MechLab.ArmorInstallTechPoints * armorDiff;
			int num2 = this.Constants.MechLab.ArmorInstallCost * armorDiff;
			return new WorkOrderEntry_ModifyMechArmor(text, Strings.T("Modify Armor - {0}", new object[] { location.ToString() }), mechSimGameUID, num, location, frontArmor, rearArmor, num2, "");
		}

		// Token: 0x06009267 RID: 37479 RVA: 0x00262FB4 File Offset: 0x002611B4
		public WorkOrderEntry_RepairMechStructure CreateMechRepairWorkOrder(string mechSimGameUID, ChassisLocations location, int structureCount)
		{
			string text = Strings.T("MechLab - RepairMech - {0}", new object[] { this.GenerateSimGameUID() });
			bool flag = false;
			float num = 1f;
			float num2 = 1f;
			foreach (MechDef mechDef in this.ActiveMechs.Values)
			{
				if (mechDef.GUID == mechSimGameUID)
				{
					if (mechDef.GetChassisLocationDef(location).InternalStructure == (float)structureCount)
					{
						flag = true;
						break;
					}
					break;
				}
			}
			if (flag)
			{
				num = this.Constants.MechLab.ZeroStructureCBillModifier;
				num2 = this.Constants.MechLab.ZeroStructureTechPointModifier;
			}
			int num3 = Mathf.CeilToInt(this.Constants.MechLab.StructureRepairTechPoints * (float)structureCount * num2);
			int num4 = Mathf.CeilToInt((float)(this.Constants.MechLab.StructureRepairCost * structureCount) * num);
			return new WorkOrderEntry_RepairMechStructure(text, Strings.T("Repair 'Mech - {0}", new object[] { location.ToString() }), mechSimGameUID, num3, location, structureCount, num4, "");
		}

		// Token: 0x06009268 RID: 37480 RVA: 0x002630E4 File Offset: 0x002612E4
		public void ReadyMech(int baySlot, string id)
		{
			if (this.ScrapInactiveMech(id, false))
			{
				id = this.GetItemStatID(id, typeof(MechDef));
				string[] array = id.Split(new char[] { '.' });
				if ((BattleTechResourceType)Enum.Parse(typeof(BattleTechResourceType), array[1]) != BattleTechResourceType.MechDef)
				{
					return;
				}
				if (!this.DataManager.Exists(BattleTechResourceType.ChassisDef, array[2]))
				{
					return;
				}
				string text = this.GenerateSimGameUID();
				ChassisDef chassisDef = this.DataManager.ChassisDefs.Get(array[2]);
				string text2 = chassisDef.Description.Id.Replace("chassisdef", "mechdef");
				MechDef mechDef = this.DataManager.MechDefs.Get(text2);
				MechDef mechDef2 = new MechDef(chassisDef, text, mechDef);
				WorkOrderEntry_ReadyMech workOrderEntry_ReadyMech = new WorkOrderEntry_ReadyMech(string.Format("ReadyMech-{0}", text), Strings.T("Readying 'Mech - {0}", new object[] { chassisDef.Description.Name }), this.Constants.Story.MechReadyTime, baySlot, mechDef2, Strings.T(this.Constants.Story.MechReadiedWorkOrderCompletedText, new object[] { chassisDef.Description.Name }));
				this.MechLabQueue.Add(workOrderEntry_ReadyMech);
				this.ReadyingMechs[baySlot] = mechDef2;
				this.RoomManager.AddWorkQueueEntry(workOrderEntry_ReadyMech);
				this.UpdateMechLabWorkQueue(false);
				AudioEventManager.PlayAudioEvent("audioeventdef_simgame_vo_barks", "workqueue_readymech", WwiseManager.GlobalAudioObject, null);
			}
		}

		// Token: 0x06009269 RID: 37481 RVA: 0x00263258 File Offset: 0x00261458
		public void UpdateMechListsForSlotChange(int oldSlotIdx, int newSlotIdx)
		{
			MechDef mechDef = null;
			MechDef mechDef2 = null;
			if (this.ActiveMechs.TryGetValue(oldSlotIdx, out mechDef))
			{
				this.ActiveMechs.Remove(oldSlotIdx);
			}
			if (this.ActiveMechs.TryGetValue(newSlotIdx, out mechDef2))
			{
				this.ActiveMechs.Remove(newSlotIdx);
			}
			if (mechDef != null)
			{
				this.ActiveMechs[newSlotIdx] = mechDef;
			}
			if (mechDef2 != null)
			{
				this.ActiveMechs[oldSlotIdx] = mechDef2;
			}
			if (this.ReadyingMechs.TryGetValue(oldSlotIdx, out mechDef))
			{
				this.ReadyingMechs.Remove(oldSlotIdx);
			}
			if (this.ReadyingMechs.TryGetValue(newSlotIdx, out mechDef2))
			{
				this.ReadyingMechs.Remove(newSlotIdx);
			}
			if (mechDef != null)
			{
				this.ReadyingMechs[newSlotIdx] = mechDef;
			}
			if (mechDef2 != null)
			{
				this.ReadyingMechs[oldSlotIdx] = mechDef2;
			}
			for (int i = 0; i < this.MechLabQueue.Count; i++)
			{
				WorkOrderEntry workOrderEntry = this.MechLabQueue[i];
				if (workOrderEntry.Type == WorkOrderType.MechLabReadyMech)
				{
					WorkOrderEntry_ReadyMech workOrderEntry_ReadyMech = workOrderEntry as WorkOrderEntry_ReadyMech;
					if (workOrderEntry_ReadyMech.bayIdx == oldSlotIdx)
					{
						workOrderEntry_ReadyMech.UpdateBayIdx(newSlotIdx);
					}
					else if (workOrderEntry_ReadyMech.bayIdx == newSlotIdx)
					{
						workOrderEntry_ReadyMech.UpdateBayIdx(oldSlotIdx);
					}
				}
				else if (workOrderEntry.SubEntryCount > 0)
				{
					for (int j = workOrderEntry.SubEntryCount - 1; j >= 0; j--)
					{
						WorkOrderEntry_MechLab workOrderEntry_MechLab = (WorkOrderEntry_MechLab)workOrderEntry.SubEntries[j];
						if (workOrderEntry_MechLab.Type == WorkOrderType.MechLabReadyMech)
						{
							WorkOrderEntry_ReadyMech workOrderEntry_ReadyMech2 = workOrderEntry_MechLab as WorkOrderEntry_ReadyMech;
							if (workOrderEntry_ReadyMech2.bayIdx == oldSlotIdx)
							{
								workOrderEntry_ReadyMech2.UpdateBayIdx(newSlotIdx);
							}
							else if (workOrderEntry_ReadyMech2.bayIdx == newSlotIdx)
							{
								workOrderEntry_ReadyMech2.UpdateBayIdx(oldSlotIdx);
							}
						}
					}
				}
			}
		}

		// Token: 0x0600926A RID: 37482 RVA: 0x002633F4 File Offset: 0x002615F4
		private void StripMech(int baySlot, MechDef def)
		{
			if (def == null || (baySlot > 0 && !this.ActiveMechs.ContainsKey(baySlot)))
			{
				return;
			}
			foreach (MechComponentRef mechComponentRef in def.Inventory)
			{
				if (mechComponentRef.DamageLevel != ComponentDamageLevel.Destroyed && !mechComponentRef.IsFixed)
				{
					bool flag = mechComponentRef.DamageLevel != ComponentDamageLevel.Functional && mechComponentRef.DamageLevel != ComponentDamageLevel.Installing;
					this.AddItemStat(mechComponentRef.Def.Description.Id, SimGameState.GetTypeFromComponent(mechComponentRef.Def), flag);
				}
			}
			if (this.ActiveMechs.ContainsKey(baySlot))
			{
				this.ActiveMechs.Remove(baySlot);
			}
		}

		// Token: 0x0600926B RID: 37483 RVA: 0x00263496 File Offset: 0x00261696
		public void UnreadyMech(int baySlot, MechDef def)
		{
			this.StripMech(baySlot, def);
			this.AddItemStat(def.Chassis.Description.Id, def.GetType(), false);
		}

		// Token: 0x0600926C RID: 37484 RVA: 0x002634BD File Offset: 0x002616BD
		public void ScrapActiveMech(int baySlot, MechDef def)
		{
			this.StripMech(baySlot, def);
			this.AddFunds(Mathf.RoundToInt((float)def.Chassis.Description.Cost * this.Constants.Finances.MechScrapModifier), "Scrapping", true, true);
		}

		// Token: 0x0600926D RID: 37485 RVA: 0x002634FC File Offset: 0x002616FC
		public bool ScrapInactiveMech(string id, bool pay)
		{
			bool flag = false;
			if (this.GetItemCount(id, typeof(MechDef), SimGameState.ItemCountType.UNDAMAGED_ONLY) > 0)
			{
				this.RemoveItemStat(id, typeof(MechDef), false);
				flag = true;
				if (pay)
				{
					if (!this.DataManager.Exists(BattleTechResourceType.ChassisDef, id))
					{
						return flag;
					}
					ChassisDef chassisDef = this.DataManager.ChassisDefs.Get(id);
					this.AddFunds(Mathf.RoundToInt((float)chassisDef.Description.Cost * this.Constants.Finances.MechScrapModifier), "Scrapping", true, true);
				}
			}
			return flag;
		}

		// Token: 0x0600926E RID: 37486 RVA: 0x0026358C File Offset: 0x0026178C
		public bool ScrapMechPart(string id, float partCount, float partMax, bool pay)
		{
			bool flag = false;
			string text = id.Replace("chassisdef", "mechdef");
			if (this.GetItemCount(text, "MECHPART", SimGameState.ItemCountType.UNDAMAGED_ONLY) > 0)
			{
				this.RemoveItemStat(text, "MECHPART", false);
				flag = true;
				if (pay)
				{
					if (!this.DataManager.Exists(BattleTechResourceType.ChassisDef, id))
					{
						return flag;
					}
					int num = Mathf.RoundToInt((float)this.DataManager.ChassisDefs.Get(id).Description.Cost * this.Constants.Finances.MechScrapModifier * (partCount / partMax));
					this.AddFunds(num, "Scrapping", true, true);
				}
			}
			return flag;
		}

		// Token: 0x0600926F RID: 37487 RVA: 0x00263628 File Offset: 0x00261828
		public bool ScrapComponent(MechComponentRef componentRef, bool pay)
		{
			if (componentRef == null)
			{
				SimGameState.logger.LogError("MechBay.ScrapComponent had an invalid mechComponent. Skipping");
				return false;
			}
			DescriptionDef description = componentRef.Def.Description;
			Type type = componentRef.Def.GetType();
			bool flag = componentRef.DamageLevel > ComponentDamageLevel.Functional;
			this.RemoveItemStat(description.Id, type, flag);
			if (pay)
			{
				int num = Mathf.FloorToInt((float)description.Cost * this.Constants.Finances.MechScrapModifier);
				this.AddFunds(num, "Scrapping Component", true, true);
			}
			return true;
		}

		// Token: 0x06009270 RID: 37488 RVA: 0x002636AC File Offset: 0x002618AC
		public void InitializeMechLabEntry(WorkOrderEntry_MechLab entry, int refundAmount)
		{
			int num = entry.GetCBillCostForIncompleteTasks() - refundAmount;
			this.RoomManager.AddWorkQueueEntry(entry);
			this.AddFunds(-num, null, true, true);
			this.MoveWorkOrderItemsToQueue(entry);
			foreach (WorkOrderEntry workOrderEntry in entry.SubEntries)
			{
				this.MoveWorkOrderItemsToQueue(workOrderEntry);
			}
		}

		// Token: 0x06009271 RID: 37489 RVA: 0x00263728 File Offset: 0x00261928
		public void MoveWorkOrderItemsToQueue(WorkOrderEntry entry)
		{
			WorkOrderEntry_RepairComponent workOrderEntry_RepairComponent = entry as WorkOrderEntry_RepairComponent;
			WorkOrderEntry_InstallComponent workOrderEntry_InstallComponent = entry as WorkOrderEntry_InstallComponent;
			if (workOrderEntry_RepairComponent == null && workOrderEntry_InstallComponent == null)
			{
				return;
			}
			string text = "";
			string text2 = "";
			string text3 = "";
			ComponentType componentType = ComponentType.NotSet;
			ComponentDamageLevel componentDamageLevel = ComponentDamageLevel.Functional;
			ChassisLocations chassisLocations = ChassisLocations.None;
			int num = -1;
			if (workOrderEntry_RepairComponent != null)
			{
				text = workOrderEntry_RepairComponent.MechID;
				text2 = workOrderEntry_RepairComponent.MechComponentID;
				text3 = workOrderEntry_RepairComponent.ComponentSimGameUID;
				componentType = workOrderEntry_RepairComponent.ComponentType;
				componentDamageLevel = workOrderEntry_RepairComponent.DamageLevel;
			}
			else if (workOrderEntry_InstallComponent != null)
			{
				text = workOrderEntry_InstallComponent.MechID;
				text2 = workOrderEntry_InstallComponent.MechComponentID;
				text3 = workOrderEntry_InstallComponent.ComponentSimGameUID;
				componentType = workOrderEntry_InstallComponent.ComponentType;
				componentDamageLevel = workOrderEntry_InstallComponent.DamageLevel;
				chassisLocations = workOrderEntry_InstallComponent.DesiredLocation;
				num = workOrderEntry_InstallComponent.HardpointSlot;
			}
			bool flag = false;
			MechComponentRef mechComponentRefForUID = this.GetMechComponentRefForUID(this.GetMechByID(text), text3, text2, componentType, componentDamageLevel, chassisLocations, num, ref flag);
			if (mechComponentRefForUID != null && flag)
			{
				this.RemoveItemStat(mechComponentRefForUID.ComponentDefID, SimGameState.GetTypeFromComponent(mechComponentRefForUID.ComponentDefType), mechComponentRefForUID.DamageLevel != ComponentDamageLevel.Functional && mechComponentRefForUID.DamageLevel != ComponentDamageLevel.Installing);
				if (mechComponentRefForUID.DamageLevel == ComponentDamageLevel.Functional)
				{
					mechComponentRefForUID.DamageLevel = ComponentDamageLevel.Installing;
				}
				else if (mechComponentRefForUID.DamageLevel == ComponentDamageLevel.NonFunctional)
				{
					mechComponentRefForUID.DamageLevel = ComponentDamageLevel.InstallingNonFunctional;
				}
				this.WorkOrderComponents.Add(mechComponentRefForUID);
			}
		}

		// Token: 0x06009272 RID: 37490 RVA: 0x00263860 File Offset: 0x00261A60
		public void ReturnWorkOrderItemsToInventory(WorkOrderEntry entry)
		{
			WorkOrderEntry_RepairComponent workOrderEntry_RepairComponent = entry as WorkOrderEntry_RepairComponent;
			WorkOrderEntry_InstallComponent workOrderEntry_InstallComponent = entry as WorkOrderEntry_InstallComponent;
			if (workOrderEntry_RepairComponent == null && workOrderEntry_InstallComponent == null)
			{
				return;
			}
			string text = "";
			MechComponentRef mechComponentRef = null;
			if (workOrderEntry_RepairComponent != null)
			{
				text = workOrderEntry_RepairComponent.ComponentSimGameUID;
			}
			else if (workOrderEntry_InstallComponent != null)
			{
				text = workOrderEntry_InstallComponent.ComponentSimGameUID;
			}
			foreach (MechComponentRef mechComponentRef2 in this.WorkOrderComponents)
			{
				if (mechComponentRef2.SimGameUID == text)
				{
					mechComponentRef = mechComponentRef2;
					break;
				}
			}
			if (mechComponentRef != null)
			{
				this.WorkOrderComponents.Remove(mechComponentRef);
				if (mechComponentRef.DamageLevel == ComponentDamageLevel.Installing)
				{
					mechComponentRef.DamageLevel = ComponentDamageLevel.Functional;
				}
				else if (mechComponentRef.DamageLevel == ComponentDamageLevel.InstallingNonFunctional)
				{
					mechComponentRef.DamageLevel = ComponentDamageLevel.NonFunctional;
				}
				this.AddItemStat(mechComponentRef.ComponentDefID, SimGameState.GetTypeFromComponent(mechComponentRef.ComponentDefType), mechComponentRef.DamageLevel != ComponentDamageLevel.Functional && mechComponentRef.DamageLevel != ComponentDamageLevel.Installing);
			}
		}

		// Token: 0x06009273 RID: 37491 RVA: 0x00263958 File Offset: 0x00261B58
		public void UpdateMechLabWorkQueue(bool passDay = true)
		{
			if (this.MechLabQueue.Count < 1)
			{
				return;
			}
			if (passDay)
			{
				this.MechLabQueue[0].PayCost(this.MechTechSkill);
			}
			for (int i = this.MechLabQueue.Count - 1; i >= 0; i--)
			{
				WorkOrderEntry workOrderEntry = this.MechLabQueue[i];
				for (int j = workOrderEntry.SubEntryCount - 1; j >= 0; j--)
				{
					WorkOrderEntry_MechLab workOrderEntry_MechLab = (WorkOrderEntry_MechLab)workOrderEntry.SubEntries[j];
					if (workOrderEntry_MechLab.IsCostPaid() && !workOrderEntry_MechLab.IsMechLabComplete)
					{
						this.CompleteWorkOrder(workOrderEntry_MechLab, true);
					}
				}
				if (workOrderEntry.IsCostPaid())
				{
					bool flag = false;
					if (workOrderEntry.Type != WorkOrderType.ArgoUpgradeGeneric && workOrderEntry.Type == WorkOrderType.MechLabReadyMech)
					{
						flag = true;
					}
					if (!flag)
					{
						for (int k = workOrderEntry.SubEntryCount - 1; k >= 0; k--)
						{
							if (((WorkOrderEntry_MechLab)workOrderEntry.SubEntries[k]).Type == WorkOrderType.MechLabReadyMech)
							{
								this.GetCrewPortrait(SimGameCrew.Crew_Yang);
								break;
							}
						}
					}
					this.CompleteWorkOrder(workOrderEntry, false);
					this.RoomManager.RemoveWorkQueueEntry(workOrderEntry, false);
					this.MechLabQueue.Remove(workOrderEntry);
				}
			}
			this.RoomManager.RefreshTimeline(false);
			float num = 0f;
			for (int l = this.MechLabQueue.Count - 1; l >= 0; l--)
			{
				num += (float)this.MechLabQueue[l].GetRemainingCost();
			}
			this.companyStats.ModifyStat<int>("SimGameState", 0, "TaskDuration", StatCollection.StatOperation.Set, Mathf.CeilToInt(num / (float)this.MechTechSkill), -1, true);
			if (this.MechLabQueue.Count == 0)
			{
				this.interruptQueue.QueuePauseNotification("Work Queue Empty", "All work orders complete, Commander. The crew is ready for the next job.", this.GetCrewPortrait(SimGameCrew.Crew_Yang), "notification_workqueuecomplete", null, "Continue", null, null);
				this.interruptQueue.DisplayIfAvailable();
			}
		}

		// Token: 0x06009274 RID: 37492 RVA: 0x00263B34 File Offset: 0x00261D34
		private void CompleteWorkOrder(WorkOrderEntry entry, bool isSubEntry)
		{
			switch (entry.Type)
			{
			case WorkOrderType.MechLabComponentRepair:
				this.ML_RepairComponent((WorkOrderEntry_RepairComponent)entry, isSubEntry);
				break;
			case WorkOrderType.MechLabMechRepair:
				this.ML_RepairMech((WorkOrderEntry_RepairMechStructure)entry);
				break;
			case WorkOrderType.MechLabComponentInstall:
				this.ML_InstallComponent((WorkOrderEntry_InstallComponent)entry);
				break;
			case WorkOrderType.MechLabModifyArmor:
				this.ML_ModifyArmor((WorkOrderEntry_ModifyMechArmor)entry);
				break;
			case WorkOrderType.MechLabReadyMech:
				this.ML_ReadyMech((WorkOrderEntry_ReadyMech)entry);
				break;
			}
			if (entry.Type == WorkOrderType.MechLabGeneric)
			{
				WorkOrderEntry_MechLab workOrderEntry_MechLab = entry as WorkOrderEntry_MechLab;
				MechDef mechByID = this.GetMechByID(workOrderEntry_MechLab.MechID);
				SimGameMLOrderCompleteMessage simGameMLOrderCompleteMessage = new SimGameMLOrderCompleteMessage(workOrderEntry_MechLab, mechByID);
				this.MessageCenter.PublishMessage(simGameMLOrderCompleteMessage);
			}
			if (this.MechLabQueue.Count < 1)
			{
				foreach (MechComponentRef mechComponentRef in this.WorkOrderComponents)
				{
					SimGameState.logger.LogError("MechLabQueue is empty, but there's an item in workOrderComponents! " + mechComponentRef.ComponentDefID + ", simId: " + mechComponentRef.SimGameUID);
				}
			}
		}

		// Token: 0x06009275 RID: 37493 RVA: 0x00263C54 File Offset: 0x00261E54
		private void ML_RepairComponent(WorkOrderEntry_RepairComponent order, bool isSubEntry)
		{
			if (order.IsMechLabComplete)
			{
				return;
			}
			MechDef mechDef = null;
			if (order.HasParent)
			{
				mechDef = this.GetMechByID(order.MechLabParent.MechID);
			}
			bool flag = false;
			MechComponentRef mechComponentRefForUID = this.GetMechComponentRefForUID(mechDef, order.ComponentSimGameUID, order.MechComponentID, order.ComponentType, order.DamageLevel, ChassisLocations.None, -1, ref flag);
			if (mechComponentRefForUID == null)
			{
				SimGameState.logger.LogError(string.Concat(new string[] { "ML_RepairComponent ", order.ID, " had an invalid mechComponentID ", order.MechComponentID, ", skipping" }));
				return;
			}
			if (!isSubEntry)
			{
				this.WorkOrderComponents.Remove(mechComponentRefForUID);
				this.AddItemStat(mechComponentRefForUID.Def.Description.Id, SimGameState.GetTypeFromComponent(mechComponentRefForUID.Def), false);
			}
			else
			{
				mechComponentRefForUID.SetData(mechComponentRefForUID.MountedLocation, mechComponentRefForUID.HardpointSlot, ComponentDamageLevel.Functional, mechComponentRefForUID.IsFixed);
				if (this.GetWorkOrderComponentReferenceCount(order.MechLabParent, order.ID, mechComponentRefForUID.SimGameUID) > 0)
				{
					if (mechComponentRefForUID.DamageLevel == ComponentDamageLevel.Functional)
					{
						mechComponentRefForUID.DamageLevel = ComponentDamageLevel.Installing;
					}
					else if (mechComponentRefForUID.DamageLevel == ComponentDamageLevel.NonFunctional)
					{
						mechComponentRefForUID.DamageLevel = ComponentDamageLevel.InstallingNonFunctional;
					}
					if (!this.WorkOrderComponents.Contains(mechComponentRefForUID))
					{
						this.WorkOrderComponents.Add(mechComponentRefForUID);
					}
				}
				else
				{
					this.WorkOrderComponents.Remove(mechComponentRefForUID);
					if (mechComponentRefForUID.MountedLocation == ChassisLocations.None)
					{
						this.AddItemStat(mechComponentRefForUID.Def.Description.Id, SimGameState.GetTypeFromComponent(mechComponentRefForUID.Def), false);
					}
				}
			}
			order.SetMechLabComplete(true);
		}

		// Token: 0x06009276 RID: 37494 RVA: 0x00263DD8 File Offset: 0x00261FD8
		private void ML_InstallComponent(WorkOrderEntry_InstallComponent order)
		{
			if (order.IsMechLabComplete)
			{
				return;
			}
			MechDef mechByID = this.GetMechByID(order.MechLabParent.MechID);
			bool flag = false;
			MechComponentRef mechComponentRefForUID = this.GetMechComponentRefForUID(mechByID, order.ComponentSimGameUID, order.MechComponentID, order.ComponentType, order.DamageLevel, order.DesiredLocation, order.HardpointSlot, ref flag);
			if (mechByID == null)
			{
				SimGameState.logger.LogError(string.Concat(new string[]
				{
					"ML_InstallComponent ",
					order.ID,
					" had an invalid mechID ",
					order.MechLabParent.MechID,
					", skipping"
				}));
				return;
			}
			if (mechComponentRefForUID == null)
			{
				SimGameState.logger.LogError(string.Concat(new string[] { "ML_InstallComponent ", order.ID, " had an invalid mechComponentID ", order.MechComponentID, ", skipping" }));
				return;
			}
			if (order.DesiredLocation != ChassisLocations.None)
			{
				string id = mechComponentRefForUID.Def.Description.Id;
				this.WorkOrderComponents.Remove(mechComponentRefForUID);
				List<MechComponentRef> list = new List<MechComponentRef>(mechByID.Inventory);
				mechComponentRefForUID.SetData(order.DesiredLocation, order.HardpointSlot, (mechComponentRefForUID.DamageLevel == ComponentDamageLevel.InstallingNonFunctional) ? ComponentDamageLevel.NonFunctional : ComponentDamageLevel.Functional, mechComponentRefForUID.IsFixed);
				list.Add(mechComponentRefForUID);
				mechByID.SetInventory(list.ToArray());
			}
			else
			{
				if (mechComponentRefForUID.DamageLevel != ComponentDamageLevel.Destroyed)
				{
					ComponentDamageLevel componentDamageLevel = ComponentDamageLevel.Functional;
					if (mechComponentRefForUID.DamageLevel == ComponentDamageLevel.Functional)
					{
						componentDamageLevel = ComponentDamageLevel.Installing;
					}
					else if (mechComponentRefForUID.DamageLevel == ComponentDamageLevel.NonFunctional)
					{
						componentDamageLevel = ComponentDamageLevel.InstallingNonFunctional;
					}
					mechComponentRefForUID.SetData(order.DesiredLocation, order.HardpointSlot, componentDamageLevel, mechComponentRefForUID.IsFixed);
					if (this.GetWorkOrderComponentReferenceCount(order.MechLabParent, order.ID, mechComponentRefForUID.SimGameUID) > 0)
					{
						if (!this.WorkOrderComponents.Contains(mechComponentRefForUID))
						{
							this.WorkOrderComponents.Add(mechComponentRefForUID);
						}
					}
					else
					{
						this.AddItemStat(mechComponentRefForUID.Def.Description.Id, SimGameState.GetTypeFromComponent(mechComponentRefForUID.Def), mechComponentRefForUID.DamageLevel != ComponentDamageLevel.Functional && mechComponentRefForUID.DamageLevel != ComponentDamageLevel.Installing);
					}
				}
				List<MechComponentRef> list2 = new List<MechComponentRef>();
				foreach (MechComponentRef mechComponentRef in mechByID.Inventory)
				{
					if (mechComponentRef != mechComponentRefForUID)
					{
						list2.Add(mechComponentRef);
					}
				}
				mechByID.SetInventory(list2.ToArray());
			}
			mechByID.RefreshBattleValue();
			order.SetMechLabComplete(true);
		}

		// Token: 0x06009277 RID: 37495 RVA: 0x00264030 File Offset: 0x00262230
		private void ML_RepairMech(WorkOrderEntry_RepairMechStructure order)
		{
			if (order.IsMechLabComplete)
			{
				return;
			}
			MechDef mechByID = this.GetMechByID(order.MechLabParent.MechID);
			LocationLoadoutDef locationLoadoutDef = mechByID.GetLocationLoadoutDef(order.Location);
			locationLoadoutDef.CurrentInternalStructure = mechByID.GetChassisLocationDef(order.Location).InternalStructure;
			locationLoadoutDef.CurrentArmor = locationLoadoutDef.AssignedArmor;
			locationLoadoutDef.CurrentRearArmor = locationLoadoutDef.AssignedRearArmor;
			mechByID.RefreshBattleValue();
			order.SetMechLabComplete(true);
		}

		// Token: 0x06009278 RID: 37496 RVA: 0x002640A0 File Offset: 0x002622A0
		private void ML_ModifyArmor(WorkOrderEntry_ModifyMechArmor order)
		{
			if (order.IsMechLabComplete)
			{
				return;
			}
			MechDef mechByID = this.GetMechByID(order.MechLabParent.MechID);
			LocationLoadoutDef locationLoadoutDef = mechByID.GetLocationLoadoutDef(order.Location);
			locationLoadoutDef.CurrentArmor = (float)order.DesiredFrontArmor;
			locationLoadoutDef.CurrentRearArmor = (float)order.DesiredRearArmor;
			locationLoadoutDef.AssignedArmor = (float)order.DesiredFrontArmor;
			locationLoadoutDef.AssignedRearArmor = (float)order.DesiredRearArmor;
			mechByID.RefreshBattleValue();
			order.SetMechLabComplete(true);
		}

		// Token: 0x06009279 RID: 37497 RVA: 0x00264114 File Offset: 0x00262314
		private void ML_ReadyMech(WorkOrderEntry_ReadyMech order)
		{
			if (order.IsMechLabComplete)
			{
				return;
			}
			this.ReadyingMechs.Remove(order.bayIdx);
			this.ActiveMechs[order.bayIdx] = order.Mech;
			order.Mech.RefreshBattleValue();
			order.Mech.DataManager = this.DataManager;
			order.SetMechLabComplete(true);
		}

		// Token: 0x0600927A RID: 37498 RVA: 0x00264178 File Offset: 0x00262378
		public void CancelWorkOrder(WorkOrderEntry entry, bool includeRefund = true)
		{
			SimGameState.logger.Log("CancelWorkOrder " + entry.Description + ", includeRefund?: " + includeRefund.ToString());
			if (includeRefund)
			{
				this.RefundWorkOrder(entry);
			}
			this.RemoveFromMechLabQueue(entry);
			WorkOrderType type = entry.Type;
			if (type == WorkOrderType.ArgoUpgradeGeneric)
			{
				this.CancelArgoUpgrade(false);
				return;
			}
			switch (type)
			{
			case WorkOrderType.MechLabGeneric:
				this.Cancel_ML_FullMechWorkOrder((WorkOrderEntry_MechLab)entry);
				return;
			case WorkOrderType.MechLabComponentRepair:
				this.Cancel_ML_RepairComponent((WorkOrderEntry_RepairComponent)entry);
				return;
			case WorkOrderType.MechLabMechRepair:
				this.Cancel_ML_RepairMech((WorkOrderEntry_RepairMechStructure)entry);
				return;
			case WorkOrderType.MechLabComponentInstall:
				this.Cancel_ML_InstallComponent((WorkOrderEntry_InstallComponent)entry);
				return;
			case WorkOrderType.MechLabModifyArmor:
				this.Cancel_ML_ModifyArmor((WorkOrderEntry_ModifyMechArmor)entry);
				return;
			case WorkOrderType.MechLabReadyMech:
				this.Cancel_ML_ReadyMech((WorkOrderEntry_ReadyMech)entry);
				return;
			default:
				return;
			}
		}

		// Token: 0x0600927B RID: 37499 RVA: 0x00264240 File Offset: 0x00262440
		private void RefundWorkOrder(WorkOrderEntry entry)
		{
			int workOrderRefundAmount = this.GetWorkOrderRefundAmount(entry);
			if (workOrderRefundAmount > 0)
			{
				this.AddFunds(workOrderRefundAmount, null, true, false);
			}
		}

		// Token: 0x0600927C RID: 37500 RVA: 0x00264264 File Offset: 0x00262464
		public int GetWorkOrderRefundAmount(WorkOrderEntry entry)
		{
			WorkOrderEntry_MechLab workOrderEntry_MechLab = entry as WorkOrderEntry_MechLab;
			if (workOrderEntry_MechLab != null)
			{
				int cbillCostForIncompleteTasks = workOrderEntry_MechLab.GetCBillCostForIncompleteTasks();
				float mechLabRefundModifier = this.Constants.Finances.MechLabRefundModifier;
				float num;
				if (entry.GetCostPaid() == 0)
				{
					num = (float)cbillCostForIncompleteTasks;
				}
				else
				{
					num = (float)cbillCostForIncompleteTasks * mechLabRefundModifier;
				}
				return (int)num;
			}
			if (entry == this.CurrentUpgradeEntry)
			{
				int cBillCost = this.CurrentUpgradeEntry.cBillCost;
				float mechLabRefundModifier2 = this.Constants.Finances.MechLabRefundModifier;
				float num2;
				if (entry.GetCostPaid() == 0)
				{
					num2 = (float)cBillCost;
				}
				else
				{
					num2 = (float)cBillCost * mechLabRefundModifier2;
				}
				return (int)num2;
			}
			SimGameState.logger.LogError(entry.ID + " is not a MechLab or Argo work order! Is there something we should refund?");
			return -1;
		}

		// Token: 0x0600927D RID: 37501 RVA: 0x00264314 File Offset: 0x00262514
		private void RemoveFromMechLabQueue(WorkOrderEntry entry)
		{
			List<WorkOrderEntry> list;
			if (entry.Parent == null)
			{
				list = this.MechLabQueue;
			}
			else
			{
				list = entry.Parent.SubEntries;
			}
			list.Remove(entry);
			if (entry.Parent != null && entry.Parent.SubEntryCount < 1)
			{
				this.MechLabQueue.Remove(entry.Parent);
			}
		}

		// Token: 0x0600927E RID: 37502 RVA: 0x00264370 File Offset: 0x00262570
		private void Cancel_ML_FullMechWorkOrder(WorkOrderEntry_MechLab order)
		{
			for (int i = order.SubEntryCount - 1; i >= 0; i--)
			{
				WorkOrderEntry workOrderEntry = order.SubEntries[i];
				this.CancelWorkOrder(workOrderEntry, false);
			}
		}

		// Token: 0x0600927F RID: 37503 RVA: 0x002643A5 File Offset: 0x002625A5
		private void Cancel_ML_RepairComponent(WorkOrderEntry_RepairComponent order)
		{
			this.ReturnWorkOrderItemsToInventory(order);
		}

		// Token: 0x06009280 RID: 37504 RVA: 0x002643A5 File Offset: 0x002625A5
		private void Cancel_ML_InstallComponent(WorkOrderEntry_InstallComponent order)
		{
			this.ReturnWorkOrderItemsToInventory(order);
		}

		// Token: 0x06009281 RID: 37505 RVA: 0x0000D184 File Offset: 0x0000B384
		private void Cancel_ML_RepairMech(WorkOrderEntry_RepairMechStructure order)
		{
		}

		// Token: 0x06009282 RID: 37506 RVA: 0x0000D184 File Offset: 0x0000B384
		private void Cancel_ML_ModifyArmor(WorkOrderEntry_ModifyMechArmor order)
		{
		}

		// Token: 0x06009283 RID: 37507 RVA: 0x002643B0 File Offset: 0x002625B0
		private void Cancel_ML_ReadyMech(WorkOrderEntry_ReadyMech order)
		{
			MechDef mech = order.Mech;
			this.UnreadyMech(order.bayIdx, mech);
			this.ReadyingMechs.Remove(order.bayIdx);
		}

		// Token: 0x06009284 RID: 37508 RVA: 0x002643E4 File Offset: 0x002625E4
		private void UpdateArgoUpgrades(bool passDay = true)
		{
			if (this.CurrentUpgradeEntry == null)
			{
				return;
			}
			if (passDay)
			{
				this.CurrentUpgradeEntry.PayCost(this.DailyUpgradeValue);
				if (this.CurrentUpgradeEntry.IsCostPaid())
				{
					this.CompleteArgoUpgrade(this.CurrentUpgradeEntry);
					this.RoomManager.RemoveWorkQueueEntry(this.CurrentUpgradeEntry, false);
					this.interruptQueue.QueuePauseNotification("Work Order Complete", Strings.T("Work on {0} is complete.", new object[] { this.CurrentUpgradeEntry.Description }), this.GetCrewPortrait(SimGameCrew.Crew_Farah), "workqueue_argoupgradecomplete", null, "Continue", null, null);
					this.CancelArgoUpgrade(false);
					return;
				}
			}
			this.RoomManager.RefreshTimeline(false);
		}

		// Token: 0x06009285 RID: 37509 RVA: 0x00264494 File Offset: 0x00262694
		public void QueueArgoUpgrade(ShipModuleUpgrade requestedUpgrade)
		{
			if (this.CurrentUpgradeEntry != null)
			{
				this.CancelArgoUpgrade(true);
			}
			int num = Mathf.CeilToInt((float)requestedUpgrade.PurchaseCost * this.Constants.CareerMode.ArgoUpgradeCostMultiplier);
			WorkOrderEntry_ArgoUpgradeGeneric workOrderEntry_ArgoUpgradeGeneric = new WorkOrderEntry_ArgoUpgradeGeneric(requestedUpgrade.Description.Id, requestedUpgrade.Description.Id, Strings.T("Argo Upgrade: {0}", new object[] { requestedUpgrade.Description.Name }), requestedUpgrade.TechCost, num, "");
			this.AddFunds(-num, "Upgrades", true, true);
			this.CurrentUpgradeEntry = workOrderEntry_ArgoUpgradeGeneric;
			this.CurrentUpgradeTimelineElement = this.RoomManager.AddWorkQueueEntry(workOrderEntry_ArgoUpgradeGeneric);
			this.RoomManager.RefreshTimeline(false);
		}

		// Token: 0x06009286 RID: 37510 RVA: 0x00264548 File Offset: 0x00262748
		private void CancelArgoUpgrade(bool refund)
		{
			if (refund)
			{
				ShipModuleUpgrade shipModuleUpgrade = this.DataManager.ShipUpgradeDefs.Get(this.CurrentUpgradeEntry.upgradeID);
				this.AddFunds(Mathf.CeilToInt((float)shipModuleUpgrade.PurchaseCost * this.Constants.CareerMode.ArgoUpgradeCostMultiplier), "Upgrade Refund", true, false);
			}
			this.CurrentUpgradeTimelineElement = null;
			this.CurrentUpgradeEntry = null;
			this.RoomManager.RefreshTimeline(false);
		}

		// Token: 0x06009287 RID: 37511 RVA: 0x002645B8 File Offset: 0x002627B8
		private void CompleteArgoUpgrade(WorkOrderEntry_ArgoUpgradeGeneric order)
		{
			ShipModuleUpgrade shipModuleUpgrade = this.DataManager.ShipUpgradeDefs.Get(order.upgradeID);
			this.AddArgoUpgrade(shipModuleUpgrade);
		}

		// Token: 0x06009288 RID: 37512 RVA: 0x002645E3 File Offset: 0x002627E3
		public bool HasShipUpgrade(string id, List<string> upgradesToCheck = null)
		{
			if (upgradesToCheck == null)
			{
				upgradesToCheck = this.PurchasedArgoUpgrades;
			}
			return upgradesToCheck.Contains(id);
		}

		// Token: 0x06009289 RID: 37513 RVA: 0x002645F8 File Offset: 0x002627F8
		public bool HasShipUpgrade(TagSet idList, List<string> upgradesToCheck = null)
		{
			if (idList == null)
			{
				return false;
			}
			if (upgradesToCheck == null)
			{
				upgradesToCheck = this.PurchasedArgoUpgrades;
			}
			foreach (string text in idList)
			{
				if (!this.HasShipUpgrade(text, upgradesToCheck))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x0600928A RID: 37514 RVA: 0x0026465C File Offset: 0x0026285C
		public bool UpgradeInProgress(string id)
		{
			return this.CurrentUpgradeEntry != null && this.CurrentUpgradeEntry.upgradeID == id;
		}

		// Token: 0x0600928B RID: 37515 RVA: 0x0026467C File Offset: 0x0026287C
		public void AddArgoUpgrade(ShipModuleUpgrade upgrade)
		{
			this.shipUpgrades.Add(upgrade);
			this.purchasedArgoUpgrades.Add(upgrade.Description.Id);
			if (this.CurDropship == DropshipType.Argo)
			{
				if (upgrade.Tags != null && !upgrade.Tags.IsEmpty)
				{
					this.companyTags.AddRange(upgrade.Tags);
				}
				foreach (SimGameStat simGameStat in upgrade.Stats)
				{
					this.SetCompanyStat(simGameStat);
				}
				if (upgrade.Actions != null)
				{
					SimGameResultAction[] actions = upgrade.Actions;
					for (int i = 0; i < actions.Length; i++)
					{
						SimGameState.ApplyEventAction(actions[i], null);
					}
				}
			}
			this.RoomManager.RefreshTimeline(false);
		}

		// Token: 0x0600928C RID: 37516 RVA: 0x00264734 File Offset: 0x00262934
		public void ApplyArgoUpgrades()
		{
			if (this.CurDropship != DropshipType.Argo)
			{
				return;
			}
			if (this.companyTags.Contains("ARGO_UpgradesApplied"))
			{
				return;
			}
			foreach (ShipModuleUpgrade shipModuleUpgrade in this.shipUpgrades)
			{
				if (shipModuleUpgrade.Tags != null && !shipModuleUpgrade.Tags.IsEmpty)
				{
					this.companyTags.AddRange(shipModuleUpgrade.Tags);
				}
				foreach (SimGameStat simGameStat in shipModuleUpgrade.Stats)
				{
					this.SetCompanyStat(simGameStat);
				}
			}
			this.companyTags.Add("ARGO_UpgradesApplied");
		}

		// Token: 0x0600928D RID: 37517 RVA: 0x002647FC File Offset: 0x002629FC
		public bool WorkOrderIsMechTech(WorkOrderType type)
		{
			return type == WorkOrderType.ArgoUpgradeGeneric || type == WorkOrderType.MechLabGeneric || type == WorkOrderType.MechLabComponentRepair || type == WorkOrderType.MechLabMechRepair || type == WorkOrderType.MechLabComponentInstall || type == WorkOrderType.MechLabModifyArmor || type == WorkOrderType.MechLabReadyMech;
		}

		// Token: 0x0600928E RID: 37518 RVA: 0x00264824 File Offset: 0x00262A24
		public void RestoreMechPostCombat(MechDef mech)
		{
			this.RestoreArmorIfUndamaged(mech.Head, mech.GetChassisLocationDef(ChassisLocations.Head));
			this.RestoreArmorIfUndamaged(mech.CenterTorso, mech.GetChassisLocationDef(ChassisLocations.CenterTorso));
			this.RestoreArmorIfUndamaged(mech.LeftTorso, mech.GetChassisLocationDef(ChassisLocations.LeftTorso));
			this.RestoreArmorIfUndamaged(mech.RightTorso, mech.GetChassisLocationDef(ChassisLocations.RightTorso));
			this.RestoreArmorIfUndamaged(mech.LeftArm, mech.GetChassisLocationDef(ChassisLocations.LeftArm));
			this.RestoreArmorIfUndamaged(mech.RightArm, mech.GetChassisLocationDef(ChassisLocations.RightArm));
			this.RestoreArmorIfUndamaged(mech.LeftLeg, mech.GetChassisLocationDef(ChassisLocations.LeftLeg));
			this.RestoreArmorIfUndamaged(mech.RightLeg, mech.GetChassisLocationDef(ChassisLocations.RightLeg));
			foreach (MechComponentRef mechComponentRef in mech.Inventory)
			{
				if (mechComponentRef.DamageLevel == ComponentDamageLevel.NonFunctional)
				{
					mechComponentRef.DamageLevel = ComponentDamageLevel.Functional;
				}
			}
		}

		// Token: 0x0600928F RID: 37519 RVA: 0x002648F9 File Offset: 0x00262AF9
		public void RestoreArmorIfUndamaged(LocationLoadoutDef loadoutDef, LocationDef chassisLocationDef)
		{
			if (loadoutDef.CurrentInternalStructure <= 0f)
			{
				loadoutDef.CurrentArmor = 0f;
				loadoutDef.CurrentRearArmor = 0f;
				return;
			}
			loadoutDef.CurrentArmor = loadoutDef.AssignedArmor;
			loadoutDef.CurrentRearArmor = loadoutDef.AssignedRearArmor;
		}

		// Token: 0x06009290 RID: 37520 RVA: 0x00264938 File Offset: 0x00262B38
		public void Mech_InstantRepairAll(MechDef d)
		{
			this.Mech_InstantRepairStructure(d, ChassisLocations.Head);
			this.Mech_InstantRepairStructure(d, ChassisLocations.CenterTorso);
			this.Mech_InstantRepairStructure(d, ChassisLocations.LeftTorso);
			this.Mech_InstantRepairStructure(d, ChassisLocations.RightTorso);
			this.Mech_InstantRepairStructure(d, ChassisLocations.LeftArm);
			this.Mech_InstantRepairStructure(d, ChassisLocations.RightArm);
			this.Mech_InstantRepairStructure(d, ChassisLocations.LeftLeg);
			this.Mech_InstantRepairStructure(d, ChassisLocations.RightLeg);
			foreach (MechComponentRef mechComponentRef in d.Inventory)
			{
				if (mechComponentRef.DamageLevel != ComponentDamageLevel.Functional && mechComponentRef.DamageLevel != ComponentDamageLevel.Installing && mechComponentRef.DamageLevel != ComponentDamageLevel.Destroyed)
				{
					mechComponentRef.SetData(mechComponentRef.MountedLocation, mechComponentRef.HardpointSlot, ComponentDamageLevel.Functional, mechComponentRef.IsFixed);
				}
			}
		}

		// Token: 0x06009291 RID: 37521 RVA: 0x002649D8 File Offset: 0x00262BD8
		public void Mech_InstantRepairStructure(MechDef d, ChassisLocations loc)
		{
			LocationDef chassisLocationDef = d.GetChassisLocationDef(loc);
			d.GetLocationLoadoutDef(loc).CurrentInternalStructure = chassisLocationDef.InternalStructure;
		}

		// Token: 0x06009292 RID: 37522 RVA: 0x00264A00 File Offset: 0x00262C00
		public void Mech_InstantDamageStructure(MechDef d, int damage, ChassisLocations loc)
		{
			LocationDef chassisLocationDef = d.GetChassisLocationDef(loc);
			LocationLoadoutDef locationLoadoutDef = d.GetLocationLoadoutDef(loc);
			locationLoadoutDef.CurrentInternalStructure = Mathf.Clamp(locationLoadoutDef.CurrentInternalStructure - (float)damage, 0f, chassisLocationDef.InternalStructure);
		}

		// Token: 0x06009293 RID: 37523 RVA: 0x00264A3A File Offset: 0x00262C3A
		public void Mech_InstantStripArmor(MechDef d, ChassisLocations loc)
		{
			d.GetChassisLocationDef(loc);
			LocationLoadoutDef locationLoadoutDef = d.GetLocationLoadoutDef(loc);
			locationLoadoutDef.CurrentArmor = 0f;
			locationLoadoutDef.CurrentRearArmor = 0f;
		}

		// Token: 0x17001982 RID: 6530
		// (get) Token: 0x06009294 RID: 37524 RVA: 0x00264A60 File Offset: 0x00262C60
		// (set) Token: 0x06009295 RID: 37525 RVA: 0x00264A68 File Offset: 0x00262C68
		public GameInstance BattleTechGame { get; private set; }

		// Token: 0x17001983 RID: 6531
		// (get) Token: 0x06009296 RID: 37526 RVA: 0x00264A71 File Offset: 0x00262C71
		// (set) Token: 0x06009297 RID: 37527 RVA: 0x00264A79 File Offset: 0x00262C79
		public SimGameConstants Constants { get; private set; }

		// Token: 0x17001984 RID: 6532
		// (get) Token: 0x06009298 RID: 37528 RVA: 0x00264A82 File Offset: 0x00262C82
		// (set) Token: 0x06009299 RID: 37529 RVA: 0x00264A8A File Offset: 0x00262C8A
		public CombatGameConstants CombatConstants { get; private set; }

		// Token: 0x17001985 RID: 6533
		// (get) Token: 0x0600929A RID: 37530 RVA: 0x00264A93 File Offset: 0x00262C93
		public MessageCenter MessageCenter
		{
			get
			{
				return this.BattleTechGame.MessageCenter;
			}
		}

		// Token: 0x17001986 RID: 6534
		// (get) Token: 0x0600929B RID: 37531 RVA: 0x00264AA0 File Offset: 0x00262CA0
		public DataManager DataManager
		{
			get
			{
				return this.BattleTechGame.DataManager;
			}
		}

		// Token: 0x17001987 RID: 6535
		// (get) Token: 0x0600929C RID: 37532 RVA: 0x00264AAD File Offset: 0x00262CAD
		// (set) Token: 0x0600929D RID: 37533 RVA: 0x00264AB5 File Offset: 0x00262CB5
		public NetworkRandom NetworkRandom { get; protected set; }

		// Token: 0x17001988 RID: 6536
		// (get) Token: 0x0600929E RID: 37534 RVA: 0x00264ABE File Offset: 0x00262CBE
		public MessageRecorder MessageRecorder
		{
			get
			{
				return this.BattleTechGame.MessageRecorder;
			}
		}

		// Token: 0x17001989 RID: 6537
		// (get) Token: 0x0600929F RID: 37535 RVA: 0x00264ACB File Offset: 0x00262CCB
		// (set) Token: 0x060092A0 RID: 37536 RVA: 0x00264AD3 File Offset: 0x00262CD3
		public SimGameCameraController CameraController { get; protected set; }

		// Token: 0x1700198A RID: 6538
		// (get) Token: 0x060092A1 RID: 37537 RVA: 0x00264ADC File Offset: 0x00262CDC
		// (set) Token: 0x060092A2 RID: 37538 RVA: 0x00264AE4 File Offset: 0x00262CE4
		public SimGameSpaceController SpaceController { get; protected set; }

		// Token: 0x1700198B RID: 6539
		// (get) Token: 0x060092A3 RID: 37539 RVA: 0x00264AED File Offset: 0x00262CED
		// (set) Token: 0x060092A4 RID: 37540 RVA: 0x00264AF5 File Offset: 0x00262CF5
		public SGUIParent UIParent { get; protected set; }

		// Token: 0x1700198C RID: 6540
		// (get) Token: 0x060092A5 RID: 37541 RVA: 0x00264AFE File Offset: 0x00262CFE
		// (set) Token: 0x060092A6 RID: 37542 RVA: 0x00264B06 File Offset: 0x00262D06
		public SimGameConversationManager ConversationManager { get; protected set; }

		// Token: 0x1700198D RID: 6541
		// (get) Token: 0x060092A7 RID: 37543 RVA: 0x00264B0F File Offset: 0x00262D0F
		// (set) Token: 0x060092A8 RID: 37544 RVA: 0x00264B17 File Offset: 0x00262D17
		public SGSimBattleWidget BattleSimPanel { get; protected set; }

		// Token: 0x1700198E RID: 6542
		// (get) Token: 0x060092A9 RID: 37545 RVA: 0x00264B20 File Offset: 0x00262D20
		// (set) Token: 0x060092AA RID: 37546 RVA: 0x00264B28 File Offset: 0x00262D28
		public SGDebugEventWidget DebugWidget { get; protected set; }

		// Token: 0x1700198F RID: 6543
		// (get) Token: 0x060092AB RID: 37547 RVA: 0x00264B31 File Offset: 0x00262D31
		// (set) Token: 0x060092AC RID: 37548 RVA: 0x00264B39 File Offset: 0x00262D39
		public SGDialogWidget DialogPanel { get; protected set; }

		// Token: 0x17001990 RID: 6544
		// (get) Token: 0x060092AD RID: 37549 RVA: 0x00264B42 File Offset: 0x00262D42
		// (set) Token: 0x060092AE RID: 37550 RVA: 0x00264B4A File Offset: 0x00262D4A
		public UITitleScreenOverlay TitleOverlay { get; protected set; }

		// Token: 0x17001991 RID: 6545
		// (get) Token: 0x060092AF RID: 37551 RVA: 0x00264B53 File Offset: 0x00262D53
		// (set) Token: 0x060092B0 RID: 37552 RVA: 0x00264B5B File Offset: 0x00262D5B
		public UISummaryScreenOverlay SummaryOverlay { get; protected set; }

		// Token: 0x17001992 RID: 6546
		// (get) Token: 0x060092B1 RID: 37553 RVA: 0x00264B64 File Offset: 0x00262D64
		// (set) Token: 0x060092B2 RID: 37554 RVA: 0x00264B6C File Offset: 0x00262D6C
		public bool VideoPlayerActive { get; protected set; }

		// Token: 0x17001993 RID: 6547
		// (get) Token: 0x060092B3 RID: 37555 RVA: 0x00264B75 File Offset: 0x00262D75
		// (set) Token: 0x060092B4 RID: 37556 RVA: 0x00264B7D File Offset: 0x00262D7D
		public SGCharacterCreationWidget CharacterCreation { get; protected set; }

		// Token: 0x17001994 RID: 6548
		// (get) Token: 0x060092B5 RID: 37557 RVA: 0x00264B86 File Offset: 0x00262D86
		// (set) Token: 0x060092B6 RID: 37558 RVA: 0x00264B8E File Offset: 0x00262D8E
		public CreditsModule Credits { get; protected set; }

		// Token: 0x17001995 RID: 6549
		// (get) Token: 0x060092B7 RID: 37559 RVA: 0x00264B97 File Offset: 0x00262D97
		// (set) Token: 0x060092B8 RID: 37560 RVA: 0x00264B9F File Offset: 0x00262D9F
		public Starmap Starmap { get; private set; }

		// Token: 0x17001996 RID: 6550
		// (get) Token: 0x060092B9 RID: 37561 RVA: 0x00264BA8 File Offset: 0x00262DA8
		// (set) Token: 0x060092BA RID: 37562 RVA: 0x00264BB0 File Offset: 0x00262DB0
		public string DebugSeedFolder { get; private set; }

		// Token: 0x060092BB RID: 37563 RVA: 0x00264BB9 File Offset: 0x00262DB9
		public void SetStarmap(Starmap map)
		{
			if (map == null)
			{
				return;
			}
			this.Starmap = map;
		}

		// Token: 0x060092BC RID: 37564 RVA: 0x00264BCC File Offset: 0x00262DCC
		public void SetBattleSimPanel(SGSimBattleWidget panel)
		{
			this.BattleSimPanel = panel;
			panel.gameObject.SetActive(false);
		}

		// Token: 0x060092BD RID: 37565 RVA: 0x00264BE1 File Offset: 0x00262DE1
		public void SetDebugWidget(SGDebugEventWidget panel)
		{
			this.DebugWidget = panel;
		}

		// Token: 0x060092BE RID: 37566 RVA: 0x00264BEA File Offset: 0x00262DEA
		public void SetTitleOverlay(UITitleScreenOverlay panel)
		{
			this.TitleOverlay = panel;
		}

		// Token: 0x060092BF RID: 37567 RVA: 0x00264BF3 File Offset: 0x00262DF3
		public void SetDialogPanel(SGDialogWidget panel)
		{
			this.DialogPanel = panel;
		}

		// Token: 0x060092C0 RID: 37568 RVA: 0x00264BFC File Offset: 0x00262DFC
		public void SetSummaryOverlay(UISummaryScreenOverlay panel)
		{
			this.SummaryOverlay = panel;
		}

		// Token: 0x060092C1 RID: 37569 RVA: 0x00264C05 File Offset: 0x00262E05
		public SGVideoPlayer GetVideoPlayer()
		{
			return LazySingletonBehavior<UIManager>.Instance.GetOrCreatePopupModule<SGVideoPlayer>("", true);
		}

		// Token: 0x060092C2 RID: 37570 RVA: 0x00264C17 File Offset: 0x00262E17
		public void SetCharacterCreationWidget(SGCharacterCreationWidget panel)
		{
			this.CharacterCreation = panel;
			this.CharacterCreation.gameObject.SetActive(false);
		}

		// Token: 0x060092C3 RID: 37571 RVA: 0x00264C31 File Offset: 0x00262E31
		public void SetCreditsModule(CreditsModule panel)
		{
			this.Credits = panel;
		}

		// Token: 0x060092C4 RID: 37572 RVA: 0x00264C3A File Offset: 0x00262E3A
		public void SetCameraControllers(SimGameCameraController controller)
		{
			this.CameraController = controller;
		}

		// Token: 0x060092C5 RID: 37573 RVA: 0x00264C43 File Offset: 0x00262E43
		public void SetSpaceController(SimGameSpaceController controller)
		{
			this.SpaceController = controller;
		}

		// Token: 0x060092C6 RID: 37574 RVA: 0x00264C4C File Offset: 0x00262E4C
		public void SetUIParent(SGUIParent parent)
		{
			this.UIParent = parent;
		}

		// Token: 0x060092C7 RID: 37575 RVA: 0x00264C55 File Offset: 0x00262E55
		public void SetTimeMoving(bool isMoving, bool updateMusic = true)
		{
			this.RoomManager.ShipRoom.SetTimeMoving(isMoving, updateMusic);
		}

		// Token: 0x060092C8 RID: 37576 RVA: 0x00264C69 File Offset: 0x00262E69
		public int GetMaxMechWarriors()
		{
			return this.Constants.Story.MaxMechWarriorsPerPod * this.companyStats.GetValue<int>(this.Constants.Story.BarracksPodsID);
		}

		// Token: 0x060092C9 RID: 37577 RVA: 0x00264C98 File Offset: 0x00262E98
		public int GetMechWarriorValue(PilotDef def)
		{
			int num = def.BaseGunnery;
			num += def.BasePiloting;
			num += def.BaseGuts;
			num += def.BaseTactics;
			int num2 = Mathf.CeilToInt((float)num * this.Constants.Finances.MechWarriorBaseCostPerPoint);
			num = def.BonusGunnery;
			num += def.BonusPiloting;
			num += def.BonusGuts;
			num += def.BonusTactics;
			return num2 + Mathf.CeilToInt((float)num * this.Constants.Finances.MechWarriorBonusCostPerPoint);
		}

		// Token: 0x060092CA RID: 37578 RVA: 0x00264D1C File Offset: 0x00262F1C
		public string GetMechWarriorSalary(PilotDef def)
		{
			if (def.IsFree)
			{
				return "-------";
			}
			float num = (float)this.GetMechWarriorValue(def);
			num *= this.GetExpenditureCostModifier(this.ExpenditureLevel);
			return Strings.T("{0} / Mo", new object[] { SimGameState.GetCBillString(Mathf.RoundToInt(num)) });
		}

		// Token: 0x060092CB RID: 37579 RVA: 0x00264D6D File Offset: 0x00262F6D
		public int GetMechWarriorHiringCost(PilotDef def)
		{
			return Mathf.CeilToInt((float)(def.BaseGunnery + def.BasePiloting + def.BaseGuts + def.BaseTactics) * this.Constants.Finances.MechWarriorHiringCostPerPoint);
		}

		// Token: 0x060092CC RID: 37580 RVA: 0x00264DA1 File Offset: 0x00262FA1
		public int GetMechWarriorPowerLevel(Pilot pilot)
		{
			return pilot.Guts + pilot.Tactics + pilot.Piloting + pilot.Gunnery;
		}

		// Token: 0x060092CD RID: 37581 RVA: 0x00264DC0 File Offset: 0x00262FC0
		public bool CanMechWarriorBeHiredAccordingToMRBRating(Pilot pilot)
		{
			int currentMRBLevel = this.GetCurrentMRBLevel();
			return (float)this.GetMechWarriorPowerLevel(pilot) <= this.Constants.Story.MRBRepHiringPowerLevelLimits[currentMRBLevel];
		}

		// Token: 0x060092CE RID: 37582 RVA: 0x00264DF4 File Offset: 0x00262FF4
		public bool CanMechWarriorBeHiredAccordingToMorale(Pilot pilot)
		{
			this.GetCurrentMRBLevel();
			int mechWarriorPowerLevel = this.GetMechWarriorPowerLevel(pilot);
			int moraleHiringLevelIndex = this.GetMoraleHiringLevelIndex();
			return moraleHiringLevelIndex < 0 || moraleHiringLevelIndex >= this.Constants.Story.MaxMoralePowerLevelLimits.Length || (float)mechWarriorPowerLevel <= this.Constants.Story.MaxMoralePowerLevelLimits[moraleHiringLevelIndex];
		}

		// Token: 0x060092CF RID: 37583 RVA: 0x00264E4C File Offset: 0x0026304C
		public int IndexOfMRBRatingToHireMechWarrior(Pilot pilot)
		{
			int mechWarriorPowerLevel = this.GetMechWarriorPowerLevel(pilot);
			int num = 0;
			while (num < this.Constants.Story.MRBRepHiringPowerLevelLimits.Length && (float)mechWarriorPowerLevel > this.Constants.Story.MRBRepHiringPowerLevelLimits[num])
			{
				num++;
			}
			return Mathf.Min(num, this.Constants.Story.MRBRepHiringPowerLevelLimits.Length - 1);
		}

		// Token: 0x060092D0 RID: 37584 RVA: 0x00264EB4 File Offset: 0x002630B4
		public void RefreshInjuries()
		{
			List<Pilot> list = new List<Pilot>(this.PilotRoster);
			list.Add(this.commander);
			foreach (Pilot pilot in list)
			{
				if (pilot.GetOutOfActionTime() > 0)
				{
					int injuryCost = this.GetInjuryCost(pilot);
					WorkOrderEntry_MedBayHeal workOrderEntry_MedBayHeal;
					if (!this.MedBayQueue.SubEntryContainsID(pilot.Description.Id))
					{
						workOrderEntry_MedBayHeal = new WorkOrderEntry_MedBayHeal(pilot.Description.Id, Strings.T("{0} out of action.", new object[] { pilot.Callsign }), injuryCost, pilot, this.MedBayQueue, Strings.T(this.Constants.Story.MedBayWorkOrderCompletedText, new object[] { pilot.Callsign }), 0);
						this.RoomManager.AddWorkQueueEntry(workOrderEntry_MedBayHeal);
					}
					else
					{
						workOrderEntry_MedBayHeal = (WorkOrderEntry_MedBayHeal)this.MedBayQueue.GetSubEntry(pilot.Description.Id);
						int cost = workOrderEntry_MedBayHeal.GetCost();
						if (cost != injuryCost)
						{
							int num = workOrderEntry_MedBayHeal.GetCostPaid();
							int timeoutRemaining = pilot.pilotDef.TimeoutRemaining;
							this.RoomManager.RemoveWorkQueueEntry(workOrderEntry_MedBayHeal, true);
							this.MedBayQueue.RemoveSubEntry(pilot.Description.Id);
							if (timeoutRemaining > 0 || injuryCost - num > 0)
							{
								if (timeoutRemaining > 0)
								{
									float num2 = (float)cost / (float)timeoutRemaining;
									float num3 = (float)num / num2;
									num = Mathf.RoundToInt((float)this.GetDailyHealValue() * num3);
								}
								workOrderEntry_MedBayHeal = new WorkOrderEntry_MedBayHeal(pilot.Description.Id, Strings.T("{0} out of action.", new object[] { pilot.Callsign }), injuryCost, pilot, this.MedBayQueue, Strings.T(this.Constants.Story.MedBayWorkOrderCompletedText, new object[] { pilot.Callsign }), num);
							}
						}
						if (workOrderEntry_MedBayHeal.IsCostPaid())
						{
							pilot.ClearInjuries("MedBay", 0, "MedBay");
							pilot.pilotDef.SetTimeoutTime(0);
							pilot.ForceRefreshDef();
							this.RoomManager.RemoveWorkQueueEntry(workOrderEntry_MedBayHeal, false);
							this.MedBayQueue.RemoveSubEntry(pilot.Description.Id);
							this.LogReport(string.Format("{0} is healed from injuries!", pilot.Description.Id));
							this.RefreshInjuries();
							return;
						}
					}
					if (this.RoomManager.GetWorkQueueEntry(workOrderEntry_MedBayHeal) == null)
					{
						this.RoomManager.AddWorkQueueEntry(workOrderEntry_MedBayHeal);
					}
				}
				else if (this.MedBayQueue.SubEntryContainsID(pilot.Description.Id))
				{
					WorkOrderEntry_MedBayHeal workOrderEntry_MedBayHeal2 = (WorkOrderEntry_MedBayHeal)this.MedBayQueue.GetSubEntry(pilot.Description.Id);
					this.RoomManager.RemoveWorkQueueEntry(workOrderEntry_MedBayHeal2, false);
					this.MedBayQueue.RemoveSubEntry(pilot.Description.Id);
				}
			}
			for (int i = this.MedBayQueue.SubEntryCount - 1; i >= 0; i--)
			{
				WorkOrderEntry_MedBayHeal workOrderEntry_MedBayHeal3 = this.MedBayQueue.SubEntries[i] as WorkOrderEntry_MedBayHeal;
				Pilot pilot2 = workOrderEntry_MedBayHeal3.Pilot;
				if (!list.Contains(pilot2))
				{
					this.RoomManager.RemoveWorkQueueEntry(workOrderEntry_MedBayHeal3, false);
					this.MedBayQueue.RemoveSubEntry(pilot2.Description.Id);
				}
			}
			this.RoomManager.RefreshTimeline(false);
		}

		// Token: 0x060092D1 RID: 37585 RVA: 0x00265220 File Offset: 0x00263420
		public int GetPilotTimeoutTimeRemaining(Pilot p)
		{
			if (this.MedBayQueue.SubEntryContainsID(p.Description.Id))
			{
				float num = (float)((WorkOrderEntry_MedBayHeal)this.MedBayQueue.GetSubEntry(p.Description.Id)).GetRemainingCost();
				float num2 = (float)this.GetDailyHealValue();
				return Mathf.CeilToInt(num / num2);
			}
			if (p.pilotDef.Injuries > 0)
			{
				return this.GetInjuryTime(p);
			}
			if (p.pilotDef.TimeoutRemaining > 0)
			{
				return p.pilotDef.TimeoutRemaining;
			}
			return 0;
		}

		// Token: 0x060092D2 RID: 37586 RVA: 0x002652A8 File Offset: 0x002634A8
		private int GetInjuryCost(Pilot p)
		{
			int num = 0;
			if (p.LethalInjuries)
			{
				num += this.Constants.Pilot.LethalDamageCost;
			}
			if (p.IsIncapacitated)
			{
				num += this.Constants.Pilot.IncapacitatedDamageCost;
			}
			int num2 = Mathf.Min(p.Injuries, p.Health);
			int num3 = this.Constants.Pilot.BaseInjuryDamageCost / p.Health;
			for (int i = 0; i < num2; i++)
			{
				num += num3;
			}
			return Mathf.Max(num, p.pilotDef.TimeoutRemaining * this.GetDailyHealValue());
		}

		// Token: 0x060092D3 RID: 37587 RVA: 0x0026533F File Offset: 0x0026353F
		public int GetDailyHealValue()
		{
			return this.Constants.Story.DailyHealValue + this.MedTechSkill * this.Constants.Story.MedTechSkillMod;
		}

		// Token: 0x060092D4 RID: 37588 RVA: 0x0026536C File Offset: 0x0026356C
		public void UpdateInjuries()
		{
			for (int i = 0; i < this.MedBayQueue.SubEntryCount; i++)
			{
				this.MedBayQueue[i].PayCost(this.GetDailyHealValue());
			}
			this.RefreshInjuries();
		}

		// Token: 0x060092D5 RID: 37589 RVA: 0x002653B0 File Offset: 0x002635B0
		public int GetInjuryTime(Pilot p)
		{
			float num = (float)this.GetInjuryCost(p);
			float num2 = (float)this.GetDailyHealValue();
			return Mathf.CeilToInt(num / num2);
		}

		// Token: 0x060092D6 RID: 37590 RVA: 0x002653D4 File Offset: 0x002635D4
		public int GetLevelCost(int potentialLevel)
		{
			return Mathf.CeilToInt(Mathf.Pow((float)potentialLevel, this.Constants.Pilot.PilotLevelCostExponent) * this.Constants.Pilot.PilotLevelCostMultiplier);
		}

		// Token: 0x060092D7 RID: 37591 RVA: 0x00265404 File Offset: 0x00263604
		public int GetLevelRangeCost(int minLevel, int maxLevel)
		{
			int num = 0;
			for (int i = minLevel; i <= maxLevel; i++)
			{
				num += this.GetLevelCost(i);
			}
			return num;
		}

		// Token: 0x060092D8 RID: 37592 RVA: 0x0026542C File Offset: 0x0026362C
		public bool DismissPilot(string pilotID)
		{
			Pilot pilot = this.PilotRoster.Find((Pilot x) => x.pilotDef.Description.Id == pilotID);
			return this.DismissPilot(pilot);
		}

		// Token: 0x060092D9 RID: 37593 RVA: 0x00265468 File Offset: 0x00263668
		public bool DismissPilot(Pilot p)
		{
			if (p == null || !this.PilotRoster.Contains(p))
			{
				return false;
			}
			this.companyStats.ModifyStat<int>("Mission", 0, "COMPANY_MechWarriorsFired", StatCollection.StatOperation.Int_Add, 1, -1, true);
			this.PilotRoster.Remove(p);
			this.RefreshInjuries();
			this.RoomManager.RefreshDisplay();
			SimGameMechWarriorPersonnelChangeMessage simGameMechWarriorPersonnelChangeMessage = new SimGameMechWarriorPersonnelChangeMessage(p, SimGameMechWarriorPersonnelChangeMessage.PersonnelChangeType.FIRED);
			this.MessageCenter.PublishMessage(simGameMechWarriorPersonnelChangeMessage);
			return true;
		}

		// Token: 0x060092DA RID: 37594 RVA: 0x002654D5 File Offset: 0x002636D5
		public bool AddPilotToRoster(string pilotDefID)
		{
			if (string.IsNullOrEmpty(pilotDefID))
			{
				return false;
			}
			this.RequestItem<PilotDef>(pilotDefID, delegate(PilotDef obj)
			{
				if (obj == null)
				{
					return;
				}
				this.AddPilotToRoster(obj, false, false);
			}, BattleTechResourceType.PilotDef);
			return true;
		}

		// Token: 0x060092DB RID: 37595 RVA: 0x002654F8 File Offset: 0x002636F8
		public bool AddPilotToHiringHall(string pilotDefID, string starSystemID)
		{
			if (string.IsNullOrEmpty(pilotDefID))
			{
				return false;
			}
			if (!this.starDict.ContainsKey(starSystemID))
			{
				return false;
			}
			StarSystem system = this.starDict[starSystemID];
			this.RequestItem<PilotDef>(pilotDefID, delegate(PilotDef obj)
			{
				if (obj == null)
				{
					return;
				}
				Debug.Log(string.Format("Added {0} to hiring hall", obj.Description.Name));
				this.AddPilotToHiringHall(obj, system);
			}, BattleTechResourceType.PilotDef);
			return true;
		}

		// Token: 0x060092DC RID: 37596 RVA: 0x00265554 File Offset: 0x00263754
		public bool AddPilotToHiringHall(PilotDef def, StarSystem system)
		{
			if (def == null || system == null)
			{
				return false;
			}
			system.AddAvailablePilot(def, true);
			return true;
		}

		// Token: 0x060092DD RID: 37597 RVA: 0x00265568 File Offset: 0x00263768
		public bool AddPilotToRoster(PilotDef def, bool updatePilotDiscardPile = false, bool initialHiringDontSpawnMessage = false)
		{
			if (def == null)
			{
				return false;
			}
			if (def.IsRonin)
			{
				this.UsedRoninIDs.Add(def.Description.Id);
				def = def.CopyToSim();
			}
			else if (updatePilotDiscardPile)
			{
				this.pilotGenVoiceDiscardPile.Add(def.Voice);
			}
			this.companyStats.ModifyStat<int>("Mission", 0, "COMPANY_MechWarriorsHired", StatCollection.StatOperation.Int_Add, 1, -1, true);
			Pilot pilot = new Pilot(def, this.GenerateSimGameUID(), true);
			this.PilotRoster.Add(pilot, 0);
			if (!string.IsNullOrEmpty(def.Description.Icon))
			{
				LoadRequest loadRequest = this.DataManager.CreateLoadRequest(null, false);
				loadRequest.AddBlindLoadRequest(BattleTechResourceType.Sprite, def.Description.Icon, new bool?(false));
				loadRequest.ProcessRequests(10U);
			}
			else if (def.PortraitSettings != null)
			{
				def.PortraitSettings.RenderPortrait(this.DataManager, null, Array.Empty<PortraitManager.PortraitSizes>());
			}
			if (!initialHiringDontSpawnMessage)
			{
				SimGameMechWarriorPersonnelChangeMessage simGameMechWarriorPersonnelChangeMessage = new SimGameMechWarriorPersonnelChangeMessage(pilot, SimGameMechWarriorPersonnelChangeMessage.PersonnelChangeType.HIRED);
				this.MessageCenter.PublishMessage(simGameMechWarriorPersonnelChangeMessage);
				ReportMechwarriorHiredMessage reportMechwarriorHiredMessage = new ReportMechwarriorHiredMessage(pilot);
				this.MessageCenter.PublishMessage(reportMechwarriorHiredMessage);
			}
			return true;
		}

		// Token: 0x060092DE RID: 37598 RVA: 0x00265678 File Offset: 0x00263878
		public Pilot GetPilot(string pilotID)
		{
			if (string.IsNullOrEmpty(pilotID))
			{
				return null;
			}
			Pilot pilot = this.PilotRoster.Find((Pilot x) => x.pilotDef.Description.Id == pilotID);
			if (pilot == null && this.commander.pilotDef.Description.Id == pilotID)
			{
				pilot = this.commander;
			}
			return pilot;
		}

		// Token: 0x060092DF RID: 37599 RVA: 0x002656E8 File Offset: 0x002638E8
		public bool KillPilot(string pilotID, bool fromEvent = false, string causeOfDeathOverride = null)
		{
			Pilot pilot = this.PilotRoster.Find((Pilot x) => x.pilotDef.Description.Id == pilotID);
			return this.KillPilot(pilot, fromEvent, null, causeOfDeathOverride);
		}

		// Token: 0x060092E0 RID: 37600 RVA: 0x00265724 File Offset: 0x00263924
		public bool IsDekkerAlive()
		{
			return this.PilotRoster.Find((Pilot x) => x.pilotDef.Description.Id == "pilot_sim_starter_dekker") != null;
		}

		// Token: 0x060092E1 RID: 37601 RVA: 0x00265758 File Offset: 0x00263958
		public bool KillPilot(Pilot p, bool fromEvent = false, string StarSystemID = null, string causeOfDeathOverride = null)
		{
			if (p == null || !this.PilotRoster.Contains(p))
			{
				return false;
			}
			PilotDef pilotDef = p.pilotDef;
			if (pilotDef != null)
			{
				pilotDef.SetDayOfDeath(this.daysPassed);
				if (fromEvent || pilotDef.RecentInjuryDamageType == DamageType.NOT_SET)
				{
					if (string.IsNullOrEmpty(causeOfDeathOverride))
					{
						pilotDef.GenerateMisfortuneDeathDescription();
					}
					else
					{
						pilotDef.SetRecentInjuryDamageType(DamageType.OverrideString);
						pilotDef.SetRecentInjuryOverrideString(causeOfDeathOverride);
					}
				}
				if (StarSystemID == null)
				{
					pilotDef.SetDiedInSystemID(this.CurSystem.ID);
				}
			}
			this.Graveyard.Add(p, 0);
			this.PilotRoster.Remove(p);
			SimGameMechWarriorPersonnelChangeMessage simGameMechWarriorPersonnelChangeMessage = new SimGameMechWarriorPersonnelChangeMessage(p, SimGameMechWarriorPersonnelChangeMessage.PersonnelChangeType.KILLED);
			this.MessageCenter.PublishMessage(simGameMechWarriorPersonnelChangeMessage);
			this.interruptQueue.QueueMechwarriorDeathEntry("MechWarrior Casualty", Strings.T("{0} has died.", new object[] { p.Name }), Array.Empty<GenericPopupButtonSettings>());
			return true;
		}

		// Token: 0x060092E2 RID: 37602 RVA: 0x0026582C File Offset: 0x00263A2C
		public PilotDef GetUnusedRonin()
		{
			List<PilotDef> list = new List<PilotDef>(this.RoninPilots);
			list.Shuffle<PilotDef>();
			while (list.Count > 0)
			{
				if (!this.usedRoninIDs.Contains(list[0].Description.Id) && this.IsRoninWhitelisted(list[0]))
				{
					return list[0];
				}
				list.RemoveAt(0);
			}
			return null;
		}

		// Token: 0x060092E3 RID: 37603 RVA: 0x00265893 File Offset: 0x00263A93
		public bool IsRoninWhitelisted(PilotDef p)
		{
			return !p.RoninRequiresWhitelist || this.WhitelistedRonin.Contains(p.Description.Id);
		}

		// Token: 0x060092E4 RID: 37604 RVA: 0x002658B8 File Offset: 0x00263AB8
		public void AddTech(TechDef def, bool mechTech = true)
		{
			string text;
			List<TechDef> list;
			if (mechTech)
			{
				text = "MechTechSkill";
				list = this.MechTechs;
			}
			else
			{
				text = "MedTechSkill";
				list = this.MedTechs;
			}
			list.Add(def);
			if (!this.companyStats.ContainsStatistic(text))
			{
				this.companyStats.AddStatistic<int>(text, def.Skill);
			}
			else
			{
				this.companyStats.ModifyStat<int>("SimGame", 0, text, StatCollection.StatOperation.Int_Add, def.Skill, -1, true);
			}
			this.RoomManager.RefreshTimeline(!mechTech);
		}

		// Token: 0x060092E5 RID: 37605 RVA: 0x0026593C File Offset: 0x00263B3C
		public void RemoveTech(TechDef def)
		{
			if (def == null)
			{
				return;
			}
			bool flag = false;
			string text;
			if (this.MechTechs.Contains(def))
			{
				text = "MechTechSkill";
				this.MechTechs.Remove(def);
			}
			else
			{
				if (!this.MedTechs.Contains(def))
				{
					return;
				}
				text = "MedTechSkill";
				this.MedTechs.Remove(def);
				flag = true;
			}
			this.companyStats.ModifyStat<int>("SimGame", 0, text, StatCollection.StatOperation.Int_Subtract, def.Skill, -1, true);
			this.RoomManager.RefreshTimeline(flag);
		}

		// Token: 0x060092E6 RID: 37606 RVA: 0x002659C0 File Offset: 0x00263BC0
		public void RemoveTech(string id)
		{
			for (int i = 0; i < this.MechTechs.Count; i++)
			{
				if (this.MechTechs[i].Description.Id.CompareTo(id) == 0)
				{
					this.RemoveTech(this.MechTechs[i]);
					return;
				}
			}
			for (int j = 0; j < this.MedTechs.Count; j++)
			{
				if (this.MedTechs[j].Description.Id.CompareTo(id) == 0)
				{
					this.RemoveTech(this.MedTechs[j]);
					return;
				}
			}
		}

		// Token: 0x060092E7 RID: 37607 RVA: 0x00265A5C File Offset: 0x00263C5C
		public int GetMaxTechs(bool mechTechs)
		{
			string text;
			if (mechTechs)
			{
				text = this.Constants.Story.MaxMechTechID;
			}
			else
			{
				text = this.Constants.Story.MaxMedTechID;
			}
			int num = 0;
			if (this.companyStats.ContainsStatistic(text))
			{
				num = this.companyStats.GetValue<int>(text);
			}
			return num;
		}

		// Token: 0x060092E8 RID: 37608 RVA: 0x00265AAE File Offset: 0x00263CAE
		public int GetTechHiringCost(TechDef def)
		{
			return Mathf.CeilToInt((float)def.Skill * this.Constants.Finances.TechHiringCostPerPoint);
		}

		// Token: 0x060092E9 RID: 37609 RVA: 0x00265AD0 File Offset: 0x00263CD0
		public SVGAsset GetPilotVeteranIcon(Pilot p)
		{
			int num = 1;
			List<AbilityDef> primaryPilotAbilities = SimGameState.GetPrimaryPilotAbilities(p.pilotDef);
			if (p.Piloting == 10 || p.Gunnery == 10 || p.Guts == 10 || p.Tactics == 10)
			{
				num = 5;
			}
			else if (primaryPilotAbilities.Count > 0)
			{
				num = primaryPilotAbilities.Count + 1;
			}
			return this.DataManager.SVGCache.GetAsset(string.Format("{0}{1}{2}", "uixSvgIcon_mwrank_", "Rank", num));
		}

		// Token: 0x060092EA RID: 37610 RVA: 0x00265B54 File Offset: 0x00263D54
		public int GetPilotRank(Pilot p)
		{
			int num = 1;
			List<AbilityDef> primaryPilotAbilities = SimGameState.GetPrimaryPilotAbilities(p.pilotDef);
			if (p.Piloting == 10 || p.Gunnery == 10 || p.Guts == 10 || p.Tactics == 10)
			{
				num = 5;
			}
			else if (primaryPilotAbilities.Count > 0)
			{
				num = primaryPilotAbilities.Count + 1;
			}
			return num;
		}

		// Token: 0x060092EB RID: 37611 RVA: 0x00265BB0 File Offset: 0x00263DB0
		public SVGAsset GetPilotRoninIcon(Pilot p)
		{
			if (p.Description.Id == this.commander.Description.Id)
			{
				return this.DataManager.SVGCache.GetAsset("uixSvgIcon_mwrank_Commander");
			}
			if (p.pilotDef.IsVanguard)
			{
				return this.DataManager.SVGCache.GetAsset("uixSvgIcon_mwrank_KSBacker");
			}
			if (p.pilotDef.IsRonin)
			{
				return this.DataManager.SVGCache.GetAsset("uixSvgIcon_mwrank_Ronin");
			}
			return null;
		}

		// Token: 0x060092EC RID: 37612 RVA: 0x00265C3C File Offset: 0x00263E3C
		public static int GetTotalPilotSkill(Pilot p)
		{
			return p.Piloting + p.Gunnery + p.Guts + p.Tactics;
		}

		// Token: 0x060092ED RID: 37613 RVA: 0x00265C5C File Offset: 0x00263E5C
		public string GetPrimaryPilotStatString(Pilot p)
		{
			Dictionary<SkillType, int> sortedSkillCount = this.GetSortedSkillCount(p.pilotDef);
			List<SkillType> list = new List<SkillType>();
			List<SkillType> list2 = new List<SkillType>();
			foreach (SkillType skillType in sortedSkillCount.Keys)
			{
				if (sortedSkillCount[skillType] == 1)
				{
					list2.Add(skillType);
				}
				else if (sortedSkillCount[skillType] == 2)
				{
					list.Add(skillType);
				}
			}
			if (list.Count >= 1)
			{
				return list[0].ToString();
			}
			if (list2.Count == 1)
			{
				return list2[0].ToString();
			}
			if (list2.Count > 1)
			{
				return SimGameState.GetPrimaryPilotAbilities(p.pilotDef)[0].ReqSkill.ToString();
			}
			return "";
		}

		// Token: 0x060092EE RID: 37614 RVA: 0x00265D60 File Offset: 0x00263F60
		public string GetSecondaryPilotStatString(Pilot p, bool allowEqual)
		{
			Dictionary<SkillType, int> sortedSkillCount = this.GetSortedSkillCount(p.pilotDef);
			List<SkillType> list = new List<SkillType>();
			List<SkillType> list2 = new List<SkillType>();
			foreach (SkillType skillType in sortedSkillCount.Keys)
			{
				if (sortedSkillCount[skillType] == 1)
				{
					list2.Add(skillType);
				}
				else if (sortedSkillCount[skillType] == 2)
				{
					list.Add(skillType);
				}
			}
			if (list.Count >= 1)
			{
				if (list2.Count > 0)
				{
					return list2[0].ToString();
				}
			}
			else if (allowEqual && list2.Count > 1)
			{
				List<AbilityDef> primaryPilotAbilities = SimGameState.GetPrimaryPilotAbilities(p.pilotDef);
				SkillType? skillType2 = null;
				foreach (AbilityDef abilityDef in primaryPilotAbilities)
				{
					if (skillType2 == null)
					{
						skillType2 = new SkillType?(abilityDef.ReqSkill);
					}
					else if (abilityDef.ReqSkill != skillType2.Value)
					{
						return abilityDef.ReqSkill.ToString();
					}
				}
			}
			return "";
		}

		// Token: 0x060092EF RID: 37615 RVA: 0x00265EC4 File Offset: 0x002640C4
		public string GetPilotTypeString(Pilot p)
		{
			string primaryPilotStatString = this.GetPrimaryPilotStatString(p);
			if (string.IsNullOrEmpty(primaryPilotStatString))
			{
				return this.Constants.Progression.GeneralistName;
			}
			string text = this.GetSecondaryPilotStatString(p, false);
			if (string.IsNullOrEmpty(text))
			{
				text = primaryPilotStatString;
			}
			List<string> list = new List<string>(this.Constants.Progression.SkillOrder);
			string[][] expertiseNames = this.Constants.Progression.ExpertiseNames;
			int num = list.IndexOf(primaryPilotStatString);
			int num2 = list.IndexOf(text);
			return expertiseNames[num][num2];
		}

		// Token: 0x060092F0 RID: 37616 RVA: 0x00265F44 File Offset: 0x00264144
		public static List<AbilityDef> GetPrimaryPilotAbilities(PilotDef p)
		{
			List<AbilityDef> abilityDefs = p.AbilityDefs;
			List<AbilityDef> list = new List<AbilityDef>();
			if (abilityDefs == null)
			{
				return list;
			}
			foreach (AbilityDef abilityDef in abilityDefs)
			{
				if (abilityDef.IsPrimaryAbility)
				{
					list.Add(abilityDef);
				}
			}
			return list;
		}

		// Token: 0x060092F1 RID: 37617 RVA: 0x00265FB0 File Offset: 0x002641B0
		public Dictionary<SkillType, int> GetSortedSkillCount(PilotDef p)
		{
			Dictionary<SkillType, int> dictionary = new Dictionary<SkillType, int>();
			foreach (AbilityDef abilityDef in SimGameState.GetPrimaryPilotAbilities(p))
			{
				if (!dictionary.ContainsKey(abilityDef.ReqSkill))
				{
					dictionary.Add(abilityDef.ReqSkill, 1);
				}
				else
				{
					Dictionary<SkillType, int> dictionary2 = dictionary;
					SkillType reqSkill = abilityDef.ReqSkill;
					int num = dictionary2[reqSkill];
					dictionary2[reqSkill] = num + 1;
				}
			}
			return dictionary;
		}

		// Token: 0x060092F2 RID: 37618 RVA: 0x0026603C File Offset: 0x0026423C
		public bool CanPilotTakeAbility(PilotDef p, AbilityDef newAbility, bool checkSecondTier = false)
		{
			if (!newAbility.IsPrimaryAbility)
			{
				return true;
			}
			List<AbilityDef> primaryPilotAbilities = SimGameState.GetPrimaryPilotAbilities(p);
			if (primaryPilotAbilities == null)
			{
				return true;
			}
			if (primaryPilotAbilities.Count >= 3)
			{
				return false;
			}
			Dictionary<SkillType, int> sortedSkillCount = this.GetSortedSkillCount(p);
			return (sortedSkillCount.Count <= 1 || sortedSkillCount.ContainsKey(newAbility.ReqSkill)) && (!sortedSkillCount.ContainsKey(newAbility.ReqSkill) || sortedSkillCount[newAbility.ReqSkill] <= 1) && (!checkSecondTier || sortedSkillCount.ContainsKey(newAbility.ReqSkill) || primaryPilotAbilities.Count <= 1);
		}

		// Token: 0x060092F3 RID: 37619 RVA: 0x002660C7 File Offset: 0x002642C7
		public string GetPilotFullExpertise(Pilot p)
		{
			if (string.IsNullOrEmpty(this.GetPrimaryPilotStatString(p)))
			{
				return this.Constants.Progression.GeneralistName;
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(this.GetPilotTypeString(p));
			return stringBuilder.ToString();
		}

		// Token: 0x060092F4 RID: 37620 RVA: 0x00266100 File Offset: 0x00264300
		public List<AbilityDef> GetAbilitiesByLevel(SkillType t, int level)
		{
			List<AbilityDef> list = new List<AbilityDef>();
			if (level <= 0 || level > 10)
			{
				return list;
			}
			string[][] array;
			switch (t)
			{
			case SkillType.Piloting:
				array = this.Constants.Progression.PilotingSkills;
				break;
			case SkillType.Gunnery:
				array = this.Constants.Progression.GunnerySkills;
				break;
			case SkillType.Guts:
				array = this.Constants.Progression.GutsSkills;
				break;
			case SkillType.Tactics:
				array = this.Constants.Progression.TacticsSkills;
				break;
			default:
				return list;
			}
			foreach (string text in array[level - 1])
			{
				AbilityDef abilityDef = this.DataManager.AbilityDefs.Get(text);
				list.Add(abilityDef);
			}
			return list;
		}

		// Token: 0x060092F5 RID: 37621 RVA: 0x002661BC File Offset: 0x002643BC
		public Dictionary<string, AbilityDef> GetAllVisibleTraitsByLevel(SkillType t, int endLevel, int startLevel = 1)
		{
			Dictionary<string, AbilityDef> dictionary = new Dictionary<string, AbilityDef>();
			if (endLevel <= 0 || endLevel > 10)
			{
				return dictionary;
			}
			for (int i = startLevel; i <= endLevel; i++)
			{
				foreach (AbilityDef abilityDef in this.GetAbilitiesByLevel(t, i))
				{
					if (abilityDef.Type != null)
					{
						dictionary[abilityDef.Type] = abilityDef;
					}
				}
			}
			return dictionary;
		}

		// Token: 0x060092F6 RID: 37622 RVA: 0x00266240 File Offset: 0x00264440
		public bool CanPilotBeCareerModeStarter(PilotDef p)
		{
			return p.BasePiloting <= this.Constants.CareerMode.MaxStartingPilotSkill && p.BaseGunnery <= this.Constants.CareerMode.MaxStartingPilotSkill && p.BaseTactics <= this.Constants.CareerMode.MaxStartingPilotSkill && p.BaseGuts <= this.Constants.CareerMode.MaxStartingPilotSkill;
		}

		// Token: 0x060092F7 RID: 37623 RVA: 0x002662B0 File Offset: 0x002644B0
		public UIColor GetPilotTypeColor(Pilot p)
		{
			if (p.pilotDef.Description.Id == this.commander.Description.Id)
			{
				return UIColor.SimPilotCommander;
			}
			if (p.pilotDef.IsVanguard)
			{
				return UIColor.SimPilotBacker;
			}
			if (p.pilotDef.IsRonin)
			{
				return UIColor.SimPilotRonin;
			}
			return UIColor.SimPilotStandard;
		}

		// Token: 0x060092F8 RID: 37624 RVA: 0x00266309 File Offset: 0x00264509
		public void SetCommander(Pilot p)
		{
			this.commander = p;
		}

		// Token: 0x060092F9 RID: 37625 RVA: 0x00266314 File Offset: 0x00264514
		public void UpgradePilot(Pilot tempPilot)
		{
			if (tempPilot.GUID == this.Commander.GUID)
			{
				this.SetCommander(tempPilot);
				AudioEventManager.PlayAudioEvent("audioeventdef_simgame_vo_barks", "simgeneric_mechwarriortraining", WwiseManager.GlobalAudioObject, null);
			}
			else
			{
				for (int i = 0; i < this.PilotRoster.Count; i++)
				{
					if (this.PilotRoster[i].GUID == tempPilot.GUID)
					{
						ReportMechWarriorSkillUpMessage reportMechWarriorSkillUpMessage = new ReportMechWarriorSkillUpMessage(this.PilotRoster[i], tempPilot);
						this.MessageCenter.PublishMessage(reportMechWarriorSkillUpMessage);
						this.PilotRoster.RemoveAt(i);
						this.PilotRoster.Insert(tempPilot, i, 0);
						AudioEventManager.PlayAudioEvent("audioeventdef_simgame_vo_barks", "simgeneric_mechwarriortraining", WwiseManager.GlobalAudioObject, null);
						break;
					}
				}
			}
			if (this.MedBayQueue.SubEntryContainsID(tempPilot.Description.Id))
			{
				((WorkOrderEntry_MedBayHeal)this.MedBayQueue.GetSubEntry(tempPilot.Description.Id)).SetPilot(tempPilot);
			}
			SimGameMechWarriorSkillUpMessage simGameMechWarriorSkillUpMessage = new SimGameMechWarriorSkillUpMessage(tempPilot);
			this.MessageCenter.PublishMessage(simGameMechWarriorSkillUpMessage);
		}

		// Token: 0x060092FA RID: 37626 RVA: 0x00266430 File Offset: 0x00264630
		public int GetTemporaryTagLength(Pilot p, string tag)
		{
			foreach (TemporarySimGameResult temporarySimGameResult in this.TemporaryResultTracker)
			{
				if (temporarySimGameResult != null && temporarySimGameResult.TargetPilot != null && temporarySimGameResult.TargetPilot.GUID == p.GUID && temporarySimGameResult.AddedTags.Contains(tag))
				{
					return temporarySimGameResult.ResultDuration - temporarySimGameResult.DaysElapsed;
				}
			}
			return 0;
		}

		// Token: 0x060092FB RID: 37627 RVA: 0x002664C0 File Offset: 0x002646C0
		public void VersionUpdateCheck()
		{
			if (this.CompletedContract != null)
			{
				return;
			}
			for (int i = 0; i < this.UpgradeStartingMechWarriors.Length; i++)
			{
				foreach (Pilot pilot in this.PilotRoster)
				{
					if (pilot.pilotDef.Description.Id == this.UpgradeStartingMechWarriors[i])
					{
						pilot.pilotDef.SetHiringHallStats(false, true, false, false);
						if (pilot.pilotDef.IsRonin && !this.usedRoninIDs.Contains(pilot.pilotDef.Description.Id))
						{
							this.usedRoninIDs.Add(pilot.pilotDef.Description.Id);
						}
					}
				}
			}
			TagDataStruct tagDataStruct = null;
			if (this.DoesCommanderNeedRespec(out tagDataStruct))
			{
				this.ForceAllPilotRespec(tagDataStruct);
			}
		}

		// Token: 0x060092FC RID: 37628 RVA: 0x002665B4 File Offset: 0x002647B4
		public bool DoesCommanderNeedRespec(out TagDataStruct reason)
		{
			reason = null;
			if (this.Constants.Story.CampaignCommanderUpdateTags == null || this.Constants.Story.CampaignCommanderUpdateTags.Length == 0)
			{
				return false;
			}
			foreach (string text in this.Constants.Story.CampaignCommanderUpdateTags)
			{
				if (!this.CompanyTags.Contains(text))
				{
					TagDataStruct item = (this.Context.GetObject(GameContextObjectTagEnum.TagDataStructFetcher) as TagDataStructFetcher).GetItem(text, false);
					if (item != null)
					{
						reason = item;
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060092FD RID: 37629 RVA: 0x00266640 File Offset: 0x00264840
		public void ForceAllPilotRespec(TagDataStruct tagReason = null)
		{
			this.RespecPilot(this.commander);
			foreach (Pilot pilot in this.PilotRoster)
			{
				this.RespecPilot(pilot);
			}
			string text = Strings.T("We have changed the balance, thus we're resetting your commander. You can reallocate in the Barracks.");
			string text2 = Strings.T("Commander Reset");
			if (tagReason != null)
			{
				text = tagReason.DescriptionTag;
				text2 = tagReason.FriendlyName;
			}
			this.interruptQueue.QueueGenericPopup_NonImmediate(text2, text, false, Array.Empty<GenericPopupButtonSettings>()).AddButton("Continue", null, true, null);
			if (this.CompletedContract == null)
			{
				this._forceInterruptCheck = true;
			}
		}

		// Token: 0x060092FE RID: 37630 RVA: 0x002666F0 File Offset: 0x002648F0
		private void RespecPilot(Pilot pilot)
		{
			PilotDef pilotDef = pilot.pilotDef.CopyToSim();
			foreach (string text in this.Constants.Story.CampaignCommanderUpdateTags)
			{
				if (!this.CompanyTags.Contains(text))
				{
					this.CompanyTags.Add(text);
				}
			}
			int num = 0;
			if (pilotDef.BonusPiloting > 0)
			{
				num += this.GetLevelRangeCost(pilotDef.BasePiloting, pilotDef.SkillPiloting - 1);
			}
			if (pilotDef.BonusGunnery > 0)
			{
				num += this.GetLevelRangeCost(pilotDef.BaseGunnery, pilotDef.SkillGunnery - 1);
			}
			if (pilotDef.BonusGuts > 0)
			{
				num += this.GetLevelRangeCost(pilotDef.BaseGuts, pilotDef.SkillGuts - 1);
			}
			if (pilotDef.BonusTactics > 0)
			{
				num += this.GetLevelRangeCost(pilotDef.BaseTactics, pilotDef.SkillTactics - 1);
			}
			if (num <= 0)
			{
				return;
			}
			pilotDef.abilityDefNames.Clear();
			List<string> abilities = SimGameState.GetAbilities(pilotDef.BaseGunnery, pilotDef.BasePiloting, pilotDef.BaseGuts, pilotDef.BaseTactics);
			pilotDef.abilityDefNames.AddRange(abilities);
			pilotDef.SetSpentExperience(0);
			pilotDef.ForceRefreshAbilityDefs();
			pilotDef.ResetBonusStats();
			pilot.FromPilotDef(pilotDef);
			pilot.AddExperience(0, "Respec", num);
		}

		// Token: 0x060092FF RID: 37631 RVA: 0x00266834 File Offset: 0x00264A34
		public static List<string> GetAbilities(StatCollection stats)
		{
			int num = Mathf.FloorToInt(SimGameState.GetGunnerySkill(stats));
			int num2 = Mathf.FloorToInt(SimGameState.GetPilotingSkill(stats));
			int num3 = Mathf.FloorToInt(SimGameState.GetGutsSkill(stats));
			int num4 = Mathf.FloorToInt(SimGameState.GetTacticsSkill(stats));
			return SimGameState.GetAbilities(num, num2, num3, num4);
		}

		// Token: 0x06009300 RID: 37632 RVA: 0x00266878 File Offset: 0x00264A78
		public static List<string> GetAbilities(int gunnery, int piloting, int guts, int tactics)
		{
			List<string> list = new List<string>();
			SimPilotProgressionConstantsDef progression = SceneSingletonBehavior<UnityGameInstance>.Instance.Game.Simulation.Constants.Progression;
			list.AddRange(SimGameState.GetAbilityDefsForSkill(progression.GunnerySkills, gunnery));
			list.AddRange(SimGameState.GetAbilityDefsForSkill(progression.PilotingSkills, piloting));
			list.AddRange(SimGameState.GetAbilityDefsForSkill(progression.GutsSkills, guts));
			list.AddRange(SimGameState.GetAbilityDefsForSkill(progression.TacticsSkills, tactics));
			return list;
		}

		// Token: 0x06009301 RID: 37633 RVA: 0x002668EC File Offset: 0x00264AEC
		public static List<string> GetAbilityDefsForSkill(string[][] abilityDefConsts, int abilityLevel)
		{
			List<string> list = new List<string>();
			for (int i = 0; i < abilityLevel; i++)
			{
				list.AddRange(abilityDefConsts[i]);
			}
			return list;
		}

		// Token: 0x06009302 RID: 37634 RVA: 0x00266915 File Offset: 0x00264B15
		public static float GetGunnerySkill(StatCollection stats)
		{
			return stats.GetValue<float>("Gunnery");
		}

		// Token: 0x06009303 RID: 37635 RVA: 0x00266922 File Offset: 0x00264B22
		public static float GetPilotingSkill(StatCollection stats)
		{
			return stats.GetValue<float>("Piloting");
		}

		// Token: 0x06009304 RID: 37636 RVA: 0x0026692F File Offset: 0x00264B2F
		public static float GetGutsSkill(StatCollection stats)
		{
			return stats.GetValue<float>("Guts");
		}

		// Token: 0x06009305 RID: 37637 RVA: 0x0026693C File Offset: 0x00264B3C
		public static float GetTacticsSkill(StatCollection stats)
		{
			return stats.GetValue<float>("Tactics");
		}

		// Token: 0x06009306 RID: 37638 RVA: 0x0026694C File Offset: 0x00264B4C
		public void SetupRoninTooltip(HBSTooltip RoninTooltip, Pilot pilot)
		{
			if (RoninTooltip != null)
			{
				string text;
				if (pilot.pilotDef.IsVanguard)
				{
					text = "UnitMechWarriorKSBacker";
				}
				else if (pilot.pilotDef.IsRonin)
				{
					text = "UnitMechWarriorSpecial";
				}
				else
				{
					if (pilot != UnityGameInstance.BattleTechGame.Simulation.Commander)
					{
						return;
					}
					text = "UnitMechWarriorCommander";
				}
				if (!string.IsNullOrEmpty(text) && UnityGameInstance.BattleTechGame.DataManager.BaseDescriptionDefs.Exists(text))
				{
					BaseDescriptionDef baseDescriptionDef = UnityGameInstance.BattleTechGame.DataManager.BaseDescriptionDefs.Get(text);
					RoninTooltip.SetDefaultStateData(TooltipUtilities.GetStateDataFromObject(baseDescriptionDef));
				}
			}
		}

		// Token: 0x17001997 RID: 6551
		// (get) Token: 0x06009307 RID: 37639 RVA: 0x002669ED File Offset: 0x00264BED
		// (set) Token: 0x06009308 RID: 37640 RVA: 0x002669F5 File Offset: 0x00264BF5
		public string InstanceGUID { get; private set; }

		// Token: 0x17001998 RID: 6552
		// (get) Token: 0x06009309 RID: 37641 RVA: 0x002669FE File Offset: 0x00264BFE
		public bool IsCampaign
		{
			get
			{
				return this.SimGameMode == SimGameState.SimGameType.KAMEA_CAMPAIGN;
			}
		}

		// Token: 0x17001999 RID: 6553
		// (get) Token: 0x0600930A RID: 37642 RVA: 0x00266A09 File Offset: 0x00264C09
		public bool IsIronmanCampaign
		{
			get
			{
				return this.Constants.Story.IronmanMode;
			}
		}

		// Token: 0x1700199A RID: 6554
		// (get) Token: 0x0600930B RID: 37643 RVA: 0x00266A1B File Offset: 0x00264C1B
		public bool Saving
		{
			get
			{
				return this.HasInitStateBits(SimGameState.InitStates.ACTIVELY_SAVING);
			}
		}

		// Token: 0x1700199B RID: 6555
		// (get) Token: 0x0600930C RID: 37644 RVA: 0x00266A28 File Offset: 0x00264C28
		// (set) Token: 0x0600930D RID: 37645 RVA: 0x00266A30 File Offset: 0x00264C30
		public bool UpdateMilestonesOnLoad { get; private set; }

		// Token: 0x1700199C RID: 6556
		// (get) Token: 0x0600930E RID: 37646 RVA: 0x00266A39 File Offset: 0x00264C39
		// (set) Token: 0x0600930F RID: 37647 RVA: 0x00266A41 File Offset: 0x00264C41
		public string SaveActiveContractName { get; private set; }

		// Token: 0x06009310 RID: 37648 RVA: 0x00266A4C File Offset: 0x00264C4C
		private bool TEMP_HamburgerMenuAccessLogic(SaveReason reason, bool logDetails)
		{
			if (!this.HasInitStateBits(SimGameState.InitStates.UX_SYSTEMS_CREATED))
			{
				return true;
			}
			bool isOpen = this.interruptQueue.IsOpen;
			bool isClosing = this.interruptQueue.IsClosing;
			bool hasQueue = this.interruptQueue.HasQueue;
			bool saveIsOpen = this.interruptQueue.SaveIsOpen;
			if (hasQueue || (isOpen && !saveIsOpen && !isClosing))
			{
				if (logDetails)
				{
					GameInstance.gameInfoLogger.Log("[TEMP_HamburgerMenuAccessLogic] !interruptQueue.CanSave");
				}
				return false;
			}
			if (this.TravelManager.InTransition)
			{
				if (logDetails)
				{
					GameInstance.gameInfoLogger.Log("[TEMP_HamburgerMenuAccessLogic] TravelManager.InTransition");
				}
				return false;
			}
			if (this.ConversationManager.IsOn)
			{
				if (logDetails)
				{
					GameInstance.gameInfoLogger.Log("[TEMP_HamburgerMenuAccessLogic] ConversationManager.IsOn");
				}
				return false;
			}
			if (this.VideoPlayerActive)
			{
				if (logDetails)
				{
					GameInstance.gameInfoLogger.Log("[TEMP_HamburgerMenuAccessLogic] VideoPlayer.isActiveAndEnabled");
				}
				return false;
			}
			if (this.CharacterCreation != null && this.CharacterCreation.isActiveAndEnabled && reason != SaveReason.SIM_GAME_FIRST_SAVE)
			{
				if (logDetails)
				{
					GameInstance.gameInfoLogger.Log("[TEMP_HamburgerMenuAccessLogic] CharacterCreation");
				}
				return false;
			}
			if (this.TravelManager != null && this.TravelManager.TravelState == SimGameTravelStatus.TRANSITION_ANIMATING)
			{
				if (logDetails)
				{
					GameInstance.gameInfoLogger.Log("[TEMP_HamburgerMenuAccessLogic] TravelManager Travel State");
				}
				return false;
			}
			return true;
		}

		// Token: 0x06009311 RID: 37649 RVA: 0x00266B70 File Offset: 0x00264D70
		public bool CanSave(SaveReason reason, bool logDetails)
		{
			if (!this.TEMP_HamburgerMenuAccessLogic(reason, logDetails))
			{
				SimGameState.logger.LogError("Hamburger Menu should protect against this!");
				return false;
			}
			bool flag = this.HasInitStateBits(SimGameState.InitStates.UX_SYSTEMS_CREATED);
			if (logDetails && !flag)
			{
				if (logDetails)
				{
					GameInstance.gameInfoLogger.Log("[CanSave] no ATTACHED_UX_STATE bits");
				}
			}
			else if (logDetails)
			{
				GameInstance.gameInfoLogger.Log("[CanSave] yes, yes you can");
			}
			return flag;
		}

		// Token: 0x06009312 RID: 37650 RVA: 0x00266BD0 File Offset: 0x00264DD0
		public void BeginSave()
		{
			SimGameState.logger.Log("[BeginSave]");
			this.SetInitStateBits(SimGameState.InitStates.ACTIVELY_SAVING);
			if (!this.UXAttached)
			{
				return;
			}
			SimGameState.logger.Log("[BeginSave] pause and set time moving false");
			this.PauseTimer();
			this.SetTimeMoving(false, true);
		}

		// Token: 0x06009313 RID: 37651 RVA: 0x00266C20 File Offset: 0x00264E20
		public void SaveSerializationComplete()
		{
			this.RemoveInitStateBits(SimGameState.InitStates.ACTIVELY_SAVING);
			if (!this.UXAttached)
			{
				return;
			}
			SimGameState.logger.Log("[SaveSerializationComplete] resuming timer");
			if (this.interruptQueue.HasQueue && !this.interruptQueue.IsOpen)
			{
				this.interruptQueue.DisplayIfAvailable();
			}
			this.ResumeTimer();
		}

		// Token: 0x06009314 RID: 37652 RVA: 0x00266C7C File Offset: 0x00264E7C
		public void SaveComplete()
		{
			SimGameState.logger.Log("[SaveComplete]");
		}

		// Token: 0x06009315 RID: 37653 RVA: 0x00266C8D File Offset: 0x00264E8D
		public void LoadNextScene()
		{
			LevelLoader.LoadScene("MainMenu", "Insterstitial_Cleanup");
		}

		// Token: 0x06009316 RID: 37654 RVA: 0x00266CA0 File Offset: 0x00264EA0
		public void Dehydrate(SimGameSave save, SerializableReferenceContainer references)
		{
			save.failedCampaign = this.campaignFailed;
			references.AddItem<Contract>("activeBreadcrumb", this.activeBreadcrumb);
			references.AddItem<Contract>("pendingBreadcrumb", this.pendingBreadcrumb);
			references.AddItemList<Contract>("globalContracts", this.globalContracts);
			save.TravelStatus = (int)this.TravelManager.TravelState;
			if (this.UXAttached && this.Starmap.ActivePath != null)
			{
				save.ActiveTravelPath = new List<string>();
				foreach (StarSystemNode starSystemNode in this.Starmap.ActivePath)
				{
					save.ActiveTravelPath.Add(starSystemNode.System.ID);
				}
				save.TravelIndex = this.Starmap.travelIndex;
			}
			else
			{
				save.ActiveTravelPath = null;
			}
			save.PurchasedShipUpgrades = this.purchasedArgoUpgrades;
			save.CampaignStartDate = this.GetCampaignStartDate().ToString();
			save.Day = this.DaysPassed;
			save.DaysRemainingInQuarter = this.DayRemainingInQuarter;
			save.ProRatedRefund = this.ProRateRefund;
			save.Seed = this.NetworkRandom.seed;
			save.RandomCalls = this.NetworkRandom.randomCalls;
			save.ConsumedMilestones = this.ConsumedMilestones;
			save.Heraldry = this.Player1sMercUnitHeraldryDef;
			references.AddItemList<StarSystem>("StarSystems", this.StarSystems);
			references.AddItem<StarSystem>("NearestToTarget", this.NearestToTarget);
			references.AddItem<StarSystem>("CurSystem", this.CurSystem);
			references.AddItem<StarSystem>("TargetSystem", this.TargetSystem);
			references.OperateOnAllSourceObjects<StarSystem>(delegate(StarSystem x)
			{
				bool flag = x == this.CurSystem;
				x.Dehydrate(references, flag);
			});
			save.MechTechs = this.MechTechs;
			save.MedTechs = this.MedTechs;
			save.GlobalReferences.AddItemDictionary<int, MechDef>("ActiveMechs", this.ActiveMechs);
			save.GlobalReferences.AddItemDictionary<int, MechDef>("ReadyingMechs", this.ReadyingMechs);
			save.LastUsedMechs = this.LastUsedMechs;
			save.LastUsedPilots = this.LastUsedPilots;
			save.Graveyard.SetList("Graveyard", this.Graveyard);
			save.PilotRoster.SetList("PilotRoster", this.PilotRoster);
			save.GlobalReferences.AddItem<Pilot>("Commander", this.commander);
			save.CompanyTags = this.companyTags;
			save.CompanyStats = this.companyStats;
			save.CurDropship = this.CurDropship;
			save.ContractBits = this.contractBits;
			save.GlobalReferences.AddItemList<WorkOrderEntry>("MechLabQueue", this.MechLabQueue);
			save.GlobalReferences.AddItem<WorkOrderEntry_MedBayGeneric>("MedBayQueue", this.MedBayQueue);
			save.GlobalReferences.AddItem<WorkOrderEntry_TravelGeneric>("TravelOrder", this.TravelOrder);
			save.GlobalReferences.AddItemList<MechComponentRef>("WorkOrderComponents", this.WorkOrderComponents);
			save.GlobalReferences.AddItem<WorkOrderEntry_ArgoUpgradeGeneric>("CurrentUpgradeEntry", this.CurrentUpgradeEntry);
			save.GlobalReferences.OperateOnAllSourceObjects<WorkOrderEntry>(delegate(WorkOrderEntry x)
			{
				if (x != null)
				{
					x.Dehydrate(save);
				}
			});
			references.AddItemList<TemporarySimGameResult>("TemporaryResultTracker", this.TemporaryResultTracker);
			references.OperateOnAllSourceObjects<TemporarySimGameResult>(delegate(TemporarySimGameResult x)
			{
				x.Dehydrate(references);
			});
			references.AddItem<SimGameEventTracker>("companyEventTracker", this.companyEventTracker);
			references.AddItem<SimGameEventTracker>("mechWarriorEventTracker", this.mechWarriorEventTracker);
			references.AddItem<SimGameEventTracker>("deadEventTracker", this.deadEventTracker);
			references.AddItem<SimGameEventTracker>("moraleEventTracker", this.moraleEventTracker);
			references.AddItemList<SimGameEventTracker>("specialEventTracker", this.specialEventTracker);
			references.OperateOnAllSourceObjects<SimGameEventTracker>(delegate(SimGameEventTracker x)
			{
				x.Dehydrate(save);
			});
			save.GlobalReferences.OperateOnAllSourceObjects<MechDef>(delegate(MechDef x)
			{
				x.Dehydrate(save.GlobalReferences);
			});
			save.contractScope = this.ContractScope;
			this.Context.Dehydrate(this, save, references);
			save.SimGameContext = this.Context;
			save.UIDCount = this.UIDCount;
			save.RepairStateKeys = new List<int>();
			save.RepairStateValues = new List<int>();
			foreach (ArgoController.RepairStateLocations repairStateLocations in this.argoLocationRepairStates.Keys)
			{
				save.RepairStateKeys.Add((int)repairStateLocations);
				save.RepairStateValues.Add(this.argoLocationRepairStates[repairStateLocations]);
			}
			save.UsedRoninIDs = new List<string>(this.usedRoninIDs);
			save.CharacterList = new List<int>();
			for (int i = 0; i < this.characterList.Count; i++)
			{
				save.CharacterList.Add((int)this.characterList[i]);
			}
			save.CharacterStatus = new List<bool>(this.characterStatus);
			save.MapDiscardPile = new List<string>(this.mapDiscardPile);
			save.ContractDiscardPile = new List<string>(this.contractDiscardPile);
			save.PilotGenCallsignDiscardPile = new List<string>(this.pilotGenCallsignDiscardPile);
			save.PilotGenVoiceDiscardPile = new List<string>(this.pilotGenVoiceDiscardPile);
			save.PilotGenPortraitDiscardPile = new List<string>(this.pilotGenPortraitDiscardPile);
			save.AllowDebug = this.AllowDebug;
			save.SimGameMode = this.SimGameMode;
			save.PreviouslyAttachedHeadState = this.HasInitStateBits(SimGameState.InitStates.UX_ATTACHED_PREVIOUSLY);
			save.IgnoredContractEmployers = new List<int>();
			foreach (string text in this.ignoredContractEmployers)
			{
				save.IgnoredContractEmployers.Add(FactionEnumeration.GetFactionByName(text).ID);
			}
			save.IgnoredContractTargets = new List<int>();
			foreach (string text2 in this.ignoredContractTargets)
			{
				save.IgnoredContractTargets.Add(FactionEnumeration.GetFactionByName(text2).ID);
			}
			save.DisplayedFactions = new List<string>();
			foreach (string text3 in this.displayedFactions)
			{
				save.DisplayedFactions.Add(text3);
			}
			save.ConversationIDRefsWeHaveSelected = new List<string>();
			foreach (string text4 in this.AlreadyClickedConversationResponses)
			{
				save.ConversationIDRefsWeHaveSelected.Add(text4);
			}
			save.WhitelistedRonin = new List<string>();
			foreach (string text5 in this.WhitelistedRonin)
			{
				save.WhitelistedRonin.Add(text5);
			}
			save.ActiveContractName = this.SaveActiveContractName;
			this.SaveActiveContractName = null;
			save.ConstantOverrides = this.constantOverrides.Dehydrate();
			save.DifficultySettings = this.difficultySettings.Dehydrate();
			save.FlashpointDiscardPile = new List<string>(this.flashpointDiscardPile);
			if (this.activeFlashpoint != null)
			{
				save.ActiveFlashpoint = this.activeFlashpoint.Def.Description.Id;
			}
			else
			{
				save.ActiveFlashpoint = null;
			}
			save.InFlashpointCooldown = this.inFlashpointCooldown;
			save.FlashpointCooldownDays = this.flashpointCooldownDays;
			save.FlashpointCooldownDaysHasValue = true;
			save.InFlashpointCooldownHasValue = true;
			save.CompletedFlashpoints = this.completedFlashpoints;
			save.AvailableFlashpointList = new List<Flashpoint>(this.availableFlashpointList.Count);
			foreach (Flashpoint flashpoint in this.availableFlashpointList)
			{
				flashpoint.Dehydrate(references);
				save.AvailableFlashpointList.Add(flashpoint);
			}
			save.VisitedStarSystems = new List<string>(this.VisitedStarSystems.Count);
			for (int j = 0; j < this.VisitedStarSystems.Count; j++)
			{
				save.VisitedStarSystems.Add(this.VisitedStarSystems[j]);
			}
			save.AlliedFactions = new List<string>(this.AlliedFactions.Count);
			for (int k = 0; k < this.AlliedFactions.Count; k++)
			{
				save.AlliedFactions.Add(this.AlliedFactions[k].ToString());
			}
			save.CareerModeEndAlliedFactions = new List<string>(this.CareerModeEndAlliedFactions.Count);
			for (int l = 0; l < this.CareerModeEndAlliedFactions.Count; l++)
			{
				save.AlliedFactions.Add(this.CareerModeEndAlliedFactions[l].ToString());
			}
			save.CareerModeFlashpointStartDate = this.careerModeFlashpointStartDate;
			save.CareerModeLocked = this.careerModeLocked;
			references.OperateOnAllSourceObjects<Contract>(delegate(Contract x)
			{
				x.Dehydrate(null);
			});
		}

		// Token: 0x06009317 RID: 37655 RVA: 0x002677F8 File Offset: 0x002659F8
		public void Rehydrate(GameInstanceSave gameInstanceSave)
		{
			SimGameSave save = gameInstanceSave.SimGameSave;
			SerializableReferenceContainer globalReferences = gameInstanceSave.GlobalReferences;
			globalReferences.ResetOperateOnAllForAll();
			this.SimGameMode = save.SimGameMode;
			if (this.SimGameMode == SimGameState.SimGameType.INVALID_UNSET)
			{
				this.SimGameMode = SimGameState.SimGameType.KAMEA_CAMPAIGN;
			}
			this.savedTravelData = save.GetSaveTravelData();
			save.SimGameContext.Rehydrate(this, save, globalReferences);
			this.Context = new SimGameContext(this.BattleTechGame.GlobalGameContext);
			this.Context.SetObject(GameContextObjectTagEnum.Company, this);
			this.Context.MemberwiseCopyFrom(save.SimGameContext);
			this.SetCampaignStartDate(save.CampaignStartDate);
			this.DaysPassed = save.Day;
			this.DayRemainingInQuarter = save.DaysRemainingInQuarter;
			this.ProRateRefund = save.ProRatedRefund;
			this.ConsumedMilestones = save.ConsumedMilestones;
			this.Player1sMercUnitHeraldryDef = save.Heraldry;
			this.Player1sMercUnitHeraldryDef.RequestResources(this.DataManager, null);
			this.NetworkRandom.Synchronize(save.Seed, save.RandomCalls);
			if (this.starDict == null)
			{
				this.starDict = new Dictionary<string, StarSystem>();
			}
			else
			{
				this.starDict.Clear();
			}
			this.starSystems = globalReferences.GetItemList<StarSystem>("StarSystems");
			this.TargetSystem = globalReferences.GetItem<StarSystem>("TargetSystem");
			this.NearestToTarget = globalReferences.GetItem<StarSystem>("NearestToTarget");
			this.CurSystem = globalReferences.GetItem<StarSystem>("CurSystem");
			globalReferences.OperateOnAllSourceObjects<StarSystem>(delegate(StarSystem x)
			{
				bool flag = x == this.CurSystem;
				x.Rehydrate(this, globalReferences, flag);
			});
			List<string> list = new List<string>(this.DataManager.SystemDefs.Keys);
			for (int i = this.starSystems.Count - 1; i >= 0; i--)
			{
				if (this.SimGameMode == SimGameState.SimGameType.CAREER && !this.starSystems[i].Def.StartingSystemModes.Contains(this.SimGameMode))
				{
					this.starSystems.RemoveAt(i);
				}
			}
			foreach (string text in list)
			{
				StarSystemDef newSystemDef = this.DataManager.SystemDefs.Get(text);
				if (this.starSystems.Find((StarSystem x) => x.Def.CoreSystemID == newSystemDef.CoreSystemID) == null && newSystemDef.StartingSystemModes.Contains(this.SimGameMode))
				{
					StarSystem starSystem = new StarSystem(newSystemDef, this);
					this.starSystems.Add(starSystem);
				}
			}
			foreach (StarSystem starSystem2 in this.StarSystems)
			{
				this.starDict.Add(starSystem2.ID, starSystem2);
			}
			globalReferences.OperateOnAllSourceObjects<Contract>(delegate(Contract x)
			{
				x.Hydrate(this.Context);
				x.SetTargetSystemFromLoad(this.starDict);
			});
			foreach (StarSystem starSystem3 in this.starSystems)
			{
				starSystem3.SetContractTargets(this.starDict);
			}
			this.ActiveMechs = save.GlobalReferences.GetItemDictionary<int, MechDef>("ActiveMechs");
			this.ReadyingMechs = save.GlobalReferences.GetItemDictionary<int, MechDef>("ReadyingMechs");
			this.LastUsedMechs = save.LastUsedMechs;
			this.LastUsedPilots = save.LastUsedPilots;
			this.MechTechs = save.MechTechs;
			this.MedTechs = save.MedTechs;
			save.GlobalReferences.OperateOnAllSourceObjects<Pilot>(delegate(Pilot x)
			{
				if (x != null)
				{
					x.Hydrate(null, null);
					x.SimGameInitFromSave();
				}
			});
			this.Graveyard = save.Graveyard.GetList("Graveyard");
			this.PilotRoster = save.PilotRoster.GetList("PilotRoster");
			this.commander = save.GlobalReferences.GetItem<Pilot>("Commander");
			this.companyTags = save.CompanyTags;
			this.companyStats = save.CompanyStats;
			this.InitCompanyStatValidators();
			this.purchasedArgoUpgrades = save.PurchasedShipUpgrades;
			foreach (string text2 in this.purchasedArgoUpgrades)
			{
				ShipModuleUpgrade shipModuleUpgrade = this.DataManager.ShipUpgradeDefs.Get(text2);
				this.shipUpgrades.Add(shipModuleUpgrade);
			}
			save.GlobalReferences.OperateOnAllSourceObjects<MechDef>(delegate(MechDef x)
			{
				x.Hydrate(save.GlobalReferences);
				x.InitFromSave(this.DataManager);
			});
			this.CurDropship = save.CurDropship;
			this.contractBits = save.ContractBits;
			if (this.contractBits != null)
			{
				foreach (ContractData contractData in this.contractBits)
				{
					SimGameState.AddContractData addContractData = new SimGameState.AddContractData(contractData);
					this.AddContract(addContractData);
				}
			}
			this.TemporaryResultTracker = globalReferences.GetItemList<TemporarySimGameResult>("TemporaryResultTracker");
			globalReferences.OperateOnAllSourceObjects<TemporarySimGameResult>(delegate(TemporarySimGameResult x)
			{
				x.Hydrate(globalReferences);
			});
			save.GlobalReferences.OperateOnAllSourceObjects<WorkOrderEntry>(delegate(WorkOrderEntry x)
			{
				if (x != null)
				{
					x.Hydrate(save);
				}
			});
			save.GlobalReferences.OperateOnAllSourceObjects<MechComponentRef>(delegate(MechComponentRef x)
			{
				x.DataManager = this.DataManager;
				x.RefreshComponentDef();
			});
			this.MechLabQueue = save.GlobalReferences.GetItemList<WorkOrderEntry>("MechLabQueue");
			this.MedBayQueue = save.GlobalReferences.GetItem<WorkOrderEntry_MedBayGeneric>("MedBayQueue");
			this.TravelOrder = save.GlobalReferences.GetItem<WorkOrderEntry_TravelGeneric>("TravelOrder");
			this.WorkOrderComponents = save.GlobalReferences.GetItemList<MechComponentRef>("WorkOrderComponents");
			this.CurrentUpgradeEntry = save.GlobalReferences.GetItem<WorkOrderEntry_ArgoUpgradeGeneric>("CurrentUpgradeEntry");
			this.specialEventTracker = globalReferences.GetItemList<SimGameEventTracker>("specialEventTracker");
			this.companyEventTracker = globalReferences.GetItem<SimGameEventTracker>("companyEventTracker");
			this.mechWarriorEventTracker = globalReferences.GetItem<SimGameEventTracker>("mechWarriorEventTracker");
			this.deadEventTracker = globalReferences.GetItem<SimGameEventTracker>("deadEventTracker");
			this.moraleEventTracker = globalReferences.GetItem<SimGameEventTracker>("moraleEventTracker");
			globalReferences.OperateOnAllSourceObjects<SimGameEventTracker>(delegate(SimGameEventTracker x)
			{
				x.Hydrate(this, save);
			});
			this.ContractScope = save.contractScope;
			this.activeBreadcrumb = globalReferences.GetItem<Contract>("activeBreadcrumb");
			if (globalReferences.HasItem("pendingBreadcrumb"))
			{
				this.pendingBreadcrumb = globalReferences.GetItem<Contract>("pendingBreadcrumb");
			}
			this.globalContracts = globalReferences.GetItemList<Contract>("globalContracts");
			if (save.UIDCount > this.UIDCount)
			{
				this.UIDCount = save.UIDCount;
			}
			if (save.RepairStateKeys != null)
			{
				this.argoLocationRepairStates = new Dictionary<ArgoController.RepairStateLocations, int>();
				for (int j = 0; j < save.RepairStateKeys.Count; j++)
				{
					this.argoLocationRepairStates.Add((ArgoController.RepairStateLocations)save.RepairStateKeys[j], save.RepairStateValues[j]);
				}
			}
			if (save.UsedRoninIDs != null)
			{
				this.usedRoninIDs = new List<string>(save.UsedRoninIDs);
			}
			if (save.CharacterList != null)
			{
				this.characterList = new List<SimGameState.SimGameCharacterType>();
				for (int k = 0; k < save.CharacterList.Count; k++)
				{
					this.characterList.Add((SimGameState.SimGameCharacterType)save.CharacterList[k]);
				}
			}
			if (save.CharacterStatus != null)
			{
				this.characterStatus = new List<bool>(save.CharacterStatus);
			}
			if (save.MapDiscardPile != null)
			{
				this.mapDiscardPile = new List<string>(save.MapDiscardPile);
			}
			if (save.ContractDiscardPile != null)
			{
				this.contractDiscardPile = new List<string>(save.ContractDiscardPile);
			}
			if (save.PilotGenCallsignDiscardPile != null)
			{
				this.pilotGenCallsignDiscardPile = new List<string>(save.PilotGenCallsignDiscardPile);
			}
			if (save.PilotGenVoiceDiscardPile != null)
			{
				this.pilotGenVoiceDiscardPile = new List<string>(save.PilotGenVoiceDiscardPile);
			}
			if (save.PilotGenPortraitDiscardPile != null)
			{
				this.pilotGenPortraitDiscardPile = new List<string>(save.PilotGenPortraitDiscardPile);
			}
			if (save.IgnoredContractEmployers != null)
			{
				this.ignoredContractEmployers = new List<string>();
				foreach (int num in save.IgnoredContractEmployers)
				{
					this.ignoredContractEmployers.Add(FactionEnumeration.GetFactionByID(num).Name);
				}
			}
			if (save.IgnoredContractTargets != null)
			{
				this.ignoredContractTargets = new List<string>();
				foreach (int num2 in save.IgnoredContractTargets)
				{
					this.ignoredContractTargets.Add(FactionEnumeration.GetFactionByID(num2).Name);
				}
				if (save.SimGameMode == SimGameState.SimGameType.CAREER)
				{
					this.AddCareerModeIgnoredContractTargets(this.ignoredContractTargets);
				}
			}
			if (save.DisplayedFactions != null)
			{
				this.displayedFactions = new List<string>();
				foreach (string text3 in save.DisplayedFactions)
				{
					this.displayedFactions.Add(text3);
				}
				foreach (FactionValue factionValue in FactionEnumeration.GetStartingDisplayFactionList(save.SimGameMode == SimGameState.SimGameType.CAREER))
				{
					string name = factionValue.Name;
					if (!this.displayedFactions.Contains(name))
					{
						this.displayedFactions.Add(name);
					}
				}
				if (save.SimGameMode == SimGameState.SimGameType.CAREER)
				{
					foreach (FactionValue factionValue2 in FactionEnumeration.FactionList.FindAll((FactionValue faction) => faction.IsCareerIgnoredContractTarget))
					{
						this.displayedFactions.Remove(factionValue2.Name);
					}
				}
			}
			if (save.ConversationIDRefsWeHaveSelected != null)
			{
				this.AlreadyClickedConversationResponses = new List<string>();
				foreach (string text4 in save.ConversationIDRefsWeHaveSelected)
				{
					this.AlreadyClickedConversationResponses.Add(text4);
				}
			}
			if (save.WhitelistedRonin != null)
			{
				this.WhitelistedRonin = new List<string>();
				foreach (string text5 in save.WhitelistedRonin)
				{
					this.WhitelistedRonin.Add(text5);
				}
			}
			this.AllowDebug = save.AllowDebug;
			this.companyEventTracker.SetValidScopes(SimGameState.CompanyTrackerScope);
			this.mechWarriorEventTracker.SetValidScopes(SimGameState.MechWarriorTrackerScope);
			this.moraleEventTracker.SetValidScopes(SimGameState.MoraleTrackerScope);
			this.deadEventTracker.SetValidScopes(SimGameState.DeadTrackerScope);
			this.constantOverrides.Initialize(this);
			string text6 = ((this.SimGameMode == SimGameState.SimGameType.CAREER) ? "CareerDifficultySettings" : "DifficultySettings");
			this.difficultySettings.Initialize(this, this.DataManager.SimGameDifficultySettingLists.Get(text6), false);
			if (save.DifficultySettings != null)
			{
				this.difficultySettings.Rehydrate(save.DifficultySettings);
			}
			else
			{
				this.DifficultySettings.ApplyAllSettings(true);
				this.DifficultySettings.RefreshCareerScoreMultiplier();
			}
			Flashpoint flashpoint = null;
			if (save.FlashpointDiscardPile != null)
			{
				this.flashpointDiscardPile = new List<string>(save.FlashpointDiscardPile);
			}
			if (save.CompletedFlashpoints != null && save.CompletedFlashpoints.Count > 0)
			{
				this.completedFlashpoints = save.CompletedFlashpoints;
			}
			if (save.InFlashpointCooldownHasValue)
			{
				this.inFlashpointCooldown = save.InFlashpointCooldown;
			}
			if (save.FlashpointCooldownDaysHasValue)
			{
				this.flashpointCooldownDays = save.FlashpointCooldownDays;
			}
			if (save.AvailableFlashpointList != null)
			{
				this.availableFlashpointList = new List<Flashpoint>(save.AvailableFlashpointList);
				foreach (Flashpoint flashpoint2 in this.availableFlashpointList)
				{
					flashpoint2.Rehydrate(this, globalReferences);
					if (flashpoint == null && !string.IsNullOrEmpty(save.ActiveFlashpoint) && flashpoint2.Def.Description.Id == save.ActiveFlashpoint)
					{
						flashpoint = flashpoint2;
					}
				}
				if (flashpoint != null)
				{
					this.activeFlashpoint = flashpoint;
				}
			}
			if (this.activeFlashpoint != null)
			{
				this.Context.SetObject(GameContextObjectTagEnum.TargetFlashpoint, this.activeFlashpoint);
			}
			if (save.VisitedStarSystems != null)
			{
				this.VisitedStarSystems.Clear();
				for (int l = 0; l < save.VisitedStarSystems.Count; l++)
				{
					this.VisitedStarSystems.Add(save.VisitedStarSystems[l]);
				}
			}
			if (save.AlliedFactions != null)
			{
				this.AlliedFactions.Clear();
				for (int m = 0; m < save.AlliedFactions.Count; m++)
				{
					try
					{
						string text7 = save.AlliedFactions[m];
						this.AlliedFactions.Add(text7);
					}
					catch
					{
					}
				}
			}
			if (save.CareerModeEndAlliedFactions != null)
			{
				this.CareerModeEndAlliedFactions.Clear();
				for (int n = 0; n < save.CareerModeEndAlliedFactions.Count; n++)
				{
					try
					{
						FactionValue factionByName = FactionEnumeration.GetFactionByName(save.CareerModeEndAlliedFactions[n]);
						this.CareerModeEndAlliedFactions.Add(factionByName.Name);
					}
					catch
					{
					}
				}
			}
			this.careerModeLocked = save.CareerModeLocked;
			this.careerModeFlashpointStartDate = save.CareerModeFlashpointStartDate;
			this.SetFactionValidators(true);
		}

		// Token: 0x06009318 RID: 37656 RVA: 0x00268824 File Offset: 0x00266A24
		public SimGameState.TriggerSaveNowResult TriggerSaveNow(SaveReason reason, SimGameState.TriggerSaveNowOption option)
		{
			bool isOpen = this.interruptQueue.IsOpen;
			bool isClosing = this.interruptQueue.IsClosing;
			bool hasQueue = this.interruptQueue.HasQueue;
			bool saveIsOpen = this.interruptQueue.SaveIsOpen;
			if (!hasQueue && (!isOpen || (isOpen && isClosing && !saveIsOpen)))
			{
				this.BattleTechGame.Save(reason);
				return SimGameState.TriggerSaveNowResult.SAVED_NOW;
			}
			if (option == SimGameState.TriggerSaveNowOption.DONT_QUEUE)
			{
				SimGameState.logger.LogWarning("Skipping save because active queue and queueIfUnable is false");
				return SimGameState.TriggerSaveNowResult.DID_NOT_QUEUE;
			}
			if (saveIsOpen)
			{
				SimGameState.logger.LogWarning("Skipping save because a save is already in progress");
				return SimGameState.TriggerSaveNowResult.DID_NOT_QUEUE;
			}
			SimGameInterruptManager.SaveEntry saveEntry = new SimGameInterruptManager.SaveEntry(reason);
			this.interruptQueue.AddInterrupt(saveEntry, true);
			return SimGameState.TriggerSaveNowResult.DID_QUEUE;
		}

		// Token: 0x06009319 RID: 37657 RVA: 0x002688C4 File Offset: 0x00266AC4
		public void TriggerIronManSave()
		{
			if (!this.IsIronmanCampaign)
			{
				return;
			}
			this.TriggerSaveNow(SaveReason.SIM_GAME_IRONMAN_AUTO_SAVE, SimGameState.TriggerSaveNowOption.QUEUE_IF_NEEDED);
		}

		// Token: 0x0600931A RID: 37658 RVA: 0x00003969 File Offset: 0x00001B69
		public bool IsSavingOrLoading()
		{
			return false;
		}

		// Token: 0x0600931B RID: 37659 RVA: 0x000AB2CC File Offset: 0x000A94CC
		public string ToLoadString()
		{
			return "";
		}

		// Token: 0x1700199D RID: 6557
		// (get) Token: 0x0600931C RID: 37660 RVA: 0x002688D9 File Offset: 0x00266AD9
		public SimGameTravelStatus TravelState
		{
			get
			{
				return this.TravelManager.TravelState;
			}
		}

		// Token: 0x0600931D RID: 37661 RVA: 0x002688E6 File Offset: 0x00266AE6
		public bool IsTransitionAnimating()
		{
			return this.TravelState == SimGameTravelStatus.TRANSITION_ANIMATING;
		}

		// Token: 0x0600931E RID: 37662 RVA: 0x002688F4 File Offset: 0x00266AF4
		public void TravelToSystemByString(string loc, bool timeSkip)
		{
			StarSystemNode systemByID = this.Starmap.GetSystemByID(loc);
			if (systemByID != null)
			{
				this.SetCurrentSystem(systemByID.System, false, timeSkip);
			}
		}

		// Token: 0x0600931F RID: 37663 RVA: 0x0026891F File Offset: 0x00266B1F
		public void SetTravelTime(int val, string sourceID = null)
		{
			if (sourceID == null)
			{
				sourceID = "SimGameState";
			}
			this.companyStats.ModifyStat<int>(sourceID, 0, "TravelTime", StatCollection.StatOperation.Set, val, -1, true);
		}

		// Token: 0x06009320 RID: 37664 RVA: 0x00268942 File Offset: 0x00266B42
		public bool CanChangeState()
		{
			return !this.IsTransitionAnimating();
		}

		// Token: 0x06009321 RID: 37665 RVA: 0x00268950 File Offset: 0x00266B50
		public void ShowChangeDestinationDuringTransitNotification()
		{
			string text = Strings.T("Sorry Commander, we're already at full burn toward {0}. I can't plot a new course until we've shed this momentum and arrived in orbit.", new object[] { this.CurSystem.Name });
			this.interruptQueue.QueuePauseNotification("", text, this.GetCrewPortrait(SimGameCrew.Crew_Sumire), null, null, "Continue", null, null);
		}

		// Token: 0x06009322 RID: 37666 RVA: 0x0026899E File Offset: 0x00266B9E
		public void UpdateCompanyStatsFromTravel(SimGameTravelStatus newStatus)
		{
			this.companyStats.Set<SimGameTravelStatus>("Travel", newStatus);
		}

		// Token: 0x06009323 RID: 37667 RVA: 0x002689B4 File Offset: 0x00266BB4
		public Dictionary<string, string> GetTravelRestrictions()
		{
			if (this.restrictionDictionary == null)
			{
				this.restrictionDictionary = new Dictionary<string, string>();
				int num = 0;
				foreach (string text in this.Constants.Travel.RestrictionTags)
				{
					string text2 = "Restricted";
					if (this.Constants.Travel.RestrictionTagMessages.Length > num)
					{
						text2 = this.Constants.Travel.RestrictionTagMessages[num];
					}
					this.restrictionDictionary.Add(text, text2);
					num++;
				}
			}
			return this.restrictionDictionary;
		}

		// Token: 0x06009324 RID: 37668 RVA: 0x00268A44 File Offset: 0x00266C44
		public Flashpoint GetFlashpointInSystem(StarSystem theSystem)
		{
			foreach (Flashpoint flashpoint in this.AvailableFlashpoints)
			{
				if (flashpoint.CurSystem == theSystem)
				{
					return flashpoint;
				}
			}
			return null;
		}

		// Token: 0x06009325 RID: 37669 RVA: 0x00268AA0 File Offset: 0x00266CA0
		public void VisitSystem(StarSystem system)
		{
			if (!this.VisitedStarSystems.Contains(system.ID))
			{
				this.VisitedStarSystems.Add(system.ID);
			}
		}

		// Token: 0x06009326 RID: 37670 RVA: 0x00268AC6 File Offset: 0x00266CC6
		public bool SystemBeenVisited(StarSystem system)
		{
			return this.VisitedStarSystems.Contains(system.ID);
		}

		// Token: 0x04005C09 RID: 23561
		public const string CASTDEF_PREFIX = "castDef_";

		// Token: 0x04005C0A RID: 23562
		public const string CASTDEF_SUFFIX = "Default";

		// Token: 0x04005C0B RID: 23563
		public const string CREW_HEADER = "Crew_";

		// Token: 0x04005C0C RID: 23564
		[JsonIgnore]
		[NonSerialized]
		public static ILog logger = HBS.Logging.Logger.GetLogger("SimGameState");

		// Token: 0x04005C0D RID: 23565
		[JsonIgnore]
		[NonSerialized]
		public static SimGameReport Report = new SimGameReport();

		// Token: 0x04005C0E RID: 23566
		private static readonly DateTime year3025 = new DateTime(3025, 1, 1);

		// Token: 0x04005C0F RID: 23567
		private string campaignStartDate = SimGameState.year3025.ToString();

		// Token: 0x04005C10 RID: 23568
		private DateTime? privateCampaignStartDate;

		// Token: 0x04005C11 RID: 23569
		private int daysPassed;

		// Token: 0x04005C12 RID: 23570
		private SimGameEventTracker companyEventTracker = new SimGameEventTracker();

		// Token: 0x04005C13 RID: 23571
		private SimGameEventTracker mechWarriorEventTracker = new SimGameEventTracker();

		// Token: 0x04005C14 RID: 23572
		private SimGameEventTracker deadEventTracker = new SimGameEventTracker();

		// Token: 0x04005C15 RID: 23573
		private List<SimGameEventTracker> specialEventTracker = new List<SimGameEventTracker>();

		// Token: 0x04005C16 RID: 23574
		private SimGameEventTracker moraleEventTracker = new SimGameEventTracker();

		// Token: 0x04005C17 RID: 23575
		private List<string> ConsumedMilestones = new List<string>();

		// Token: 0x04005C18 RID: 23576
		public WeightedList<Pilot> PilotRoster = new WeightedList<Pilot>(WeightedListType.SimpleRandom, null, null, 0);

		// Token: 0x04005C19 RID: 23577
		public WeightedList<Pilot> Graveyard = new WeightedList<Pilot>(WeightedListType.SimpleRandom, null, null, 0);

		// Token: 0x04005C1A RID: 23578
		public Dictionary<int, MechDef> ActiveMechs = new Dictionary<int, MechDef>();

		// Token: 0x04005C1B RID: 23579
		public Dictionary<int, MechDef> ReadyingMechs = new Dictionary<int, MechDef>();

		// Token: 0x04005C1C RID: 23580
		private List<string> LastUsedMechs = new List<string>();

		// Token: 0x04005C1D RID: 23581
		private List<string> LastUsedPilots = new List<string>();

		// Token: 0x04005C1E RID: 23582
		public List<MechComponentRef> WorkOrderComponents = new List<MechComponentRef>();

		// Token: 0x04005C20 RID: 23584
		private List<StarSystem> starSystems = new List<StarSystem>();

		// Token: 0x04005C21 RID: 23585
		private List<SimGameMilestoneDef> milestones = new List<SimGameMilestoneDef>();

		// Token: 0x04005C22 RID: 23586
		private Pilot commander;

		// Token: 0x04005C23 RID: 23587
		private TagSet companyTags = new TagSet();

		// Token: 0x04005C24 RID: 23588
		private StatCollection companyStats = new StatCollection();

		// Token: 0x04005C25 RID: 23589
		public HeraldryDef Player1sMercUnitHeraldryDef = new HeraldryDef();

		// Token: 0x04005C26 RID: 23590
		private List<string> purchasedArgoUpgrades = new List<string>();

		// Token: 0x04005C27 RID: 23591
		private List<ShipModuleUpgrade> shipUpgrades = new List<ShipModuleUpgrade>();

		// Token: 0x04005C28 RID: 23592
		private int ProRateRefund;

		// Token: 0x04005C2D RID: 23597
		public WorkOrderEntry_MedBayGeneric MedBayQueue = new WorkOrderEntry_MedBayGeneric("MedQueue", "MedBay", 0, "");

		// Token: 0x04005C2E RID: 23598
		private WorkOrderEntry_Notification CurrentNotification = new WorkOrderEntry_Notification(WorkOrderType.NotificationGeneric, "Notification", "", "");

		// Token: 0x04005C30 RID: 23600
		public TaskManagementElement FinancialReportItem;

		// Token: 0x04005C31 RID: 23601
		public WorkOrderEntry_ArgoUpgradeGeneric CurrentUpgradeEntry;

		// Token: 0x04005C32 RID: 23602
		public TaskManagementElement CurrentUpgradeTimelineElement;

		// Token: 0x04005C33 RID: 23603
		public List<TemporarySimGameResult> TemporaryResultTracker = new List<TemporarySimGameResult>();

		// Token: 0x04005C34 RID: 23604
		public StarSystem TargetSystem;

		// Token: 0x04005C35 RID: 23605
		public StarSystem NearestToTarget;

		// Token: 0x04005C36 RID: 23606
		private List<Contract> globalContracts = new List<Contract>();

		// Token: 0x04005C37 RID: 23607
		private Contract activeBreadcrumb;

		// Token: 0x04005C38 RID: 23608
		private Contract pendingBreadcrumb;

		// Token: 0x04005C39 RID: 23609
		private List<ContractData> contractBits = new List<ContractData>();

		// Token: 0x04005C3B RID: 23611
		public ContractScope ContractScope = ContractScope.STANDARD;

		// Token: 0x04005C3C RID: 23612
		private bool AllowDebug;

		// Token: 0x04005C3E RID: 23614
		private Dictionary<ArgoController.RepairStateLocations, int> argoLocationRepairStates = new Dictionary<ArgoController.RepairStateLocations, int>();

		// Token: 0x04005C3F RID: 23615
		private List<string> usedRoninIDs = new List<string>();

		// Token: 0x04005C40 RID: 23616
		private List<SimGameState.SimGameCharacterType> characterList = new List<SimGameState.SimGameCharacterType>();

		// Token: 0x04005C41 RID: 23617
		private List<bool> characterStatus = new List<bool>();

		// Token: 0x04005C42 RID: 23618
		protected List<string> mapDiscardPile = new List<string>();

		// Token: 0x04005C43 RID: 23619
		protected List<string> contractDiscardPile = new List<string>();

		// Token: 0x04005C44 RID: 23620
		public List<string> pilotGenCallsignDiscardPile = new List<string>();

		// Token: 0x04005C45 RID: 23621
		public List<string> pilotGenVoiceDiscardPile = new List<string>();

		// Token: 0x04005C46 RID: 23622
		public List<string> pilotGenPortraitDiscardPile = new List<string>();

		// Token: 0x04005C47 RID: 23623
		public Dictionary<string, List<string>> pilotableActorPaintDiscardPile = new Dictionary<string, List<string>>();

		// Token: 0x04005C48 RID: 23624
		private List<string> ignoredContractEmployers = new List<string>();

		// Token: 0x04005C49 RID: 23625
		private List<string> ignoredContractTargets = new List<string>();

		// Token: 0x04005C4A RID: 23626
		public List<string> displayedFactions = new List<string>();

		// Token: 0x04005C4B RID: 23627
		private SimGameConstantOverride constantOverrides = new SimGameConstantOverride();

		// Token: 0x04005C4C RID: 23628
		private SimGameDifficulty difficultySettings = new SimGameDifficulty();

		// Token: 0x04005C4D RID: 23629
		private bool campaignFailed;

		// Token: 0x04005C4E RID: 23630
		private bool completeBreadcrumbProcessQueued;

		// Token: 0x04005C4F RID: 23631
		public List<string> flashpointDiscardPile = new List<string>();

		// Token: 0x04005C50 RID: 23632
		public List<string> completedFlashpoints = new List<string>();

		// Token: 0x04005C51 RID: 23633
		private List<Flashpoint> availableFlashpointList = new List<Flashpoint>();

		// Token: 0x04005C52 RID: 23634
		private Flashpoint activeFlashpoint;

		// Token: 0x04005C53 RID: 23635
		private bool inFlashpointCooldown;

		// Token: 0x04005C54 RID: 23636
		private int flashpointCooldownDays;

		// Token: 0x04005C55 RID: 23637
		protected List<string> WhitelistedRonin = new List<string>();

		// Token: 0x04005C56 RID: 23638
		protected List<string> AlliedFactions = new List<string>();

		// Token: 0x04005C57 RID: 23639
		protected List<string> VisitedStarSystems = new List<string>();

		// Token: 0x04005C58 RID: 23640
		protected List<string> CareerModeEndAlliedFactions = new List<string>();

		// Token: 0x04005C59 RID: 23641
		private bool careerModeLocked;

		// Token: 0x04005C5A RID: 23642
		public SimGameContext Context;

		// Token: 0x04005C5B RID: 23643
		private ConversationSpeakerList ConversationSpeakers;

		// Token: 0x04005C5C RID: 23644
		private List<LifepathNodeDef> lifenodes = new List<LifepathNodeDef>();

		// Token: 0x04005C5D RID: 23645
		private List<SimGameStringList> stringsLists = new List<SimGameStringList>();

		// Token: 0x04005C5E RID: 23646
		private Dictionary<string, FactionDef> factions = new Dictionary<string, FactionDef>();

		// Token: 0x04005C5F RID: 23647
		public Contract SimulatedContract;

		// Token: 0x04005C60 RID: 23648
		private Dictionary<string, StarSystem> starDict = new Dictionary<string, StarSystem>();

		// Token: 0x04005C61 RID: 23649
		private Dictionary<string, List<StarSystem>> factStoreDict = new Dictionary<string, List<StarSystem>>();

		// Token: 0x04005C66 RID: 23654
		public List<string> HabitableTags = new List<string>();

		// Token: 0x04005C68 RID: 23656
		private bool canTimeElapse = true;

		// Token: 0x04005C69 RID: 23657
		private float realTimeElapsed;

		// Token: 0x04005C6A RID: 23658
		private int breadcrumbTravelAcrued;

		// Token: 0x04005C6B RID: 23659
		private bool needQueuedMilestoneCheck;

		// Token: 0x04005C6C RID: 23660
		private bool IsInBreadcrumbArrival;

		// Token: 0x04005C6E RID: 23662
		public Contract potentialTravelContract;

		// Token: 0x04005C70 RID: 23664
		private Dictionary<SimGameCrew, CastDef> _crewDefs = new Dictionary<SimGameCrew, CastDef>();

		// Token: 0x04005C71 RID: 23665
		private const string DEFAULT_PRIORITY_MISSION_TITLE = "Priority Mission";

		// Token: 0x04005C72 RID: 23666
		private const string DEFAULT_TRAVEL_CONTRACT_TITLE = "Travel Contract";

		// Token: 0x04005C74 RID: 23668
		protected Dictionary<WeightClass, List<string>> allAcquirableMechs = new Dictionary<WeightClass, List<string>>();

		// Token: 0x04005C75 RID: 23669
		protected int MaxActiveFlashpoints;

		// Token: 0x04005C76 RID: 23670
		protected int InitialFlashpointMinCooldown;

		// Token: 0x04005C77 RID: 23671
		protected int InitialFlashpointMaxCooldown;

		// Token: 0x04005C78 RID: 23672
		protected int FlashpointMinCooldown;

		// Token: 0x04005C79 RID: 23673
		protected int FlashpointMaxCooldown;

		// Token: 0x04005C7A RID: 23674
		protected int MaxGenFlashpointsPerDay;

		// Token: 0x04005C7B RID: 23675
		private Dictionary<long, BaseDescriptionDef> _contractTypeDescriptions = new Dictionary<long, BaseDescriptionDef>();

		// Token: 0x04005C7C RID: 23676
		private List<PilotDef> _roninPilots = new List<PilotDef>();

		// Token: 0x04005C7D RID: 23677
		public Dictionary<SimGameState.SimGameCharacterType, string> _conversationList = new Dictionary<SimGameState.SimGameCharacterType, string>();

		// Token: 0x04005C7E RID: 23678
		private Contract _selectedContract;

		// Token: 0x04005C7F RID: 23679
		private bool _selectedContractTravel;

		// Token: 0x04005C80 RID: 23680
		private bool _selectedContractSimulated;

		// Token: 0x04005C81 RID: 23681
		private bool _selectedContractForced;

		// Token: 0x04005C82 RID: 23682
		protected SimGameInterruptManager interruptQueue;

		// Token: 0x04005C83 RID: 23683
		public bool HasSimShipBeenSet;

		// Token: 0x04005C84 RID: 23684
		private bool _forceInterruptCheck;

		// Token: 0x04005C85 RID: 23685
		private List<FlashpointDef> flashpointPool = new List<FlashpointDef>();

		// Token: 0x04005C86 RID: 23686
		private List<FlashpointDef> initialFlashpointPool = new List<FlashpointDef>();

		// Token: 0x04005C87 RID: 23687
		private Dictionary<SimGameState.CareerModeScoreTypes, float> careerModeTargetValues = new Dictionary<SimGameState.CareerModeScoreTypes, float>();

		// Token: 0x04005C88 RID: 23688
		private Dictionary<SimGameState.CareerModeScoreTypes, float> careerModePerUnitValues = new Dictionary<SimGameState.CareerModeScoreTypes, float>();

		// Token: 0x04005C89 RID: 23689
		private Dictionary<SimGameState.CareerModeScoreTypes, SimGameState.CareerModeScoreCalulation> careerModeScoreCalculators = new Dictionary<SimGameState.CareerModeScoreTypes, SimGameState.CareerModeScoreCalulation>();

		// Token: 0x04005C8A RID: 23690
		private int careerModeFlashpointStartDate;

		// Token: 0x04005C8B RID: 23691
		public FactionMissionResultStatementBuckets factionMissionResultStatements = new FactionMissionResultStatementBuckets();

		// Token: 0x04005C8C RID: 23692
		public CSVReader starmapStoreManifest;

		// Token: 0x04005C8D RID: 23693
		public List<string> AlreadyClickedConversationResponses;

		// Token: 0x04005C8E RID: 23694
		private static readonly EventScope[] CompanyTrackerScope = new EventScope[]
		{
			EventScope.Company,
			EventScope.MechWarrior
		};

		// Token: 0x04005C8F RID: 23695
		private static readonly EventScope[] MechWarriorTrackerScope = new EventScope[] { EventScope.MechWarrior };

		// Token: 0x04005C90 RID: 23696
		private static readonly EventScope[] MoraleTrackerScope = new EventScope[]
		{
			EventScope.Company,
			EventScope.MechWarrior
		};

		// Token: 0x04005C91 RID: 23697
		private static readonly EventScope[] DeadTrackerScope = new EventScope[] { EventScope.DeadMechWarrior };

		// Token: 0x04005C95 RID: 23701
		private UnityAction onHeavyMetalLootPopupClosed;

		// Token: 0x04005C96 RID: 23702
		private const string EXCLUDED_PROCEDURAL_MAP_NAME = "mapGeneral_jumbledKarst_aDes";

		// Token: 0x04005C97 RID: 23703
		private const string EXCLUDED_PROECUDRAL_ENC_NAME = "encGeneral_ThreeWayBattle";

		// Token: 0x04005C98 RID: 23704
		private const int MAX_DEBUG_CONTRACT_COUNT = 1000;

		// Token: 0x04005C99 RID: 23705
		private const float CONTRACT_WAIT_TIME = 0.2f;

		// Token: 0x04005C9A RID: 23706
		public const char CBILL_ICON = '¢';

		// Token: 0x04005C9B RID: 23707
		private static int WeightMod = 100;

		// Token: 0x04005C9C RID: 23708
		private const string PRIORITY_MISSION_DEF_ID = "ContractTypePriority";

		// Token: 0x04005C9D RID: 23709
		private string PLAYER_CREST_ADDENDUM = "PlayerEmblems";

		// Token: 0x04005C9E RID: 23710
		private string CONVERSATION_TEXTURE_ADDENDUM = "ConversationTexture";

		// Token: 0x04005C9F RID: 23711
		private SimGameState.InitStates previousInitState;

		// Token: 0x04005CA0 RID: 23712
		private SimGameState.InitStates initState;

		// Token: 0x04005CA1 RID: 23713
		public const string BARRACKS_ICON_PREFIX = "uixSvgIcon_mwrank_";

		// Token: 0x04005CA2 RID: 23714
		public const string BARRACKS_KS_BACKER = "KSBacker";

		// Token: 0x04005CA3 RID: 23715
		public const string BARRACKS_ICON_RANK = "Rank";

		// Token: 0x04005CA4 RID: 23716
		public const string BARRACKS_ICON_RONIN = "Ronin";

		// Token: 0x04005CA5 RID: 23717
		public const string BARRACKS_ICON_COMMANDER = "Commander";

		// Token: 0x04005CA6 RID: 23718
		public const string REPORT_DAILY = "DailyLog";

		// Token: 0x04005CA7 RID: 23719
		public const string REPORT_DAY = "DAY";

		// Token: 0x04005CA8 RID: 23720
		public const string REPORT_TAG = "Tag Check";

		// Token: 0x04005CA9 RID: 23721
		public const string REPORT_STAT = "Stat Check";

		// Token: 0x04005CAA RID: 23722
		public const string SIMGAMESTATE_ID = "SimGameState";

		// Token: 0x04005CAB RID: 23723
		public const string SIMGAME_QUARTERLY = "SimGame_Monthly";

		// Token: 0x04005CAC RID: 23724
		public const string COMPANYSTAT_EXPENSE = "ExpenseLevel";

		// Token: 0x04005CAD RID: 23725
		public const string COMPANYSTAT_FUNDS = "Funds";

		// Token: 0x04005CAE RID: 23726
		public const string COMPANYSTAT_FUNDS_EVERGAINED = "FundsEverGained";

		// Token: 0x04005CAF RID: 23727
		public const string COMPANYSTAT_DIFFICULTY = "Difficulty";

		// Token: 0x04005CB0 RID: 23728
		public const string COMPANYSTAT_REPUTATION = "Reputation";

		// Token: 0x04005CB1 RID: 23729
		public const string COMPANYSTAT_INFLUENCE = "Influence";

		// Token: 0x04005CB2 RID: 23730
		public const string COMPANYSTAT_TRAVEL = "Travel";

		// Token: 0x04005CB3 RID: 23731
		public const string COMPANYSTAT_CAMPAIGNSTARTDATE = "CampaignStartDate";

		// Token: 0x04005CB4 RID: 23732
		public const string COMPANYSTAT_DAY = "Day";

		// Token: 0x04005CB5 RID: 23733
		public const string COMPANYSTAT_TASKDURATION = "TaskDuration";

		// Token: 0x04005CB6 RID: 23734
		public const string COMPANYSTAT_TRAVELTIME = "TravelTime";

		// Token: 0x04005CB7 RID: 23735
		public const string COMPANYSTAT_ITEMCOUNT = "Item";

		// Token: 0x04005CB8 RID: 23736
		public const string COMPANYSTAT_MEDTECH = "MedTechSkill";

		// Token: 0x04005CB9 RID: 23737
		public const string COMPANYSTAT_MECHTECH = "MechTechSkill";

		// Token: 0x04005CBA RID: 23738
		public const string COMPANYSTAT_UPGRADEVALUE = "UpgradeValue";

		// Token: 0x04005CBB RID: 23739
		public const string COMPANYSTAT_MORALE = "Morale";

		// Token: 0x04005CBC RID: 23740
		public const string COMPANYSTAT_TARGET_SYSTEM = "TargetSystem";

		// Token: 0x04005CBD RID: 23741
		public const string COMPANYSTAT_DAILY_XP = "ExperiencePerDay";

		// Token: 0x04005CBE RID: 23742
		public const string COMPANYSTAT_XP_CAP = "ExperiencePerDayCap";

		// Token: 0x04005CBF RID: 23743
		public const string COMPANYSTAT_SHIP_TYPE = "ShipType";

		// Token: 0x04005CC0 RID: 23744
		public const string COMPANYSTAT_STORY_SECTION = "StorySection";

		// Token: 0x04005CC1 RID: 23745
		public const string DAMAGED_ITEM = "DAMAGED";

		// Token: 0x04005CC2 RID: 23746
		public const string COMPANYSTAT_TARGET = "Target";

		// Token: 0x04005CC3 RID: 23747
		public const string COMPANYSTAT_EMPLOYER = "Employer";

		// Token: 0x04005CC4 RID: 23748
		public const string COMPANYSTAT_NEUTRALTOALL = "NeutralToAll";

		// Token: 0x04005CC5 RID: 23749
		public const string COMPANYSTAT_HOSTILETOALL = "HostileToAll";

		// Token: 0x04005CC6 RID: 23750
		public const string COMPANYSTAT_IS_GREATHOUSE = "IsGreatHouse";

		// Token: 0x04005CC7 RID: 23751
		public const string COMPANYSTAT_IS_OWNER = "IsOwner";

		// Token: 0x04005CC8 RID: 23752
		public const string MECH_PART_ITEM = "MECHPART";

		// Token: 0x04005CC9 RID: 23753
		public const string CAST_DEF_PREFIX = "castDef_";

		// Token: 0x04005CCA RID: 23754
		public const string FACTION_DEF_PREFIX = "faction_";

		// Token: 0x04005CCB RID: 23755
		public const string CONTRACT_TYPE_DESC_PREFIX = "ContractType";

		// Token: 0x04005CCC RID: 23756
		public const string COMPANY_TAG_HAS_UPGRADES = "ARGO_UpgradesApplied";

		// Token: 0x04005CCD RID: 23757
		public const string COMPANYSTAT_TOTAL_MECH_KILLS = "COMPANY_MechKills";

		// Token: 0x04005CCE RID: 23758
		public const string COMPANYSTAT_TOTAL_OTHER_KILLS = "COMPANY_OtherKills";

		// Token: 0x04005CCF RID: 23759
		public const string COMPANYSTAT_TOTAL_MISSIONS_ATTEMPTED = "COMPANY_MissionsAttempted";

		// Token: 0x04005CD0 RID: 23760
		public const string COMPANYSTAT_TOTAL_MISSIONS_COMPLETED = "COMPANY_MissionsSucceeded";

		// Token: 0x04005CD1 RID: 23761
		public const string COMPANYSTAT_TOTAL_MISSIONS_GOOD_FAITH = "COMPANY_MissionsGoodFaith";

		// Token: 0x04005CD2 RID: 23762
		public const string COMPANYSTAT_TOTAL_MISSIONS_FAILED = "COMPANY_MissionFailures";

		// Token: 0x04005CD3 RID: 23763
		public const string COMPANYSTAT_TOTAL_MISSION_DIFFICULTY = "COMPANY_MissionAggregateDifficulty";

		// Token: 0x04005CD4 RID: 23764
		public const string COMPANYSTAT_QUARTER_STARTING_FUNDS = "COMPANY_MonthlyStartingFunds";

		// Token: 0x04005CD5 RID: 23765
		public const string COMPANYSTAT_QUARTER_STARTING_MORALE = "COMPANY_MonthlyStartingMorale";

		// Token: 0x04005CD6 RID: 23766
		public const string COMPANYSTAT_HIRING_MW_ADDED = "COMPANY_MechWarriorsHired";

		// Token: 0x04005CD7 RID: 23767
		public const string COMPANYSTAT_HIRING_MW_FIRED = "COMPANY_MechWarriorsFired";

		// Token: 0x04005CD8 RID: 23768
		public const string COMPANYTAG_MW_HIGHMORALE = "pilot_morale_high";

		// Token: 0x04005CD9 RID: 23769
		public const string COMPANYTAG_MW_LOWMORALE = "pilot_morale_low";

		// Token: 0x04005CDA RID: 23770
		public const string COMPANYSTAT_MECHS_ADDED = "COMPANY_MechsAdded";

		// Token: 0x04005CDB RID: 23771
		public const string COMPANYSTAT_HASVISITEDROOM_CMDCENTER = "COMPANY_HasVisitedRoom_CmdCenter";

		// Token: 0x04005CDC RID: 23772
		public const string COMPANYSTAT_HASVISITEDROOM_MECHBAY = "COMPANY_HasVisitedRoom_MechBay";

		// Token: 0x04005CDD RID: 23773
		public const string COMPANYSTAT_HASVISITEDROOM_BARRACKS = "COMPANY_HasVisitedRoom_Barracks";

		// Token: 0x04005CDE RID: 23774
		public const string COMPANYSTAT_HASVISITEDROOM_NAVIGATION = "COMPANY_HasVisitedRoom_Navigation";

		// Token: 0x04005CDF RID: 23775
		public const string COMPANYSTAT_HASVISITEDROOM_ENGINEERING = "COMPANY_HasVisitedRoom_Engineering";

		// Token: 0x04005CE0 RID: 23776
		public const string COMPANYSTAT_HASVISITEDROOM_CPTQUARTERS = "COMPANY_HasVisitedRoom_CptQuarters";

		// Token: 0x04005CE1 RID: 23777
		public const string COMPANYSTAT_HASVISITEDROOM_SHOP = "COMPANY_HasVisitedRoom_Shop";

		// Token: 0x04005CE2 RID: 23778
		public const string COMPANYSTAT_HASVISITEDROOM_HIRING = "COMPANY_HasVisitedRoom_HiringHall";

		// Token: 0x04005CE3 RID: 23779
		public const string COMPANYSTAT_HASSEENTUTORIAL_MECHLAB = "COMPANY_HasSeenTutorial_MechLab";

		// Token: 0x04005CE4 RID: 23780
		public const string COMPANYSTAT_HASSEENECMREVIEW_MECHLAB = "COMPANY_HasSeenECMReview_MechLab";

		// Token: 0x04005CE5 RID: 23781
		public const string COMPANYSTAT_NOTIFICATIONSHOWN_LOWFUNDS = "COMPANY_NotificationViewed_LowFunds";

		// Token: 0x04005CE6 RID: 23782
		public const string COMPANYSTAT_NOTIFICATIONSHOWN_ARGOUPGRADENEEDED = "COMPANY_NotificationViewed_ArgoUpgradeNeeded";

		// Token: 0x04005CE7 RID: 23783
		public const string COMPANYSTAT_NOTIFICATIONSHOWN_MECHREPAIRSNEEDED = "COMPANY_NotificationViewed_BattleMechRepairsNeeded";

		// Token: 0x04005CE8 RID: 23784
		public const string GAMEMODE_TAG_HEADER = "SYSTEM_GAMEMODE_";

		// Token: 0x04005CE9 RID: 23785
		public const string HERALDRYDEF_CAREER_HEADER = "heraldrydef_career";

		// Token: 0x04005CEA RID: 23786
		public const string COMMANDER_CAREER_HEADER = "commander_career";

		// Token: 0x04005CEB RID: 23787
		public const string SIMGAME_CHARACTER_TOOLIP = "TooltipSimGameCharacter";

		// Token: 0x04005CEC RID: 23788
		public const string TEMP_RESULT_TAG_HEADER = "MODIFIED_TAG_";

		// Token: 0x04005CED RID: 23789
		public const string TEMP_RESULT_STAT_HEADER = "MODIFIED_STAT_";

		// Token: 0x04005CEE RID: 23790
		public const string DEFAULT_DIFFICULTY_SETTINGS = "DifficultySettings";

		// Token: 0x04005CEF RID: 23791
		public const string DEFAULT_CAREER_DIFFICULTY_SETTINGS = "CareerDifficultySettings";

		// Token: 0x04005CF0 RID: 23792
		public const string COMPANYSTAT_PROLOGUE_SKIP = "SkipPrologue";

		// Token: 0x04005CF1 RID: 23793
		public const string FLASHPOINT_INTRO_IMAGE = "uixTxrSpot_flashpointExample";

		// Token: 0x04005CF2 RID: 23794
		public const string FLASHPOINT_CONTRACT_IMAGE = "uixTxrSpot_flashpointContract2";

		// Token: 0x04005CF3 RID: 23795
		public const string HEAVY_METAL_FREE_CONTENT_IMAGE = "uixTxrSpot_flashpointExample";

		// Token: 0x04005CF4 RID: 23796
		public const string NEW_STARMAP_TECH_IMAGE = "uixTxrSpot_StarmapV2-Example";

		// Token: 0x04005CF5 RID: 23797
		public const string CAREERMODE_ALL_LIGHT_CHASSIS = "careerModeAllLightChassis";

		// Token: 0x04005CF6 RID: 23798
		public const string CAREERMODE_ALL_MEDIUM_CHASSIS = "careerModeAllMediumChassis";

		// Token: 0x04005CF7 RID: 23799
		public const string CAREERMODE_ALL_HEAVY_CHASSIS = "careerModeAllHeavyChassis";

		// Token: 0x04005CF8 RID: 23800
		public const string CAREERMODE_ALL_ASSAULT_CHASSIS = "careerModeAllAssaultChassis";

		// Token: 0x04005CF9 RID: 23801
		public const string CAREERMODE_LOCKED_STAT_HEADER = "CAREER_MODE_FINAL_";

		// Token: 0x04005CFA RID: 23802
		public const string ALLIED_TAG_HEADER = "ALLIED_FACTION_";

		// Token: 0x04005CFB RID: 23803
		public const string ENEMY_TAG_HEADER = "ENEMY_FACTION_";

		// Token: 0x04005CFC RID: 23804
		public const string ALLIANCE_BREAK_HEADER = "AllianceBroken";

		// Token: 0x04005CFD RID: 23805
		public const string FACTION_REP_NOTIFICATION_HEADER = "REP_NOTIFICATION_STATE";

		// Token: 0x04005CFE RID: 23806
		public const string FACTION_REP_NOTIFICATION_TIMESTAMP = "LAST_REP_NOTIFICATION_DAY";

		// Token: 0x04005CFF RID: 23807
		public const string COMPANY_TAG_NOTIFIED_OF_FLASHPOINTS = "HasSeenFlashpointNotification";

		// Token: 0x04005D00 RID: 23808
		public const string COMPANY_TAG_NOTIFIED_OF_NEWSTARMAPTECH = "HasSeenNewStarmapTechNotification";

		// Token: 0x04005D01 RID: 23809
		public const int MAX_UNITS_IN_LANCE = 4;

		// Token: 0x04005D02 RID: 23810
		public const string COMPANYSTAT_CAREER_FINAL_DIFFICULTY_MOD = "COMPANY_Career_FinalDiffMod";

		// Token: 0x04005D03 RID: 23811
		public const string COMPANY_TAG_NOTIFIED_OF_HEAVYMETAL_FREE = "HasSeenHeavyMetalFreeContentPopup";

		// Token: 0x04005D04 RID: 23812
		public const string COMPANY_TAG_NOTIFIED_OF_HEAVYMETAL_LOOT = "HasSeenHeavyMetalLootPopup";

		// Token: 0x04005D05 RID: 23813
		public const string HEAVY_METAL_REWARD_COLLECTION = "itemCollection_HM_careerStarter";

		// Token: 0x04005D06 RID: 23814
		private const string STARSYSTEM_DEF_PREFIX = "starsystemdef_";

		// Token: 0x04005D18 RID: 23832
		public PilotGenerator PilotGenerator;

		// Token: 0x04005D19 RID: 23833
		public SGRoomManager RoomManager;

		// Token: 0x04005D1A RID: 23834
		public SGTravelManager TravelManager;

		// Token: 0x04005D1B RID: 23835
		public ItemCollectionResultGenerator ItemCollectionResultGen;

		// Token: 0x04005D1C RID: 23836
		public readonly string[] INJURY_NAMES = new string[]
		{
			"Uninjured", "Minor", "Minor", "Moderate", "Moderate", "Serious", "Serious", "Severe", "Severe", "Critical",
			"Mortal"
		};

		// Token: 0x04005D1D RID: 23837
		public readonly string[] COLORS = new string[] { "Red", "Blue", "Green", "Yellow", "Gold", "Silver", "Black", "White", "Violet", "Teal" };

		// Token: 0x04005D1E RID: 23838
		public readonly string[] ANIMALS = new string[] { "Tiger", "Badger", "Shark", "Turtle", "Falcon", "Wolf", "Snake", "Platypus", "Fox", "Beetle" };

		// Token: 0x04005D1F RID: 23839
		private const int DEFAULT_SEED = 0;

		// Token: 0x04005D20 RID: 23840
		private uint UIDCount;

		// Token: 0x04005D21 RID: 23841
		private string DebugSeed;

		// Token: 0x04005D23 RID: 23843
		private SimGameReport.ReportEntry DailyReport;

		// Token: 0x04005D24 RID: 23844
		public static SimGameLogLevel ReportLevel = SimGameLogLevel.CRITICAL;

		// Token: 0x04005D25 RID: 23845
		private readonly string[] UpgradeStartingMechWarriors = new string[] { "pilot_sim_starter_glitch", "pilot_sim_starter_behemoth", "pilot_sim_starter_dekker", "pilot_sim_starter_medusa" };

		// Token: 0x04005D26 RID: 23846
		private const int MAX_RANK = 10;

		// Token: 0x04005D27 RID: 23847
		private GameInstanceSave save;

		// Token: 0x04005D28 RID: 23848
		public StarmapSave starmapSave;

		// Token: 0x04005D2C RID: 23852
		private SaveTravelData savedTravelData;

		// Token: 0x04005D2D RID: 23853
		private Dictionary<string, string> restrictionDictionary;

		// Token: 0x020010CA RID: 4298
		// (Invoke) Token: 0x06009331 RID: 37681
		protected delegate float CareerModeScoreCalulation();

		// Token: 0x020010CB RID: 4299
		public enum ItemCountType
		{
			// Token: 0x04005D2F RID: 23855
			ALL,
			// Token: 0x04005D30 RID: 23856
			UNDAMAGED_ONLY,
			// Token: 0x04005D31 RID: 23857
			DAMAGED_ONLY
		}

		// Token: 0x020010CC RID: 4300
		private struct PotentialContract
		{
			// Token: 0x04005D32 RID: 23858
			public ContractOverride contractOverride;

			// Token: 0x04005D33 RID: 23859
			public FactionValue employer;

			// Token: 0x04005D34 RID: 23860
			public FactionValue target;

			// Token: 0x04005D35 RID: 23861
			public FactionValue employerAlly;

			// Token: 0x04005D36 RID: 23862
			public FactionValue targetAlly;

			// Token: 0x04005D37 RID: 23863
			public FactionValue NeutralToAll;

			// Token: 0x04005D38 RID: 23864
			public FactionValue HostileToAll;

			// Token: 0x04005D39 RID: 23865
			public int difficulty;
		}

		// Token: 0x020010CD RID: 4301
		private class FilteredComparisonResults
		{
			// Token: 0x1700199E RID: 6558
			// (get) Token: 0x06009334 RID: 37684 RVA: 0x00268C47 File Offset: 0x00266E47
			public bool IsEmpty
			{
				get
				{
					return !this.Strings.Any<string>() && !this.Factions.Any<string>();
				}
			}

			// Token: 0x06009335 RID: 37685 RVA: 0x00268C66 File Offset: 0x00266E66
			public FilteredComparisonResults(IEnumerable<string> strings, IEnumerable<string> factions)
			{
				this.Strings = new HashSet<string>(strings.Distinct<string>());
				this.Factions = new HashSet<string>(factions.Distinct<string>());
			}

			// Token: 0x04005D3A RID: 23866
			public HashSet<string> Strings;

			// Token: 0x04005D3B RID: 23867
			public IEnumerable<string> Factions;
		}

		// Token: 0x020010CE RID: 4302
		private class ContractParticipants
		{
			// Token: 0x06009336 RID: 37686 RVA: 0x00268C90 File Offset: 0x00266E90
			public ContractParticipants(FactionValue target, WeightedList<FactionValue> targetAllies, WeightedList<FactionValue> employerAllies, List<FactionValue> neutrals, List<FactionValue> hostiles)
			{
				this.Target = target;
				this.TargetAllies = targetAllies;
				this.EmployerAllies = employerAllies;
				this.NeutralToAll = neutrals;
				this.HostileToAll = hostiles;
			}

			// Token: 0x04005D3C RID: 23868
			public FactionValue Target;

			// Token: 0x04005D3D RID: 23869
			public WeightedList<FactionValue> EmployerAllies;

			// Token: 0x04005D3E RID: 23870
			public WeightedList<FactionValue> TargetAllies;

			// Token: 0x04005D3F RID: 23871
			public List<FactionValue> NeutralToAll;

			// Token: 0x04005D40 RID: 23872
			public List<FactionValue> HostileToAll;
		}

		// Token: 0x020010CF RID: 4303
		private class ChosenContractParticipants
		{
			// Token: 0x06009337 RID: 37687 RVA: 0x00268CC0 File Offset: 0x00266EC0
			public ChosenContractParticipants()
			{
				FactionValue invalidUnsetFactionValue = FactionEnumeration.GetInvalidUnsetFactionValue();
				this.Player1 = FactionEnumeration.GetPlayer1sMercUnitFactionValue();
				this.Player2 = FactionEnumeration.GetPlayer2sMercUnitFactionValue();
				this.Employer = invalidUnsetFactionValue;
				this.EmployersAlly = invalidUnsetFactionValue;
				this.Target = invalidUnsetFactionValue;
				this.TargetsAlly = invalidUnsetFactionValue;
				this.NeutralToAll = invalidUnsetFactionValue;
				this.HostileToAll = invalidUnsetFactionValue;
			}

			// Token: 0x06009338 RID: 37688 RVA: 0x00268D1C File Offset: 0x00266F1C
			public ChosenContractParticipants(FactionValue player1, FactionValue player2, FactionValue employer, FactionValue employersAlly, FactionValue target, FactionValue targetsAlly, FactionValue neutralToAll, FactionValue hostileToAll)
			{
				this.Player1 = player1;
				this.Player2 = player2;
				this.Employer = employer;
				this.EmployersAlly = employersAlly;
				this.Target = target;
				this.TargetsAlly = targetsAlly;
				this.NeutralToAll = neutralToAll;
				this.HostileToAll = hostileToAll;
			}

			// Token: 0x04005D41 RID: 23873
			public FactionValue Player1;

			// Token: 0x04005D42 RID: 23874
			public FactionValue Player2;

			// Token: 0x04005D43 RID: 23875
			public FactionValue Employer;

			// Token: 0x04005D44 RID: 23876
			public FactionValue EmployersAlly;

			// Token: 0x04005D45 RID: 23877
			public FactionValue Target;

			// Token: 0x04005D46 RID: 23878
			public FactionValue TargetsAlly;

			// Token: 0x04005D47 RID: 23879
			public FactionValue NeutralToAll;

			// Token: 0x04005D48 RID: 23880
			public FactionValue HostileToAll;
		}

		// Token: 0x020010D0 RID: 4304
		private class MapEncounterContractData
		{
			// Token: 0x1700199F RID: 6559
			// (get) Token: 0x06009339 RID: 37689 RVA: 0x00268D6C File Offset: 0x00266F6C
			public bool HasContracts
			{
				get
				{
					return this.Contracts.Any<int>();
				}
			}

			// Token: 0x0600933A RID: 37690 RVA: 0x00268D79 File Offset: 0x00266F79
			public MapEncounterContractData()
			{
				this.Encounters = new Dictionary<int, List<EncounterLayer_MDD>>();
				this.Contracts = new HashSet<int>();
				this.FlatContracts = new WeightedList<SimGameState.PotentialContract>(WeightedListType.WeightedRandom, null, null, 0);
			}

			// Token: 0x0600933B RID: 37691 RVA: 0x00268DA7 File Offset: 0x00266FA7
			public void AddEncounter(int contractType, EncounterLayer_MDD encounter)
			{
				if (!this.Encounters.ContainsKey(contractType))
				{
					this.Encounters[contractType] = new List<EncounterLayer_MDD>();
				}
				this.Encounters[contractType].Add(encounter);
			}

			// Token: 0x0600933C RID: 37692 RVA: 0x00268DDA File Offset: 0x00266FDA
			public void AddContract(int contractType, SimGameState.PotentialContract contract, int weight)
			{
				this.Contracts.Add(contractType);
				this.FlatContracts.Add(contract, weight);
			}

			// Token: 0x04005D49 RID: 23881
			public Dictionary<int, List<EncounterLayer_MDD>> Encounters;

			// Token: 0x04005D4A RID: 23882
			public HashSet<int> Contracts;

			// Token: 0x04005D4B RID: 23883
			public WeightedList<SimGameState.PotentialContract> FlatContracts;
		}

		// Token: 0x020010D1 RID: 4305
		private class ContractDifficultyRange
		{
			// Token: 0x0600933D RID: 37693 RVA: 0x00268DF6 File Offset: 0x00266FF6
			public ContractDifficultyRange(int minDiff, int maxDiff, ContractDifficulty minDiffClamped, ContractDifficulty maxDiffClamped)
			{
				this.MinDifficulty = minDiff;
				this.MinDifficultyClamped = minDiffClamped;
				this.MaxDifficulty = maxDiff;
				this.MaxDifficultyClamped = maxDiffClamped;
			}

			// Token: 0x04005D4C RID: 23884
			public int MinDifficulty;

			// Token: 0x04005D4D RID: 23885
			public int MaxDifficulty;

			// Token: 0x04005D4E RID: 23886
			public ContractDifficulty MinDifficultyClamped;

			// Token: 0x04005D4F RID: 23887
			public ContractDifficulty MaxDifficultyClamped;
		}

		// Token: 0x020010D2 RID: 4306
		public class AddContractData
		{
			// Token: 0x0600933E RID: 37694 RVA: 0x00003956 File Offset: 0x00001B56
			public AddContractData()
			{
			}

			// Token: 0x0600933F RID: 37695 RVA: 0x00268E1C File Offset: 0x0026701C
			public AddContractData(ContractData saveBits)
			{
				this.ContractName = saveBits.conName;
				this.Employer = saveBits.conEmployer;
				this.Target = saveBits.conTarget;
				this.IsGlobal = true;
				this.TargetSystem = saveBits.conLocation;
				this.TargetAlly = saveBits.conAlly;
				this.EmployerAlly = saveBits.conEmployersAlly;
				this.SaveGuid = saveBits.GUID;
				this.FromSave = true;
			}

			// Token: 0x04005D50 RID: 23888
			public string Map;

			// Token: 0x04005D51 RID: 23889
			public string MapPath;

			// Token: 0x04005D52 RID: 23890
			public string EncounterGuid;

			// Token: 0x04005D53 RID: 23891
			public string ContractName;

			// Token: 0x04005D54 RID: 23892
			public int Difficulty;

			// Token: 0x04005D55 RID: 23893
			public string NextNodeId;

			// Token: 0x04005D56 RID: 23894
			public string OnContractFailureMilestone;

			// Token: 0x04005D57 RID: 23895
			public bool CarryOverNegotiation;

			// Token: 0x04005D58 RID: 23896
			public bool IsGlobal;

			// Token: 0x04005D59 RID: 23897
			public int RandomSeed;

			// Token: 0x04005D5A RID: 23898
			public string TargetSystem;

			// Token: 0x04005D5B RID: 23899
			public string SaveGuid;

			// Token: 0x04005D5C RID: 23900
			public bool FromSave;

			// Token: 0x04005D5D RID: 23901
			public string Employer;

			// Token: 0x04005D5E RID: 23902
			public string EmployerAlly;

			// Token: 0x04005D5F RID: 23903
			public string Target;

			// Token: 0x04005D60 RID: 23904
			public string TargetAlly;

			// Token: 0x04005D61 RID: 23905
			public string NeutralToAll;

			// Token: 0x04005D62 RID: 23906
			public string HostileToAll;
		}

		// Token: 0x020010D3 RID: 4307
		private class ValidFactionResult
		{
			// Token: 0x06009340 RID: 37696 RVA: 0x00268E91 File Offset: 0x00267091
			public static string GetStringHeader()
			{
				return "System, Map, Contract, Difficulty, Scope, IsValid, Employer, Target, EmployerAlly, TargetAlly, Neutral, Hostile";
			}

			// Token: 0x06009341 RID: 37697 RVA: 0x00268E98 File Offset: 0x00267098
			public override string ToString()
			{
				return string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}", new object[]
				{
					this.System,
					this.Maps,
					this.Contract,
					this.Difficulty,
					this.Scope,
					this.Valid,
					this.Employer.Name,
					this.Target.Name,
					this.EmployerAlly.Name,
					this.TargetAlly.Name,
					this.Neutral.Name,
					this.Hostile.Name
				});
			}

			// Token: 0x04005D63 RID: 23907
			public bool Valid;

			// Token: 0x04005D64 RID: 23908
			public FactionValue Employer;

			// Token: 0x04005D65 RID: 23909
			public FactionValue EmployerAlly;

			// Token: 0x04005D66 RID: 23910
			public FactionValue Target;

			// Token: 0x04005D67 RID: 23911
			public FactionValue TargetAlly;

			// Token: 0x04005D68 RID: 23912
			public FactionValue Neutral;

			// Token: 0x04005D69 RID: 23913
			public FactionValue Hostile;

			// Token: 0x04005D6A RID: 23914
			public string System;

			// Token: 0x04005D6B RID: 23915
			public string Maps;

			// Token: 0x04005D6C RID: 23916
			public string Contract;

			// Token: 0x04005D6D RID: 23917
			public int Difficulty;

			// Token: 0x04005D6E RID: 23918
			public string Scope;
		}

		// Token: 0x020010D4 RID: 4308
		private enum InitStates
		{
			// Token: 0x04005D70 RID: 23920
			UNLOADED,
			// Token: 0x04005D71 RID: 23921
			INITIALIZED,
			// Token: 0x04005D72 RID: 23922
			DEFS_LOADED,
			// Token: 0x04005D73 RID: 23923
			HEADLESS_ON_READY_SUCCESS = 4,
			// Token: 0x04005D74 RID: 23924
			UX_SYSTEMS_CREATED = 8,
			// Token: 0x04005D75 RID: 23925
			HEADLESS_STATE = 7,
			// Token: 0x04005D76 RID: 23926
			ATTACHED_UX_STATE,
			// Token: 0x04005D77 RID: 23927
			FROM_SAVE = 16,
			// Token: 0x04005D78 RID: 23928
			UX_ATTACHED_PREVIOUSLY = 32,
			// Token: 0x04005D79 RID: 23929
			ASYNC_ATTACHING_UX_STATE = 64,
			// Token: 0x04005D7A RID: 23930
			ASYNC_LOADING_DEFS = 128,
			// Token: 0x04005D7B RID: 23931
			REQUEST_ATTACH_UX_STATE = 256,
			// Token: 0x04005D7C RID: 23932
			REQUEST_DEFS_LOAD = 512,
			// Token: 0x04005D7D RID: 23933
			REQUEST_AUTO_HEADLESS_STATE_ON_READY = 3,
			// Token: 0x04005D7E RID: 23934
			ACTIVELY_SAVING = 1024,
			// Token: 0x04005D7F RID: 23935
			UPDATE_MILESTONE_ON_SAVE_LOADED = 2048
		}

		// Token: 0x020010D5 RID: 4309
		[Serializable]
		public enum SimGameCharacterType
		{
			// Token: 0x04005D81 RID: 23937
			UNSET,
			// Token: 0x04005D82 RID: 23938
			ALEXANDER,
			// Token: 0x04005D83 RID: 23939
			DARIUS,
			// Token: 0x04005D84 RID: 23940
			FARAH,
			// Token: 0x04005D85 RID: 23941
			KAMEA,
			// Token: 0x04005D86 RID: 23942
			SUMIRE,
			// Token: 0x04005D87 RID: 23943
			YANG,
			// Token: 0x04005D88 RID: 23944
			MONITOR,
			// Token: 0x04005D89 RID: 23945
			NAVSCREEN,
			// Token: 0x04005D8A RID: 23946
			DEFAULT,
			// Token: 0x04005D8B RID: 23947
			CONTRACTS,
			// Token: 0x04005D8C RID: 23948
			MECHWARRIORS,
			// Token: 0x04005D8D RID: 23949
			MEMORIAL,
			// Token: 0x04005D8E RID: 23950
			ARGOUPGRADE,
			// Token: 0x04005D8F RID: 23951
			MECHLAB,
			// Token: 0x04005D90 RID: 23952
			BREAKDOWN,
			// Token: 0x04005D91 RID: 23953
			COMMANDER,
			// Token: 0x04005D92 RID: 23954
			HOLOGRAM,
			// Token: 0x04005D93 RID: 23955
			HERALDRY
		}

		// Token: 0x020010D6 RID: 4310
		public enum CareerModeScoreTypes
		{
			// Token: 0x04005D95 RID: 23957
			CBillScore,
			// Token: 0x04005D96 RID: 23958
			ContractScore,
			// Token: 0x04005D97 RID: 23959
			ChassisScore,
			// Token: 0x04005D98 RID: 23960
			WeightScore,
			// Token: 0x04005D99 RID: 23961
			AllChassis,
			// Token: 0x04005D9A RID: 23962
			PilotExperience,
			// Token: 0x04005D9B RID: 23963
			SystemsVisited,
			// Token: 0x04005D9C RID: 23964
			AllSystems,
			// Token: 0x04005D9D RID: 23965
			PositiveReputation,
			// Token: 0x04005D9E RID: 23966
			NegativeReputation,
			// Token: 0x04005D9F RID: 23967
			MaxedReputation,
			// Token: 0x04005DA0 RID: 23968
			ArgoUpgrade,
			// Token: 0x04005DA1 RID: 23969
			AllArgoUpgrade,
			// Token: 0x04005DA2 RID: 23970
			MoraleScore,
			// Token: 0x04005DA3 RID: 23971
			MRBScore,
			// Token: 0x04005DA4 RID: 23972
			MaxMRBScore
		}

		// Token: 0x020010D7 RID: 4311
		public enum FactionReputationStateForNotifications
		{
			// Token: 0x04005DA6 RID: 23974
			ShownNegative = -1,
			// Token: 0x04005DA7 RID: 23975
			Neutral,
			// Token: 0x04005DA8 RID: 23976
			ShownPositive,
			// Token: 0x04005DA9 RID: 23977
			ShownAllied
		}

		// Token: 0x020010D8 RID: 4312
		public enum TriggerSaveNowResult
		{
			// Token: 0x04005DAB RID: 23979
			UNKNOWN,
			// Token: 0x04005DAC RID: 23980
			SAVED_NOW,
			// Token: 0x04005DAD RID: 23981
			DID_QUEUE,
			// Token: 0x04005DAE RID: 23982
			DID_NOT_QUEUE
		}

		// Token: 0x020010D9 RID: 4313
		public enum TriggerSaveNowOption
		{
			// Token: 0x04005DB0 RID: 23984
			DONT_QUEUE,
			// Token: 0x04005DB1 RID: 23985
			QUEUE_IF_NEEDED
		}

		// Token: 0x020010DA RID: 4314
		[SerializableEnum("SimGameType")]
		public enum SimGameType
		{
			// Token: 0x04005DB3 RID: 23987
			INVALID_UNSET,
			// Token: 0x04005DB4 RID: 23988
			KAMEA_CAMPAIGN,
			// Token: 0x04005DB5 RID: 23989
			CAREER,
			// Token: 0x04005DB6 RID: 23990
			NONE
		}
	}
}
