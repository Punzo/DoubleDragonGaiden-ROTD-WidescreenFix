using System.Text;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.UI;

// Dumps the render chain (cameras, canvases, large UI images and sprites) to the BepInEx log, to see
// how screens are built at ultrawide resolutions. Enabled with DebugDump in the config.
public static class Diagnostics
{
    public static void Dump(ManualLogSource log, string scene)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"===== DUMP scene={scene} screen={Screen.width}x{Screen.height} aspect={(float)Screen.width / Screen.height:0.###}");

        foreach (var cam in Object.FindObjectsOfType<Camera>(true))
        {
            var rt = cam.targetTexture;
            sb.AppendLine($"CAM {Path(cam.transform)} enabled={cam.isActiveAndEnabled} depth={cam.depth} ortho={cam.orthographic}/{cam.orthographicSize} " +
                          $"rect={cam.rect} target={(rt ? $"{rt.name} {rt.width}x{rt.height}" : "screen")} mask=0x{cam.cullingMask:X} clear={cam.clearFlags}");
        }

        foreach (var canvas in Object.FindObjectsOfType<Canvas>(true))
        {
            if (!canvas.isRootCanvas) continue;
            var rect = ((RectTransform)canvas.transform).rect;
            sb.AppendLine($"CANVAS {Path(canvas.transform)} active={canvas.isActiveAndEnabled} mode={canvas.renderMode} cam={(canvas.worldCamera ? canvas.worldCamera.name : "-")} " +
                          $"scale={canvas.scaleFactor:0.###} size={rect.width:0.#}x{rect.height:0.#} sort={canvas.sortingOrder}");
        }

        // Large UI images are the backgrounds and full-screen panels.
        foreach (var g in Object.FindObjectsOfType<MaskableGraphic>(true))
        {
            var t = g.rectTransform;
            if (t.rect.width < 600f && t.rect.height < 400f) continue;
            string tex = g is Image img ? $"sprite={(img.sprite ? img.sprite.name : "-")} type={img.type} preserve={img.preserveAspect}"
                       : g is RawImage raw ? $"tex={(raw.texture ? raw.texture.name : "-")} uv={raw.uvRect}" : g.GetType().Name;
            sb.AppendLine($"UI {Path(t)} active={g.isActiveAndEnabled} {g.GetType().Name} {tex} color={g.color} size={t.rect.width:0.#}x{t.rect.height:0.#} " +
                          $"anchors={t.anchorMin}-{t.anchorMax} pos={t.anchoredPosition} scale={t.localScale}");
        }

        // Large sprites (menus drawn in the world rather than on a canvas).
        foreach (var sr in Object.FindObjectsOfType<SpriteRenderer>(true))
        {
            if (sr.sprite == null || sr.bounds.size.x < 3f) continue;
            sb.AppendLine($"SPRITE {Path(sr.transform)} active={sr.enabled && sr.gameObject.activeInHierarchy} sprite={sr.sprite.name} draw={sr.drawMode} " +
                          $"bounds={sr.bounds.size} pos={sr.transform.position} layer={LayerMask.LayerToName(sr.gameObject.layer)}");
        }
        log.LogInfo(sb.ToString());
    }

    static string Path(Transform t)
    {
        string p = t.name;
        for (var parent = t.parent; parent != null; parent = parent.parent)
            p = parent.name + "/" + p;
        return p;
    }
}
