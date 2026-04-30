using System;
using System.Collections.Generic;
using BattleTech.Serialization;
using fastJSON;
using HBS.Collections;
using HBS.Util;
using UnityEngine;

namespace BattleTech
{
	// Token: 0x02001065 RID: 4197
	[SerializableContract("StarSystemDef")]
	public class StarSystemDef : IJsonTemplated
	{
		// Token: 0x17001894 RID: 6292
		// (get) Token: 0x06008CF4 RID: 36084 RVA: 0x0023E1F5 File Offset: 0x0023C3F5
		// (set) Token: 0x06008CF5 RID: 36085 RVA: 0x0023E1FD File Offset: 0x0023C3FD
		[SerializableMember(SerializationTarget.SaveGame)]
		public BaseDescriptionDef Description { get; private set; }

		// Token: 0x17001895 RID: 6293
		// (get) Token: 0x06008CF6 RID: 36086 RVA: 0x0023E206 File Offset: 0x0023C406
		// (set) Token: 0x06008CF7 RID: 36087 RVA: 0x0023E20E File Offset: 0x0023C40E
		public string CoreSystemID { get; private set; }

		// Token: 0x17001896 RID: 6294
		// (get) Token: 0x06008CF8 RID: 36088 RVA: 0x0023E217 File Offset: 0x0023C417
		// (set) Token: 0x06008CF9 RID: 36089 RVA: 0x0023E21F File Offset: 0x0023C41F
		[JsonSerialized]
		[SerializableMember(SerializationTarget.SaveGame)]
		public FakeVector3 Position { get; private set; }

		// Token: 0x17001897 RID: 6295
		// (get) Token: 0x06008CFA RID: 36090 RVA: 0x0023E228 File Offset: 0x0023C428
		// (set) Token: 0x06008CFB RID: 36091 RVA: 0x0023E230 File Offset: 0x0023C430
		[SerializableMember(SerializationTarget.SaveGame)]
		public TagSet Tags { get; private set; }

		// Token: 0x17001898 RID: 6296
		// (get) Token: 0x06008CFC RID: 36092 RVA: 0x0023E239 File Offset: 0x0023C439
		// (set) Token: 0x06008CFD RID: 36093 RVA: 0x0023E241 File Offset: 0x0023C441
		[SerializableMember(SerializationTarget.SaveGame)]
		public List<Biome.BIOMESKIN> SupportedBiomes { get; private set; }

		// Token: 0x17001899 RID: 6297
		// (get) Token: 0x06008CFE RID: 36094 RVA: 0x0023E24A File Offset: 0x0023C44A
		// (set) Token: 0x06008CFF RID: 36095 RVA: 0x0023E252 File Offset: 0x0023C452
		[SerializableMember(SerializationTarget.SaveGame)]
		public TagSet MapRequiredTags { get; private set; }

		// Token: 0x1700189A RID: 6298
		// (get) Token: 0x06008D00 RID: 36096 RVA: 0x0023E25B File Offset: 0x0023C45B
		// (set) Token: 0x06008D01 RID: 36097 RVA: 0x0023E263 File Offset: 0x0023C463
		[SerializableMember(SerializationTarget.SaveGame)]
		public TagSet MapExcludedTags { get; private set; }

		// Token: 0x1700189B RID: 6299
		// (get) Token: 0x06008D02 RID: 36098 RVA: 0x0023E26C File Offset: 0x0023C46C
		// (set) Token: 0x06008D03 RID: 36099 RVA: 0x0023E274 File Offset: 0x0023C474
		[SerializableMember(SerializationTarget.SaveGame)]
		public bool FuelingStation { get; private set; }

		// Token: 0x1700189C RID: 6300
		// (get) Token: 0x06008D04 RID: 36100 RVA: 0x0023E27D File Offset: 0x0023C47D
		// (set) Token: 0x06008D05 RID: 36101 RVA: 0x0023E285 File Offset: 0x0023C485
		[SerializableMember(SerializationTarget.SaveGame)]
		public int JumpDistance { get; private set; }

