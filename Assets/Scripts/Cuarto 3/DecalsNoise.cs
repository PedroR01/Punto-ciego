using System;
using System.Collections;
using UnityEngine;

public class DecalsNoise : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private float fadeDuration = 2f;

    [Header("Noise Config")]
    public float changeInterval = 0.5f; // Segundos entre cambios

    public Vector2 rangeX = new Vector2(-5f, 5f);
    public Vector2 rangeY = new Vector2(-3f, 3f);
    public Vector2 scaleRange = new Vector2(0.5f, 2f);
    public Vector2 alphaRange = new Vector2(0.1f, 1f);

    private Vector2 defaultPos;
    private float timer;

    private void OnEnable()
    {
        VideoFeedback.OnVideoEnded += EraseDecal;
    }

    private void OnDisable()
    {
        VideoFeedback.OnVideoEnded -= EraseDecal;
    }

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        defaultPos = transform.position;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= changeInterval)
        {
            Noise();
            timer = 0f;
        }
    }

    private void Noise()
    {
        // 1. Aleatorizar Posición
        float posX = UnityEngine.Random.Range(rangeX.x, rangeX.y);
        float posY = UnityEngine.Random.Range(rangeY.x, rangeY.y);
        transform.position = new Vector3(defaultPos.x + posX, defaultPos.y + posY, transform.position.z);

        // 2. Aleatorizar Escala
        float scale = UnityEngine.Random.Range(scaleRange.x, scaleRange.y);
        transform.localScale = new Vector3(scale, scale, 1f);

        // 3. Aleatorizar Alpha (Transparencia)
        float newAlpha = UnityEngine.Random.Range(alphaRange.x, alphaRange.y);
        Color currentColor = spriteRenderer.color;
        spriteRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
    }

    private void EraseDecal(InteractionObjects relatedObject)
    {
        if (Enum.TryParse(transform.tag, true, out InteractionObjects result) && result.Equals(relatedObject))
        {
            StartCoroutine(FadeOut());
        }
    }

    private IEnumerator FadeOut()
    {
        float currentTime = 0f;

        Color startColor = spriteRenderer.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            float t = Mathf.Clamp01(currentTime / fadeDuration);

            spriteRenderer.color = Color.Lerp(startColor, endColor, t);

            yield return null;
        }
        this.gameObject.SetActive(false);
    }
}