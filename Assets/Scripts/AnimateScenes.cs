using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;

public class AnimateScenes : MonoBehaviour
{
    [Header("Fade")]
    [SerializeField] private RawImage blackScreen;

    [SerializeField] private float fadeDuration = 1.5f;

    [Header("Subtitles")]
    [SerializeField] private Image[] subtitles;

    [SerializeField] private float subtitlesTime;

    [Header("Visual Resource")]
    [SerializeField] private RawImage visualResource;

    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private VideoClip videoFinal;
    [SerializeField] private GameObject placaFinal;

    [SerializeField]
    private float videoTimeStop = 10f;

    [SerializeField]
    private GameObject readyButton;

    [Header("Audio")]
    [SerializeField] private SceneTransitionAudio audioConfig;

    [Header("Behaviour")]
    [SerializeField] private bool isMenu;

    [SerializeField] private bool isFinal;

    private bool readyToContinue = false;

    private bool isTransitioning;

    private void Awake()
    {
        if (!isMenu)
        {
            GetComponent<Image>().enabled = false;
            GetComponent<Button>().enabled = false;
            if (subtitles.Length > 0)
                StartCoroutine(ShowSubtitlesRoutine(0));
        }
    }

    private void OnEnable()
    {
        // Subscribe to the end event when the script becomes active
        if (isMenu)
            this.GetComponentInChildren<VideoPlayer>().loopPointReached += OnVideoFinished;
    }

    private void OnDisable()
    {
        // Always unsubscribe from events to prevent memory leaks
        if (isMenu)
            this.GetComponentInChildren<VideoPlayer>().loopPointReached -= OnVideoFinished;
    }

    private void OnVideoFinished(VideoPlayer source)
    {
        readyButton.SetActive(true);
    }

    // --------------------------------------------------

    public void ExecuteTransition()
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionRoutine());
    }

    // --------------------------------------------------

    private IEnumerator ShowSubtitlesRoutine(int index)
    {
        subtitles[index].gameObject.SetActive(true);
        yield return new WaitForSeconds(subtitlesTime);
#warning Aca quedaría mejor si se hace un fade out y fade in a los subtitulos.
        subtitles[index].gameObject.SetActive(false);
    }

    private IEnumerator TransitionRoutine()
    {
        isTransitioning = true;

        yield return FadeToBlack();

        if (!isMenu)
        {
            yield return PlayVideo();
            yield return FadeToBlack();
        }
        if (isFinal)
        {
            yield return PlayVideo(videoFinal);
            placaFinal.SetActive(true);
            yield return null;
        }
        else
        {
            LoadNextScene();
        }
    }

    // --------------------------------------------------
    // FADE
    // --------------------------------------------------

    private IEnumerator FadeToBlack()
    {
        //transform.GetComponent<Image>().enabled = false;
        blackScreen.gameObject.SetActive(true);

        float elapsed = 0f;
        Color color = blackScreen.color;
        color.a = 0;
        blackScreen.color = color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(0, 1, elapsed / fadeDuration);
            blackScreen.color = color;
            yield return null;
        }

        color.a = 1;
        blackScreen.color = color;
    }

    // --------------------------------------------------
    // AUDIO
    // --------------------------------------------------

    private IEnumerator PlayAudio(AudioSource audio)
    {
        if (audio == null) yield break;

        audio.Play();
        yield return new WaitWhile(() => audio.isPlaying);
    }

    // --------------------------------------------------
    // VIDEO
    // --------------------------------------------------

    private IEnumerator PlayVideo()
    {
        if (videoPlayer == null) yield break;

        visualResource.gameObject.SetActive(true);

        videoPlayer.Prepare();
        yield return new WaitUntil(() => videoPlayer.isPrepared);

        blackScreen.gameObject.SetActive(false);
        videoPlayer.Play();

        if (subtitles.Length > 0)
            StartCoroutine(ShowSubtitlesRoutine(1));

        if (audioConfig.audioOut == null) yield break;

        audioConfig.audioOut.Play();
        yield return new WaitWhile(() => videoPlayer.time < videoTimeStop);
        videoPlayer.Pause();

        yield return new WaitWhile(() => audioConfig.audioOut.isPlaying);
        readyButton.SetActive(true);
        yield return new WaitUntil(() => readyToContinue);

        readyToContinue = false;
        readyButton.SetActive(false);
        videoPlayer.Play();
        yield return new WaitWhile(() => videoPlayer.isPlaying);

        readyButton.SetActive(true);
        yield return new WaitUntil(() => readyToContinue);

        readyToContinue = false;
        readyButton.SetActive(false);
    }

    private IEnumerator PlayVideo(VideoClip video)
    {
        videoPlayer.clip = video;
        videoPlayer.Prepare();
        yield return new WaitUntil(() => videoPlayer.isPrepared);

        blackScreen.gameObject.SetActive(false);
        videoPlayer.Play();
        yield return new WaitWhile(() => videoPlayer.isPlaying);
        videoPlayer.Pause();
        readyButton.SetActive(true);
        yield return new WaitUntil(() => readyToContinue);
        readyToContinue = false;
        readyButton.SetActive(false);
    }

    // --------------------------------------------------
    // SCENE
    // --------------------------------------------------

    private void LoadNextScene()
    {
        int nextIndex =
            (SceneManager.GetActiveScene().buildIndex + 1)
            % SceneManager.sceneCountInBuildSettings;

        SceneManager.LoadScene(nextIndex);
    }

    public void ResumeReadyToContinue()
    {
        readyToContinue = true;
    }
}