		// Token: 0x1700189D RID: 6301
		// (get) Token: 0x06008D06 RID: 36102 RVA: 0x0023E28E File Offset: 0x0023C48E
		// (set) Token: 0x06008D07 RID: 36103 RVA: 0x0023E296 File Offset: 0x0023C496
		[SerializableMember(SerializationTarget.SaveGame)]
		private protected Faction Owner { protected get; private set; }

		// Token: 0x1700189E RID: 6302
		// (get) Token: 0x06008D08 RID: 36104 RVA: 0x0023E2A0 File Offset: 0x0023C4A0
		// (set) Token: 0x06008D09 RID: 36105 RVA: 0x0023E2F7 File Offset: 0x0023C4F7
		public FactionValue OwnerValue
		{
			get
			{
				if (string.IsNullOrEmpty(this.ownerID))
				{
					this.UpgradeToDataDrivenEnums();
				}
				if (this.ownerValue == null || this.ownerValue.Name != this.ownerID)
				{
					this.ownerValue = FactionEnumeration.GetFactionByName(this.ownerID);
				}
				return this.ownerValue;
			}
			private set
			{
				this.ownerValue = value;
				this.ownerID = this.ownerValue.Name;
			}
		}

		// Token: 0x06008D0A RID: 36106 RVA: 0x0023E314 File Offset: 0x0023C514
		private void UpgradeToDataDrivenEnums()
		{
			if (string.IsNullOrEmpty(this.ownerID) || this.ownerID == FactionEnumeration.INVALID_DEFAULT_ID)
			{
				this.ownerID = this.Owner.ToString();
			}
			if (this.contractEmployerIDs == null)
			{
				this.contractEmployerIDs = this.ConvertFactionList(this.ContractEmployers);
			}
			if (this.contractTargetIDs == null)
			{
				this.contractTargetIDs = this.ConvertFactionList(this.ContractTargets);
			}
			if (string.IsNullOrEmpty(this.factionShopOwnerID))
			{
				this.factionShopOwnerID = this.FactionShopOwner.ToString();
			}
		}

		// Token: 0x06008D0B RID: 36107 RVA: 0x0023E3B8 File Offset: 0x0023C5B8
		private List<string> ConvertFactionList(List<Faction> factionList)
		{
			List<string> list = new List<string>();
			for (int i = 0; i < factionList.Count; i++)
			{
				list.Add(factionList[i].ToString());
			}
			return list;
		}

		// Token: 0x1700189F RID: 6303
		// (get) Token: 0x06008D0C RID: 36108 RVA: 0x0023E3F8 File Offset: 0x0023C5F8
		// (set) Token: 0x06008D0D RID: 36109 RVA: 0x0023E400 File Offset: 0x0023C600
		[SerializableMember(SerializationTarget.SaveGame)]
		private List<Faction> ContractEmployers { get; set; }

		// Token: 0x170018A0 RID: 6304
		// (get) Token: 0x06008D0E RID: 36110 RVA: 0x0023E409 File Offset: 0x0023C609
		public List<string> ContractEmployerIDList
		{
			get
			{
				if (this.contractEmployerIDs == null)
				{
					this.UpgradeToDataDrivenEnums();
				}
				return this.contractEmployerIDs;
			}
		}

		// Token: 0x170018A1 RID: 6305
		// (get) Token: 0x06008D0F RID: 36111 RVA: 0x0023E41F File Offset: 0x0023C61F
		// (set) Token: 0x06008D10 RID: 36112 RVA: 0x0023E427 File Offset: 0x0023C627
		[SerializableMember(SerializationTarget.SaveGame)]
		public List<Faction> ContractTargets { get; private set; }

		// Token: 0x170018A2 RID: 6306
		// (get) Token: 0x06008D11 RID: 36113 RVA: 0x0023E430 File Offset: 0x0023C630
		public List<string> ContractTargetIDList
		{
			get
			{
				if (this.contractTargetIDs == null)
				{
					this.UpgradeToDataDrivenEnums();
				}
				return this.contractTargetIDs;
			}
		}

