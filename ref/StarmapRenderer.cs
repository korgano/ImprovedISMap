using System;
using System.Collections;
using System.Collections.Generic;
using BattleTech.UI;
using HBS;
using UnityEngine;

namespace BattleTech
{
	// Token: 0x0200101C RID: 4124
	public class StarmapRenderer : MonoBehaviour
	{
		// Token: 0x17001803 RID: 6147
		// (get) Token: 0x06008A4C RID: 35404 RVA: 0x00233BD8 File Offset: 0x00231DD8
		private SimGameCameraController cameraController
		{
			get
			{
				if (this._cameraController == null)
				{
					this._cameraController = Object.FindObjectOfType<SimGameCameraController>();
				}
				return this._cameraController;
			}
		}

		// Token: 0x17001804 RID: 6148
		// (get) Token: 0x06008A4D RID: 35405 RVA: 0x00233BF9 File Offset: 0x00231DF9
		public float ZoomLevel
		{
			get
			{
				return this.zoomLevel;
			}
		}

		// Token: 0x17001805 RID: 6149
		// (get) Token: 0x06008A4E RID: 35406 RVA: 0x00233C01 File Offset: 0x00231E01
		// (set) Token: 0x06008A4F RID: 35407 RVA: 0x00233C09 File Offset: 0x00231E09
		public bool StarmapVisible { get; set; }

		// Token: 0x17001806 RID: 6150
		// (get) Token: 0x06008A50 RID: 35408 RVA: 0x00233C14 File Offset: 0x00231E14
		private Camera mainCamera
		{
			get
			{
				if (this._mainCamera == null)
				{
					this._mainCamera = Camera.main;
				}
				if (this._mainCamera == null)
				{
					this._mainCamera = GameObject.FindGameObjectWithTag("SpaceCamera").GetComponent<Camera>();
				}
				return this._mainCamera;
			}
		}

		// Token: 0x17001807 RID: 6151
		// (get) Token: 0x06008A51 RID: 35409 RVA: 0x00233C63 File Offset: 0x00231E63
		private Material screenMaterial
		{
			get
			{
				if (this._screenMaterial == null)
				{
					this._screenMaterial = new Material(Shader.Find("Hidden/BT-StarmapScreen"));
					this.starmapDisplay.sharedMaterial = this._screenMaterial;
				}
				return this._screenMaterial;
			}
		}

		// Token: 0x17001808 RID: 6152
		// (get) Token: 0x06008A52 RID: 35410 RVA: 0x00233CA0 File Offset: 0x00231EA0
		private RenderTexture starmapRT
		{
			get
			{
				if (this._starmapRT == null || !this._starmapRT.IsCreated() || this._starmapRT.width != this.mainCamera.pixelWidth || this._starmapRT.height != this.mainCamera.pixelHeight)
				{
					this.starmapCamera.targetTexture = null;
					Object.DestroyImmediate(this._starmapRT);
					this._starmapRT = new RenderTexture(this.mainCamera.pixelWidth, this.mainCamera.pixelHeight, 0, RenderTextureFormat.RGB111110Float, RenderTextureReadWrite.Linear)
					{
						name = "Starmap RT",
						wrapMode = TextureWrapMode.Clamp,
						antiAliasing = 1
					};
					this._starmapRT.Create();
					this.starmapCamera.targetTexture = this._starmapRT;
					if (this.fakeCamera != null)
					{
						this.fakeCamera.CopyFrom(this.starmapCamera);
						this.fakeCamera.enabled = false;
					}
				}
				return this._starmapRT;
			}
		}

