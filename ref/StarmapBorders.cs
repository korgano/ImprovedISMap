using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace BattleTech
{
	// Token: 0x0200101A RID: 4122
	public class StarmapBorders : MonoBehaviour
	{
		// Token: 0x170017FF RID: 6143
		// (get) Token: 0x06008A3E RID: 35390 RVA: 0x00232F36 File Offset: 0x00231136
		private Material generateMaterial
		{
			get
			{
				if (this._generateMaterial == null)
				{
					this._generateMaterial = new Material(Shader.Find("Hidden/BT-StarmapBorderGenerate"));
				}
				return this._generateMaterial;
			}
		}

		// Token: 0x17001800 RID: 6144
		// (get) Token: 0x06008A3F RID: 35391 RVA: 0x00232F61 File Offset: 0x00231161
		private Material renderMaterial
		{
			get
			{
				if (this._renderMaterial == null)
				{
					this._renderMaterial = new Material(Shader.Find("Hidden/BT-StarmapBorderDisplay"));
				}
				return this._renderMaterial;
			}
		}

		// Token: 0x17001801 RID: 6145
		// (get) Token: 0x06008A40 RID: 35392 RVA: 0x00232F8C File Offset: 0x0023118C
		private Material blurMaterial
		{
			get
			{
				if (this._blurMaterial == null)
				{
					this._blurMaterial = new Material(Shader.Find("Hidden/BT-Blur"));
				}
				return this._blurMaterial;
			}
		}

		// Token: 0x17001802 RID: 6146
		// (get) Token: 0x06008A41 RID: 35393 RVA: 0x00232FB7 File Offset: 0x002311B7
		private Material coneMaterial
		{
			get
			{
				if (this._coneMaterial == null)
				{
					this._coneMaterial = new Material(SystemInfo.usesReversedZBuffer ? Shader.Find("Unlit/Color") : Shader.Find("Hidden/BT-StarmapConeZ"));
				}
				return this._coneMaterial;
			}
		}

		// Token: 0x06008A42 RID: 35394 RVA: 0x00232FF8 File Offset: 0x002311F8
		private bool CheckRenderTexture()
		{
			if (this.borderMaps == null || !this.borderMaps.IsCreated() || this.borderMaps.width != StarmapBorders.size * 2 || this.borderMaps.height != StarmapBorders.size)
			{
				Object.DestroyImmediate(this.borderMaps);
				this.borderMaps = new RenderTexture(StarmapBorders.size * 2, StarmapBorders.size, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
				{
					filterMode = FilterMode.Bilinear,
					wrapMode = TextureWrapMode.Clamp,
					useMipMap = true,
					autoGenerateMips = false
				};
				Object.DestroyImmediate(this.travelZone);
				this.travelZone = new RenderTexture(StarmapBorders.size * 2, StarmapBorders.size, 0, RenderTextureFormat.R8, RenderTextureReadWrite.Linear)
				{
					filterMode = FilterMode.Bilinear,
					wrapMode = TextureWrapMode.Clamp,
					useMipMap = true,
					autoGenerateMips = false
				};
				return true;
			}
			return false;
		}

		// Token: 0x06008A43 RID: 35395 RVA: 0x002330D0 File Offset: 0x002312D0
		private void GenerateColorList()
		{
			this.planetColors.Clear();
			for (int i = 0; i < this.mainStarmap.starmap.VisisbleSystem.Count; i++)
			{
				Color mapBorderColor = this.mainStarmap.starmap.VisisbleSystem[i].System.Def.OwnerValue.GetMapBorderColor();
				this.planetColors.Add(new Vector4(mapBorderColor.linear.r, mapBorderColor.linear.g, mapBorderColor.linear.b, 1f));
			}
		}

		// Token: 0x06008A44 RID: 35396 RVA: 0x0023316C File Offset: 0x0023136C
		private void RenderBorders()
		{
			if (this.mainStarmap.starmap.VisisbleSystem == null)
			{
				return;
			}
			this.GenerateColorList();
			this.generateMaterial.SetVectorArray(StarmapBorders.Uniforms._StarPositions, this.mainStarmap.starmap.VisibleSystemPositions);
			this.generateMaterial.SetVectorArray(StarmapBorders.Uniforms._StarProps, this.planetColors);
			this.generateMaterial.SetInt(StarmapBorders.Uniforms._NumStars, this.mainStarmap.starmap.VisibleSystemPositions.Count);
			if (this.buf == null)
			{
				this.buf = new CommandBuffer();
				this.buf.name = "Border Generate Buffer";
			}
			this.buf.Clear();
			this.buf.GetTemporaryRT(StarmapBorders.Uniforms._rtTemp, this.borderMaps.width * 2, this.borderMaps.height * 2, 24, FilterMode.Bilinear, this.borderMaps.format, RenderTextureReadWrite.Linear);
			this.buf.GetTemporaryRT(StarmapBorders.Uniforms._rtTemp2, this.borderMaps.width * 2, this.borderMaps.height * 2, 0, FilterMode.Bilinear, this.borderMaps.format, RenderTextureReadWrite.Linear);
			this.buf.GetTemporaryRT(StarmapBorders.Uniforms._rtTemp3, this.borderMaps.width, this.borderMaps.height, 0, FilterMode.Bilinear, this.borderMaps.format, RenderTextureReadWrite.Linear);
			this.buf.GetTemporaryRT(StarmapBorders.Uniforms._rtTemp4, this.borderMaps.width, this.borderMaps.height, 0, FilterMode.Bilinear, this.borderMaps.format, RenderTextureReadWrite.Linear);
			this.buf.SetRenderTarget(StarmapBorders.Uniforms._rtTemp);
			this.buf.ClearRenderTarget(true, true, new Color(0f, 0f, 0f, 0f), 1f);
			Matrix4x4 matrix4x = Matrix4x4.Ortho(-0.5f, 1.5f, -0.5f, 1.5f, this.orthoStart, this.orthoEnd);
			if (SystemInfo.usesReversedZBuffer)
			{
				matrix4x[2, 0] = -matrix4x[2, 0];
				matrix4x[2, 1] = -matrix4x[2, 1];
				matrix4x[2, 2] = -matrix4x[2, 2];
				matrix4x[2, 3] = -matrix4x[2, 3];
			}
			this.buf.SetViewProjectionMatrices(Matrix4x4.TRS(new Vector3(0f, 0f, this.cameraOffset), Quaternion.identity, Vector3.one), matrix4x);
			MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
			for (int i = 0; i < this.mainStarmap.starmap.VisibleSystemPositions.Count; i++)
			{
				materialPropertyBlock.Clear();
				materialPropertyBlock.SetColor("_Color", new Color(this.planetColors[i].x, this.planetColors[i].y, this.planetColors[i].z).gamma);
				Matrix4x4 matrix4x2 = Matrix4x4.TRS(this.mainStarmap.starmap.VisibleSystemPositions[i], Quaternion.identity, new Vector3(1f, 2f, 1f));
				this.buf.DrawMesh(this.coneMesh, matrix4x2, this.coneMaterial, 0, 0, materialPropertyBlock);
			}
			this.buf.Blit(StarmapBorders.Uniforms._rtTemp, StarmapBorders.Uniforms._rtTemp2, this.generateMaterial, 1);
			this.buf.Blit(StarmapBorders.Uniforms._rtTemp2, StarmapBorders.Uniforms._rtTemp3);
			this.buf.SetGlobalFloat("_Size", 1f);
			this.buf.SetGlobalVector("_Direction", new Vector2(1f, 0f));
			this.buf.Blit(StarmapBorders.Uniforms._rtTemp3, StarmapBorders.Uniforms._rtTemp4, this.blurMaterial, 0);
			this.buf.SetGlobalVector("_Direction", new Vector2(0f, 1f));
			this.buf.Blit(StarmapBorders.Uniforms._rtTemp4, StarmapBorders.Uniforms._rtTemp3, this.blurMaterial, 0);
			this.buf.Blit(StarmapBorders.Uniforms._rtTemp3, this.borderMaps);
			bool flag = true;
			List<float> list = new List<float>();
			for (int j = 0; j < this.mainStarmap.starmap.VisibleSystemPositions.Count; j++)
			{
				bool flag2 = this.mainStarmap.starmap.CanTravelToNode(this.mainStarmap.starmap.VisisbleSystem[j].System.ID);
				flag = flag && flag2;
				list.Add(flag2 ? 1f : 0f);
			}
			if (flag)
			{
				this.buf.SetRenderTarget(this.travelZone);
				this.buf.ClearRenderTarget(false, true, Color.white);
			}
			else
			{
				this.buf.SetRenderTarget(StarmapBorders.Uniforms._rtTemp);
				this.buf.ClearRenderTarget(false, true, Color.black);
				this.buf.SetViewProjectionMatrices(Matrix4x4.TRS(new Vector3(0f, 0f, -1f), Quaternion.identity, Vector3.one), matrix4x);
				materialPropertyBlock.Clear();
				materialPropertyBlock.SetColor("_Color", Color.white);
				for (int k = 0; k < list.Count; k++)
				{
					if (list[k] == 1f)
					{
						Matrix4x4 matrix4x3 = Matrix4x4.TRS(this.mainStarmap.starmap.VisibleSystemPositions[k], Quaternion.identity, new Vector3(1f, 2f, 1f));
						this.buf.DrawMesh(this.coneMesh, matrix4x3, this.coneMaterial, 0, 0, materialPropertyBlock);
					}
				}
				this.buf.Blit(StarmapBorders.Uniforms._rtTemp, StarmapBorders.Uniforms._rtTemp2, this.generateMaterial, 1);
				this.buf.Blit(StarmapBorders.Uniforms._rtTemp2, StarmapBorders.Uniforms._rtTemp3);
				this.buf.SetGlobalFloat("_Size", 1f);
				this.buf.SetGlobalVector("_Direction", new Vector2(1f, 0f));
				this.buf.Blit(StarmapBorders.Uniforms._rtTemp3, StarmapBorders.Uniforms._rtTemp4, this.blurMaterial, 0);
				this.buf.SetGlobalVector("_Direction", new Vector2(0f, 1f));
				this.buf.Blit(StarmapBorders.Uniforms._rtTemp4, StarmapBorders.Uniforms._rtTemp3, this.blurMaterial, 0);
				this.buf.Blit(StarmapBorders.Uniforms._rtTemp3, this.travelZone);
			}
			this.buf.ReleaseTemporaryRT(StarmapBorders.Uniforms._rtTemp);
			this.buf.ReleaseTemporaryRT(StarmapBorders.Uniforms._rtTemp2);
			this.buf.ReleaseTemporaryRT(StarmapBorders.Uniforms._rtTemp3);
			this.buf.ReleaseTemporaryRT(StarmapBorders.Uniforms._rtTemp4);
			Graphics.ExecuteCommandBuffer(this.buf);
			this.borderMaps.GenerateMips();
			this.renderMaterial.SetTexture(StarmapBorders.Uniforms._TravelTex, this.travelZone);
			this.renderMaterial.SetTexture(StarmapBorders.Uniforms._MainTex, this.borderMaps);
			this.renderMaterial.SetTexture(StarmapBorders.Uniforms._GridTex, this.gridTex);
			this.renderMaterial.SetTexture(StarmapBorders.Uniforms._PlusTex, this.plusTex);
			this.renderMaterial.SetColor(StarmapBorders.Uniforms._TravelColor, new Color(this.travelColor.r, this.travelColor.g, this.travelColor.b, this.travelIntensity));
		}

		// Token: 0x06008A45 RID: 35397 RVA: 0x00233952 File Offset: 0x00231B52
		private void OnEnable()
		{
			this.borderRenderer.sharedMaterial = this.renderMaterial;
		}

		// Token: 0x06008A46 RID: 35398 RVA: 0x00233965 File Offset: 0x00231B65
		private void OnValidate()
		{
			this.renderMaterial.SetColor(StarmapBorders.Uniforms._TravelColor, new Color(this.travelColor.r, this.travelColor.g, this.travelColor.b, this.travelIntensity));
		}

		// Token: 0x06008A47 RID: 35399 RVA: 0x002339A4 File Offset: 0x00231BA4
		private void OnWillRenderObject()
		{
			if (this.mainStarmap.starmap == null)
			{
				return;
			}
			if (this.continuous || this.CheckRenderTexture() || this.mainStarmap.refreshBorders)
			{
				this.RenderBorders();
				this.mainStarmap.refreshBorders = false;
			}
		}

		// Token: 0x06008A48 RID: 35400 RVA: 0x002339F4 File Offset: 0x00231BF4
		private void OnDestroy()
		{
			if (this.borderMaps != null)
			{
				Object.DestroyImmediate(this.borderMaps);
				this.borderMaps = null;
			}
			if (this._generateMaterial != null)
			{
				Object.DestroyImmediate(this._generateMaterial);
				this._generateMaterial = null;
			}
			if (this._renderMaterial != null)
			{
				Object.DestroyImmediate(this._renderMaterial);
				this._renderMaterial = null;
			}
			if (this._blurMaterial != null)
			{
				Object.DestroyImmediate(this._blurMaterial);
				this._blurMaterial = null;
			}
			if (this._coneMaterial != null)
			{
				Object.DestroyImmediate(this._coneMaterial);
				this._coneMaterial = null;
			}
		}

		// Token: 0x04005519 RID: 21785
		private static readonly int size = 512;

		// Token: 0x0400551A RID: 21786
		public bool continuous;

		// Token: 0x0400551B RID: 21787
		public MeshRenderer borderRenderer;

		// Token: 0x0400551C RID: 21788
		public StarmapRenderer mainStarmap;

		// Token: 0x0400551D RID: 21789
		public Texture2D gridTex;

		// Token: 0x0400551E RID: 21790
		public Texture2D plusTex;

		// Token: 0x0400551F RID: 21791
		public float cameraOffset = -1f;

		// Token: 0x04005520 RID: 21792
		public float orthoStart = 0.01f;

		// Token: 0x04005521 RID: 21793
		public float orthoEnd = 10f;

		// Token: 0x04005522 RID: 21794
		public Mesh coneMesh;

		// Token: 0x04005523 RID: 21795
		private List<Vector4> planetColors = new List<Vector4>();

		// Token: 0x04005524 RID: 21796
		private CommandBuffer buf;

		// Token: 0x04005525 RID: 21797
		private Material _generateMaterial;

		// Token: 0x04005526 RID: 21798
		private Material _renderMaterial;

		// Token: 0x04005527 RID: 21799
		private Material _blurMaterial;

		// Token: 0x04005528 RID: 21800
		private Material _coneMaterial;

		// Token: 0x04005529 RID: 21801
		private RenderTexture borderMaps;

		// Token: 0x0400552A RID: 21802
		private RenderTexture travelZone;

		// Token: 0x0400552B RID: 21803
		public Color travelColor = Color.white;

		// Token: 0x0400552C RID: 21804
		public float travelIntensity = 1f;

		// Token: 0x0200101B RID: 4123
		private static class Uniforms
		{
			// Token: 0x0400552D RID: 21805
			internal static readonly int _MainTex = Shader.PropertyToID("_MainTex");

			// Token: 0x0400552E RID: 21806
			internal static readonly int _StarPositions = Shader.PropertyToID("_StarPositions");

			// Token: 0x0400552F RID: 21807
			internal static readonly int _StarProps = Shader.PropertyToID("_StarProps");

			// Token: 0x04005530 RID: 21808
			internal static readonly int _NumStars = Shader.PropertyToID("_NumStars");

			// Token: 0x04005531 RID: 21809
			internal static readonly int _GridTex = Shader.PropertyToID("_GridTex");

			// Token: 0x04005532 RID: 21810
			internal static readonly int _TravelProps = Shader.PropertyToID("_TravelProps");

			// Token: 0x04005533 RID: 21811
			internal static readonly int _TravelTex = Shader.PropertyToID("_TravelTex");

			// Token: 0x04005534 RID: 21812
			internal static readonly int _PlusTex = Shader.PropertyToID("_PlusTex");

			// Token: 0x04005535 RID: 21813
			internal static readonly int _TravelColor = Shader.PropertyToID("_TravelColor");

			// Token: 0x04005536 RID: 21814
			internal static readonly int _rtTemp = Shader.PropertyToID("_rtTemp");

			// Token: 0x04005537 RID: 21815
			internal static readonly int _rtTemp2 = Shader.PropertyToID("_rtTemp2");

			// Token: 0x04005538 RID: 21816
			internal static readonly int _rtTemp3 = Shader.PropertyToID("_rtTemp3");

			// Token: 0x04005539 RID: 21817
			internal static readonly int _rtTemp4 = Shader.PropertyToID("_rtTemp4");
		}
	}
}
