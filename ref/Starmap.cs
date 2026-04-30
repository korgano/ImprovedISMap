using System;
using System.Collections.Generic;
using BattleTech.UI;
using HBS.Nav;
using Localize;
using UnityEngine;

namespace BattleTech
{
	// Token: 0x02001114 RID: 4372
	public class Starmap : MonoBehaviour
	{
		// Token: 0x170019C4 RID: 6596
		// (get) Token: 0x0600949F RID: 38047 RVA: 0x0026D9DC File Offset: 0x0026BBDC
		public StarSystemNode CurPlanet
		{
			get
			{
				if (this.sim == null)
				{
					return null;
				}
				return this.planetMap[this.sim.CurSystem.ID];
			}
		}

		// Token: 0x170019C5 RID: 6597
		// (get) Token: 0x060094A0 RID: 38048 RVA: 0x0026DA03 File Offset: 0x0026BC03
		public List<StarSystemNode> PlanetList
		{
			get
			{
				return this.planetList;
			}
		}

		// Token: 0x170019C6 RID: 6598
		// (get) Token: 0x060094A1 RID: 38049 RVA: 0x0026DA0B File Offset: 0x0026BC0B
		// (set) Token: 0x060094A2 RID: 38050 RVA: 0x0026DA13 File Offset: 0x0026BC13
		public List<StarSystemNode> VisisbleSystem { get; private set; }

		// Token: 0x170019C7 RID: 6599
		// (get) Token: 0x060094A3 RID: 38051 RVA: 0x0026DA1C File Offset: 0x0026BC1C
		// (set) Token: 0x060094A4 RID: 38052 RVA: 0x0026DA24 File Offset: 0x0026BC24
		public List<Vector4> VisibleSystemPositions { get; private set; }

		// Token: 0x170019C8 RID: 6600
		// (get) Token: 0x060094A5 RID: 38053 RVA: 0x0026DA2D File Offset: 0x0026BC2D
		// (set) Token: 0x060094A6 RID: 38054 RVA: 0x0026DA35 File Offset: 0x0026BC35
		public List<Vector4> VisibleSystemProperties { get; private set; }

		// Token: 0x170019C9 RID: 6601
		// (get) Token: 0x060094A7 RID: 38055 RVA: 0x0026DA3E File Offset: 0x0026BC3E
		// (set) Token: 0x060094A8 RID: 38056 RVA: 0x0026DA46 File Offset: 0x0026BC46
		public Vector2 MapSize { get; private set; }

		// Token: 0x170019CA RID: 6602
		// (get) Token: 0x060094A9 RID: 38057 RVA: 0x0026DA4F File Offset: 0x0026BC4F
		// (set) Token: 0x060094AA RID: 38058 RVA: 0x0026DA57 File Offset: 0x0026BC57
		public Vector2 MapOffset { get; private set; }

		// Token: 0x170019CB RID: 6603
		// (get) Token: 0x060094AB RID: 38059 RVA: 0x0026DA60 File Offset: 0x0026BC60
		private int Count
		{
			get
			{
				return this.planetList.Count;
			}
		}

		// Token: 0x170019CC RID: 6604
		// (get) Token: 0x060094AC RID: 38060 RVA: 0x0026DA6D File Offset: 0x0026BC6D
		// (set) Token: 0x060094AD RID: 38061 RVA: 0x0026DA75 File Offset: 0x0026BC75
		public StarSystemNode CurSelected { get; private set; }

		// Token: 0x170019CD RID: 6605
		// (get) Token: 0x060094AE RID: 38062 RVA: 0x0026DA7E File Offset: 0x0026BC7E
		// (set) Token: 0x060094AF RID: 38063 RVA: 0x0026DA86 File Offset: 0x0026BC86
		public StarSystemNode CurHovered { get; private set; }