		// Token: 0x17001809 RID: 6153
		// (get) Token: 0x06008A53 RID: 35411 RVA: 0x00233DA0 File Offset: 0x00231FA0
		private LineRenderer plannedPath
		{
			get
			{
				if (this._plannedPath == null)
				{
					this._plannedPath = new GameObject("Planned Path")
					{
						transform = 
						{
							parent = this.starParent
						},
						layer = LayerMask.NameToLayer("Starmap")
					}.AddComponent<LineRenderer>();
					this._plannedPath.widthMultiplier = 0.15f;
					this._plannedPath.startWidth = 0.5f;
					this._plannedPath.endWidth = 0.5f;
					this._plannedPath.textureMode = LineTextureMode.Tile;
					this._plannedPath.sharedMaterial = Resources.Load<Material>("Materials/starmapPlanning");
					this._plannedPath.numCornerVertices = 4;
					this._plannedPath.numCapVertices = 1;
					this._plannedPath.useWorldSpace = true;
				}
				return this._plannedPath;
			}
		}

		// Token: 0x1700180A RID: 6154
		// (get) Token: 0x06008A54 RID: 35412 RVA: 0x00233E74 File Offset: 0x00232074
		private LineRenderer activePath
		{
			get
			{
				if (this._activePath == null)
				{
					this._activePath = new GameObject("Active Path")
					{
						transform = 
						{
							parent = this.starParent
						},
						layer = LayerMask.NameToLayer("Starmap")
					}.AddComponent<LineRenderer>();
					this._activePath.widthMultiplier = 0.15f;
					this._activePath.startWidth = 0.5f;
					this._activePath.endWidth = 0.5f;
					this._activePath.textureMode = LineTextureMode.Tile;
					this._activePath.sharedMaterial = Resources.Load<Material>("Materials/starmapActive");
					this._activePath.numCornerVertices = 4;
					this._activePath.numCapVertices = 1;
					this._activePath.useWorldSpace = true;
				}
				return this._activePath;
			}
		}

		// Token: 0x06008A55 RID: 35413 RVA: 0x00233F48 File Offset: 0x00232148
		public static Vector3 NormalizeToMapSpace(Vector2 normalizedPos)
		{
			Vector3 vector = normalizedPos;
			vector.x = (vector.x * 2f - 1f) * 100f;
			vector.y = (vector.y * 2f - 1f) * 100f * 0.5625f;
			vector.z = 0f;
			return vector;
		}

		// Token: 0x06008A56 RID: 35414 RVA: 0x00233FB0 File Offset: 0x002321B0
		public Vector2 NormalizeToViewportPoint(Vector2 normalizedPos)
		{
			Vector3 vector = StarmapRenderer.NormalizeToMapSpace(normalizedPos);
			return this.starmapCamera.WorldToViewportPoint(vector);
		}

		// Token: 0x06008A57 RID: 35415 RVA: 0x00233FD8 File Offset: 0x002321D8
		public Vector2 NormalizeToScreenPoint(Vector2 normalizedPos)
		{
			Vector3 vector = StarmapRenderer.NormalizeToMapSpace(normalizedPos);
			return this.starmapCamera.WorldToScreenPoint(vector);
		}

		// Token: 0x06008A58 RID: 35416 RVA: 0x00234000 File Offset: 0x00232200
		private Bounds FactionBounds(FactionValue faction, out int count)
		{
			Bounds bounds = default(Bounds);
			bool flag = false;
			count = 0;
			Vector3 vector = Vector3.zero;
			for (int i = 0; i < this.starmap.VisisbleSystem.Count; i++)
			{
				StarSystemNode starSystemNode = this.starmap.VisisbleSystem[i];
				if (starSystemNode.System.Def.OwnerValue.Equals(faction))
				{
					Vector3 vector2 = StarmapRenderer.NormalizeToMapSpace(starSystemNode.NormalizedPosition);
					count++;
					if (!flag)
					{
						bounds.center = vector2;
						bounds.size = new Vector3(0.1f, 0.1f, 0.1f);
						vector = vector2;
						flag = true;
					}
					else
					{
						bounds.Encapsulate(vector2);
						vector += vector2;
					}
				}
			}
			if (count > 0)
			{
				vector /= (float)count;
			}
			bounds.center = vector;
			return bounds;
		}

