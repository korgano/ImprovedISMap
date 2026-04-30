using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech.Save.Test;
using BattleTech.Serialization;
using fastJSON;
using HBS.Collections;
using UnityEngine;

namespace BattleTech
{
	// Token: 0x02001115 RID: 4373
	[SerializableContract("StarSystem")]
	public class StarSystem : IGuid
	{
		// Token: 0x170019D7 RID: 6615
		// (get) Token: 0x060094E6 RID: 38118 RVA: 0x0026EC48 File Offset: 0x0026CE48
		// (set) Token: 0x060094E7 RID: 38119 RVA: 0x0026EC50 File Offset: 0x0026CE50
		public StarSystemDef Def { get; private set; }

		// Token: 0x170019D8 RID: 6616
		// (get) Token: 0x060094E8 RID: 38120 RVA: 0x0026EC59 File Offset: 0x0026CE59
		// (set) Token: 0x060094E9 RID: 38121 RVA: 0x0026EC61 File Offset: 0x0026CE61
		[SerializableMember(SerializationTarget.SaveGameAndMetaData)]
		public string Name { get; private set; }

		// Token: 0x170019D9 RID: 6617
		// (get) Token: 0x060094EA RID: 38122 RVA: 0x0026EC6A File Offset: 0x0026CE6A
		// (set) Token: 0x060094EB RID: 38123 RVA: 0x0026EC72 File Offset: 0x0026CE72
		[SerializableMember(SerializationTarget.SaveGame)]
		public string ID { get; private set; }

		// Token: 0x170019DA RID: 6618
		// (get) Token: 0x060094EC RID: 38124 RVA: 0x0026EC7B File Offset: 0x0026CE7B
		public string Icon
		{
			get
			{
				return this.Def.Description.Icon;
			}
		}

		// Token: 0x170019DB RID: 6619
		// (get) Token: 0x060094ED RID: 38125 RVA: 0x0026EC8D File Offset: 0x0026CE8D
		// (set) Token: 0x060094EE RID: 38126 RVA: 0x0026EC95 File Offset: 0x0026CE95
		[SerializableMember(SerializationTarget.SaveGame)]
		public List<PilotDef> PermanentRonin { get; private set; }

		// Token: 0x170019DC RID: 6620
		// (get) Token: 0x060094EF RID: 38127 RVA: 0x0026EC9E File Offset: 0x0026CE9E
		// (set) Token: 0x060094F0 RID: 38128 RVA: 0x0026ECA6 File Offset: 0x0026CEA6
		[SerializableMember(SerializationTarget.SaveGame)]
		public List<PilotDef> AvailablePilots { get; private set; }

		// Token: 0x170019DD RID: 6621
		// (get) Token: 0x060094F1 RID: 38129 RVA: 0x0026ECAF File Offset: 0x0026CEAF
		// (set) Token: 0x060094F2 RID: 38130 RVA: 0x0026ECB7 File Offset: 0x0026CEB7
		[SerializableMember(SerializationTarget.SaveGame)]
		public PilotDef LastPilotAdded { get; private set; }

		// Token: 0x170019DE RID: 6622
		// (get) Token: 0x060094F3 RID: 38131 RVA: 0x0026ECC0 File Offset: 0x0026CEC0
		// (set) Token: 0x060094F4 RID: 38132 RVA: 0x0026ECC8 File Offset: 0x0026CEC8
		[SerializableMember(SerializationTarget.SaveGame)]
		public List<TechDef> AvailableMedTechs { get; private set; }

		// Token: 0x170019DF RID: 6623
		// (get) Token: 0x060094F5 RID: 38133 RVA: 0x0026ECD1 File Offset: 0x0026CED1
		// (set) Token: 0x060094F6 RID: 38134 RVA: 0x0026ECD9 File Offset: 0x0026CED9
		[SerializableMember(SerializationTarget.SaveGame)]
		public List<TechDef> AvailableMechTechs { get; private set; }

		// Token: 0x170019E0 RID: 6624
		// (get) Token: 0x060094F7 RID: 38135 RVA: 0x0026ECE2 File Offset: 0x0026CEE2
		// (set) Token: 0x060094F8 RID: 38136 RVA: 0x0026ECEA File Offset: 0x0026CEEA
		public SimGameState Sim { get; private set; }

		// Token: 0x170019E1 RID: 6625
		// (get) Token: 0x060094F9 RID: 38137 RVA: 0x0026ECF3 File Offset: 0x0026CEF3
		// (set) Token: 0x060094FA RID: 38138 RVA: 0x0026ECFB File Offset: 0x0026CEFB
		public Shop SystemShop { get; private set; }

		// Token: 0x170019E2 RID: 6626
		// (get) Token: 0x060094FB RID: 38139 RVA: 0x0026ED04 File Offset: 0x0026CF04
		// (set) Token: 0x060094FC RID: 38140 RVA: 0x0026ED0C File Offset: 0x0026CF0C
		public Shop FactionShop { get; private set; }

