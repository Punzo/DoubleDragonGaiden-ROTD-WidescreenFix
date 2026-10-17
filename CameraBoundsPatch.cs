using HarmonyLib;
using UnityEngine;

// CameraManager.SetViewOffset sizes the gameplay view for 16:9 (half width = half height / 9 * 16) and
// everything else derives from it: the camera stops scrolling when that 16:9 view reaches the room edge,
// the side walls (edge colliders) sit at its edges, and enemies outside it count as off screen and get
// despawned. On an ultrawide screen this leaves invisible walls at the old 16:9 edges.
// Recompute the view with the real aspect ratio, so the camera stops when the *screen* edge reaches the
// room edge and the walls line up with the screen edges.
[HarmonyPatch(typeof(CameraManager))]
public static class CameraBoundsPatch
{
    static readonly AccessTools.FieldRef<CameraManager, Vector3> s_cornerOffset = AccessTools.FieldRefAccess<CameraManager, Vector3>("corneroffset");
    static readonly AccessTools.FieldRef<CameraManager, float> s_minX = AccessTools.FieldRefAccess<CameraManager, float>("minX");
    static readonly AccessTools.FieldRef<CameraManager, float> s_maxX = AccessTools.FieldRefAccess<CameraManager, float>("maxX");
    static readonly AccessTools.FieldRef<CameraManager, Rect> s_cameraExtents = AccessTools.FieldRefAccess<CameraManager, Rect>("m_CurrentCameraExtents");
    static readonly System.Reflection.MethodInfo s_setCameraViewRect = AccessTools.Method(typeof(CameraManager), "SetCameraViewRect");
    static readonly System.Reflection.MethodInfo s_setMinMax = AccessTools.Method(typeof(CameraManager), "SetMinMax");

    static bool Enabled() => WidescreenFix.WidenWorld.Value && WidescreenFix.WideBounds.Value && Widescreen.IsWider;

    [HarmonyPostfix]
    [HarmonyPatch("SetViewOffset")]
    static void SetViewOffset(CameraManager __instance)
    {
        if (!Enabled())
            return;
        Vector3 corner = s_cornerOffset(__instance);
        float halfHeight = corner.y;
        float halfWidth = halfHeight * Widescreen.Aspect;
        s_cornerOffset(__instance) = new Vector3(halfWidth, halfHeight, corner.z);
        s_setCameraViewRect.Invoke(__instance, new object[] { halfWidth, halfHeight });
        s_setMinMax.Invoke(__instance, null);
        CameraEdgeColliderRefs edges = __instance.GetEdges();
        if (edges != null)
        {
            edges.m_LeftCollider.localPosition = new Vector3(-halfWidth - 0.5f, 0f, 0f);
            edges.m_RightCollider.localPosition = new Vector3(halfWidth + 0.5f, 0f, 0f);
        }
    }

    // When the camera area (e.g. a locked fight arena) is narrower than the screen, min > max: centre it
    // instead of letting the camera jump between the two limits.
    [HarmonyPostfix]
    [HarmonyPatch("SetMinMax")]
    static void SetMinMax(CameraManager __instance)
    {
        if (!Enabled())
            return;
        ref float minX = ref s_minX(__instance);
        ref float maxX = ref s_maxX(__instance);
        if (minX > maxX)
            minX = maxX = s_cameraExtents(__instance).center.x;
    }

    // Keep the side walls at the screen edges, but not past the room / arena bounds when those are closer
    // (and never closer than the game's original 16:9 walls).
    [HarmonyPostfix]
    [HarmonyPatch("UpdateMainCamera")]
    static void UpdateWalls(CameraManager __instance)
    {
        if (!Enabled())
            return;
        CameraEdgeColliderRefs edges = __instance.GetEdges();
        if (edges == null || edges.m_LeftCollider == null || edges.m_RightCollider == null)
            return;

        float halfWidth = s_cornerOffset(__instance).x;
        float halfWidth16x9 = s_cornerOffset(__instance).y * Widescreen.Aspect16x9;
        float center = __instance.m_EdgeHandle.localPosition.x;
        Rect bounds = __instance.GetActiveCameraExtents();
        float left = Mathf.Min(halfWidth, Mathf.Max(halfWidth16x9, center - bounds.xMin));
        float right = Mathf.Min(halfWidth, Mathf.Max(halfWidth16x9, bounds.xMax - center));
        edges.m_LeftCollider.localPosition = new Vector3(-left - 0.5f, 0f, 0f);
        edges.m_RightCollider.localPosition = new Vector3(right + 0.5f, 0f, 0f);
    }
}
