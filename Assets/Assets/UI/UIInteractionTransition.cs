using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIInteractionTransition : MonoBehaviour
{
    [Header("Configuración")]
    public Image targetImage;          // La imagen UI del Canvas

    public float transitionDuration = 1.5f;

    [SerializeField]
    private ObjectZoneAudioTrigger soundZone;

    private Material _mat;
    private static readonly int SatProp = Shader.PropertyToID("_Saturation");

    private void Awake()
    {
        // Crear instancia propia del material para no afectar otras imágenes
        _mat = new Material(targetImage.material);
        targetImage.material = _mat;
        _mat.SetFloat(SatProp, 0f);   // Empieza en B&N
    }

    // Llamá este método desde el script del objeto interactuable
    public void ActivateColor()
    {
        StartCoroutine(FadeToColor());
    }

    private IEnumerator FadeToColor()
    {
        float elapsed = 0f;
        float startSat = _mat.GetFloat(SatProp);

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);
            // Ease-in-out para que se sienta suave
            float smooth = t * t * (3f - 2f * t);
            _mat.SetFloat(SatProp, Mathf.Lerp(startSat, 1f, smooth));
            yield return null;
        }

        _mat.SetFloat(SatProp, 1f);
        if (soundZone)
            soundZone.enabled = true;
    }
}