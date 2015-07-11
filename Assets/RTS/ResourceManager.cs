using UnityEngine;
using System.Collections;

namespace RTS {
	public static class ResourceManager {

		private static Vector3 invalidPosition = new Vector3(-99999, -99999, -99999);

		public static int ScrollWidth { get { return 15; } }
		public static float ScrollSpeed { get { return 100; } }
		public static float ZoomSpeed { get { return 100; } }

		public static float RotateSpeed { get { return 100; } }
		public static float RotateAmount { get { return 10; } }

		public static float MinCameraHeight { get { return 100; } }
		public static float MaxCameraHeight { get { return 400; } }

		public static Vector3 InvalidPosition { get { return invalidPosition; } }

	}
}