		// Token: 0x170019CE RID: 6606
		// (get) Token: 0x060094B0 RID: 38064 RVA: 0x0026DA8F File Offset: 0x0026BC8F
		// (set) Token: 0x060094B1 RID: 38065 RVA: 0x0026DA97 File Offset: 0x0026BC97
		public StarmapRenderer Screen { get; private set; }

		// Token: 0x170019CF RID: 6607
		// (get) Token: 0x060094B2 RID: 38066 RVA: 0x0026DAA0 File Offset: 0x0026BCA0
		// (set) Token: 0x060094B3 RID: 38067 RVA: 0x0026DAA8 File Offset: 0x0026BCA8
		public List<StarSystemNode> PotentialPath { get; private set; }

		// Token: 0x170019D0 RID: 6608
		// (get) Token: 0x060094B4 RID: 38068 RVA: 0x0026DAB1 File Offset: 0x0026BCB1
		// (set) Token: 0x060094B5 RID: 38069 RVA: 0x0026DAB9 File Offset: 0x0026BCB9
		public List<StarSystemNode> ActivePath { get; private set; }

		// Token: 0x170019D1 RID: 6609
		// (get) Token: 0x060094B6 RID: 38070 RVA: 0x0026DAC2 File Offset: 0x0026BCC2
		// (set) Token: 0x060094B7 RID: 38071 RVA: 0x0026DACA File Offset: 0x0026BCCA
		public int ProjectedTravelCost { get; private set; }

		// Token: 0x170019D2 RID: 6610
		// (get) Token: 0x060094B8 RID: 38072 RVA: 0x0026DAD3 File Offset: 0x0026BCD3
		// (set) Token: 0x060094B9 RID: 38073 RVA: 0x0026DADB File Offset: 0x0026BCDB
		public int ProjectedTravelTime { get; private set; }

		// Token: 0x170019D3 RID: 6611
		// (get) Token: 0x060094BA RID: 38074 RVA: 0x0026DAE4 File Offset: 0x0026BCE4
		// (set) Token: 0x060094BB RID: 38075 RVA: 0x0026DAEC File Offset: 0x0026BCEC
		public StarSystemNode Destination { get; private set; }

		// Token: 0x170019D4 RID: 6612
		// (get) Token: 0x060094BC RID: 38076 RVA: 0x0026DAF5 File Offset: 0x0026BCF5
		// (set) Token: 0x060094BD RID: 38077 RVA: 0x0026DB02 File Offset: 0x0026BD02
		public WorkOrderEntry_TravelGeneric TravelOrder
		{
			get
			{
				return this.sim.TravelOrder;
			}
			set
			{
				this.sim.SetTravelOrder(value);
			}
		}

		// Token: 0x170019D5 RID: 6613
		// (get) Token: 0x060094BE RID: 38078 RVA: 0x0026DB10 File Offset: 0x0026BD10
		// (set) Token: 0x060094BF RID: 38079 RVA: 0x0026DB1D File Offset: 0x0026BD1D
		public TaskManagementElement TravelItem
		{
			get
			{
				return this.sim.TravelItem;
			}
			private set
			{
				this.sim.SetTravelItem(value);
			}
		}

		// Token: 0x170019D6 RID: 6614
		// (get) Token: 0x060094C0 RID: 38080 RVA: 0x0026DB2B File Offset: 0x0026BD2B
		// (set) Token: 0x060094C1 RID: 38081 RVA: 0x0026DB33 File Offset: 0x0026BD33
		public int travelIndex { get; private set; }

		// Token: 0x060094C2 RID: 38082 RVA: 0x0026DB3C File Offset: 0x0026BD3C
		public void PopulateMap(SimGameState simGame)
		{
			this.sim = simGame;
			TravelConstantsDef travel = this.sim.Constants.Travel;
			this.PopulateMap(simGame.StarSystems, travel);
			this.Screen = this.sim.CameraController.StarmapScreen;
			this.Screen.SetSimState(simGame);
			this.Screen.PopulateMap(this);
			this.Screen.RefreshBorders();
		}