		// Token: 0x170019E3 RID: 6627
		// (get) Token: 0x060094FD RID: 38141 RVA: 0x0026ED15 File Offset: 0x0026CF15
		// (set) Token: 0x060094FE RID: 38142 RVA: 0x0026ED1D File Offset: 0x0026CF1D
		public Shop BlackMarketShop { get; private set; }

		// Token: 0x170019E4 RID: 6628
		// (get) Token: 0x060094FF RID: 38143 RVA: 0x0026ED26 File Offset: 0x0026CF26
		// (set) Token: 0x06009500 RID: 38144 RVA: 0x0026ED2E File Offset: 0x0026CF2E
		[JsonIgnore]
		[SerializableMember(SerializationTarget.SaveGame)]
		public StatCollection Stats { get; private set; }

		// Token: 0x170019E5 RID: 6629
		// (get) Token: 0x06009501 RID: 38145 RVA: 0x0026ED37 File Offset: 0x0026CF37
		public bool Refueling
		{
			get
			{
				return this.Def.FuelingStation;
			}
		}

		// Token: 0x170019E6 RID: 6630
		// (get) Token: 0x06009502 RID: 38146 RVA: 0x0026ED44 File Offset: 0x0026CF44
		public int JumpDistance
		{
			get
			{
				if (this.Sim == null)
				{
					return this.Def.JumpDistance;
				}
				return this.Sim.GetInSystemTransitTime(this.Def.JumpDistance);
			}
		}

		// Token: 0x170019E7 RID: 6631
		// (get) Token: 0x06009503 RID: 38147 RVA: 0x0026ED70 File Offset: 0x0026CF70
		public SimGameSpaceController.StarType StarType
		{
			get
			{
				return this.Def.StarType;
			}
		}

		// Token: 0x170019E8 RID: 6632
		// (get) Token: 0x06009504 RID: 38148 RVA: 0x0026ED7D File Offset: 0x0026CF7D
		public FakeVector3 Position
		{
			get
			{
				return this.Def.Position;
			}
		}

		// Token: 0x170019E9 RID: 6633
		// (get) Token: 0x06009505 RID: 38149 RVA: 0x0026ED8A File Offset: 0x0026CF8A
		public FactionValue OwnerValue
		{
			get
			{
				return this.Def.OwnerValue;
			}
		}

		// Token: 0x170019EA RID: 6634
		// (get) Token: 0x06009506 RID: 38150 RVA: 0x0026ED97 File Offset: 0x0026CF97
		public FactionDef OwnerDef
		{
			get
			{
				return this.Sim.GetFactionDef(this.Def.OwnerValue.Name);
			}
		}

		// Token: 0x170019EB RID: 6635
		// (get) Token: 0x06009507 RID: 38151 RVA: 0x0026EDB4 File Offset: 0x0026CFB4
		public int OwnerReputation
		{
			get
			{
				return this.Sim.GetRawReputation(this.OwnerValue);
			}
		}

		// Token: 0x170019EC RID: 6636
		// (get) Token: 0x06009508 RID: 38152 RVA: 0x0026EDC7 File Offset: 0x0026CFC7
		// (set) Token: 0x06009509 RID: 38153 RVA: 0x0026EDCF File Offset: 0x0026CFCF
		[SerializableMember(SerializationTarget.SaveGame)]
		public TagSet Tags { get; private set; }

		// Token: 0x170019ED RID: 6637
		// (get) Token: 0x0600950A RID: 38154 RVA: 0x0026EDD8 File Offset: 0x0026CFD8
		// (set) Token: 0x0600950B RID: 38155 RVA: 0x0026EDE0 File Offset: 0x0026CFE0
		[SerializableMember(SerializationTarget.SaveGame)]
		public int LastRefreshDay { get; private set; }

		// Token: 0x170019EE RID: 6638
		// (get) Token: 0x0600950C RID: 38156 RVA: 0x0026EDE9 File Offset: 0x0026CFE9
		// (set) Token: 0x0600950D RID: 38157 RVA: 0x0026EDF1 File Offset: 0x0026CFF1
		[SerializableMember(SerializationTarget.SaveGame)]
		public float CurMaxContracts { get; private set; }

		// Token: 0x170019EF RID: 6639
		// (get) Token: 0x0600950E RID: 38158 RVA: 0x0026EDFA File Offset: 0x0026CFFA
		// (set) Token: 0x0600950F RID: 38159 RVA: 0x0026EE02 File Offset: 0x0026D002
		[SerializableMember(SerializationTarget.SaveGame)]
		public int MissionsCompleted { get; private set; }

