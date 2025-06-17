using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Utilitaire statique pour mesurer et analyser les FPS
/// </summary>
public class FpsUtility : MonoBehaviour
{
    private const int LOW_FPS_THRESHOLD = 40;
    private const int NORMAL_FPS_THRESHOLD = 60;
    private const float UPDATE_INTERVAL = 0.1f;
    private const int MAX_HISTORY_SAMPLES = 600; // 60 secondes à 10 échantillons/sec
    
    private static int currentFps;
    private static List<int> fpsHistory = new List<int>();
    
    private float deltaTime;
    private float lastUpdateTime;
    private int frameCount;
    
    
    void Start()
    {
        lastUpdateTime = Time.time;
    }
    
    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        frameCount++;
        
        if (Time.time - lastUpdateTime >= UPDATE_INTERVAL)
        {
            float actualInterval = Time.time - lastUpdateTime;
            int fps = Mathf.RoundToInt(frameCount / actualInterval);
            
            currentFps = fps;
            UpdateFpsHistory(fps);
            
            frameCount = 0;
            lastUpdateTime = Time.time;
        }
    }
    
    private static void UpdateFpsHistory(int fps)
    {
        fpsHistory.Add(fps);
        
        if (fpsHistory.Count > MAX_HISTORY_SAMPLES)
        {
            fpsHistory.RemoveAt(0);
        }
    }
    
    /// <summary>
    /// Retourne les FPS actuels
    /// </summary>
    /// <returns>FPS actuels</returns>
    public static int GetCurrentFps()
    {
        return currentFps;
    }
    
    /// <summary>
    /// Retourne la moyenne des FPS sur la dernière minute
    /// </summary>
    /// <returns>Moyenne des FPS sur 60 secondes</returns>
    public static float GetAverageFpsLastMinute()
    {
        if (fpsHistory.Count == 0)
            return 0f;
            
        return Mathf.Round((float)fpsHistory.Average());
    }
    
    /// <summary>
    /// Retourne la couleur correspondant aux FPS donnés
    /// </summary>
    /// <param name="fps">Valeur FPS à évaluer</param>
    /// <returns>Rouge si <40, Jaune si 40-60, Vert si >60</returns>
    public static Color GetFpsColor(int fps)
    {
        if (fps < LOW_FPS_THRESHOLD)
            return Color.red;
        if (fps <= NORMAL_FPS_THRESHOLD)
            return Color.yellow;
        return Color.green;
    }
    
    /// <summary>
    /// Retourne la couleur des FPS actuels
    /// </summary>
    /// <returns>Couleur correspondant aux FPS actuels</returns>
    public static Color GetCurrentFpsColor()
    {
        return GetFpsColor(currentFps);
    }
    
    public static Color GetAverageFpsColor()
    {
        float avgFps = GetAverageFpsLastMinute();
        return GetFpsColor(Mathf.RoundToInt(avgFps));
    }

    
}