		// Token: 0x060094C3 RID: 38083 RVA: 0x0026DBA8 File Offset: 0x0026BDA8
		public void PopulateMap(List<StarSystem> systems, TravelConstantsDef con)
		{
			this.VisisbleSystem = new List<StarSystemNode>();
			this.VisibleSystemPositions = new List<Vector4>();
			this.VisibleSystemProperties = new List<Vector4>();
			this.planetMap = new Dictionary<string, StarSystemNode>();
			for (int i = 0; i < systems.Count; i++)
			{
				StarSystem starSystem = systems[i];
				StarSystemNode starSystemNode = new StarSystemNode(this, starSystem, starSystem.Def.FuelingStation ? con.FuelStationFuelTime : con.DefaultFuelTime, i);
				this.planetList.Add(starSystemNode);
				this.planetMap.Add(starSystem.ID, starSystemNode);
			}
			for (int j = 0; j < this.planetList.Count; j++)
			{
				StarSystemNode starSystemNode2 = this.planetList[j];
				for (int k = 0; k < this.planetList.Count; k++)
				{
					if (j != k)
					{
						StarSystemNode starSystemNode3 = this.planetList[k];
						if ((starSystemNode2.WorldPosition - starSystemNode3.WorldPosition).worldMagnitude <= (float)con.MaxJumpDistance)
						{
							starSystemNode2.AdjacentSystems.Add(starSystemNode3);
						}
					}
				}
			}
			this.SetBoundaries(-9999, 9999, -9999, 9999);
		}

		// Token: 0x060094C4 RID: 38084 RVA: 0x0026DCE0 File Offset: 0x0026BEE0
		public void SetBoundaries(int left = -9999, int right = 9999, int top = -9999, int bottom = 9999)
		{
			this.VisisbleSystem.Clear();
			this.VisibleSystemPositions.Clear();
			this.VisibleSystemProperties.Clear();
			float num = 9999f;
			float num2 = -9999f;
			float num3 = 9999f;
			float num4 = -9999f;
			for (int i = 0; i < this.planetList.Count; i++)
			{
				StarSystemNode starSystemNode = this.planetList[i];
				Vector3 position = starSystemNode.Position;
				if (position.x >= (float)left && position.x <= (float)right && position.y >= (float)top && position.y <= (float)bottom)
				{
					if (position.x < num)
					{
						num = position.x;
					}
					if (position.x > num2)
					{
						num2 = position.x;
					}
					if (position.y < num3)
					{
						num3 = position.y;
					}
					if (position.y > num4)
					{
						num4 = position.y;
					}
					this.VisisbleSystem.Add(starSystemNode);
				}
			}
			this.MapOffset = new Vector2(-num, -num3);
			this.MapSize = new Vector2(Mathf.Abs(num - num2), Mathf.Abs(num3 - num4));
			for (int j = 0; j < this.planetList.Count; j++)
			{
				this.VisibleSystemPositions.Add(this.planetList[j].NormalizedPosition);
				Vector4 vector = new Vector4(this.planetList[j].System.Def.FuelingStation ? 1f : 0f, (float)this.planetList[j].System.Def.OwnerValue.ID, (this.planetList[j] == this.objectivePlanet) ? 1f : 0f, 0f);
				this.VisibleSystemProperties.Add(vector);
			}
		}

		// Token: 0x060094C5 RID: 38085 RVA: 0x0026DED4 File Offset: 0x0026C0D4
		public void SetObjectivePlanetByID(string id)
		{
			StarSystemNode systemByID = this.GetSystemByID(id);
			this.SetObjectivePlanet(systemByID);
		}

