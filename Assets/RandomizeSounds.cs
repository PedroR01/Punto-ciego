using UnityEngine;
using System.Collections;

public class RandomizeSounds : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip[] audioClips;

    [Header("Timing")]
    public float minDelay = 2f;

    public float maxDelay = 8f;

    [Header("Audio Variation")]
    public float minVolume = 0.7f;

    public float maxVolume = 1f;

    public float minPitch = 0.95f;
    public float maxPitch = 1.05f;

    [Header("Polyphony")]
    [Tooltip("Cantidad máxima de sonidos simultáneos")]
    public int audioSourcePoolSize = 5;

    private AudioSource[] audioSources;

    private void Start()
    {
        // Crear pool de AudioSources
        audioSources = new AudioSource[audioSourcePoolSize];

        for (int i = 0; i < audioSourcePoolSize; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            audioSources[i] = source;
        }

        StartCoroutine(RandomPlaybackLoop());
    }

    private IEnumerator RandomPlaybackLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

            PlayRandomSound();
        }
    }

    private void PlayRandomSound()
    {
        if (audioClips.Length == 0)
            return;

        AudioSource source = GetAvailableSource();

        if (source == null)
            return; // Todas las fuentes ocupadas

        source.clip = audioClips[Random.Range(0, audioClips.Length)];
        source.volume = Random.Range(minVolume, maxVolume);
        source.pitch = Random.Range(minPitch, maxPitch);

        source.Play();
    }

    private AudioSource GetAvailableSource()
    {
        foreach (AudioSource source in audioSources)
        {
            if (!source.isPlaying)
                return source;
        }

        return null;
    }
}