		// Token: 0x170019F0 RID: 6640
		// (get) Token: 0x06009510 RID: 38160 RVA: 0x0026EE0B File Offset: 0x0026D00B
		// (set) Token: 0x06009511 RID: 38161 RVA: 0x0026EE13 File Offset: 0x0026D013
		[SerializableMember(SerializationTarget.SaveGame)]
		public int CurMaxBreadcrumbs { get; private set; }

		// Token: 0x170019F1 RID: 6641
		// (get) Token: 0x06009512 RID: 38162 RVA: 0x0026EE1C File Offset: 0x0026D01C
		// (set) Token: 0x06009513 RID: 38163 RVA: 0x0026EE24 File Offset: 0x0026D024
		[SerializableMember(SerializationTarget.SaveGame)]
		public int CurBreadcrumbOverride { get; private set; }

		// Token: 0x170019F2 RID: 6642
		// (get) Token: 0x06009514 RID: 38164 RVA: 0x0026EE2D File Offset: 0x0026D02D
		// (set) Token: 0x06009515 RID: 38165 RVA: 0x0026EE35 File Offset: 0x0026D035
		[SerializableMember(SerializationTarget.SaveGame)]
		public string GUID { get; private set; }

		// Token: 0x170019F3 RID: 6643
		// (get) Token: 0x06009516 RID: 38166 RVA: 0x0026EE3E File Offset: 0x0026D03E
		public bool InitialContractsFetched
		{
			get
			{
				return this.initialContractsFetched;
			}
		}

		// Token: 0x170019F4 RID: 6644
		// (get) Token: 0x06009517 RID: 38167 RVA: 0x0026EE46 File Offset: 0x0026D046
		public List<Contract> SystemContracts
		{
			get
			{
				return this.activeSystemContracts;
			}
		}

		// Token: 0x170019F5 RID: 6645
		// (get) Token: 0x06009518 RID: 38168 RVA: 0x0026EE4E File Offset: 0x0026D04E
		public List<Contract> SystemBreadcrumbs
		{
			get
			{
				return this.activeSystemBreadcrumbs;
			}
		}

		// Token: 0x06009519 RID: 38169 RVA: 0x0026EE56 File Offset: 0x0026D056
		public ulong GetSystemTooltipID()
		{
			return this.TooltipID | 1UL;
		}

		// Token: 0x0600951A RID: 38170 RVA: 0x0026EE61 File Offset: 0x0026D061
		public void SetTooltipID()
		{
			this.TooltipID = StarSystem.NextTooltipID;
			StarSystem.NextTooltipID += 1UL;
		}

		// Token: 0x170019F6 RID: 6646
		// (get) Token: 0x0600951B RID: 38171 RVA: 0x0026EE7B File Offset: 0x0026D07B
		public bool Habitable
		{
			get
			{
				return this.Def.Tags.Any((string x) => this.Sim.HabitableTags.Any((string y) => x.CompareTo(y) == 0));
			}
		}

		// Token: 0x0600951C RID: 38172 RVA: 0x0026EE9C File Offset: 0x0026D09C
		protected StarSystem()
		{
			this.Discount = new StarSystem.Discounts(this, false);
		}

		// Token: 0x0600951D RID: 38173 RVA: 0x0026EF20 File Offset: 0x0026D120
		public StarSystem(StarSystemDef def, SimGameState state = null)
		{
			this.Sim = state;
			this.Name = def.Description.Name;
			this.ID = def.CoreSystemID;
			this.SetTooltipID();
			this.SetNewStarSystemDef(def, false);
			this.SystemShop = new Shop(this.Sim, this, this.Def.SystemShopItems, Shop.RefreshType.None, Shop.ShopType.System);
			this.FactionShop = new Shop(this.Sim, this, this.Def.FactionShopItems, Shop.RefreshType.None, Shop.ShopType.Faction);
			this.BlackMarketShop = new Shop(this.Sim, this, this.Def.BlackMarketShopItems, Shop.RefreshType.None, Shop.ShopType.BlackMarket);
		}

