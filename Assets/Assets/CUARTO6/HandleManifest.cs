using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class HandleManifest : MonoBehaviour
{
    [SerializeField] private VideoClip videoFinal;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private AnimateScenes sceneAnimations;
    [SerializeField] private GameObject botones;

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
        videoPlayer.Stop(); // Para reiniciarlo a 0 el clip en caso de que ya se haya reproducido

        yield return sceneAnimations.FadeToBlack();
        this.GetComponent<Image>().enabled = true;
        botones.SetActive(true);
        sceneAnimations.DisableBlackScreen();
        isPlaying = false;
    }
}