		// Token: 0x170018A3 RID: 6307
		// (get) Token: 0x06008D12 RID: 36114 RVA: 0x0023E446 File Offset: 0x0023C646
		// (set) Token: 0x06008D13 RID: 36115 RVA: 0x0023E44E File Offset: 0x0023C64E
		[SerializableMember(SerializationTarget.SaveGame)]
		public int ShopRefreshRate { get; private set; }

		// Token: 0x170018A4 RID: 6308
		// (get) Token: 0x06008D14 RID: 36116 RVA: 0x0023E457 File Offset: 0x0023C657
		// (set) Token: 0x06008D15 RID: 36117 RVA: 0x0023E45F File Offset: 0x0023C65F
		[SerializableMember(SerializationTarget.SaveGame)]
		public int ShopMaxInventory { get; private set; }

		// Token: 0x170018A5 RID: 6309
		// (get) Token: 0x06008D16 RID: 36118 RVA: 0x0023E468 File Offset: 0x0023C668
		// (set) Token: 0x06008D17 RID: 36119 RVA: 0x0023E470 File Offset: 0x0023C670
		[SerializableMember(SerializationTarget.SaveGame)]
		public int ShopMaxSpecials { get; private set; }

		// Token: 0x170018A6 RID: 6310
		// (get) Token: 0x06008D18 RID: 36120 RVA: 0x0023E479 File Offset: 0x0023C679
		// (set) Token: 0x06008D19 RID: 36121 RVA: 0x0023E481 File Offset: 0x0023C681
		[SerializableMember(SerializationTarget.SaveGame)]
		public List<StarSystemDef.SystemInfluenceDef> SystemInfluence { get; private set; }

		// Token: 0x170018A7 RID: 6311
		// (get) Token: 0x06008D1A RID: 36122 RVA: 0x0023E48A File Offset: 0x0023C68A
		// (set) Token: 0x06008D1B RID: 36123 RVA: 0x0023E492 File Offset: 0x0023C692
		[SerializableMember(SerializationTarget.SaveGame)]
		public List<RequirementDef> TravelRequirements { get; private set; }

		// Token: 0x170018A8 RID: 6312
		// (get) Token: 0x06008D1C RID: 36124 RVA: 0x0023E49B File Offset: 0x0023C69B
		// (set) Token: 0x06008D1D RID: 36125 RVA: 0x0023E4A3 File Offset: 0x0023C6A3
		public List<SimGameState.SimGameType> StartingSystemModes { get; private set; }

		// Token: 0x170018A9 RID: 6313
		// (get) Token: 0x06008D1E RID: 36126 RVA: 0x0023E4AC File Offset: 0x0023C6AC
		// (set) Token: 0x06008D1F RID: 36127 RVA: 0x0023E4B4 File Offset: 0x0023C6B4
		public Vector2[] StarPosition { get; private set; }

		// Token: 0x170018AA RID: 6314
		// (get) Token: 0x06008D20 RID: 36128 RVA: 0x0023E4BD File Offset: 0x0023C6BD
		// (set) Token: 0x06008D21 RID: 36129 RVA: 0x0023E4C5 File Offset: 0x0023C6C5
		public SimGameSpaceController.StarType StarType { get; private set; }

		// Token: 0x170018AB RID: 6315
		// (get) Token: 0x06008D22 RID: 36130 RVA: 0x0023E4CE File Offset: 0x0023C6CE
		// (set) Token: 0x06008D23 RID: 36131 RVA: 0x0023E4D6 File Offset: 0x0023C6D6
		public bool Depletable { get; private set; }

		// Token: 0x170018AC RID: 6316
		// (get) Token: 0x06008D24 RID: 36132 RVA: 0x0023E4DF File Offset: 0x0023C6DF
		// (set) Token: 0x06008D25 RID: 36133 RVA: 0x0023E4E7 File Offset: 0x0023C6E7
		public bool UseSystemRoninHiringChance { get; private set; }