		// Token: 0x0600951E RID: 38174 RVA: 0x0026F024 File Offset: 0x0026D224
		public void SetNewStarSystemDef(StarSystemDef def, bool resetShops = false)
		{
			this.Stats = new StatCollection();
			this.Discount = new StarSystem.Discounts(this, true);
			this.Def = def;
			this.SystemID = def.Description.Id;
			this.PermanentRonin = new List<PilotDef>();
			this.AvailablePilots = new List<PilotDef>();
			this.AvailableMedTechs = new List<TechDef>();
			this.AvailableMechTechs = new List<TechDef>();
			this.Tags = new TagSet(def.Tags);
			foreach (string text in def.ContractEmployerIDList)
			{
				string text2 = string.Format("{0}.{1}", "Employer", text);
				this.Stats.AddStatistic<float>(text2, 1f);
			}
			foreach (string text3 in def.ContractTargetIDList)
			{
				string text4 = string.Format("{0}.{1}", "Target", text3);
				this.Stats.AddStatistic<float>(text4, 1f);
			}
			foreach (StarSystemDef.SystemInfluenceDef systemInfluenceDef in def.SystemInfluence)
			{
				string text5 = string.Format("{0}.{1}", "Influence", systemInfluenceDef.FactionValue.Name);
				this.Stats.AddStatistic<float>(text5, systemInfluenceDef.Influence);
			}
			foreach (FactionValue factionValue in FactionEnumeration.FactionList)
			{
				if (!factionValue.IsInvalidUnset && !factionValue.IsPlayer1sMercUnit)
				{
					string text6 = string.Format("{0}.{1}", "Owner", factionValue);
					this.Stats.AddStatistic<int>(text6, this.OwnerValue.Equals(factionValue) ? 1 : 0);
				}
			}
			this.Stats.AddStatistic<int>("Employer.IsOwner", 0);
			this.Stats.AddStatistic<int>("Target.IsOwner", 0);
			this.CurMaxContracts = (float)this.GetSystemMaxContracts();
			if (resetShops)
			{
				this.SystemShop = new Shop(this.Sim, this, this.Def.SystemShopItems, Shop.RefreshType.ForceRefresh, Shop.ShopType.System);
				this.FactionShop = new Shop(this.Sim, this, this.Def.FactionShopItems, Shop.RefreshType.ForceRefresh, Shop.ShopType.Faction);
				this.BlackMarketShop = new Shop(this.Sim, this, this.Def.BlackMarketShopItems, Shop.RefreshType.ForceRefresh, Shop.ShopType.BlackMarket);
			}
		}

		// Token: 0x0600951F RID: 38175 RVA: 0x0026F2E4 File Offset: 0x0026D4E4
		private int GetSystemMaxContracts()
		{
			if (!this.Def.UseMaxContractOverride)
			{
				return this.Sim.Constants.Story.MaxContractsPerSystem;
			}
			return this.Def.MaxContractOverride;
		}

		// Token: 0x06009520 RID: 38176 RVA: 0x0026F314 File Offset: 0x0026D514
		public void Dehydrate(SerializableReferenceContainer globalReferences, bool saveShops)
		{
			globalReferences.AddItemList<Contract>(this, "ActiveContracts", this.activeSystemContracts);
			globalReferences.AddItemList<Contract>(this, "ActiveBreadcrumbs", this.activeSystemBreadcrumbs);
			if (saveShops)
			{
				globalReferences.AddItem<Shop>("ActiveSystemShop", this.SystemShop);
				globalReferences.AddItem<Shop>("ActiveFactionShop", this.FactionShop);
				globalReferences.AddItem<Shop>("ActiveBlackMarketShop", this.BlackMarketShop);
			}
			if (this.AvailablePilots == null)
			{
				this.AvailablePilots = new List<PilotDef>();
			}
			if (this.PermanentRonin == null)
			{
				this.PermanentRonin = new List<PilotDef>();
			}
			globalReferences.AddItemList<PilotDef>(this, "AvailablePilots", this.AvailablePilots);
			globalReferences.AddItemList<PilotDef>(this, "PermanentRonin", this.PermanentRonin);
		}

