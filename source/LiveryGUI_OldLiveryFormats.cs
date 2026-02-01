using System;
using UnityEngine;

// Snapshots of whatever old versions, for possible mechanisms to resolve possible future
// PB-versus-mod-saved-liveries version differences.
//
// Unused for now. This might not be the best approach. Perhaps PB already has some built-in
// code-path that should be leveraged instead, like it does for trying to load and upgrade
// old versions of some other things.
//
// This manual-versioning approach might also miss versions of PB, if I don't manually grab them.

namespace LiveryGUI.OldVersions
{
	// Token: 0x0200142E RID: 5166
	[Serializable]
	public class DataContainerEquipmentLivery_v1_2_0 : DataContainer
	{
		// Token: 0x040076B9 RID: 30393
		public bool hidden;

		// Token: 0x040076BA RID: 30394
		public int priority;

		// Token: 0x040076BB RID: 30395
		[HideInInspector]
		public string textName;

		// Token: 0x040076BC RID: 30396
		[HideInInspector]
		public string source;

		// Token: 0x040076BD RID: 30397
		public int rating;

		// Token: 0x040076BE RID: 30398
		public string pattern;

		// Token: 0x040076BF RID: 30399
		public Color colorPrimary = new Color(0.5f, 0.5f, 0.5f, 0f);

		// Token: 0x040076C0 RID: 30400
		public Color colorSecondary = new Color(0.5f, 0.5f, 0.5f, 0f);

		// Token: 0x040076C1 RID: 30401
		public Color colorTertiary = new Color(0.5f, 0.5f, 0.5f, 0f);

		// Token: 0x040076C2 RID: 30402
		public Vector4 materialPrimary = new Vector4(0f, 0.2f, 0.5f, 0f);

		// Token: 0x040076C3 RID: 30403
		public Vector4 materialSecondary = new Vector4(0f, 0.2f, 0.5f, 0f);

		// Token: 0x040076C4 RID: 30404
		public Vector4 materialTertiary = new Vector4(0f, 0.2f, 0.5f, 0f);

		// Token: 0x040076C5 RID: 30405
		public Vector4 effect = new Vector4(0f, 0f, 0f, 0f);

		// Token: 0x040076C6 RID: 30406
		public string contentSource;

		// Token: 0x040076C7 RID: 30407
		public Vector4 contentParameters = new Vector4(0f, 0f, 0f, 0f);
	}
}