		// Token: 0x06008A59 RID: 35417 RVA: 0x002340DC File Offset: 0x002322DC
		private void SetLogo(GameObject logoObject, Bounds bounds, int count)
		{
			if (count == 0)
			{
				logoObject.SetActive(false);
				return;
			}
			logoObject.SetActive(true);
			logoObject.transform.position = bounds.center;
			logoObject.transform.localScale = new Vector3(20f, 20f, 20f);
		}

		// Token: 0x06008A5A RID: 35418 RVA: 0x0023412C File Offset: 0x0023232C
		public void SetSimState(SimGameState theSimState)
		{
			this.simState = theSimState;
		}

		// Token: 0x06008A5B RID: 35419 RVA: 0x00234138 File Offset: 0x00232338
		public void PopulateMap(Starmap map)
		{
			this.starmap = map;
			foreach (StarSystemNode starSystemNode in map.VisisbleSystem)
			{
				GameObject gameObject = Object.Instantiate<GameObject>(this.starPrototype);
				gameObject.name = starSystemNode.System.Name;
				gameObject.transform.parent = this.starParent;
				StarmapSystemRenderer component = gameObject.GetComponent<StarmapSystemRenderer>();
				this.systemDictionary.Add(gameObject, component);
				this.InitializeSysRenderer(starSystemNode, component);
			}
		}

		// Token: 0x06008A5C RID: 35420 RVA: 0x002341D8 File Offset: 0x002323D8
		private void InitializeSysRenderer(StarSystemNode node, StarmapSystemRenderer renderer)
		{
			bool flag = this.starmap.CanTravelToNode(node, false);
			bool flag2 = false;
			if (this.simState != null)
			{
				flag2 = this.simState.SystemBeenVisited(node.System);
			}
			Color color = (flag ? node.System.Def.OwnerValue.GetMapColor() : this.unavailableColor);
			if (renderer.Init(node, color, flag, flag2))
			{
				this.RefreshBorders();
			}
		}

		// Token: 0x06008A5D RID: 35421 RVA: 0x00234244 File Offset: 0x00232444
		public void RefreshSystems()
		{
			this.starmapCamera.gameObject.SetActive(true);
			foreach (StarmapSystemRenderer starmapSystemRenderer in this.systemDictionary.Values)
			{
				this.InitializeSysRenderer(starmapSystemRenderer.system, starmapSystemRenderer);
				if (this.starmap.CurSelected != null && this.starmap.CurSelected.System.ID == starmapSystemRenderer.system.System.ID)
				{
					starmapSystemRenderer.Selected();
				}
				else
				{
					starmapSystemRenderer.Deselected();
				}
			}
			FactionValue auriganDirectorateFactionValue = FactionEnumeration.GetAuriganDirectorateFactionValue();
			FactionValue auriganRestorationFactionValue = FactionEnumeration.GetAuriganRestorationFactionValue();
			this.PlaceLogo(auriganDirectorateFactionValue, this.directorateLogo);
			this.PlaceLogo(auriganRestorationFactionValue, this.restorationLogo);
		}

		// Token: 0x06008A5E RID: 35422 RVA: 0x00234320 File Offset: 0x00232520
		private void PlaceLogo(FactionValue faction, GameObject logo)
		{
			if (logo == null)
			{
				return;
			}
			List<StarSystemNode> list = new List<StarSystemNode>();
			foreach (StarSystemNode starSystemNode in this.starmap.VisisbleSystem)
			{
				if (starSystemNode.System.OwnerValue.Equals(faction))
				{
					list.Add(starSystemNode);
				}
			}
			if (list.Count == 0)
			{
				logo.SetActive(false);
				return;
			}
			Vector2 vector = list[0].NormalizedPosition;
			for (int i = 1; i < list.Count; i++)
			{
				vector += list[i].NormalizedPosition;
			}
			vector /= (float)list.Count;
			logo.SetActive(true);
			logo.transform.position = StarmapRenderer.NormalizeToMapSpace(vector);
		}

		// Token: 0x06008A5F RID: 35423 RVA: 0x00234408 File Offset: 0x00232608
		public void RefreshBorders()
		{
			this.refreshBorders = true;
		}