		// Token: 0x06009521 RID: 38177 RVA: 0x0026F3C8 File Offset: 0x0026D5C8
		public void Rehydrate(SimGameState sim, SerializableReferenceContainer globalReferences, bool loadShops)
		{
			this.SetSimGameState(sim);
			this.Def = sim.DataManager.SystemDefs.Get(this.SystemID);
			Shop.RefreshType refreshType = Shop.RefreshType.None;
			if (loadShops)
			{
				if (globalReferences.HasItem("ActiveSystemShop"))
				{
					this.SystemShop = globalReferences.GetItem<Shop>("ActiveSystemShop");
					this.FactionShop = globalReferences.GetItem<Shop>("ActiveFactionShop");
					this.BlackMarketShop = globalReferences.GetItem<Shop>("ActiveBlackMarketShop");
				}
				refreshType = Shop.RefreshType.RefreshIfEmpty;
			}
			if (this.SystemShop == null)
			{
				this.SystemShop = new Shop();
			}
			this.SystemShop.Rehydrate(sim, this, this.Def.SystemShopItems, refreshType, Shop.ShopType.System);
			if (this.FactionShop == null)
			{
				this.FactionShop = new Shop();
			}
			this.FactionShop.Rehydrate(sim, this, this.Def.FactionShopItems, refreshType, Shop.ShopType.Faction);
			if (this.BlackMarketShop == null)
			{
				this.BlackMarketShop = new Shop();
			}
			this.BlackMarketShop.Rehydrate(sim, this, this.Def.BlackMarketShopItems, refreshType, Shop.ShopType.BlackMarket);
			this.activeSystemContracts = globalReferences.GetItemList<Contract>(this, "ActiveContracts");
			this.activeSystemBreadcrumbs = globalReferences.GetItemList<Contract>(this, "ActiveBreadcrumbs");
			if (globalReferences.HasItemList(this, "AvailablePilots"))
			{
				this.AvailablePilots = globalReferences.GetItemList<PilotDef>(this, "AvailablePilots");
			}
			if (globalReferences.HasItemList(this, "PermanentRonin"))
			{
				this.PermanentRonin = globalReferences.GetItemList<PilotDef>(this, "PermanentRonin");
			}
			for (int i = this.AvailablePilots.Count - 1; i >= 0; i--)
			{
				if (this.AvailablePilots[i] == null)
				{
					this.AvailablePilots.RemoveAt(i);
					Debug.LogError(string.Format("Found an NULL Pilot in Starsystem {0} at index {1}", this.Name, i));
				}
				else
				{
					if (this.PilotIconMap.ContainsKey(this.AvailablePilots[i].Description.Name))
					{
						this.AvailablePilots[i].Description.SetIcon(this.PilotIconMap[this.AvailablePilots[i].Description.Name]);
					}
					this.AvailablePilots[i].InitFromSave();
				}
			}
			for (int j = this.PermanentRonin.Count - 1; j >= 0; j--)
			{
				if (this.PermanentRonin[j] == null)
				{
					this.PermanentRonin.RemoveAt(j);
					Debug.LogError(string.Format("Found an NULL Pilot in Starsystem {0} at index {1}", this.Name, j));
				}
				else
				{
					if (this.PilotIconMap.ContainsKey(this.PermanentRonin[j].Description.Name))
					{
						this.PermanentRonin[j].Description.SetIcon(this.PilotIconMap[this.PermanentRonin[j].Description.Name]);
					}
					this.PermanentRonin[j].InitFromSave();
					if (!this.Sim.UsedRoninIDs.Contains(this.PermanentRonin[j].Description.Id) && !this.AvailablePilots.Contains(this.PermanentRonin[j]))
					{
						this.AvailablePilots.Add(this.PermanentRonin[j]);
					}
				}
			}
			this.SetTooltipID();
		}

		// Token: 0x06009522 RID: 38178 RVA: 0x0026F70C File Offset: 0x0026D90C
		public void SetContractTargets(Dictionary<string, StarSystem> simStarmap)
		{
			foreach (Contract contract in this.activeSystemContracts)
			{
				if (contract != null)
				{
					contract.SetTargetSystemFromLoad(simStarmap);
				}
			}
			foreach (Contract contract2 in this.activeSystemBreadcrumbs)
			{
				if (contract2 != null)
				{
					contract2.SetTargetSystemFromLoad(simStarmap);
				}
			}
		}

		// Token: 0x06009523 RID: 38179 RVA: 0x0026F7A8 File Offset: 0x0026D9A8
		public void SetSimGameState(SimGameState state)
		{
			this.Sim = state;
		}

		// Token: 0x06009524 RID: 38180 RVA: 0x0026F7B4 File Offset: 0x0026D9B4
		public void AddAvailablePilot(PilotDef def, bool isRonin)
		{
			this.AvailablePilots.Add(def);
			if (isRonin)
			{
				this.PermanentRonin.Add(def);
			}
			this.LastPilotAdded = def;
			Debug.Log(string.Format("Last Pilot Added is now set to {0}", this.LastPilotAdded.Description.Name));
		}

		// Token: 0x06009525 RID: 38181 RVA: 0x0026F802 File Offset: 0x0026DA02
		public PilotDef GetLastPilotDefAddedToHiring()
		{
			return this.LastPilotAdded;
		}

		// Token: 0x06009526 RID: 38182 RVA: 0x0026F80C File Offset: 0x0026DA0C
		public Pilot GetLastPilotAddedToHiring()
		{
			Pilot pilot;
			if (this.LastPilotAdded == null)
			{
				Debug.LogError("Attempted to get LastPilotAdded but it is Null");
				pilot = null;
			}
			else
			{
				pilot = new Pilot(this.LastPilotAdded, "", true);
			}
			return pilot;
		}

		// Token: 0x06009527 RID: 38183 RVA: 0x0026F842 File Offset: 0x0026DA42
		public string GetLastPilotAddedToHiringName()
		{
			if (this.LastPilotAdded == null && this.LastPilotAdded.Description != null)
			{
				return "ERROR: No Last Pilot Found";
			}
			return this.LastPilotAdded.Description.Callsign;
		}

