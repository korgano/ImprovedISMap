using System;
using System.Collections.Generic;
using BattleTech;
using UnityEngine;

// Token: 0x0200028B RID: 651
public class StarmapPopulator : MonoBehaviour
{
	// Token: 0x06000BAE RID: 2990 RVA: 0x0003C8B0 File Offset: 0x0003AAB0
	private void Start()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		List<StarSystem> list = new List<StarSystem>();
		for (int i = 0; i < this.mapFiles.Count; i++)
		{
			StarSystemDef starSystemDef = new StarSystemDef();
			starSystemDef.FromJSON(this.mapFiles[i].text);
			StarSystem starSystem = new StarSystem(starSystemDef, null);
			list.Add(starSystem);
		}
		SimGameConstants simGameConstants = new SimGameConstants(this.constantFile.text);
		this.starmap.PopulateMap(list, simGameConstants.Travel);
	}

	// Token: 0x04000F47 RID: 3911
	public Starmap starmap;

	// Token: 0x04000F48 RID: 3912
	public List<TextAsset> mapFiles;

	// Token: 0x04000F49 RID: 3913
	public TextAsset constantFile;
}