		// Token: 0x060094C6 RID: 38086 RVA: 0x0026DEF0 File Offset: 0x0026C0F0
		private void SetObjectivePlanet(StarSystemNode planet)
		{
			this.objectivePlanet = planet;
			this.VisibleSystemPositions.Clear();
			this.VisibleSystemProperties.Clear();
			for (int i = 0; i < this.planetList.Count; i++)
			{
				this.VisibleSystemPositions.Add(this.planetList[i].NormalizedPosition);
				Vector4 vector = new Vector4(this.planetList[i].System.Def.FuelingStation ? 1f : 0f, (float)this.planetList[i].System.Def.OwnerValue.ID, (this.planetList[i] == this.objectivePlanet) ? 1f : 0f, 0f);
				this.VisibleSystemProperties.Add(vector);
			}
		}

		// Token: 0x060094C7 RID: 38087 RVA: 0x0026DFDC File Offset: 0x0026C1DC
		public void SetActivePathFromLoad(List<string> LoadedTravelPath, int TravelIndex)
		{
			this.travelIndex = TravelIndex;
			this.ActivePath = new List<StarSystemNode>();
			this.PotentialPath = new List<StarSystemNode>();
			foreach (string text in LoadedTravelPath)
			{
				this.ActivePath.Add(this.GetSystemByID(text));
			}
			int travelTime = this.sim.TravelTime;
			this.Destination = this.ActivePath[this.ActivePath.Count - 1];
			this.TravelOrder = this.sim.TravelOrder;
			this.TravelItem = this.sim.RoomManager.GetWorkQueueEntry(this.TravelOrder);
			if (this.TravelItem == null)
			{
				this.TravelItem = this.sim.RoomManager.AddWorkQueueEntry(this.TravelOrder);
			}
			this.sim.SetTimeMoving(false, true);
			this.Screen.UpdateActivePath();
		}

		// Token: 0x060094C8 RID: 38088 RVA: 0x0026E0EC File Offset: 0x0026C2EC
		private void Update()
		{
			if (!this.starmapPathfinder.IsDone)
			{
				this.starmapPathfinder.Step();
			}
			if (this.tempPathFinderList != null)
			{
				for (int i = this.tempPathFinderList.Count - 1; i >= 0; i--)
				{
					if (!this.tempPathFinderList[i].IsDone)
					{
						this.tempPathFinderList[i].Step();
					}
					else
					{
						this.tempPathFinderList.RemoveAt(i);
					}
				}
			}
		}

		// Token: 0x060094C9 RID: 38089 RVA: 0x0026E168 File Offset: 0x0026C368
		public void FindRouteTo(string id)
		{
			StarSystemNode systemByID = this.GetSystemByID(this.sim.CurSystem.ID);
			StarSystemNode systemByID2 = this.GetSystemByID(id);
			this.starmapPathfinder.InitFindPath(systemByID, systemByID2, 1, 1E-06f, new Action<AStar.AStarResult>(this.OnPathfindingComplete));
		}

		// Token: 0x060094CA RID: 38090 RVA: 0x0026E1B4 File Offset: 0x0026C3B4
		public void FindRouteTo(StarSystemNode targetNode)
		{
			StarSystemNode systemByID = this.GetSystemByID(this.sim.CurSystem.ID);
			this.starmapPathfinder.InitFindPath(systemByID, targetNode, 1, 1E-06f, new Action<AStar.AStarResult>(this.OnPathfindingComplete));
		}

		// Token: 0x060094CB RID: 38091 RVA: 0x0026E1F8 File Offset: 0x0026C3F8
		public void FindRouteTo(StarSystem targetSystem, Action<AStar.AStarResult> result)
		{
			StarSystemNode systemByID = this.GetSystemByID(this.sim.CurSystem.ID);
			StarSystemNode systemByID2 = this.GetSystemByID(targetSystem.ID);
			if (systemByID == systemByID2)
			{
				List<INavNode> list = new List<INavNode>();
				AStar.AStarResult astarResult = new AStar.AStarResult();
				list.Add(systemByID);
				astarResult.path = list;
				result(astarResult);
				return;
			}
			AStar.PathFinder pathFinder = new AStar.PathFinder();
			pathFinder.InitFindPath(systemByID, systemByID2, 1, 1E-06f, result);
			this.tempPathFinderList.Add(pathFinder);
		}

