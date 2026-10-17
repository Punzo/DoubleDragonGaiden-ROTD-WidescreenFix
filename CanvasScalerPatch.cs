using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

// Scale every UI canvas by screen height (1920x1080 reference) so the UI keeps its 16:9 size on wider
// screens. Done inside CanvasScaler.Handle, which runs every frame before the canvas is drawn, so new
// canvases (loading screens, menus) never show for a moment with the game's original scaling.
[HarmonyPatch(typeof(CanvasScaler), "Handle")]
public static class CanvasScalerPatch
{
    static readonly Vector2 s_reference = new Vector2(1920f, 1080f);

    static void Prefix(CanvasScaler __instance)
    {
        if (__instance.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
            __instance.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        if (__instance.referenceResolution != s_reference)
            __instance.referenceResolution = s_reference;
        if (__instance.screenMatchMode != CanvasScaler.ScreenMatchMode.MatchWidthOrHeight)
            __instance.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        if (__instance.matchWidthOrHeight != 1f)
            __instance.matchWidthOrHeight = 1f;
    }
}
