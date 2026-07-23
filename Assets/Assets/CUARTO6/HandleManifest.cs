using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class HandleManifest : MonoBehaviour
{
    [SerializeField] private VideoClip videoFinal;
    [SerializeField] private VideoPlayer videoPlayer;

    [SerializeField] private GameObject returnButton;
    private bool returnConfirmed = false;
    private bool isPlaying = false;

    public void ExecuteManifest()
    {
        if (isPlaying) return;
        StartCoroutine(PlayVideo());
    }

    private IEnumerator PlayVideo()
    {
        isPlaying = true;
        if (videoPlayer == null) yield break;

        videoPlayer.clip = videoFinal;
        videoPlayer.Prepare();
        yield return new WaitUntil(() => videoPlayer.isPrepared);

        videoPlayer.Play();
        yield return new WaitWhile(() => videoPlayer.isPlaying);
        returnButton.SetActive(true);
        yield return new WaitUntil(() => returnConfirmed);
        returnConfirmed = false;
        returnButton.SetActive(false);
    }
}