		// Token: 0x060094CC RID: 38092 RVA: 0x0026E274 File Offset: 0x0026C474
		private void OnPathfindingComplete(AStar.AStarResult result)
		{
			if (result.status == PathStatus.Complete)
			{
				this.PotentialPath = new List<StarSystemNode>();
				this.ProjectedTravelTime = this.DistanceToJumpship();
				this.ProjectedTravelCost = 0;
				int count = result.path.Count;
				this.PendingTravelOrder = new WorkOrderEntry_TravelGeneric("Travel", Strings.T("Travel to {0}", new object[] { this.CurSelected.System.Def.Description.Name }), 0, "");
				if (this.ProjectedTravelTime > 0)
				{
					new WorkOrderEntry_TravelToJumpPoint("Travel", Strings.T("Travel to {0} Jump Point", new object[] { this.CurPlanet.System.Def.Description.Name }), this.ProjectedTravelTime, this.PendingTravelOrder, "");
				}
				for (int i = 0; i < count; i++)
				{
					StarSystemNode starSystemNode = (StarSystemNode)result.path[i];
					this.PotentialPath.Add(starSystemNode);
					if (i + 1 < count)
					{
						this.ProjectedTravelTime += starSystemNode.Cost;
						this.ProjectedTravelCost += this.sim.Constants.Finances.JumpShipCost;
						StarSystemNode starSystemNode2 = (StarSystemNode)result.path[i + 1];
						new WorkOrderEntry_TravelJumping("Travel", Strings.T("Jumping to {0}", new object[] { starSystemNode2.System.Def.Description.Name }), starSystemNode.Cost, this.PendingTravelOrder, "");
					}
					else
					{
						int jumpDistance = starSystemNode.System.JumpDistance;
						this.ProjectedTravelTime += jumpDistance;
						new WorkOrderEntry_TravelToSystem("Travel", Strings.T("Traveling to {0} System", new object[] { starSystemNode.System.Def.Description.Name }), jumpDistance, this.PendingTravelOrder, "");
					}
				}
				this.Screen.UpdatePlannedPath();
				this.StarSystemRouted.Invoke(this.CurSelected.System);
				return;
			}
			Debug.LogError("Invalid route: " + result.status);
		}

		// Token: 0x060094CD RID: 38093 RVA: 0x0026E4AC File Offset: 0x0026C6AC
		public void SetActivePath()
		{
			if (this.PotentialPath == null)
			{
				return;
			}
			this.CreateActivePath(true);
			this.Screen.UpdateActivePath();
			this.Screen.ClearPlannedPath();
		}

		// Token: 0x060094CE RID: 38094 RVA: 0x0026E4D4 File Offset: 0x0026C6D4
		public void MoveToCurrentSystem()
		{
			this.PotentialPath = new List<StarSystemNode>();
			this.PotentialPath.Add(this.CurPlanet);
			this.CreateActivePath(true);
		}

