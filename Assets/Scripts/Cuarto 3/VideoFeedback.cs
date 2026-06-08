using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoFeedback : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField]
    private VideoPlayer videoPlayer;

    [SerializeField] private RawImage visualResource;

    [Header("Resources")]
    [SerializeField]
    private VideoClip[] interactionObjectsVideos;

    [SerializeField] private RenderTexture[] InteractionObjectsRenders;

    [SerializeField] private AudioSource decalEraseSound;

#warning Hardcodeado...
    [SerializeField] private FirstPersonMovement playerMovement;

    public static event Action<InteractionObjects> OnVideoEnded;

    private void OnEnable()
    {
        CuartoDosGM.OnCorrectState += SelectVideo;
    }

    private void OnDisable()
    {
        CuartoDosGM.OnCorrectState -= SelectVideo;
    }

    private void SelectVideo(InteractionContext relatedObject)
    {
        StartCoroutine(PlayVideo(relatedObject.objectType));
    }

    private IEnumerator PlayVideo(InteractionObjects relatedObject)
    {
        if (videoPlayer == null) yield break;

        RenderTexture defaultRender = videoPlayer.targetTexture;
        VideoClip defaultClip = videoPlayer.clip;

        visualResource.gameObject.SetActive(true);
        Color newAlpha = visualResource.color; newAlpha.a = .85f;
        visualResource.color = newAlpha;

        switch (relatedObject)
        {
            case InteractionObjects.Canvas:
                videoPlayer.clip = interactionObjectsVideos[0];
                visualResource.texture = InteractionObjectsRenders[0];
                videoPlayer.targetTexture = InteractionObjectsRenders[0];
                break;

            case InteractionObjects.Guitar:
                videoPlayer.clip = interactionObjectsVideos[1];
                visualResource.texture = InteractionObjectsRenders[1];
                videoPlayer.targetTexture = InteractionObjectsRenders[1];
                break;

            case InteractionObjects.Ball:
                videoPlayer.clip = interactionObjectsVideos[2];
                visualResource.texture = InteractionObjectsRenders[2];
                videoPlayer.targetTexture = InteractionObjectsRenders[2];

                break;

            case InteractionObjects.Skate:
                videoPlayer.clip = interactionObjectsVideos[3];
                visualResource.texture = InteractionObjectsRenders[3];
                videoPlayer.targetTexture = InteractionObjectsRenders[3];
                break;

            case InteractionObjects.Computer:
                videoPlayer.clip = interactionObjectsVideos[4];
                visualResource.texture = InteractionObjectsRenders[4];
                videoPlayer.targetTexture = InteractionObjectsRenders[4];

                break;
        }

        videoPlayer.Prepare();
        yield return new WaitUntil(() => videoPlayer.isPrepared);

        playerMovement.enabled = false;

        videoPlayer.Play();
        yield return new WaitWhile(() => videoPlayer.isPlaying);

        videoPlayer.clip = defaultClip;
        visualResource.texture = defaultRender;
        videoPlayer.targetTexture = defaultRender;
        playerMovement.enabled = true;

        OnVideoEnded?.Invoke(relatedObject);
        decalEraseSound.Play();

        newAlpha.a = 1f;
        visualResource.color = newAlpha;

        visualResource.gameObject.SetActive(false);
    }
}