		// Token: 0x06009528 RID: 38184 RVA: 0x0026F870 File Offset: 0x0026DA70
		public void GeneratePilots(int count)
		{
			this.AvailablePilots.Clear();
			List<PilotDef> list = new List<PilotDef>();
			float num = (this.Def.UseSystemRoninHiringChance ? this.Def.RoninHiringChance : this.Sim.Constants.Story.DefaultRoninHiringChance);
			List<PilotDef> list2 = this.Sim.PilotGenerator.GeneratePilots(count, this.Def.GetDifficulty(this.Sim.SimGameMode), num, out list);
			for (int i = list.Count - 1; i >= 0; i--)
			{
				if (this.Sim.UsedRoninIDs.Contains(list[i].Description.Id))
				{
					list.RemoveAt(i);
				}
			}
			for (int j = this.PermanentRonin.Count - 1; j >= 0; j--)
			{
				if (this.Sim.UsedRoninIDs.Contains(this.PermanentRonin[j].Description.Id))
				{
					this.PermanentRonin.RemoveAt(j);
				}
			}
			this.AvailablePilots.AddRange(list2);
			if (list.Count > 0)
			{
				this.AvailablePilots.AddRange(list);
			}
			if (this.PermanentRonin.Count > 0)
			{
				this.AvailablePilots.AddRange(this.PermanentRonin);
			}
		}

		// Token: 0x06009529 RID: 38185 RVA: 0x0026F9B8 File Offset: 0x0026DBB8
		public int GetPurchaseCostAfterReputationModifier(int baseCost)
		{
			float reputationShopAdjustment = this.Sim.GetReputationShopAdjustment(this.OwnerValue);
			float num = (float)baseCost;
			float num2 = reputationShopAdjustment * num;
			return Mathf.CeilToInt(num + num2);
		}

		// Token: 0x0600952A RID: 38186 RVA: 0x0026F9E4 File Offset: 0x0026DBE4
		public void HirePilot(PilotDef def)
		{
			if (!this.AvailablePilots.Contains(def))
			{
				return;
			}
			this.AvailablePilots.Remove(def);
			if (this.PermanentRonin.Contains(def))
			{
				this.PermanentRonin.Remove(def);
				this.Sim.UsedRoninIDs.Add(def.Description.Id);
			}
			def.SetDayOfHire(this.Sim.DaysPassed);
			this.Sim.AddPilotToRoster(def, true, false);
			int purchaseCostAfterReputationModifier = this.GetPurchaseCostAfterReputationModifier(this.Sim.GetMechWarriorHiringCost(def));
			this.Sim.AddFunds(-purchaseCostAfterReputationModifier, null, true, true);
		}

		// Token: 0x0600952B RID: 38187 RVA: 0x0026FA88 File Offset: 0x0026DC88
		public void GenerateTechs(int count, bool mech = true)
		{
			List<TechDef> list;
			if (mech)
			{
				list = this.AvailableMechTechs;
			}
			else
			{
				list = this.AvailableMedTechs;
			}
			list.Clear();
			list.AddRange(this.Sim.PilotGenerator.GenerateTech(count, this.Def.GetDifficulty(this.Sim.SimGameMode)));
		}

		// Token: 0x0600952C RID: 38188 RVA: 0x0026FADB File Offset: 0x0026DCDB
		public void ResetContracts()
		{
			this.initialContractsFetched = false;
			this.activeSystemContracts.Clear();
			this.activeSystemBreadcrumbs.Clear();
		}

		// Token: 0x0600952D RID: 38189 RVA: 0x0026FAFA File Offset: 0x0026DCFA
		public void DeleteContracts()
		{
			this.activeSystemContracts.Clear();
			this.activeSystemBreadcrumbs.Clear();
		}

		// Token: 0x0600952E RID: 38190 RVA: 0x0026FB12 File Offset: 0x0026DD12
		public void OnSystemExit()
		{
			this.ResetContracts();
		}

		// Token: 0x0600952F RID: 38191 RVA: 0x0026FB1A File Offset: 0x0026DD1A
		public void OnSystemChange()
		{
			this.RefreshShops();
		}

		// Token: 0x06009530 RID: 38192 RVA: 0x0026FB24 File Offset: 0x0026DD24
		public void RefreshSystem()
		{
			this.GeneratePilots(this.Sim.Constants.Story.DefaultPilotsPerSystem);
			this.GenerateTechs(this.Sim.Constants.Story.DefaultMechTechsPerSystem, true);
			this.GenerateTechs(this.Sim.Constants.Story.DefaultMedTechsPerSystem, false);
			this.RefreshShops();
			this.RefreshBreadcrumbs();
		}

		// Token: 0x06009531 RID: 38193 RVA: 0x0026FB90 File Offset: 0x0026DD90
		public void RefreshShops()
		{
			this.RefreshShop(this.SystemShop);
			this.RefreshShop(this.FactionShop);
			this.RefreshShop(this.BlackMarketShop);
		}

		// Token: 0x06009532 RID: 38194 RVA: 0x0026FBB6 File Offset: 0x0026DDB6
		public void RefreshShop(Shop _shop)
		{
			if (!_shop.IsPending)
			{
				_shop.RefreshShop();
			}
		}