		// Token: 0x060094CF RID: 38095 RVA: 0x0026E4FC File Offset: 0x0026C6FC
		private void CreateActivePath(bool causeStateChange = true)
		{
			this.ActivePath = this.PotentialPath;
			this.Destination = this.ActivePath[this.ActivePath.Count - 1];
			this.PotentialPath = null;
			SimGameTravelStatus simGameTravelStatus = this.sim.TravelState;
			int num = this.sim.TravelTime;
			this.travelIndex = 0;
			switch (simGameTravelStatus)
			{
			case SimGameTravelStatus.IN_SYSTEM:
				simGameTravelStatus = SimGameTravelStatus.TRANSIT_TO_JUMP;
				num = this.sim.CurSystem.JumpDistance;
				break;
			case SimGameTravelStatus.AT_JUMP_POINT:
			case SimGameTravelStatus.WARMING_ENGINES:
				simGameTravelStatus = SimGameTravelStatus.WARMING_ENGINES;
				num = this.CurPlanet.Cost;
				break;
			case SimGameTravelStatus.TRANSIT_FROM_JUMP:
				simGameTravelStatus = SimGameTravelStatus.TRANSIT_FROM_JUMP;
				num = Math.Max(0, this.sim.CurSystem.JumpDistance - num);
				break;
			}
			if (causeStateChange)
			{
				this.sim.TravelManager.SetTravelState(simGameTravelStatus, false);
			}
			this.sim.SetTravelTime(num, null);
			AudioEventManager.PlayAudioEvent("audioeventdef_simgame_vo_barks", "travel_confirmcourse", WwiseManager.GlobalAudioObject, null);
			if (this.TravelItem != null)
			{
				this.sim.RoomManager.RemoveWorkQueueEntry(this.TravelItem.Entry, false);
			}
			this.TravelOrder = this.PendingTravelOrder;
			this.TravelItem = this.sim.RoomManager.AddWorkQueueEntry(this.TravelOrder);
		}

		// Token: 0x060094D0 RID: 38096 RVA: 0x0026E63F File Offset: 0x0026C83F
		public void PayTravelOrderCost(int cost)
		{
			if (this.TravelOrder == null)
			{
				Debug.Log("Attempted to pay cost on Nonexistant TravelOrder");
				return;
			}
			this.TravelOrder.PayCost(cost);
		}

		// Token: 0x060094D1 RID: 38097 RVA: 0x0026E661 File Offset: 0x0026C861
		public void UpdateTravelItem()
		{
			this.TravelItem.UpdateItem(0);
		}

		// Token: 0x060094D2 RID: 38098 RVA: 0x0026E670 File Offset: 0x0026C870
		public int GetCurrentTravelIndexCost()
		{
			if (this.ActivePath.Count == 0)
			{
				return 0;
			}
			return this.ActivePath[this.travelIndex].Cost;
		}

		// Token: 0x060094D3 RID: 38099 RVA: 0x0026E698 File Offset: 0x0026C898
		public void IncrementTravelIndexAndUpdateCurrentSystem()
		{
			int num = this.travelIndex + 1;
			this.travelIndex = num;
			this.sim.SetCurrentSystem(this.ActivePath[this.travelIndex].System, false, false);
			this.sim.LogReport(string.Format("Jumped to {0} System", this.ActivePath[this.travelIndex].System.Def.Description.Name));
		}

		// Token: 0x060094D4 RID: 38100 RVA: 0x0026E712 File Offset: 0x0026C912
		public void RemoveCompletedTravelOrder()
		{
			this.sim.RoomManager.RemoveWorkQueueEntry(this.TravelOrder, false);
		}

		// Token: 0x060094D5 RID: 38101 RVA: 0x0026E72B File Offset: 0x0026C92B
		public void CompletedTravelClear()
		{
			this.TravelItem = null;
			this.TravelOrder = null;
			this.ActivePath = null;
			this.Screen.ClearActivePath();
		}

		// Token: 0x060094D6 RID: 38102 RVA: 0x0026E750 File Offset: 0x0026C950
		public int DistanceToJumpship()
		{
			int jumpDistance = this.sim.CurSystem.JumpDistance;
			switch (this.sim.TravelState)
			{
			case SimGameTravelStatus.IN_SYSTEM:
				return jumpDistance;
			case SimGameTravelStatus.AT_JUMP_POINT:
			case SimGameTravelStatus.WARMING_ENGINES:
				return 0;
			case SimGameTravelStatus.TRANSIT_TO_JUMP:
				return this.sim.TravelTime;
			case SimGameTravelStatus.TRANSIT_FROM_JUMP:
				return jumpDistance - this.sim.TravelTime;
			default:
				return 0;
			}
		}