		// Token: 0x06008A60 RID: 35424 RVA: 0x00234414 File Offset: 0x00232614
		private void UpdatedPath(List<StarSystemNode> pathList, LineRenderer renderer)
		{
			if (pathList == null)
			{
				return;
			}
			Vector3[] array = new Vector3[pathList.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = StarmapRenderer.NormalizeToMapSpace(pathList[i].NormalizedPosition);
			}
			renderer.positionCount = array.Length;
			renderer.SetPositions(array);
		}

		// Token: 0x06008A61 RID: 35425 RVA: 0x00234467 File Offset: 0x00232667
		private void ClearPath(LineRenderer renderer)
		{
			renderer.positionCount = 0;
		}

		// Token: 0x06008A62 RID: 35426 RVA: 0x00234470 File Offset: 0x00232670
		public void AllowInput(bool canInput)
		{
			this.allowInput = canInput;
		}

		// Token: 0x06008A63 RID: 35427 RVA: 0x00234479 File Offset: 0x00232679
		public void UpdatePlannedPath()
		{
			this.UpdatedPath(this.starmap.PotentialPath, this.plannedPath);
		}

		// Token: 0x06008A64 RID: 35428 RVA: 0x00234492 File Offset: 0x00232692
		public void UpdateActivePath()
		{
			this.UpdatedPath(this.starmap.ActivePath, this.activePath);
		}

		// Token: 0x06008A65 RID: 35429 RVA: 0x002344AB File Offset: 0x002326AB
		public void ClearPlannedPath()
		{
			this.ClearPath(this.plannedPath);
		}

		// Token: 0x06008A66 RID: 35430 RVA: 0x002344B9 File Offset: 0x002326B9
		public void ClearActivePath()
		{
			this.ClearPath(this.activePath);
		}

		// Token: 0x06008A67 RID: 35431 RVA: 0x002344C8 File Offset: 0x002326C8
		private void Awake()
		{
			this.starmapCamera.transform.position = new Vector3(0f, 0f, -100f);
			this.starmapCamera.targetTexture = this.starmapRT;
			GameObject gameObject = new GameObject("Fake Camera");
			this.fakeCamera = gameObject.AddComponent<Camera>();
			this.fakeCamera.CopyFrom(this.starmapCamera);
			this.fakeCamera.enabled = false;
		}

		// Token: 0x06008A68 RID: 35432 RVA: 0x00234540 File Offset: 0x00232740
		public StarmapSystemRenderer GetSystemRenderer(string systemId)
		{
			StarSystemNode systemByID = this.starmap.GetSystemByID(systemId);
			return this.GetSystemRenderer(systemByID);
		}

		// Token: 0x06008A69 RID: 35433 RVA: 0x00234564 File Offset: 0x00232764
		public StarmapSystemRenderer GetSystemRenderer(StarSystemNode systemNode)
		{
			foreach (KeyValuePair<GameObject, StarmapSystemRenderer> keyValuePair in this.systemDictionary)
			{
				StarmapSystemRenderer value = keyValuePair.Value;
				if (value.system == systemNode)
				{
					return value;
				}
			}
			return null;
		}

		// Token: 0x06008A6A RID: 35434 RVA: 0x002345C8 File Offset: 0x002327C8
		public void SetObjective(StarSystemNode objective)
		{
			foreach (KeyValuePair<GameObject, StarmapSystemRenderer> keyValuePair in this.systemDictionary)
			{
				StarmapSystemRenderer value = keyValuePair.Value;
				value.SetNotification(value.system == objective);
			}
		}

		// Token: 0x06008A6B RID: 35435 RVA: 0x0023462C File Offset: 0x0023282C
		public void SetCurrent(StarSystemNode objective)
		{
			foreach (KeyValuePair<GameObject, StarmapSystemRenderer> keyValuePair in this.systemDictionary)
			{
				StarmapSystemRenderer value = keyValuePair.Value;
				value.SetArgo(value.system == objective);
			}
		}