		// Token: 0x06009533 RID: 38195 RVA: 0x0026FBC8 File Offset: 0x0026DDC8
		public bool CanUseSystemStore()
		{
			FactionValue ownerValue = this.Def.OwnerValue;
			return this.Sim.GetReputation(ownerValue) > SimGameReputation.LOATHED;
		}

		// Token: 0x06009534 RID: 38196 RVA: 0x0026FBF4 File Offset: 0x0026DDF4
		public bool HasFactionStore()
		{
			return !this.FactionShop.IsEmpty;
		}

		// Token: 0x06009535 RID: 38197 RVA: 0x0026FC04 File Offset: 0x0026DE04
		public bool CanUseFactionStore()
		{
			if (this.HasFactionStore())
			{
				FactionValue factionShopOwnerValue = this.Def.FactionShopOwnerValue;
				return this.Sim.IsFactionAlly(factionShopOwnerValue, null);
			}
			return false;
		}

		// Token: 0x06009536 RID: 38198 RVA: 0x0026FC39 File Offset: 0x0026DE39
		public bool HasBlackMarketStore()
		{
			return !this.BlackMarketShop.IsEmpty;
		}

		// Token: 0x06009537 RID: 38199 RVA: 0x0026FC49 File Offset: 0x0026DE49
		public bool CanUseBlackMarketStore()
		{
			return this.HasBlackMarketStore() && this.Sim.CompanyTags.Contains(this.Sim.Constants.Story.BlackMarketTag);
		}

		// Token: 0x06009538 RID: 38200 RVA: 0x0026FC80 File Offset: 0x0026DE80
		public void UpdateSystemDay()
		{
			if (this.Sim.DaysPassed - this.LastRefreshDay > this.Sim.Constants.Story.DefaultContractRefreshRate)
			{
				this.LastRefreshDay = this.Sim.DaysPassed;
				if (this.CurMaxContracts < (float)this.GetSystemMaxContracts() && !this.Def.Depletable)
				{
					this.CurMaxContracts = Mathf.Min((float)this.GetSystemMaxContracts(), this.CurMaxContracts + this.Sim.Constants.Story.ContractRenewalPerWeek);
				}
			}
		}

		// Token: 0x06009539 RID: 38201 RVA: 0x0026FD14 File Offset: 0x0026DF14
		public void CompletedContract()
		{
			int missionsCompleted = this.MissionsCompleted;
			this.MissionsCompleted = missionsCompleted + 1;
			this.CurMaxContracts = Mathf.Max(0f, this.CurMaxContracts - this.Sim.Constants.Story.ContractSuccessReduction);
			this.RefreshBreadcrumbs();
		}

		// Token: 0x0600953A RID: 38202 RVA: 0x0026FD63 File Offset: 0x0026DF63
		public void SetCurBreadcrumbOverride(int val)
		{
			this.CurBreadcrumbOverride = val;
			this.CurMaxBreadcrumbs = val;
		}

		// Token: 0x0600953B RID: 38203 RVA: 0x0026FD74 File Offset: 0x0026DF74
		public void RefreshBreadcrumbs()
		{
			if (this.CurBreadcrumbOverride > 0)
			{
				this.CurMaxBreadcrumbs = this.CurBreadcrumbOverride;
				return;
			}
			this.CurMaxBreadcrumbs = 0;
			int num = this.MissionsCompleted;
			if (num < this.Sim.Constants.Story.MissionsForFirstBreadcrumb)
			{
				return;
			}
			int curMaxBreadcrumbs = this.CurMaxBreadcrumbs;
			this.CurMaxBreadcrumbs = curMaxBreadcrumbs + 1;
			num -= this.Sim.Constants.Story.MissionsForFirstBreadcrumb;
			this.CurMaxBreadcrumbs += num / this.Sim.Constants.Story.MissionsForAdditionalBreadcrumb;
			this.CurMaxBreadcrumbs = Mathf.Min(this.CurMaxBreadcrumbs, this.Sim.Constants.Story.MaxBreadcrumbsPerSystem);
		}

		// Token: 0x0600953C RID: 38204 RVA: 0x0026FE30 File Offset: 0x0026E030
		public void SetGuid(string newGuid)
		{
			this.GUID = newGuid;
		}

		// Token: 0x0600953D RID: 38205 RVA: 0x0026FE3C File Offset: 0x0026E03C
		public void SetCurrentContractFactions(FactionValue employer = null, FactionValue target = null)
		{
			if (employer == null)
			{
				employer = FactionEnumeration.GetInvalidUnsetFactionValue();
			}
			if (target == null)
			{
				target = FactionEnumeration.GetInvalidUnsetFactionValue();
			}
			this.Stats.ModifyStat<int>("Temp", 0, "Employer.IsOwner", StatCollection.StatOperation.Set, employer.Equals(this.OwnerValue) ? 1 : 0, -1, true);
			this.Stats.ModifyStat<int>("Temp", 0, "Target.IsOwner", StatCollection.StatOperation.Set, target.Equals(this.OwnerValue) ? 1 : 0, -1, true);
		}

