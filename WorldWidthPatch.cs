using HarmonyLib;
using UnityEngine;

// The game renders the world into fixed 16:9 render textures (704x396 by default) that sit on a
// world-space canvas filmed by FinalFGCamera. On a wider screen the camera sees past the canvas
// edges, which shows up as vertical black bars. Widen the canvases and render textures to the
// screen aspect ratio, keeping the height (and so the pixel scale) unchanged.
[HarmonyPatch(typeof(CameraManager), "SyncTextures")]
public static class WorldWidthPatch
{
    static void Postfix(CameraManager __instance, GameInfo _gameinfo)
    {
        if (!WidescreenFix.WidenWorld.Value || Screen.height <= 0)
            return;
        try
        {
            RectTransform postCanvas = __instance.m_PostEffectsCanvas;
            int height = Mathf.RoundToInt(postCanvas.rect.height);
            int width = Mathf.RoundToInt(postCanvas.rect.width);
            float aspect = (float)Screen.width / Screen.height;
            int target = Mathf.CeilToInt(height * aspect / 2f) * 2;
            if (height <= 0 || target <= width)
                return;

            postCanvas.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, target);
            __instance.m_CompositingCanvas.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, target);

            if (_gameinfo != null)
            {
                Resize(_gameinfo.m_BackgroundFX, target);
                Resize(_gameinfo.m_ConeHull1, target);
                Resize(_gameinfo.m_ConeHull2, target);
                Resize(_gameinfo.m_NormalMapFX, target);
                Resize(_gameinfo.m_PixelColors, target);
                Resize(_gameinfo.m_PixelDepth, target);
                Resize(_gameinfo.m_PostEffectsTexture, target);
                Resize(_gameinfo.m_SkyMapFX, target * 4);
            }
            WidescreenFix.Log.LogInfo($"[WidescreenFix] World render widened from {width}x{height} to {target}x{height} (screen {Screen.width}x{Screen.height}).");
        }
        catch (System.Exception ex)
        {
            WidescreenFix.Log.LogError($"[WidescreenFix] Could not widen world render: {ex}");
        }
    }

    static void Resize(RenderTexture rt, int width)
    {
        if (rt == null || rt.width == width)
            return;
        if (rt.IsCreated())
            rt.Release();
        rt.width = width;
        rt.Create();
    }
}
