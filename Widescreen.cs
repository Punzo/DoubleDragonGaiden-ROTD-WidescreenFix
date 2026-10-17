using UnityEngine;

// Shared screen maths. UI canvases are scaled by height against a 1920x1080 reference (CanvasScalerPatch),
// so a canvas is always 1080 units high and 1080 * aspect units wide.
public static class Widescreen
{
    public const float Aspect16x9 = 16f / 9f;

    public static float Aspect => Screen.height > 0 ? (float)Screen.width / Screen.height : Aspect16x9;

    public static bool IsWider => Aspect > Aspect16x9 + 0.01f;

    public static float CanvasWidth => 1080f * Mathf.Max(Aspect, Aspect16x9);

    // Resize a rect horizontally while keeping its centre where it was, whatever its pivot.
    public static void WidenKeepingCenter(RectTransform t, float newWidth)
    {
        float oldWidth = t.rect.width;
        if (newWidth <= oldWidth)
            return;
        t.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
        Vector3 p = t.localPosition;
        p.x += (0.5f - t.pivot.x) * (oldWidth - newWidth);
        t.localPosition = p;
    }
}
