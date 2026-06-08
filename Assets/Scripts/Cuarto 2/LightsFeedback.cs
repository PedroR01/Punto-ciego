using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class LightsFeedback : MonoBehaviour
{
    [Header("Lights")]
    [SerializeField] private Light[] lights;

    [SerializeField] private float transitionDuration = 2f;

    [Header("Post Processing")]
    [SerializeField] private Volume volume;

    [Header("States")]
    [SerializeField] private VisualFeedbackState defaultState;

    [SerializeField] private VisualFeedbackState correctState;
    [SerializeField] private VisualFeedbackState incorrectState;

    private float[] defaultLightIntensities;
    private Color[] defaultLightColors;

    private Vignette vignette;
    private LiftGammaGain colorAdjustments;

    private Coroutine transitionRoutine;

    private void Awake()
    {
        CacheLightsDefaults();
        CachePostProcessing();
    }

    private void OnEnable()
    {
        CuartoDosGM.OnDefaultState += () => ApplyState(defaultState);
        CuartoDosGM.OnCorrectState += (InteractionContext _) => ApplyState(correctState);
        CuartoDosGM.OnIncorrectState += () => ApplyState(incorrectState);
    }

    private void OnDisable()
    {
        CuartoDosGM.OnDefaultState -= () => ApplyState(defaultState);
        CuartoDosGM.OnCorrectState -= (InteractionContext _) => ApplyState(correctState);
        CuartoDosGM.OnIncorrectState -= () => ApplyState(incorrectState);
    }

    // --------------------------------------------------

    private void CacheLightsDefaults()
    {
        defaultLightIntensities = new float[lights.Length];
        defaultLightColors = new Color[lights.Length];

        for (int i = 0; i < lights.Length; i++)
        {
            defaultLightIntensities[i] = lights[i].intensity;
            defaultLightColors[i] = lights[i].color;
        }
    }

    private void CachePostProcessing()
    {
        volume.profile.TryGet(out vignette);
        volume.profile.TryGet(out colorAdjustments);
    }

    // --------------------------------------------------

    private void ApplyState(VisualFeedbackState state)
    {
        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        transitionRoutine = StartCoroutine(TransitionRoutine(state));
    }

    private IEnumerator TransitionRoutine(VisualFeedbackState target)
    {
        float elapsed = 0f;

        float[] startIntensities = new float[lights.Length];
        Color[] startColors = new Color[lights.Length];

        for (int i = 0; i < lights.Length; i++)
        {
            startIntensities[i] = lights[i].intensity;
            startColors[i] = lights[i].color;
        }

        float startVignette = vignette.intensity.value;
        Vector4 startLift = colorAdjustments.lift.value;
        Vector4 startGamma = colorAdjustments.gamma.value;
        Vector4 startGain = colorAdjustments.gain.value;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;

            for (int i = 0; i < lights.Length; i++)
            {
                lights[i].intensity = Mathf.Lerp(
                    startIntensities[i],
                    defaultLightIntensities[i] * target.lightIntensityMultiplier,
                    t
                );

                lights[i].color = Color.Lerp(
                    startColors[i],
                    target.lightColor,
                    t
                );
            }

            vignette.intensity.value = Mathf.Lerp(startVignette, target.vignetteIntensity, t);
            colorAdjustments.lift.Override(Vector4.Lerp(startLift, target.lift, t));
            colorAdjustments.gamma.Override(Vector4.Lerp(startGamma, target.gamma, t));
            colorAdjustments.gain.Override(Vector4.Lerp(startGain, target.gain, t));

            yield return null;
        }
    }
}