		// Token: 0x0600953E RID: 38206 RVA: 0x0026FEB5 File Offset: 0x0026E0B5
		public void GenerateInitialContracts(Action onContractsFetched = null)
		{
			this.contractRetrievalCallback = onContractsFetched;
			this.Sim.GeneratePotentialContracts(true, new Action(this.OnInitialContractFetched), null, true);
		}

		// Token: 0x0600953F RID: 38207 RVA: 0x0026FED8 File Offset: 0x0026E0D8
		private void OnInitialContractFetched()
		{
			this.initialContractsFetched = true;
			if (this.contractRetrievalCallback != null)
			{
				this.contractRetrievalCallback();
				this.contractRetrievalCallback = null;
			}
		}

		// Token: 0x04005EC3 RID: 24259
		public const string STARSYSTEM_SHOPDISCOUNT = "Discount.Shop";

		// Token: 0x04005EC4 RID: 24260
		public const string STARSYSTEM_HIRINGDISCOUNT = "Discount.Hiring";

		// Token: 0x04005EC5 RID: 24261
		public const string STARSYSTEM_EMPLOYER = "Employer";

		// Token: 0x04005EC6 RID: 24262
		public const string STARSYSTEM_TARGET = "Target";

		// Token: 0x04005EC7 RID: 24263
		public const string STARSYSTEM_INFLUENCE = "Influence";

		// Token: 0x04005EC8 RID: 24264
		public const string STARSYSTEM_OWNER = "Owner";

		// Token: 0x04005EC9 RID: 24265
		public const string STARSYSTEM_EMPLOYER_IS_OWNER = "Employer.IsOwner";

		// Token: 0x04005ECA RID: 24266
		public const string STARSYSTEM_TARGET_IS_OWNER = "Target.IsOwner";

		// Token: 0x04005ECB RID: 24267
		[SerializableMember(SerializationTarget.SaveGame)]
		private string SystemID;

		// Token: 0x04005ED9 RID: 24281
		public StarSystem.Discounts Discount;

		// Token: 0x04005EE1 RID: 24289
		[SerializableMember(SerializationTarget.SaveGame)]
		private List<Contract> activeSystemContracts = new List<Contract>();

		// Token: 0x04005EE2 RID: 24290
		[SerializableMember(SerializationTarget.SaveGame)]
		private List<Contract> activeSystemBreadcrumbs = new List<Contract>();

		// Token: 0x04005EE3 RID: 24291
		[SerializableMember(SerializationTarget.SaveGame)]
		protected bool initialContractsFetched;

		// Token: 0x04005EE4 RID: 24292
		private Action contractRetrievalCallback;

		// Token: 0x04005EE5 RID: 24293
		private static ulong NextTooltipID;

		// Token: 0x04005EE6 RID: 24294
		public const ulong SystemObjectID = 1UL;

		// Token: 0x04005EE7 RID: 24295
		private ulong TooltipID;

		// Token: 0x04005EE8 RID: 24296
		private Dictionary<string, string> PilotIconMap = new Dictionary<string, string>
		{
			{ "Dekker", "guiTxrPort_Dekker_holder_utr" },
			{ "Medusa", "guiTxrPort_Medusa_holder_utr" },
			{ "Glitch", "guiTxrPort_Glitch_holder_utr" },
			{ "Behemoth", "guiTxrPort_Behemoth_holder_utr" }
		};

		// Token: 0x04005EE9 RID: 24297
		private const string NO_PILOT_FOUND_ERROR = "ERROR: No Last Pilot Found";

		// Token: 0x02001116 RID: 4374
		public class Discounts
		{
			// Token: 0x170019F7 RID: 6647
			// (get) Token: 0x06009542 RID: 38210 RVA: 0x0026FF32 File Offset: 0x0026E132
			public float Shop
			{
				get
				{
					return this.parent.Stats.GetValue<float>("Discount.Shop");
				}
			}

			// Token: 0x170019F8 RID: 6648
			// (get) Token: 0x06009543 RID: 38211 RVA: 0x0026FF49 File Offset: 0x0026E149
			public float Discount
			{
				get
				{
					return this.parent.Stats.GetValue<float>("Discount.Hiring");
				}
			}

			// Token: 0x06009544 RID: 38212 RVA: 0x0026FF60 File Offset: 0x0026E160
			public Discounts(StarSystem parent, bool addStats)
			{
				this.parent = parent;
				if (addStats)
				{
					parent.Stats.AddStatistic<float>("Discount.Shop", 0f);
					parent.Stats.AddStatistic<float>("Discount.Hiring", 0f);
				}
			}

			// Token: 0x04005EEA RID: 24298
			private StarSystem parent;
		}
	}
}
