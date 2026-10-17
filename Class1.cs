using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine.SceneManagement;

[BepInPlugin("com.yourname.widescreenfix", "Widescreen Fix 32:9", "1.1.0")]
public class WidescreenFix : BaseUnityPlugin
{
    internal static ManualLogSource Log;
    internal static ConfigEntry<bool> WidenWorld;
    internal static ConfigEntry<bool> WideBounds;
    private ConfigEntry<bool> m_StretchOverlays;
    private ConfigEntry<bool> m_DebugDump;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Log = Logger;
        WidenWorld = Config.Bind("General", "WidenWorld", true, "Widen the game's world render to the screen aspect ratio (removes the black bars at the sides).");
        WideBounds = Config.Bind("General", "WideBounds", true, "Move the invisible side walls to the real screen edges (never past the room or arena bounds) and keep enemies in the side areas counted as on screen.");
        m_StretchOverlays = Config.Bind("General", "StretchOverlays", true, "Stretch full-screen fades, flashes and cutscene letterbox bars to the full screen width.");
        m_DebugDump = Config.Bind("Debug", "DebugDump", false, "Log cameras, canvases and UI images after each scene load (for development).");

        // UI: scale every canvas by height, so it keeps its 16:9 size and layout on wider screens.
        Harmony.CreateAndPatchAll(typeof(CanvasScalerPatch));
        Harmony.CreateAndPatchAll(typeof(WorldWidthPatch));
        Harmony.CreateAndPatchAll(typeof(CameraBoundsPatch));
        SceneManager.sceneLoaded += OnSceneLoaded;
        Logger.LogInfo("WidescreenFix loaded.");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Logger.LogInfo($"[WidescreenFix] Scene loaded: {scene.name}");
        if (m_StretchOverlays.Value && WidenWorld.Value)
            OverlayStretch.Apply(Logger);
        if (m_DebugDump.Value)
            Diagnostics.Dump(Logger, scene.name);
    }
}
