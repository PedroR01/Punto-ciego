using UnityEngine;
using Cinemachine;

public class CameraShakeFeedback : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera vcam;
    [SerializeField] private float shakeIntensity = .2f;
    [SerializeField] private float shakeDuration = 4f;

    private CinemachineBasicMultiChannelPerlin noise;

    [SerializeField]
    private NoiseSettings walkingCamNoise;

    [SerializeField]
    private NoiseSettings shakeInteractionNoise;

    private void Awake()
    {
        noise = vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    private void OnEnable()
    {
        CuartoDosGM.OnIncorrectState += ShakeHard;
        CuartoDosGM.OnCorrectState += ShakeSoft;
        CuartoDosGM.OnDefaultState += CancelShake;
    }

    private void OnDisable()
    {
        CuartoDosGM.OnIncorrectState -= ShakeHard;
        CuartoDosGM.OnCorrectState -= ShakeSoft;
        CuartoDosGM.OnDefaultState -= CancelShake;
    }

    private void ShakeHard()
    {
        noise.m_NoiseProfile = shakeInteractionNoise;
        noise.m_AmplitudeGain = shakeIntensity;
        noise.m_FrequencyGain = .2f;
    }

    private void ShakeSoft(InteractionContext _)
    {
        StartCoroutine(ShakeRoutine());
    }

    private void CancelShake()
    {
        noise.m_NoiseProfile = walkingCamNoise;
        noise.m_AmplitudeGain = 1;
        noise.m_FrequencyGain = .9f;
    }

    private System.Collections.IEnumerator ShakeRoutine()
    {
        noise.m_NoiseProfile = shakeInteractionNoise;
        noise.m_AmplitudeGain = shakeIntensity;
        noise.m_FrequencyGain = .9f;
        yield return new WaitForSeconds(shakeDuration);
        noise.m_NoiseProfile = walkingCamNoise;
        noise.m_AmplitudeGain = 1;
        noise.m_FrequencyGain = .9f;
    }
}