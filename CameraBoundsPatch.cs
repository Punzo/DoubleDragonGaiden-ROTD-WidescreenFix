using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

// CameraManager sizes its gameplay bounds for 16:9 (half width = half height / 9 * 16):
//  - the left/right edge colliders (the "invisible walls" that keep players on screen),
//  - the on-screen rectangle used to despawn enemies that stay off screen too long.
// With the world widened to the screen, move the walls to the real screen edges (but never past the
// room / arena bounds) and treat the full visible width as on screen. The camera's own room clamping
// is left at 16:9 on purpose, so its behaviour in narrow rooms and arenas is unchanged.
[HarmonyPatch(typeof(CameraManager))]
public static class CameraBoundsPatch
{
    static readonly AccessTools.FieldRef<CameraManager, Vector3> s_cornerOffset = AccessTools.FieldRefAccess<CameraManager, Vector3>("corneroffset");
    static readonly AccessTools.FieldRef<CameraManager, Rect> s_viewRect = AccessTools.FieldRefAccess<CameraManager, Rect>("m_CameraViewRect");
    static readonly AccessTools.FieldRef<CameraManager, List<Actor>> s_actors = AccessTools.FieldRefAccess<CameraManager, List<Actor>>("m_Actors");
    static readonly AccessTools.FieldRef<CameraManager, List<ActorModel>> s_actorModels = AccessTools.FieldRefAccess<CameraManager, List<ActorModel>>("m_ActorModels");

    static bool Enabled(out float aspect)
    {
        aspect = Screen.height > 0 ? (float)Screen.width / Screen.height : 16f / 9f;
        return WidescreenFix.WidenWorld.Value && WidescreenFix.WideBounds.Value && aspect > 16f / 9f + 0.01f;
    }

    [HarmonyPostfix]
    [HarmonyPatch("UpdateMainCamera")]
    static void UpdateWalls(CameraManager __instance)
    {
        if (!Enabled(out float aspect))
            return;
        CameraEdgeColliderRefs edges = __instance.GetEdges();
        if (edges == null || edges.m_LeftCollider == null || edges.m_RightCollider == null)
            return;

        Vector3 corner = s_cornerOffset(__instance);
        float originalHalf = corner.x;
        float wideHalf = corner.y * aspect;
        float center = __instance.m_EdgeHandle.localPosition.x;
        Rect bounds = __instance.GetActiveCameraExtents();

        float left = Mathf.Max(originalHalf, Mathf.Min(wideHalf, center - bounds.xMin));
        float right = Mathf.Max(originalHalf, Mathf.Min(wideHalf, bounds.xMax - center));
        edges.m_LeftCollider.localPosition = new Vector3(-left - 0.5f, 0f, 0f);
        edges.m_RightCollider.localPosition = new Vector3(right + 0.5f, 0f, 0f);
    }

    [HarmonyPostfix]
    [HarmonyPatch("UpdateIsOnScreen")]
    static void UpdateIsOnScreen(CameraManager __instance)
    {
        if (!Enabled(out float aspect))
            return;
        List<Actor> actors = s_actors(__instance);
        List<ActorModel> models = s_actorModels(__instance);
        if (actors == null || models == null)
            return;

        Rect view = s_viewRect(__instance);
        float wideHalf = view.height * 0.5f * aspect;
        float xMin = view.center.x - wideHalf, xMax = view.center.x + wideHalf;
        for (int i = 0; i < models.Count && i < actors.Count; i++)
        {
            ActorModel model = models[i];
            if (model.m_IsOnScreen || !actors[i].m_IsActive || !actors[i].m_IsAlive)
                continue;
            // Same test as the game, with the horizontal bounds widened to the visible area.
            Vector3 p = model.GetBodyPosition();
            if (p.y + 1.5f >= view.yMin && p.y <= view.yMax && p.x - 0.5f <= xMax && p.x + 0.5f >= xMin)
                model.m_IsOnScreen = true;
        }
    }
}