		// Token: 0x060094D7 RID: 38103 RVA: 0x0026E7B8 File Offset: 0x0026C9B8
		public StarSystemNode GetLocationByUV(Vector2 pos, Rect loc, float scale)
		{
			foreach (StarSystemNode starSystemNode in this.planetList)
			{
				Vector2 normalizedPosition = starSystemNode.NormalizedPosition;
				normalizedPosition.x = normalizedPosition.x * 2f - 1f;
				normalizedPosition.y = normalizedPosition.y * 2f - 1f;
				Vector2 vector = normalizedPosition * scale;
				vector.x = (vector.x + 1f) * 0.5f;
				vector.y = (vector.y + 1f) * 0.5f;
				if (loc.Contains(vector))
				{
					return starSystemNode;
				}
			}
			return null;
		}

		// Token: 0x060094D8 RID: 38104 RVA: 0x0026E890 File Offset: 0x0026CA90
		public StarSystemNode GetSystemByID(string id)
		{
			if (this.planetMap.ContainsKey(id))
			{
				return this.planetMap[id];
			}
			return null;
		}

		// Token: 0x060094D9 RID: 38105 RVA: 0x0026E8AE File Offset: 0x0026CAAE
		public bool CanTravelToNode(string systemId)
		{
			return this.CanTravelToNode(this.GetSystemByID(systemId), false);
		}

