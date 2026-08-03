using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using static UnityEngine.Experimental.Rendering.Universal.PixelPerfectCamera;

public class AnimateScenes : MonoBehaviour
{
    [Header("Fade")]
    [SerializeField] private RawImage blackScreen;

    [SerializeField] private float fadeDuration = 1.5f;

    [Header("Loader")]
    [SerializeField] private GameObject loaderComponent;

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

    private AsyncOperation sceneLoadOperation;

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
        if (isMenu)
            this.GetComponentInChildren<VideoPlayer>().loopPointReached += OnVideoFinished;
    }

    private void OnDisable()
    {
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
        StartCoroutine(SubtitlesFadeAnimation(index, 0, 1));

        if (index == 0)
            yield return new WaitWhile(() => audioConfig.audioIn.isPlaying);
        else
            yield return new WaitWhile(() => audioConfig.audioOut.isPlaying);

        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(SubtitlesFadeAnimation(index, 1, 0));

        subtitles[index].gameObject.SetActive(false);
    }

    private IEnumerator TransitionRoutine()
    {
        isTransitioning = true;

        if (!isMenu)
        {
            yield return FadeToBlack();
            yield return PlayVideo();
            yield return FadeToBlack();
        }
        else
        {
            LoadNextScene(out AsyncOperation op, true);
        }
        if (isFinal)
        {
            yield return PlayVideo(videoFinal);
            yield return FadeToBlack();
            placaFinal.SetActive(true);
            yield return null;
        }
    }

    // --------------------------------------------------
    // FADE
    // --------------------------------------------------

    public IEnumerator FadeToBlack() // Llamado también desde HandleManifest para video final
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

        if (audioConfig.audioOut == null) yield break;

        audioConfig.audioOut.Play();
        if (subtitles.Length > 0)
            StartCoroutine(ShowSubtitlesRoutine(1));

        yield return new WaitWhile(() => videoPlayer.time < videoTimeStop);
        videoPlayer.Pause();
        yield return new WaitWhile(() => subtitles[1].isActiveAndEnabled);

        readyButton.SetActive(true);
        yield return new WaitUntil(() => readyToContinue);

        readyToContinue = false;
        readyButton.SetActive(false);
        LoadNextScene(out AsyncOperation op); // Esto no habría que ejecutarlo en el cuarto 6 (escenario final)... gasta recursos innecesariamente.
        yield return new WaitUntil(() => op.progress >= .9f);

        videoPlayer.Play();

        long stopFrame = (long)videoPlayer.frameCount - 5;
        yield return new WaitUntil(() => videoPlayer.frame >= stopFrame);
        videoPlayer.Pause();

        readyButton.SetActive(true);
        yield return new WaitUntil(() => readyToContinue);

        readyToContinue = false;
        readyButton.SetActive(false);
        if (!isFinal)
            op.allowSceneActivation = true;
    }

    private IEnumerator PlayVideo(VideoClip video)
    {
        videoPlayer.clip = video;
        videoPlayer.Prepare();
        yield return new WaitUntil(() => videoPlayer.isPrepared);

        blackScreen.gameObject.SetActive(false);
        videoPlayer.Play();
        yield return new WaitWhile(() => videoPlayer.isPlaying);
    }

    // --------------------------------------------------
    // SCENE
    // --------------------------------------------------

    private void LoadNextScene(out AsyncOperation operation, bool showLoader = false)
    {
        int nextIndex =
            (SceneManager.GetActiveScene().buildIndex + 1)
            % SceneManager.sceneCountInBuildSettings;

        operation = SceneManager.LoadSceneAsync(nextIndex);
        operation.allowSceneActivation = false;
        sceneLoadOperation = operation;
        if (showLoader)
        {
            loaderComponent.SetActive(true);
            StartCoroutine(AnimateLoader(operation));
        }
    }

    private IEnumerator AnimateLoader(AsyncOperation op)
    {
        Image loader = loaderComponent.GetComponentInChildren<Image>();
        RectTransform loaderRect = loader.rectTransform;
        Vector3 baseScale = loaderRect.localScale;

        float pulseSpeed = 3f;      // velocidad de la pulsación
        float minScale = 0.85f;     // escala mínima (85%)
        float maxScale = 1.1f;      // escala máxima (110%)
        float minAlpha = 0.4f;      // opacidad mínima
        float maxAlpha = 1f;        // opacidad máxima
        while (op.progress < .9f)
        {
            float t = (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) / 2f; // 0 a 1

            float scaleValue = Mathf.Lerp(minScale, maxScale, t);
            loaderRect.localScale = baseScale * scaleValue;

            float alphaValue = Mathf.Lerp(minAlpha, maxAlpha, t);
            Color c = loader.color;
            c.a = alphaValue;
            loader.color = c;

            yield return null;
        }
        // Restaurar valores originales al terminar
        loaderRect.localScale = baseScale;
        Color finalColor = loader.color;
        finalColor.a = 1f;
        loader.color = finalColor;
        // Hacer animacion de fade out y desactivar componente loader
        loaderComponent.SetActive(false);
    }

    private IEnumerator SubtitlesFadeAnimation(int index, float alphaIn, float alphaOut)
    {
        float currentTime = 0f;
        Color startColor = Color.white;
        startColor.a = alphaIn;
        Color endColor = Color.white;
        endColor.a = alphaOut;

        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;

            float t = Mathf.Clamp01(currentTime / fadeDuration);
            subtitles[index].color = Color.Lerp(startColor, endColor, t);

            yield return null;
        }
        Color colorFinal = subtitles[index].color;
        colorFinal.a = alphaOut;
        subtitles[index].color = colorFinal;
    }

    public void ResumeReadyToContinue()
    {
        readyToContinue = true;
    }

    public void AllowSceneActivation()
    {
        if (sceneLoadOperation != null)
            sceneLoadOperation.allowSceneActivation = true;
    }

    public void DisableBlackScreen()
    {
        blackScreen.gameObject.SetActive(false);
    }
}