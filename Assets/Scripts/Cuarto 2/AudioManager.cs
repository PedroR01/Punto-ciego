using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Relato inicial (Audio In)")]
    [SerializeField] private AudioSource referenceAudio;

    [Header("Sonidos ambiente")]
    [SerializeField] private AudioSource[] audioSources;

    [Header("Configuración Fade In")]
    [SerializeField] private float fadeDuration = 2f;

    private float[] originalVolumes;
    private bool hasTriggered = false;

    private void Awake()
    {
        // Guardamos los volúmenes originales de cada AudioSource
        originalVolumes = new float[audioSources.Length];

        for (int i = 0; i < audioSources.Length; i++)
        {
            if (audioSources[i] != null)
            {
                originalVolumes[i] = audioSources[i].volume;
            }
        }
    }

    private void Update()
    {
        if (!hasTriggered && referenceAudio != null)
        {
            if (!referenceAudio.isPlaying && referenceAudio.time > 0)
            {
                hasTriggered = true;
                PlayAllWithFadeIn();
            }
        }
    }

    private void PlayAllWithFadeIn()
    {
        for (int i = 0; i < audioSources.Length; i++)
        {
            AudioSource audio = audioSources[i];

            if (audio != null)
            {
                float targetVolume = originalVolumes[i];

                audio.volume = 0f;
                audio.Play();

                StartCoroutine(FadeIn(audio, fadeDuration, targetVolume));
            }
        }
    }

    private IEnumerator FadeIn(AudioSource audio, float duration, float targetVol)
    {
        float currentTime = 0f;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;

            // Fade proporcional al volumen original
            audio.volume = Mathf.Lerp(0f, targetVol, currentTime / duration);

            yield return null;
        }

        audio.volume = targetVol;
    }

    public void StopAllWithFadeOut()
    {
        for (int i = 0; i < audioSources.Length; i++)
        {
            AudioSource audio = audioSources[i];

            if (audio != null)
            {
                float targetVolume = 0f;

                StartCoroutine(FadeOut(audio, fadeDuration, targetVolume));
            }
        }
    }

    private IEnumerator FadeOut(AudioSource audio, float duration, float targetVol)
    {
        float currentTime = 0f;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;

            // Fade proporcional al volumen original
            audio.volume = Mathf.Lerp(audio.volume, targetVol, currentTime / duration);

            yield return null;
        }

        audio.volume = targetVol;
    }
}