using System;
using BepInEx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[BepInPlugin("com.yourname.widescreenfix", "Widescreen Fix 32:9", "1.0.0")]
public class WidescreenFix : BaseUnityPlugin
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Logger.LogInfo("WidescreenFix Awake() called. Hooking scene events...");
        SceneManager.sceneLoaded += OnSceneLoaded; // Only sceneLoaded to avoid double-patching
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Logger.LogInfo($"[WidescreenFix] Scene loaded: {scene.name}. Applying patch...");
        ApplyPatch();
    }

    private void ApplyPatch()
    {
        try
        {
            // Log cameras but do NOT change orthographic size.
            // Unity automatically shows more horizontal content on wider aspect ratios.
            foreach (var cam in Camera.allCameras)
            {
                Logger.LogInfo($"[WidescreenFix] Camera: {cam.name}, type: {(cam.orthographic ? "Orthographic" : "Perspective")}, OrthoSize: {cam.orthographicSize}, Aspect: {cam.aspect}");
            }

            // Fix UI: match height so elements stay same vertical size and spread across the wider screen
            int count = 0;
            foreach (var scaler in GameObject.FindObjectsOfType<CanvasScaler>())
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 1f; // Match height: UI elements keep same vertical size
                count++;
            }
            Logger.LogInfo($"[WidescreenFix] Patched {count} CanvasScalers. Widescreen patch applied.");
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"[WidescreenFix] Exception in ApplyPatch: {ex}");
        }
    }
}