		// Token: 0x170018AD RID: 6317
		// (get) Token: 0x06008D26 RID: 36134 RVA: 0x0023E4F0 File Offset: 0x0023C6F0
		// (set) Token: 0x06008D27 RID: 36135 RVA: 0x0023E4F8 File Offset: 0x0023C6F8
		public float RoninHiringChance { get; private set; }

		// Token: 0x170018AE RID: 6318
		// (get) Token: 0x06008D28 RID: 36136 RVA: 0x0023E501 File Offset: 0x0023C701
		// (set) Token: 0x06008D29 RID: 36137 RVA: 0x0023E509 File Offset: 0x0023C709
		public bool UseMaxContractOverride { get; private set; }

		// Token: 0x170018AF RID: 6319
		// (get) Token: 0x06008D2A RID: 36138 RVA: 0x0023E512 File Offset: 0x0023C712
		// (set) Token: 0x06008D2B RID: 36139 RVA: 0x0023E51A File Offset: 0x0023C71A
		public int MaxContractOverride { get; private set; }

		// Token: 0x170018B0 RID: 6320
		// (get) Token: 0x06008D2C RID: 36140 RVA: 0x0023E523 File Offset: 0x0023C723
		// (set) Token: 0x06008D2D RID: 36141 RVA: 0x0023E52B File Offset: 0x0023C72B
		public List<string> SystemShopItems { get; private set; }

		// Token: 0x170018B1 RID: 6321
		// (get) Token: 0x06008D2E RID: 36142 RVA: 0x0023E534 File Offset: 0x0023C734
		// (set) Token: 0x06008D2F RID: 36143 RVA: 0x0023E53C File Offset: 0x0023C73C
		[JsonSerialized]
		private Faction FactionShopOwner { get; set; }

		// Token: 0x170018B2 RID: 6322
		// (get) Token: 0x06008D30 RID: 36144 RVA: 0x0023E548 File Offset: 0x0023C748
		// (set) Token: 0x06008D31 RID: 36145 RVA: 0x0023E59F File Offset: 0x0023C79F
		public FactionValue FactionShopOwnerValue
		{
			get
			{
				if (string.IsNullOrEmpty(this.factionShopOwnerID))
				{
					this.UpgradeToDataDrivenEnums();
				}
				if (this.factionShopOwnerValue == null || this.factionShopOwnerValue.Name != this.factionShopOwnerID)
				{
					this.factionShopOwnerValue = FactionEnumeration.GetFactionByName(this.factionShopOwnerID);
				}
				return this.factionShopOwnerValue;
			}
			private set
			{
				this.factionShopOwnerValue = value;
				this.factionShopOwnerID = this.factionShopOwnerValue.Name;
			}
		}

		// Token: 0x170018B3 RID: 6323
		// (get) Token: 0x06008D32 RID: 36146 RVA: 0x0023E5B9 File Offset: 0x0023C7B9
		// (set) Token: 0x06008D33 RID: 36147 RVA: 0x0023E5C1 File Offset: 0x0023C7C1
		public List<string> FactionShopItems { get; private set; }

		// Token: 0x170018B4 RID: 6324
		// (get) Token: 0x06008D34 RID: 36148 RVA: 0x0023E5CA File Offset: 0x0023C7CA
		// (set) Token: 0x06008D35 RID: 36149 RVA: 0x0023E5D2 File Offset: 0x0023C7D2
		public List<string> BlackMarketShopItems { get; private set; }

		// Token: 0x06008D36 RID: 36150 RVA: 0x0023E5DB File Offset: 0x0023C7DB
		public int GetDifficulty(SimGameState.SimGameType type)
		{
			if (this.DifficultyModes != null && this.DifficultyModes.Contains(type))
			{
				return this.DifficultyList[this.DifficultyModes.IndexOf(type)];
			}
			return this.DefaultDifficulty;
		}

