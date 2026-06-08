using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentSoundFeedback : MonoBehaviour
{
    [SerializeField] private List<AudioSource> audioSource;
    [SerializeField] private float changeSpeed = .3f;
    [SerializeField] private float threshold = 0.05f;

    private float[] defaultVolume;
    private float[] maxVolume;
    private float[] minVolume;

    private Coroutine volumeRoutine;

    private System.Action incorrectAction;
    private System.Action<InteractionContext> correctAction;
    private System.Action defaultAction;

    private void Awake()
    {
        defaultVolume = new float[audioSource.Count];
        maxVolume = new float[audioSource.Count];
        minVolume = new float[audioSource.Count];

        int i = 0;
        foreach (AudioSource source in audioSource)
        {
            defaultVolume[i] = source.volume;

            maxVolume[i] = defaultVolume[i] * 4f;
            minVolume[i] = defaultVolume[i] * 0.1f;

            i++;
        }
    }

    private void OnEnable()
    {
        incorrectAction = () => ChangeVolume(maxVolume);
        correctAction = (InteractionContext _) => ChangeVolume(minVolume);
        defaultAction = () => ChangeVolume(defaultVolume);

        CuartoDosGM.OnIncorrectState += incorrectAction;
        CuartoDosGM.OnCorrectState += correctAction;
        CuartoDosGM.OnDefaultState += defaultAction;
    }

    private void OnDisable()
    {
        CuartoDosGM.OnIncorrectState -= incorrectAction;
        CuartoDosGM.OnCorrectState -= correctAction;
        CuartoDosGM.OnDefaultState -= defaultAction;
    }

    private void ChangeVolume(float[] targetVolume)
    {
        if (volumeRoutine != null)
            StopCoroutine(volumeRoutine);

        volumeRoutine = StartCoroutine(VolumeRoutine(targetVolume));
    }

    private IEnumerator VolumeRoutine(float[] targetVolumes)
    {
        bool finished;

        do
        {
            finished = true;

            for (int i = 0; i < audioSource.Count; i++)
            {
                AudioSource source = audioSource[i];
                float target = targetVolumes[i];

                if (Mathf.Abs(source.volume - target) > threshold)
                {
                    source.volume = Mathf.MoveTowards(
                        source.volume,
                        target,
                        Time.deltaTime * changeSpeed
                    );

                    finished = false;
                }
            }

            yield return null;
        } while (!finished);

        // Snap final (seguridad)
        for (int i = 0; i < audioSource.Count; i++)
            audioSource[i].volume = targetVolumes[i];
    }
}