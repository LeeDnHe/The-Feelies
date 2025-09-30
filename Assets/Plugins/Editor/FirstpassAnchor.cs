#if UNITY_EDITOR
using UnityEditor;

namespace Project.Editor
{
	internal static class EditorFirstpassAnchor
	{
		[InitializeOnLoadMethod]
		private static void EnsureLoaded() { }
	}
}
#endif