		// Token: 0x06008D37 RID: 36151 RVA: 0x0023E611 File Offset: 0x0023C811
		public StarSystemDef()
		{
		}

		// Token: 0x06008D38 RID: 36152 RVA: 0x0023E624 File Offset: 0x0023C824
		public StarSystemDef(DescriptionDef description, FakeVector3 position, TagSet tags, bool fuelingStation, int jumpDistance, string ownerID, List<string> contractEmployers, List<string> contractTargets, List<StarSystemDef.SystemInfluenceDef> systemInfluence, List<RequirementDef> requirements)
		{
			this.Description = description;
			this.Position = position;
			this.Tags = tags;
			this.FuelingStation = fuelingStation;
			this.JumpDistance = jumpDistance;
			ownerID = ownerID;
			this.contractEmployerIDs = new List<string>(contractEmployers);
			this.contractTargetIDs = new List<string>(contractTargets);
			this.SystemInfluence = new List<StarSystemDef.SystemInfluenceDef>(systemInfluence);
			this.TravelRequirements = new List<RequirementDef>(requirements);
		}

		// Token: 0x06008D39 RID: 36153 RVA: 0x0023E69F File Offset: 0x0023C89F
		public void FromJSON(string json)
		{
			JSONSerializationUtility.FromJSON<StarSystemDef>(this, json);
			this.UpgradeToDataDrivenEnums();
		}

		// Token: 0x06008D3A RID: 36154 RVA: 0x0000F9E0 File Offset: 0x0000DBE0
		public string GenerateJSONTemplate()
		{
			throw new NotImplementedException();
		}

		// Token: 0x06008D3B RID: 36155 RVA: 0x0023E6AF File Offset: 0x0023C8AF
		public string ToJSON()
		{
			this.UpgradeToDataDrivenEnums();
			return JSONSerializationUtility.ToJSON<StarSystemDef>(this);
		}

		// Token: 0x040057B3 RID: 22451
		[SerializableMember(SerializationTarget.SaveGame)]
		protected string ownerID;

		// Token: 0x040057B4 RID: 22452
		private FactionValue ownerValue;

		// Token: 0x040057B6 RID: 22454
		[SerializableMember(SerializationTarget.SaveGame)]
		private List<string> contractEmployerIDs;

		// Token: 0x040057B8 RID: 22456
		[SerializableMember(SerializationTarget.SaveGame)]
		private List<string> contractTargetIDs;

		// Token: 0x040057BD RID: 22461
		protected int DefaultDifficulty;

		// Token: 0x040057BE RID: 22462
		protected List<int> DifficultyList;

		// Token: 0x040057BF RID: 22463
		protected List<SimGameState.SimGameType> DifficultyModes;

		// Token: 0x040057CB RID: 22475
		[JsonSerialized]
		protected string factionShopOwnerID = FactionEnumeration.INVALID_DEFAULT_ID;

		// Token: 0x040057CC RID: 22476
		private FactionValue factionShopOwnerValue;

		// Token: 0x02001066 RID: 4198
		[SerializableContract("StarSystemDef.SerializableSystemInfluenceDef")]
		public class SerializableSystemInfluenceDef
		{
			// Token: 0x170018B5 RID: 6325
			// (get) Token: 0x06008D3C RID: 36156 RVA: 0x0023E6C0 File Offset: 0x0023C8C0
			// (set) Token: 0x06008D3D RID: 36157 RVA: 0x0023E717 File Offset: 0x0023C917
			public FactionValue FactionValue
			{
				get
				{
					if (string.IsNullOrEmpty(this.factionID))
					{
						this.UpgradeToDataDrivenEnums();
					}
					if (this.factionValue == null || this.factionValue.Name != this.factionID)
					{
						this.factionValue = FactionEnumeration.GetFactionByName(this.factionID);
					}
					return this.factionValue;
				}
				private set
				{
					this.factionValue = value;
					this.factionID = this.factionValue.Name;
				}
			}

