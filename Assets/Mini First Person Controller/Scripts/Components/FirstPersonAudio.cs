using System.Linq;
using UnityEngine;

public class FirstPersonAudio : MonoBehaviour
{
    public FirstPersonMovement character;

    [Header("Step")]
    public AudioSource stepAudio;

    public AudioSource runningAudio;

    [Tooltip("Minimum velocity for moving audio to play")]
    public float velocityThreshold = .01f;

    private Vector2 lastCharacterPosition;
    private Vector2 CurrentCharacterPosition => new Vector2(character.transform.position.x, character.transform.position.z);

    [Header("Landing")]
    public AudioSource landingAudio;

    public AudioClip[] landingSFX;

    [Header("Jump")]
    public AudioSource jumpAudio;

    private AudioSource[] MovingAudios => new AudioSource[] { stepAudio, runningAudio };

    private void Awake()
    {
        FirstPersonMovement.OnJump += PlayJumpAudio;
    }

    private void Reset()
    {
        character = GetComponentInParent<FirstPersonMovement>();
    }

    private void FixedUpdate()
    {
        float velocity = Vector3.Distance(CurrentCharacterPosition, lastCharacterPosition);

        if (velocity >= velocityThreshold && character.IsGrounded)
            SetPlayingMovingAudio(stepAudio);
        else
            SetPlayingMovingAudio(null);

        lastCharacterPosition = CurrentCharacterPosition;
    }

    /// <summary>
    /// Pause all MovingAudios and enforce play on audioToPlay.
    /// </summary>
    /// <param name="audioToPlay">Audio that should be playing.</param>
    private void SetPlayingMovingAudio(AudioSource audioToPlay)
    {
        // Pause all MovingAudios.
        foreach (var audio in MovingAudios.Where(audio => audio != audioToPlay && audio != null))
        {
            audio.Pause();
        }

        // Play audioToPlay if it was not playing.
        if (audioToPlay)
        {
            float basePitch = character.IsRunning ? 1.1f : .8f;
            audioToPlay.panStereo = Random.Range(-0.2f, 0.2f);

            if (!audioToPlay.isPlaying)
                audioToPlay.Play();
        }
    }

    #region Play instant-related audios.

    private void PlayLandingAudio() => PlayRandomClip(landingAudio, landingSFX);

    private void PlayJumpAudio()
    {
        jumpAudio.pitch = Random.Range(1f, 1.2f);
        jumpAudio.Play();
    }

    #endregion Play instant-related audios.

    private static void PlayRandomClip(AudioSource audio, AudioClip[] clips)
    {
        if (!audio || clips.Length <= 0)
            return;

        // Get a random clip. If possible, make sure that it's not the same as the clip that is already on the audiosource.
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clips.Length > 1)
            while (clip == audio.clip)
                clip = clips[Random.Range(0, clips.Length)];

        // Play the clip.
        audio.clip = clip;
        audio.Play();
    }
}