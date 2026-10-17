using System;
using System.Collections.Generic;
using BepInEx.Logging;
using UnityEngine;

// Full-screen HUD overlays (fade-to-black cover, hit flash, cutscene letterbox bars) are authored
// 1920 wide for 16:9. Once the world fills an ultrawide screen they only cover the centre, so
// stretch them horizontally. They are solid colours and the game only moves the bars vertically.
public static class OverlayStretch
{
    static readonly string[] s_names = { "COVERALL", "COVERFLASH", "EnvelopeTop", "EnvelopeBottom" };
    static readonly HashSet<int> s_stretched = new HashSet<int>();

    public static void Apply(ManualLogSource log)
    {
        if (Screen.height <= 0)
            return;
        float factor = (float)Screen.width / Screen.height / (16f / 9f);
        if (factor <= 1.01f)
            return;

        foreach (var t in UnityEngine.Object.FindObjectsOfType<RectTransform>(true))
        {
            if (Array.IndexOf(s_names, t.name) < 0 || !s_stretched.Add(t.GetInstanceID()))
                continue;
            Vector3 s = t.localScale;
            t.localScale = new Vector3(s.x * factor, s.y, s.z);
            log.LogInfo($"[WidescreenFix] Stretched overlay {t.name} by {factor:0.##}x.");
        }
    }
}