		// Token: 0x06008A6C RID: 35436 RVA: 0x00234690 File Offset: 0x00232890
		private Vector3 GetFocusPoint()
		{
			Vector3 vector = new Vector3((float)Screen.width / 2f, (float)Screen.height / 3f * 2f, 0f);
			return this.fakeCamera.ScreenToWorldPoint(vector);
		}

		// Token: 0x06008A6D RID: 35437 RVA: 0x002346D3 File Offset: 0x002328D3
		private float DistanceFromFocusPoint(Vector3 otherPoint)
		{
			return Vector3.Distance(this.GetFocusPoint(), otherPoint);
		}

		// Token: 0x06008A6E RID: 35438 RVA: 0x002346E4 File Offset: 0x002328E4
		private void Update()
		{
			if (this.cameraController.curRoom != DropshipLocation.NAVIGATION)
			{
				this.StarmapVisible = false;
			}
			if (this.StarmapVisible && !this.starmapDisplay.gameObject.activeSelf)
			{
				this.starmapDisplay.gameObject.SetActive(true);
				Vector3 vector = StarmapRenderer.NormalizeToMapSpace(this.starmap.CurPlanet.NormalizedPosition);
				float z = this.starmapCamera.transform.position.z;
				this.starmapCamera.transform.position = new Vector3(vector.x, vector.y, z);
			}
			else if (!this.StarmapVisible && this.starmapDisplay.gameObject.activeSelf)
			{
				this.starmapDisplay.gameObject.SetActive(false);
			}
			if (this.StarmapVisible && !this.starmapCamera.enabled)
			{
				this.starmapCamera.enabled = true;
			}
			if (!this.StarmapVisible && this.starmapCamera.enabled)
			{
				this.starmapCamera.enabled = false;
			}
			if (!this.StarmapVisible)
			{
				Shader.DisableKeyword("_STARMAP");
				return;
			}
			Shader.EnableKeyword("_STARMAP");
			if (this.needsPan)
			{
				this.starmapCamera.transform.position = Vector3.SmoothDamp(this.starmapCamera.transform.position, this.cameraPanTarget, ref this.cameraPanVelocity, 0.5f, 80f);
				if (Vector3.Distance(this.starmapCamera.transform.position, this.cameraPanTarget) < 1f)
				{
					this.needsPan = false;
				}
			}
			this.screenMaterial.SetTexture("_MainTex", this.starmapRT);
			Shader.SetGlobalTexture("_StarmapTex", this.starmapRT);
			if (!this.allowInput || SimGameOptionsMenu.isUp)
			{
				this.starmap.SetHovered(null);
				this.clickBeganOnMap = false;
				return;
			}
			Vector2 vector2 = Input.mousePosition;
			if (vector2.x < 0f || vector2.y < 0f || vector2.x > (float)Screen.width || vector2.y > (float)Screen.height)
			{
				vector2 = new Vector2(-1f, -1f);
			}
			Shader.SetGlobalVector(StarmapRenderer.Uniforms._MousePos, new Vector4(vector2.x, vector2.y, vector2.x / (float)Screen.width, vector2.y / (float)Screen.height));
			RaycastHit raycastHit;
			bool flag = Physics.Raycast(this.starmapCamera.ScreenPointToRay(Input.mousePosition), ref raycastHit, this.starmapCamera.farClipPlane, 1 << LayerMask.NameToLayer("Starmap"));
			GameObject gameObject = (flag ? raycastHit.collider.gameObject : null);
			bool flag2 = flag && this.systemDictionary.ContainsKey(gameObject);
			Vector3 vector3 = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 100f);
			vector3 = this.fakeCamera.ScreenToWorldPoint(vector3);
			Vector3 vector4 = this.dragOrigin - vector3;
			if (Input.GetMouseButton(0))
			{
				if (Input.GetMouseButtonDown(0))
				{
					if (!LazySingletonBehavior<UIManager>.Instance.DoesRaycastHitUI(UIManagerRootType.UIRoot))
					{
						this.clickBeganOnMap = true;
						this.dragOrigin = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 100f);
						this.dragOrigin = this.fakeCamera.ScreenToWorldPoint(this.dragOrigin);
						this.starmapOffset = new Vector3?(this.starmapCamera.transform.position);
						this.needsPan = false;
					}
				}
				else if (this.clickBeganOnMap)
				{
					Vector3 vector5 = this.starmapOffset.Value + vector4;
					this.starmapCamera.transform.position = vector5;
				}
			}
			else
			{
				this.starmapOffset = null;
				this.clickBeganOnMap = false;
				if (Input.GetMouseButtonUp(0))
				{
					if (vector4.magnitude < 1f)
					{
						StarmapSystemRenderer starmapSystemRenderer = null;
						if (flag2)
						{
							starmapSystemRenderer = this.systemDictionary[gameObject];
							this.cameraPanTarget = new Vector3(starmapSystemRenderer.transform.position.x, starmapSystemRenderer.transform.position.y + -10f, this.starmapCamera.transform.position.z);
							Vector3 position = this.starmapCamera.transform.position;
							float num = Mathf.Max(this.cameraPanTarget.x, position.x) - Mathf.Min(this.cameraPanTarget.x, position.x);
							float num2 = Mathf.Max(this.cameraPanTarget.y, position.y) - Mathf.Min(this.cameraPanTarget.y, position.y);
							if (num > 35f || num2 > 14f)
							{
								this.needsPan = true;
							}
						}
						this.SetSelectedSystemRenderer(starmapSystemRenderer);
					}
				}
				else
				{
					StarSystemNode starSystemNode = null;
					if (flag2)
					{
						starSystemNode = this.systemDictionary[gameObject].system;
					}
					if (!LazySingletonBehavior<UIManager>.Instance.DoesRaycastHitUI(UIManagerRootType.UIRoot))
					{
						this.starmap.SetHovered(starSystemNode);
					}
				}
			}
			float num3 = -Input.GetAxis("Mouse ScrollWheel");
			if (Mathf.Abs(num3) > 0.01f && !LazySingletonBehavior<UIManager>.Instance.DoesRaycastHitUI(UIManagerRootType.UIRoot))
			{
				this.zoomLevel = Mathf.Clamp01(this.zoomLevel + num3 * 1f);
				this.starmapCamera.fieldOfView = Mathf.Lerp(this.fovMin, this.fovMax, this.zoomLevel);
				this.fakeCamera.fieldOfView = this.starmapCamera.fieldOfView;
			}
			Vector3 position2 = this.starmapCamera.transform.position;
			float num4 = Mathf.Lerp(150f, 70f, this.zoomLevel);
			float num5 = Mathf.Lerp(99f, 50f, this.zoomLevel);
			position2.x = Mathf.Clamp(position2.x, -num4, num4);
			position2.y = Mathf.Clamp(position2.y, -num5, num5);
			this.starmapCamera.transform.position = position2;
		}

		// Token: 0x06008A6F RID: 35439 RVA: 0x00234CF5 File Offset: 0x00232EF5
		public void ForceClickSystem(StarmapSystemRenderer systemRenderer)
		{
			this.SetSelectedSystemRenderer(systemRenderer);
		}

		// Token: 0x06008A70 RID: 35440 RVA: 0x00234D00 File Offset: 0x00232F00
		public void ForceClickSystem(string systemID)
		{
			StarmapSystemRenderer systemRenderer = this.GetSystemRenderer(systemID);
			this.SetSelectedSystemRenderer(systemRenderer);
		}

		// Token: 0x06008A71 RID: 35441 RVA: 0x00234D1C File Offset: 0x00232F1C
		private void SetSelectedSystemRenderer(StarmapSystemRenderer systemRenderer)
		{
			if (!(systemRenderer != this.currSystem))
			{
				if (systemRenderer == null && this.starmap.CurSelected != null)
				{
					this.GetSystemRenderer(this.starmap.CurSelected).Deselected();
					this.ClearPlannedPath();
					this.starmap.StarSystemRouted.Invoke(null);
				}
				return;
			}
			if (this.currSystem != null)
			{
				this.currSystem.Deselected();
				this.currSystem = null;
			}
			if (systemRenderer != null)
			{
				this.currSystem = systemRenderer;
				this.currSystem.Selected();
				this.starmap.SetSelectedSystem(this.currSystem.system.System);
				return;
			}
			this.ClearPlannedPath();
			this.starmap.StarSystemRouted.Invoke(null);
		}

		// Token: 0x06008A72 RID: 35442 RVA: 0x00234DEC File Offset: 0x00232FEC
		private void OnDisable()
		{
			if (this._starmapRT != null)
			{
				this.starmapCamera.targetTexture = null;
				Object.DestroyImmediate(this._starmapRT);
				this._starmapRT = null;
			}
			if (this._screenMaterial != null)
			{
				Object.DestroyImmediate(this._screenMaterial);
				this._screenMaterial = null;
			}
			Shader.DisableKeyword("_STARMAP");
		}

		// Token: 0x06008A73 RID: 35443 RVA: 0x00234E50 File Offset: 0x00233050
		private void CheckStarSafeArea(StarSystemNode newSys)
		{
			Vector2 vector = this.NormalizeToViewportPoint(newSys.NormalizedPosition) - this.safeAreaCenter;
			if (Mathf.Abs(vector.x) < this.safeAreaSize.x * 0.5f && Mathf.Abs(vector.y) < this.safeAreaSize.y * 0.5f)
			{
				return;
			}
			base.StartCoroutine(this.MoveSystemToCenter(newSys));
		}

		// Token: 0x06008A74 RID: 35444 RVA: 0x00234EC0 File Offset: 0x002330C0
		private IEnumerator MoveSystemToCenter(StarSystemNode newSys)
		{
			Vector2 vector = this.NormalizeToViewportPoint(newSys.NormalizedPosition);
			Vector2 vector2 = vector - this.safeAreaCenter;
			vector2.x = Mathf.Clamp(vector2.x, -this.safeAreaSize.x * 0.5f, this.safeAreaSize.x * 0.5f);
			vector2.y = Mathf.Clamp(vector2.y, -this.safeAreaSize.y * 0.5f, this.safeAreaSize.y * 0.5f);
			vector2 += this.safeAreaCenter;
			Vector2 vector3 = new Vector2(0.5f, 0.5f) - (vector2 - vector);
			Vector3 camStart = this.starmapCamera.transform.localPosition;
			Vector3 camEnd = this.starmapCamera.ViewportToWorldPoint(new Vector3(vector3.x, vector3.y, 100f));
			camEnd.z = camStart.z;
			float t = 0f;
			float maxT = Mathf.Clamp01(vector2.magnitude / 0.4f);
			while (t < maxT)
			{
				float num = t / maxT;
				Vector3 vector4 = default(Vector3);
				vector4.x = Mathf.SmoothStep(camStart.x, camEnd.x, num);
				vector4.y = Mathf.SmoothStep(camStart.y, camEnd.y, num);
				vector4.z = camStart.z;
				this.starmapCamera.transform.localPosition = vector4;
				t += Time.deltaTime;
				yield return null;
			}
			this.starmapCamera.transform.localPosition = camEnd;
			yield break;
		}

		// Token: 0x0400553A RID: 21818
		private const float MOUSE_MOVE_PAN_THRESHOLD = 1f;

		// Token: 0x0400553B RID: 21819
		private const float PAN_NEEDED_THRESHOLD_X = 35f;

		// Token: 0x0400553C RID: 21820
		private const float PAN_NEEDED_THRESHOLD_Y = 14f;

		// Token: 0x0400553D RID: 21821
		private const float PAN_END_THRESHOLD = 1f;

		// Token: 0x0400553E RID: 21822
		private const float PAN_TARGET_Y_OFFSET = -10f;

		// Token: 0x0400553F RID: 21823
		private const float PAN_DURATION = 0.5f;

		// Token: 0x04005540 RID: 21824
		private const float PAN_MAX_SPEED = 80f;

		// Token: 0x04005541 RID: 21825
		private const float MOUSEWHEEL_ZOOM_NEEDED_THRESHHOLD = 0.01f;

		// Token: 0x04005542 RID: 21826
		public Camera starmapCamera;

		// Token: 0x04005543 RID: 21827
		private Camera fakeCamera;

		// Token: 0x04005544 RID: 21828
		public GameObject starPrototype;

		// Token: 0x04005545 RID: 21829
		public Transform starParent;

		// Token: 0x04005546 RID: 21830
		public Starmap starmap;

		// Token: 0x04005547 RID: 21831
		public Renderer starmapDisplay;

		// Token: 0x04005548 RID: 21832
		private Dictionary<GameObject, StarmapSystemRenderer> systemDictionary = new Dictionary<GameObject, StarmapSystemRenderer>();

		// Token: 0x04005549 RID: 21833
		private StarmapSystemRenderer currSystem;

		// Token: 0x0400554A RID: 21834
		private Vector3 cameraPanTarget;

		// Token: 0x0400554B RID: 21835
		private bool needsPan;

		// Token: 0x0400554C RID: 21836
		private Vector3 cameraPanVelocity = Vector3.zero;

		// Token: 0x0400554D RID: 21837
		private bool clickBeganOnMap;

		// Token: 0x0400554E RID: 21838
		public bool debug;

		// Token: 0x0400554F RID: 21839
		private SimGameState simState;

		// Token: 0x04005550 RID: 21840
		private SimGameCameraController _cameraController;

		// Token: 0x04005551 RID: 21841
		public float fovMin = 20f;

		// Token: 0x04005552 RID: 21842
		public float fovMax = 60f;

		// Token: 0x04005553 RID: 21843
		private float zoomLevel = 0.5f;

		// Token: 0x04005555 RID: 21845
		private bool allowInput = true;

		// Token: 0x04005556 RID: 21846
		public bool refreshBorders;

		// Token: 0x04005557 RID: 21847
		public Color magistracyColor;

		// Token: 0x04005558 RID: 21848
		public Color directorateColor;

		// Token: 0x04005559 RID: 21849
		public Color restorationColor;

		// Token: 0x0400555A RID: 21850
		public Color taurianColor;

		// Token: 0x0400555B RID: 21851
		public Color liaoColor;

		// Token: 0x0400555C RID: 21852
		public Color davionColor;

		// Token: 0x0400555D RID: 21853
		public Color localColor;

		// Token: 0x0400555E RID: 21854
		public Color nofactionColor;

		// Token: 0x0400555F RID: 21855
		public Color marikColor;

		// Token: 0x04005560 RID: 21856
		public Color unavailableColor;

		// Token: 0x04005561 RID: 21857
		public GameObject directorateLogo;

		// Token: 0x04005562 RID: 21858
		public GameObject restorationLogo;

		// Token: 0x04005563 RID: 21859
		public Vector2 safeAreaCenter = new Vector2(0.5f, 0.5f);

		// Token: 0x04005564 RID: 21860
		public Vector2 safeAreaSize = new Vector2(0.8f, 0.5f);

		// Token: 0x04005565 RID: 21861
		private Vector3 dragOrigin;

		// Token: 0x04005566 RID: 21862
		private Vector3? starmapOffset;

		// Token: 0x04005567 RID: 21863
		private Camera _mainCamera;

		// Token: 0x04005568 RID: 21864
		private Material _screenMaterial;

		// Token: 0x04005569 RID: 21865
		private RenderTexture _starmapRT;

		// Token: 0x0400556A RID: 21866
		private LineRenderer _plannedPath;

		// Token: 0x0400556B RID: 21867
		private LineRenderer _activePath;

		// Token: 0x0200101D RID: 4125
		private static class Uniforms
		{
			// Token: 0x0400556C RID: 21868
			internal static readonly int _MousePos = Shader.PropertyToID("_MousePos");
		}
	}
}
