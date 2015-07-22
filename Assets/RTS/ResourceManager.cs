using UnityEngine;
using System.Collections;

namespace RTS {
	public static class ResourceManager 
	{
		private static GUISkin selectBoxSkin;
		private static Vector3 invalidPosition = new Vector3(-99999, -99999, -99999);
		private static Bounds invalidBounds = new Bounds(new Vector3(-99999, -99999, -99999), new Vector3(0,0,0));

		public static int ScrollWidth { get { return 15; } }
		public static float ScrollSpeed { get { return 100; } }
		public static float ZoomSpeed { get { return 100; } }

		public static float RotateSpeed { get { return 100; } }
		public static float RotateAmount { get { return 10; } }

		public static float MinCameraHeight { get { return 20; } }
		public static float MaxCameraHeight { get { return 300; } }

		public static Vector3 InvalidPosition { get { return invalidPosition; } }
		public static GUISkin SelectBoxSkin { get { return selectBoxSkin; } }

		public static void StoreSelectBoxItems(GUISkin skin)
		{
			selectBoxSkin = skin;
		}

		public static Bounds InvalidBounds { get { return invalidBounds; } }

		public static int BuildSpeed { get { return 2; } }
	}
}