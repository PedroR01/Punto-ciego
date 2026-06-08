using UnityEngine;

[System.Serializable]
public class VisualFeedbackState
{
    [Header("Lights")]
    public float lightIntensityMultiplier = 1f;

    public Color lightColor = Color.white;

    [Header("Post Processing")]
    [Range(0f, 1f)] public float vignetteIntensity;

    [Range(0f, 1f)] public float vignetteSmoothness;

    public Vector4 lift;
    public Vector4 gamma;
    public Vector4 gain;
}