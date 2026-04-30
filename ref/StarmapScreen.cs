using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace BattleTech
{
	// Token: 0x0200101F RID: 4127
	public class StarmapScreen : MonoBehaviour
	{
		// Token: 0x1700180D RID: 6157
		// (get) Token: 0x06008A7D RID: 35453 RVA: 0x002351AE File Offset: 0x002333AE
		private Camera mainCamera
		{
			get
			{
				if (this._mainCamera == null)
				{
					this._mainCamera = Camera.main;
				}
				return this._mainCamera;
			}
		}

		// Token: 0x1700180E RID: 6158
		// (get) Token: 0x06008A7E RID: 35454 RVA: 0x002351D0 File Offset: 0x002333D0
		private RenderTexture starRT
		{
			get
			{
				if (this._starRT == null || !this._starRT.IsCreated() || this._starRT.width != this.width || this._starRT.height != this.height)
				{
					Object.DestroyImmediate(this._starRT);
					this._starRT = new RenderTexture(this.width, this.height, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear)
					{
						name = "Star RT",
						wrapMode = TextureWrapMode.Clamp
					};
					this._starRT.Create();
				}
				return this._starRT;
			}
		}

		// Token: 0x1700180F RID: 6159
		// (get) Token: 0x06008A7F RID: 35455 RVA: 0x00235268 File Offset: 0x00233468
		private RenderTexture starBackgroundRT
		{
			get
			{
				if (this._starBackgroundRT == null || !this._starBackgroundRT.IsCreated() || this._starBackgroundRT.width != this.width || this._starBackgroundRT.height != this.height)
				{
					Object.DestroyImmediate(this._starBackgroundRT);
					this._starBackgroundRT = new RenderTexture(this.width, this.height, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear)
					{
						name = "Star Background RT",
						wrapMode = TextureWrapMode.Clamp
					};
					this._starBackgroundRT.Create();
					StarmapScreen.isDirty = true;
				}
				return this._starBackgroundRT;
			}
		}

		// Token: 0x17001810 RID: 6160
		// (get) Token: 0x06008A80 RID: 35456 RVA: 0x00235308 File Offset: 0x00233508
		private LineRenderer plannedPath
		{
			get
			{
				if (this._plannedPath == null)
				{
					GameObject gameObject = new GameObject("Planned Path");
					this._plannedPath = gameObject.AddComponent<LineRenderer>();
					this._plannedPath.startWidth = 0.02f;
					this._plannedPath.endWidth = 0.02f;
					this._plannedPath.textureMode = LineTextureMode.Tile;
					this._plannedPath.sharedMaterial = Resources.Load<Material>("Materials/starmapPlanning");
					this._plannedPath.numCornerVertices = 4;
					this._plannedPath.numCapVertices = 1;
				}
				return this._plannedPath;
			}
		}

		// Token: 0x17001811 RID: 6161
		// (get) Token: 0x06008A81 RID: 35457 RVA: 0x0023539C File Offset: 0x0023359C
		private LineRenderer activePath
		{
			get
			{
				if (this._activePath == null)
				{
					GameObject gameObject = new GameObject("Planned Path");
					this._activePath = gameObject.AddComponent<LineRenderer>();
					this._activePath.startWidth = 0.02f;
					this._activePath.endWidth = 0.02f;
					this._activePath.textureMode = LineTextureMode.Tile;
					this._activePath.sharedMaterial = Resources.Load<Material>("Materials/starmapActive");
					this._activePath.numCornerVertices = 4;
					this._activePath.numCapVertices = 1;
				}
				return this._activePath;
			}
		}

		// Token: 0x17001812 RID: 6162
		// (get) Token: 0x06008A82 RID: 35458 RVA: 0x00235430 File Offset: 0x00233630
		private Material screenMaterial
		{
			get
			{
				if (this._screenMaterial == null)
				{
					this._screenMaterial = new Material(Shader.Find("Hidden/BT-StarmapScreen"));
					base.gameObject.GetComponent<Renderer>().sharedMaterial = this._screenMaterial;
				}
				return this._screenMaterial;
			}
		}

		// Token: 0x17001813 RID: 6163
		// (get) Token: 0x06008A83 RID: 35459 RVA: 0x0023547C File Offset: 0x0023367C
		private Material starMaterial
		{
			get
			{
				if (this._starMaterial == null)
				{
					this._starMaterial = new Material(Shader.Find("Hidden/BT-StarmapStars"));
					this._starMaterial.EnableKeyword("_FULL");
				}
				return this._starMaterial;
			}
		}

		// Token: 0x17001814 RID: 6164
		// (get) Token: 0x06008A84 RID: 35460 RVA: 0x002354B7 File Offset: 0x002336B7
		private Material reticleMaterial
		{
			get
			{
				if (this._reticleMaterial == null)
				{
					this._reticleMaterial = new Material(Shader.Find("Hidden/BT-StarmapStars"));
				}
				return this._reticleMaterial;
			}
		}

		// Token: 0x17001815 RID: 6165
		// (get) Token: 0x06008A85 RID: 35461 RVA: 0x002354E2 File Offset: 0x002336E2
		private Mesh starMesh
		{
			get
			{
				if (this._starMesh == null)
				{
					this.CreateMesh();
				}
				return this._starMesh;
			}
		}

		// Token: 0x17001816 RID: 6166
		// (get) Token: 0x06008A86 RID: 35462 RVA: 0x002354FE File Offset: 0x002336FE
		private Material jumpPreviewMaterial
		{
			get
			{
				if (this._jumpPreviewMaterial == null)
				{
					this._jumpPreviewMaterial = Resources.Load<Material>("Materials/starmapPreview");
				}
				return this._jumpPreviewMaterial;
			}
		}

		// Token: 0x06008A87 RID: 35463 RVA: 0x00235524 File Offset: 0x00233724
		private LineRenderer CreateJumpPath()
		{
			LineRenderer lineRenderer = new GameObject("Jump Path")
			{
				layer = LayerMask.NameToLayer("VFXOnly")
			}.AddComponent<LineRenderer>();
			lineRenderer.startWidth = 0.005f;
			lineRenderer.endWidth = 0.005f;
			lineRenderer.textureMode = LineTextureMode.Tile;
			lineRenderer.sharedMaterial = this.jumpPreviewMaterial;
			lineRenderer.numCornerVertices = 4;
			lineRenderer.numCapVertices = 1;
			return lineRenderer;
		}

		// Token: 0x06008A88 RID: 35464 RVA: 0x00235588 File Offset: 0x00233788
		private void GenerateJumpPaths()
		{
			if (this.starmap.VisibleSystemPositions == null || this.starmap.VisisbleSystem == null)
			{
				return;
			}
			List<StarmapScreen.JumpPath> list = new List<StarmapScreen.JumpPath>();
			float num = 1f - this.bufferPercent;
			foreach (StarSystemNode starSystemNode in this.starmap.VisisbleSystem)
			{
				Vector3 vector = starSystemNode.NormalizedPosition;
				vector.x -= 0.5f;
				vector.y -= 0.5f;
				vector *= num;
				vector = base.transform.TransformPoint(vector);
				foreach (StarSystemNode starSystemNode2 in starSystemNode.AdjacentSystems)
				{
					Vector3 vector2 = starSystemNode2.NormalizedPosition;
					vector2.x -= 0.5f;
					vector2.y -= 0.5f;
					vector2 *= num;
					vector2 = base.transform.TransformPoint(vector2);
					StarmapScreen.JumpPath jumpPath = new StarmapScreen.JumpPath(vector, vector2);
					bool flag = true;
					for (int i = 0; i < list.Count; i++)
					{
						if (list[i].SamePath(jumpPath))
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						list.Add(jumpPath);
					}
				}
			}
			foreach (StarmapScreen.JumpPath jumpPath2 in list)
			{
				Vector3[] array = new Vector3[] { jumpPath2.start, jumpPath2.end };
				LineRenderer lineRenderer = this.CreateJumpPath();
				lineRenderer.positionCount = 2;
				lineRenderer.SetPositions(array);
				this.jumpPaths.Add(lineRenderer);
			}
			list.Clear();
		}

		// Token: 0x06008A89 RID: 35465 RVA: 0x002357C4 File Offset: 0x002339C4
		private void CleanupJumpPaths()
		{
			foreach (LineRenderer lineRenderer in this.jumpPaths)
			{
				Object.DestroyImmediate(lineRenderer.gameObject);
			}
			this.jumpPaths.Clear();
		}

		// Token: 0x06008A8A RID: 35466 RVA: 0x00235824 File Offset: 0x00233A24
		private void CreateMesh()
		{
			this._starMesh = new Mesh();
			Vector3[] array = new Vector3[170];
			int[] array2 = new int[170];
			for (int i = 0; i < 170; i++)
			{
				array[i] = new Vector3(0f, 0f, 0f);
				array2[i] = i;
			}
			this.starMesh.vertices = array;
			this.starMesh.SetIndices(array2, MeshTopology.Points, 0);
			this.starMesh.UploadMeshData(true);
		}

		// Token: 0x06008A8B RID: 35467 RVA: 0x002358A8 File Offset: 0x00233AA8
		private void GenerateStarMesh(Starmap starmap)
		{
			List<StarSystemNode> visisbleSystem = starmap.VisisbleSystem;
			Vector3[] array = new Vector3[visisbleSystem.Count];
			int[] array2 = new int[visisbleSystem.Count];
			for (int i = 0; i < visisbleSystem.Count; i++)
			{
				Vector2 normalizedPosition = visisbleSystem[i].NormalizedPosition;
				array[i] = new Vector3(normalizedPosition.x, normalizedPosition.y, 0f);
				array2[i] = i;
			}
			this.starMesh.vertices = array;
			this.starMesh.SetIndices(array2, MeshTopology.Points, 0);
			this.starMesh.UploadMeshData(false);
		}

		// Token: 0x06008A8C RID: 35468 RVA: 0x0023593C File Offset: 0x00233B3C
		private void RenderStarmap(Starmap starmap, Material screenMaterial, RenderTexture starRT, float size, Texture2D starTexture, float aspectRatio)
		{
			if (this.starmapCommandBuffer == null)
			{
				this.starmapCommandBuffer = new CommandBuffer();
				this.starmapCommandBuffer.name = "Starmesh Command Buffer";
			}
			if (starmap.VisibleSystemPositions == null || starmap.VisibleSystemPositions.Count == 0)
			{
				return;
			}
			if (this.jumpPaths.Count == 0)
			{
				this.GenerateJumpPaths();
			}
			this.starMaterial.SetTexture(StarmapScreen.Uniforms._MainTex, starTexture);
			this.starMaterial.SetTexture(StarmapScreen.Uniforms._FuelTex, this.fuelTex);
			this.starMaterial.SetTexture(StarmapScreen.Uniforms._InterestTex, this.interestTex);
			this.starMaterial.SetFloat(StarmapScreen.Uniforms._Buffer, 1f - this.bufferPercent);
			this.starMaterial.SetVectorArray(StarmapScreen.Uniforms._StarPositions, starmap.VisibleSystemPositions);
			this.starMaterial.SetVectorArray(StarmapScreen.Uniforms._StarProps, starmap.VisibleSystemProperties);
			this.starMaterial.SetInt(StarmapScreen.Uniforms._NumStars, starmap.VisibleSystemPositions.Count);
			this.starMaterial.SetVector(StarmapScreen.Uniforms._MapParams, new Vector4(starmap.MapSize.x, starmap.MapSize.y, starmap.MapOffset.x, starmap.MapOffset.y));
			this.starmapCommandBuffer.Clear();
			this.starmapCommandBuffer.SetGlobalFloat(StarmapScreen.Uniforms._Size, size);
			this.starmapCommandBuffer.SetGlobalFloat(StarmapScreen.Uniforms._AspectRatio, aspectRatio);
			if (StarmapScreen.isDirty)
			{
				this.starmapCommandBuffer.Blit(Texture2D.blackTexture, this.starBackgroundRT, this.starMaterial, 1);
				StarmapScreen.isDirty = false;
			}
			this.starmapCommandBuffer.Blit(this.starBackgroundRT, starRT, this.starMaterial, 2);
			this.starmapCommandBuffer.DrawMesh(this.starMesh, Matrix4x4.identity, this.starMaterial, 0, 0);
			StarSystemNode curPlanet = starmap.CurPlanet;
			if (curPlanet != null)
			{
				this.reticlePos[0] = curPlanet.NormalizedPosition;
				int num = 1;
				if (starmap.CurSelected != null)
				{
					this.reticlePos[1] = starmap.CurSelected.NormalizedPosition;
					num = 2;
				}
				this.reticleMaterial.SetTexture(StarmapScreen.Uniforms._MainTex, this.recticleTex);
				this.reticleMaterial.SetFloat(StarmapScreen.Uniforms._Buffer, 1f - this.bufferPercent);
				this.reticleMaterial.SetVectorArray(StarmapScreen.Uniforms._StarPositions, this.reticlePos);
				this.reticleMaterial.SetInt(StarmapScreen.Uniforms._NumStars, num);
				this.starmapCommandBuffer.DrawMesh(this.starMesh, Matrix4x4.identity, this.reticleMaterial, 0, 0);
			}
			Graphics.ExecuteCommandBuffer(this.starmapCommandBuffer);
			screenMaterial.SetTexture(StarmapScreen.Uniforms._MainTex, starRT);
			screenMaterial.SetFloat(StarmapScreen.Uniforms._Emissive, this.emissive);
		}

		// Token: 0x06008A8D RID: 35469 RVA: 0x00235BFC File Offset: 0x00233DFC
		private void RefreshStarmap()
		{
			if (this.starmap == null)
			{
				return;
			}
			Vector3 localScale = base.transform.localScale;
			float num = localScale.x / localScale.y;
			this.RenderStarmap(this.starmap, this.screenMaterial, this.starRT, this.size, this.starTex, num);
		}

		// Token: 0x06008A8E RID: 35470 RVA: 0x00235C57 File Offset: 0x00233E57
		public void SetCamera(Camera c)
		{
			this.curCamera = c;
		}

		// Token: 0x06008A8F RID: 35471 RVA: 0x00235C60 File Offset: 0x00233E60
		public void AllowInput(bool canInput)
		{
			this.testInput = canInput;
		}

		// Token: 0x06008A90 RID: 35472 RVA: 0x00235C69 File Offset: 0x00233E69
		public void UpdatePlannedPath()
		{
			this.UpdatedPath(this.starmap.PotentialPath, this.plannedPath);
		}

		// Token: 0x06008A91 RID: 35473 RVA: 0x00235C82 File Offset: 0x00233E82
		public void UpdateActivePath()
		{
			this.UpdatedPath(this.starmap.ActivePath, this.activePath);
		}

		// Token: 0x06008A92 RID: 35474 RVA: 0x00235C9C File Offset: 0x00233E9C
		private void UpdatedPath(List<StarSystemNode> pathList, LineRenderer renderer)
		{
			if (pathList == null)
			{
				return;
			}
			Vector3[] array = new Vector3[pathList.Count];
			float num = 1f - this.bufferPercent;
			for (int i = 0; i < array.Length; i++)
			{
				Vector3 vector = pathList[i].NormalizedPosition;
				vector.x -= 0.5f;
				vector.y -= 0.5f;
				vector *= num;
				array[i] = base.transform.TransformPoint(vector);
			}
			renderer.positionCount = array.Length;
			renderer.SetPositions(array);
		}

		// Token: 0x06008A93 RID: 35475 RVA: 0x00235D31 File Offset: 0x00233F31
		public void ClearPlannedPath()
		{
			this.ClearPath(this.plannedPath);
		}

		// Token: 0x06008A94 RID: 35476 RVA: 0x00235D3F File Offset: 0x00233F3F
		public void ClearActivePath()
		{
			this.ClearPath(this.activePath);
		}

		// Token: 0x06008A95 RID: 35477 RVA: 0x00234467 File Offset: 0x00232667
		private void ClearPath(LineRenderer renderer)
		{
			renderer.positionCount = 0;
		}

		// Token: 0x06008A96 RID: 35478 RVA: 0x00235D50 File Offset: 0x00233F50
		private void Update()
		{
			if (!this.testInput)
			{
				return;
			}
			if (Input.GetMouseButtonUp(0))
			{
				RaycastHit raycastHit;
				if (!Physics.Raycast(this.curCamera.ScreenPointToRay(Input.mousePosition), ref raycastHit))
				{
					return;
				}
				if (raycastHit.transform != base.transform)
				{
					return;
				}
				Vector3 localScale = base.transform.localScale;
				float num = localScale.y / localScale.x;
				float num2 = 1f - this.bufferPercent;
				Vector2 vector = new Vector2(raycastHit.textureCoord.x, raycastHit.textureCoord.y);
				float num3 = this.size * num;
				Rect rect = new Rect(vector.x - num3 * 0.5f, vector.y - this.size * 0.5f, num3, this.size);
				StarSystemNode locationByUV = this.starmap.GetLocationByUV(vector, rect, num2);
				if (locationByUV != null && locationByUV != this.starmap.CurSelected && locationByUV != this.starmap.CurPlanet)
				{
					this.starmap.SetSelectedSystem(locationByUV);
				}
			}
		}

		// Token: 0x06008A97 RID: 35479 RVA: 0x00235E65 File Offset: 0x00234065
		private void OnWillRenderObject()
		{
			if (Camera.current == this.mainCamera)
			{
				this.RefreshStarmap();
			}
		}

		// Token: 0x06008A98 RID: 35480 RVA: 0x00235E80 File Offset: 0x00234080
		private void OnDisable()
		{
			if (this._starMaterial != null)
			{
				Object.DestroyImmediate(this._starMaterial);
				this._starMaterial = null;
			}
			if (this._reticleMaterial != null)
			{
				Object.DestroyImmediate(this._reticleMaterial);
				this._reticleMaterial = null;
			}
			if (this._screenMaterial != null)
			{
				Object.DestroyImmediate(this._screenMaterial);
				this._screenMaterial = null;
			}
			if (this._starMesh != null)
			{
				Object.DestroyImmediate(this._starMesh);
				this._starMesh = null;
			}
			if (this._plannedPath != null)
			{
				Object.DestroyImmediate(this._plannedPath.gameObject);
				this._plannedPath = null;
			}
			if (this._activePath != null)
			{
				Object.DestroyImmediate(this._activePath.gameObject);
				this._activePath = null;
			}
			if (this._starRT != null && this._starRT.IsCreated())
			{
				Object.DestroyImmediate(this._starRT);
				this._starRT = null;
			}
			if (this._starBackgroundRT != null && this._starBackgroundRT.IsCreated())
			{
				Object.DestroyImmediate(this._starBackgroundRT);
				this._starBackgroundRT = null;
			}
		}

		// Token: 0x04005575 RID: 21877
		public Starmap starmap;

		// Token: 0x04005576 RID: 21878
		public static bool isDirty = true;

		// Token: 0x04005577 RID: 21879
		private Camera _mainCamera;

		// Token: 0x04005578 RID: 21880
		private RenderTexture _starRT;

		// Token: 0x04005579 RID: 21881
		private RenderTexture _starBackgroundRT;

		// Token: 0x0400557A RID: 21882
		private int width = 2048;

		// Token: 0x0400557B RID: 21883
		private int height = 1024;

		// Token: 0x0400557C RID: 21884
		[Range(0.01f, 0.1f)]
		public float size = 0.05f;

		// Token: 0x0400557D RID: 21885
		[Range(0f, 1f)]
		public float bufferPercent = 0.05f;

		// Token: 0x0400557E RID: 21886
		public float emissive = 1f;

		// Token: 0x0400557F RID: 21887
		public Texture2D starTex;

		// Token: 0x04005580 RID: 21888
		public Texture2D recticleTex;

		// Token: 0x04005581 RID: 21889
		public Texture2D fuelTex;

		// Token: 0x04005582 RID: 21890
		public Texture2D interestTex;

		// Token: 0x04005583 RID: 21891
		public Texture2D backgroundTex;

		// Token: 0x04005584 RID: 21892
		public Texture2D argoTex;

		// Token: 0x04005585 RID: 21893
		private const int maxStars = 170;

		// Token: 0x04005586 RID: 21894
		private bool testInput;

		// Token: 0x04005587 RID: 21895
		private Vector4[] reticlePos = new Vector4[2];

		// Token: 0x04005588 RID: 21896
		private Camera curCamera;

		// Token: 0x04005589 RID: 21897
		private LineRenderer _plannedPath;

		// Token: 0x0400558A RID: 21898
		private LineRenderer _activePath;

		// Token: 0x0400558B RID: 21899
		private List<LineRenderer> jumpPaths = new List<LineRenderer>();

		// Token: 0x0400558C RID: 21900
		private Material _screenMaterial;

		// Token: 0x0400558D RID: 21901
		private Material _starMaterial;

		// Token: 0x0400558E RID: 21902
		private Material _reticleMaterial;

		// Token: 0x0400558F RID: 21903
		private Mesh _starMesh;

		// Token: 0x04005590 RID: 21904
		private Material _jumpPreviewMaterial;

		// Token: 0x04005591 RID: 21905
		private CommandBuffer starmapCommandBuffer;

		// Token: 0x02001020 RID: 4128
		private static class Uniforms
		{
			// Token: 0x04005592 RID: 21906
			internal static readonly int _MainTex = Shader.PropertyToID("_MainTex");

			// Token: 0x04005593 RID: 21907
			internal static readonly int _Buffer = Shader.PropertyToID("_Buffer");

			// Token: 0x04005594 RID: 21908
			internal static readonly int _StarPositions = Shader.PropertyToID("_StarPositions");

			// Token: 0x04005595 RID: 21909
			internal static readonly int _StarProps = Shader.PropertyToID("_StarProps");

			// Token: 0x04005596 RID: 21910
			internal static readonly int _NumStars = Shader.PropertyToID("_NumStars");

			// Token: 0x04005597 RID: 21911
			internal static readonly int _Size = Shader.PropertyToID("_Size");

			// Token: 0x04005598 RID: 21912
			internal static readonly int _AspectRatio = Shader.PropertyToID("_AspectRatio");

			// Token: 0x04005599 RID: 21913
			internal static readonly int _Emissive = Shader.PropertyToID("_Emissive");

			// Token: 0x0400559A RID: 21914
			internal static readonly int _FuelTex = Shader.PropertyToID("_FuelTex");

			// Token: 0x0400559B RID: 21915
			internal static readonly int _InterestTex = Shader.PropertyToID("_InterestTex");

			// Token: 0x0400559C RID: 21916
			internal static readonly int _MapParams = Shader.PropertyToID("_MapParams");

			// Token: 0x0400559D RID: 21917
			internal static readonly int _CurrentSystem = Shader.PropertyToID("_CurrentSystem");

			// Token: 0x0400559E RID: 21918
			internal static readonly int _StarBackground = Shader.PropertyToID("_StarBackground");
		}

		// Token: 0x02001021 RID: 4129
		internal class JumpPath
		{
			// Token: 0x17001817 RID: 6167
			// (get) Token: 0x06008A9C RID: 35484 RVA: 0x002360F0 File Offset: 0x002342F0
			// (set) Token: 0x06008A9D RID: 35485 RVA: 0x002360F8 File Offset: 0x002342F8
			internal Vector3 start { get; private set; }

			// Token: 0x17001818 RID: 6168
			// (get) Token: 0x06008A9E RID: 35486 RVA: 0x00236101 File Offset: 0x00234301
			// (set) Token: 0x06008A9F RID: 35487 RVA: 0x00236109 File Offset: 0x00234309
			internal Vector3 end { get; private set; }

			// Token: 0x06008AA0 RID: 35488 RVA: 0x00236112 File Offset: 0x00234312
			internal JumpPath(Vector3 s, Vector3 e)
			{
				this.start = s;
				this.end = e;
			}

			// Token: 0x06008AA1 RID: 35489 RVA: 0x00236128 File Offset: 0x00234328
			private static bool SamePoint(Vector3 v1, Vector3 v2)
			{
				return Vector3.Distance(v1, v2) <= 0.01f;
			}

			// Token: 0x06008AA2 RID: 35490 RVA: 0x0023613C File Offset: 0x0023433C
			public bool SamePath(StarmapScreen.JumpPath other)
			{
				return (StarmapScreen.JumpPath.SamePoint(this.start, other.start) && StarmapScreen.JumpPath.SamePoint(this.end, other.end)) || (StarmapScreen.JumpPath.SamePoint(this.end, other.start) && StarmapScreen.JumpPath.SamePoint(this.start, other.end));
			}
		}
	}
}