		// Token: 0x060094DA RID: 38106 RVA: 0x0026E8C0 File Offset: 0x0026CAC0
		public bool CanTravelToNode(StarSystemNode node, bool popup = false)
		{
			foreach (RequirementDef requirementDef in node.System.Def.TravelRequirements)
			{
				if (!this.sim.MeetsRequirements(requirementDef, null))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x060094DB RID: 38107 RVA: 0x0026E92C File Offset: 0x0026CB2C
		public void SetHovered(StarSystemNode systemNode)
		{
			if (systemNode != this.CurHovered)
			{
				if (this.CurHovered != null && this.CurHovered != this.CurSelected)
				{
					this.Screen.GetSystemRenderer(this.CurHovered).Deselected();
				}
				this.CurHovered = systemNode;
				if (this.CurHovered != null)
				{
					this.Screen.GetSystemRenderer(this.CurHovered).Selected();
					this.StarSystemHovered.Invoke(this.CurHovered.System);
					return;
				}
				this.StarSystemHovered.Invoke(null);
			}
		}

		// Token: 0x060094DC RID: 38108 RVA: 0x0026E9B8 File Offset: 0x0026CBB8
		public bool SetSelectedSystem(string id)
		{
			StarSystemNode systemByID = this.GetSystemByID(id);
			return this.SetSelectedSystem(systemByID);
		}

		// Token: 0x060094DD RID: 38109 RVA: 0x0026E9D4 File Offset: 0x0026CBD4
		public bool SetSelectedSystem(StarSystem sys)
		{
			return this.SetSelectedSystem(sys.ID);
		}

		// Token: 0x060094DE RID: 38110 RVA: 0x0026E9E2 File Offset: 0x0026CBE2
		public bool SetSelectedSystem(StarSystemNode node)
		{
			this.CurSelected = node;
			this.FindRouteTo(node);
			return true;
		}

		// Token: 0x060094DF RID: 38111 RVA: 0x0026E9F3 File Offset: 0x0026CBF3
		public void OnTravelContractConfirmed()
		{
			this.SetActivePath();
			this.sim.StartBreadcrumb(this.ProjectedTravelCost);
			this.sim.SetSimRoomState(DropshipLocation.SHIP);
		}

		// Token: 0x060094E0 RID: 38112 RVA: 0x0026EA18 File Offset: 0x0026CC18
		public void OnTravelContractCancelled()
		{
			this.sim.CancelBreadcrumb();
		}

		// Token: 0x060094E1 RID: 38113 RVA: 0x0026EA28 File Offset: 0x0026CC28
		public void CancelTravelAndMoveToCurrentSystem()
		{
			this.PotentialPath = new List<StarSystemNode>();
			this.PotentialPath.Add(this.CurPlanet);
			this.PendingTravelOrder = new WorkOrderEntry_TravelGeneric("Travel", Strings.T("Travel to {0}", new object[] { this.CurPlanet.System.Def.Description.Name }), 0, "");
			int jumpDistance = this.sim.CurSystem.JumpDistance;
			this.ProjectedTravelTime += jumpDistance;
			new WorkOrderEntry_TravelToSystem("Travel", Strings.T("Traveling to {0} System", new object[] { this.CurPlanet.System.Def.Description.Name }), jumpDistance, this.PendingTravelOrder, "");
			this.CreateActivePath(false);
			this.sim.TravelManager.SetTravelStateFromInterrupt(SimGameTravelStatus.TRANSIT_FROM_JUMP, false);
		}

		// Token: 0x060094E2 RID: 38114 RVA: 0x0026EB10 File Offset: 0x0026CD10
		public StarSystem GetNextSystemInTravel()
		{
			StarSystem starSystem;
			if (this.travelIndex + 1 < this.ActivePath.Count)
			{
				starSystem = this.ActivePath[this.travelIndex + 1].System;
			}
			else
			{
				starSystem = this.CurPlanet.System;
			}
			return starSystem;
		}

		// Token: 0x060094E3 RID: 38115 RVA: 0x0026EB5C File Offset: 0x0026CD5C
		public StarSystem GetDestinationSystem()
		{
			return this.Destination.System;
		}

		// Token: 0x060094E4 RID: 38116 RVA: 0x0026EB6C File Offset: 0x0026CD6C
		public List<StarSystem> GetAvailableNeighborSystem(StarSystem system)
		{
			StarSystemNode starSystemNode = this.planetMap[system.ID];
			List<StarSystem> list = new List<StarSystem>();
			foreach (StarSystemNode starSystemNode2 in starSystemNode.AdjacentSystems)
			{
				if (this.CanTravelToNode(starSystemNode2, false))
				{
					list.Add(starSystemNode2.System);
				}
			}
			return list;
		}

		// Token: 0x04005EA9 RID: 24233
		private SimGameState sim;

		// Token: 0x04005EAA RID: 24234
		public const bool UseTravelReq = true;

		// Token: 0x04005EAB RID: 24235
		private List<StarSystemNode> planetList = new List<StarSystemNode>();

		// Token: 0x04005EAC RID: 24236
		private Dictionary<string, StarSystemNode> planetMap = new Dictionary<string, StarSystemNode>();

		// Token: 0x04005EB0 RID: 24240
		private AStar.PathFinder starmapPathfinder = new AStar.PathFinder();

		// Token: 0x04005EB1 RID: 24241
		private List<AStar.PathFinder> tempPathFinderList = new List<AStar.PathFinder>();

		// Token: 0x04005EB2 RID: 24242
		private StarSystemNode objectivePlanet;

		// Token: 0x04005EB3 RID: 24243
		public const float DISTANCE_SCALER = 1E-06f;

		// Token: 0x04005EB8 RID: 24248
		public StarSystemEvent StarSystemRouted = new StarSystemEvent();

		// Token: 0x04005EB9 RID: 24249
		public StarSystemEvent StarSystemHovered = new StarSystemEvent();

		// Token: 0x04005EBA RID: 24250
		public StarSystemEvent StarSystemSelected = new StarSystemEvent();

		// Token: 0x04005EC1 RID: 24257
		private WorkOrderEntry_TravelGeneric PendingTravelOrder;
	}
}