			// Token: 0x06008D3E RID: 36158 RVA: 0x0023E731 File Offset: 0x0023C931
			public SerializableSystemInfluenceDef(StarSystemDef.SystemInfluenceDef def)
			{
				this.faction = Faction.INVALID_UNSET;
				this.influence = def.Influence;
				this.FactionValue = def.FactionValue;
			}

			// Token: 0x06008D3F RID: 36159 RVA: 0x0023E764 File Offset: 0x0023C964
			private SerializableSystemInfluenceDef()
			{
			}

			// Token: 0x06008D40 RID: 36160 RVA: 0x0023E777 File Offset: 0x0023C977
			public StarSystemDef.SystemInfluenceDef GetSystemInfluenceDef()
			{
				return new StarSystemDef.SystemInfluenceDef(this.FactionValue, this.influence);
			}

			// Token: 0x06008D41 RID: 36161 RVA: 0x0023E78A File Offset: 0x0023C98A
			private void UpgradeToDataDrivenEnums()
			{
				if (string.IsNullOrEmpty(this.factionID) || this.factionID == FactionEnumeration.INVALID_DEFAULT_ID)
				{
					this.factionID = this.faction.ToString();
				}
			}

			// Token: 0x040057CF RID: 22479
			[SerializableMember(SerializationTarget.SaveGame)]
			private Faction faction;

			// Token: 0x040057D0 RID: 22480
			[SerializableMember(SerializationTarget.SaveGame)]
			private float influence;

			// Token: 0x040057D1 RID: 22481
			[SerializableMember(SerializationTarget.SaveGame)]
			private string factionID = FactionEnumeration.INVALID_DEFAULT_ID;

			// Token: 0x040057D2 RID: 22482
			private FactionValue factionValue;
		}

		// Token: 0x02001067 RID: 4199
		[SerializableContract("StarSystemDef.SystemInfluenceDef")]
		[Serializable]
		public struct SystemInfluenceDef
		{
			// Token: 0x170018B6 RID: 6326
			// (get) Token: 0x06008D42 RID: 36162 RVA: 0x0023E7C4 File Offset: 0x0023C9C4
			// (set) Token: 0x06008D43 RID: 36163 RVA: 0x0023E81B File Offset: 0x0023CA1B
			public FactionValue FactionValue
			{
				get
				{
					if (string.IsNullOrEmpty(this.factionID))
					{
						this.UpgradeToDataDrivenEnums();
					}
					if (this.factionValue == null || this.factionValue.Name != this.factionID)
					{
						this.factionValue = FactionEnumeration.GetFactionByName(this.factionID);
					}
					return this.factionValue;
				}
				private set
				{
					this.factionValue = value;
					this.factionID = this.factionValue.Name;
				}
			}

			// Token: 0x06008D44 RID: 36164 RVA: 0x0023E835 File Offset: 0x0023CA35
			public SystemInfluenceDef(FactionValue faction, float influence)
			{
				this.Faction = Faction.INVALID_UNSET;
				this.factionID = faction.Name;
				this.factionValue = faction;
				this.Influence = influence;
			}

			// Token: 0x06008D45 RID: 36165 RVA: 0x0023E858 File Offset: 0x0023CA58
			private void UpgradeToDataDrivenEnums()
			{
				if (string.IsNullOrEmpty(this.factionID) || this.factionID == FactionEnumeration.INVALID_DEFAULT_ID)
				{
					this.factionID = this.Faction.ToString();
				}
			}

			// Token: 0x040057D3 RID: 22483
			[SerializableMember(SerializationTarget.SaveGame)]
			private Faction Faction;

			// Token: 0x040057D4 RID: 22484
			[SerializableMember(SerializationTarget.SaveGame)]
			public float Influence;

			// Token: 0x040057D5 RID: 22485
			[SerializableMember(SerializationTarget.SaveGame)]
			private string factionID;

			// Token: 0x040057D6 RID: 22486
			private FactionValue factionValue;
		}
	}
}
