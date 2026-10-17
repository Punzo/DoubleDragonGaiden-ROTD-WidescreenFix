using System;
using System.Collections.Generic;
using BepInEx.Logging;
using UnityEngine;

// The cutscene letterbox bars are plain black images with a fixed 1920 width, so on an ultrawide screen
// they only cover the centre. Widen them to the full canvas width. (The fade and flash covers stretch with
// their canvas already, so they are left alone; scaling them would also stretch the loading screen inside.)
public static class OverlayStretch
{
    static readonly string[] s_names = { "EnvelopeTop", "EnvelopeBottom" };
    static readonly HashSet<int> s_widened = new HashSet<int>();

    public static void Apply(ManualLogSource log)
    {
        float canvasWidth = Widescreen.CanvasWidth;
        foreach (var t in UnityEngine.Object.FindObjectsOfType<RectTransform>(true))
        {
            if (Array.IndexOf(s_names, t.name) < 0 || t.rect.width >= canvasWidth || !s_widened.Add(t.GetInstanceID()))
                continue;
            Widescreen.WidenKeepingCenter(t, canvasWidth + 64f);
            log.LogInfo($"[WidescreenFix] Widened overlay {t.name} to {canvasWidth + 64f:0}.");
        }
    }
}
