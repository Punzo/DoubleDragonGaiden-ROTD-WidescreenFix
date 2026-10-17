using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

// Menu backgrounds are authored for 1920 wide:
//  - Main menu (GameMenu): a 1920x1080 MASK clips an animated fire image (FLAME) and a scrolling tiled
//    logo pattern (BGLOGO); the dragon boards sit on top.
//  - Mission select, game over and versus menus: a scrolling tiled logo pattern (BGLOGO).
// Tiled patterns can simply be made wider. The fire is pixel art, so instead of stretching it, mirrored
// copies are placed on both sides and kept on the same animation frame.
[HarmonyPatch]
public static class MenuBackgroundPatch
{
    static readonly Dictionary<GameMenu, Image[]> s_flameCopies = new Dictionary<GameMenu, Image[]>();

    // Runs before each menu stores m_BGHandle's start position for its scroll animation.
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameMenu), "StartBackground")]
    static void GameMenuStartBackground(GameMenu __instance)
    {
        if (!Enabled())
            return;
        WidenTiled(__instance.m_BGHandle);
        try
        {
            Image flame = __instance.m_FlameImage;
            if (flame != null)
            {
                // Widen whatever clips the background (the MASK object) to the full canvas.
                for (Transform p = flame.transform.parent; p != null && p != __instance.transform; p = p.parent)
                {
                    if (p.name == "MASK" && p is RectTransform mask)
                        Widescreen.WidenKeepingCenter(mask, Widescreen.CanvasWidth);
                }
                AddMirroredFlames(__instance, flame);
            }
        }
        catch (System.Exception ex)
        {
            WidescreenFix.Log.LogError($"[WidescreenFix] Main menu background: {ex}");
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameMenu), "UpgradeFlameBackground")]
    static void SyncFlames(GameMenu __instance)
    {
        if (s_flameCopies.TryGetValue(__instance, out Image[] copies) && __instance.m_FlameImage != null)
        {
            Sprite sprite = __instance.m_FlameImage.sprite;
            foreach (Image copy in copies)
            {
                if (copy != null && copy.sprite != sprite)
                    copy.sprite = sprite;
            }
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MissionSelectMenu), "Init")]
    static void MissionSelectInit(MissionSelectMenu __instance) { if (Enabled()) WidenTiled(__instance.m_BGHandle); }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameOverMenu), "Init")]
    static void GameOverInit(GameOverMenu __instance) { if (Enabled()) WidenTiled(__instance.m_BGHandle); }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MenuVersusMode), "Init")]
    static void VersusInit(MenuVersusMode __instance) { if (Enabled()) WidenTiled(__instance.m_BGHandle); }

    static bool Enabled() => WidescreenFix.WidenMenus.Value && Widescreen.IsWider;

    // The pattern is wider than 1920 by the length of its scroll; keep that margin on the wider canvas.
    static void WidenTiled(RectTransform handle)
    {
        if (handle == null)
            return;
        float oldWidth = handle.rect.width;
        float newWidth = oldWidth + (Widescreen.CanvasWidth - 1920f);
        Widescreen.WidenKeepingCenter(handle, newWidth);
        WidescreenFix.Log.LogInfo($"[WidescreenFix] Widened menu background {handle.name} from {oldWidth:0} to {newWidth:0}.");
    }

    static void AddMirroredFlames(GameMenu menu, Image flame)
    {
        if (s_flameCopies.ContainsKey(menu))
            return;
        RectTransform src = flame.rectTransform;
        float width = src.rect.width;
        int perSide = Mathf.CeilToInt((Widescreen.CanvasWidth - width) / 2f / width);
        var copies = new List<Image>();
        for (int i = 1; i <= perSide; i++)
        {
            foreach (int side in new[] { -1, 1 })
            {
                var go = Object.Instantiate(flame.gameObject, src.parent, false);
                go.name = $"{flame.name}_widescreen_{(side < 0 ? "L" : "R")}{i}";
                // Remove copied children (if any) so only the flame image is duplicated.
                for (int c = go.transform.childCount - 1; c >= 0; c--)
                    Object.Destroy(go.transform.GetChild(c).gameObject);
                var rt = (RectTransform)go.transform;
                rt.SetSiblingIndex(src.GetSiblingIndex());
                rt.localPosition = src.localPosition + new Vector3(side * width * i, 0f, 0f);
                // Odd copies are mirrored so their edges match the original seamlessly.
                Vector3 s = src.localScale;
                rt.localScale = new Vector3(i % 2 == 1 ? -s.x : s.x, s.y, s.z);
                copies.Add(go.GetComponent<Image>());
            }
        }
        s_flameCopies[menu] = copies.ToArray();
        WidescreenFix.Log.LogInfo($"[WidescreenFix] Added {copies.Count} mirrored flame backgrounds to the main menu.");
    }
}
