using Cinemachine;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerRaycastInteraction : MonoBehaviour
{
    public static Action<InteractionContext> OnInteraction;
    public static Action OnCompleted;

    private bool interactionTrigger = false;

    private Ray ray;
    private GameObject lastHit;

    [SerializeField]
    [Range(0, 10f)]
    private float maxDistance = 1.3f;

    [SerializeField]
    private LayerMask targetMask;

    [Header("Temporizador de mirada")]
    [SerializeField] private float requiredGazeTime = 3f;   // Segundos necesarios

    [Header("Indicador visual (centro pantalla)")]
    [SerializeField] private Image gazeIndicator; // Tu imagen de UI

    [SerializeField] private AnimationCurve opacityCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [SerializeField]
    private CinemachineVirtualCamera vCam;

    [SerializeField]
    private GameObject bookUI;

    private float gazeTimer = 0f;
    private bool isGazing = false;       // ¿Está mirando un objeto válido ahora mismo?
    private bool interactionFired = false; // Para no disparar la interacción más de una vez

    private void Awake()
    {
        // Asegurarse de que el indicador empiece invisible
        if (gazeIndicator != null)
            SetIndicatorAlpha(0f);
    }

    private void FixedUpdate()
    {
        CheckForColliders();
    }

    /// <summary>
    /// Se encarga de las interacciones por medio de la vista (POV) del jugador.
    /// </summary>
    private void CheckForColliders()
    {
        ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.5f));

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, targetMask))
        {
            //DebugRaycast(hit, Color.red);
            bool targetObject = ParseTagToInteractiveValidation(hit.transform.tag, out InteractionObjects objectType);

            // --- Cambió el objeto mirado -> resetear timer (interaccion cancelada) ---------------
            if (hit.collider.gameObject != lastHit)
            {
                ResetGaze();
                lastHit = hit.collider.gameObject;
                interactionFired = false;
            }
            // En el caso de que no se haya completado la interacción con el objeto correcto...
            if (!interactionFired)
            {
                // ── Acumular tiempo solo en objetos válidos ──────────────────────────
                if (targetObject)
                {
                    isGazing = true;
                    gazeTimer += Time.fixedDeltaTime;
                    Debug.Log(gazeTimer);
                    // Actualizar opacidad del indicador según el progreso
                    float progress = Mathf.Clamp01(gazeTimer / requiredGazeTime);
                    SetIndicatorAlpha(opacityCurve.Evaluate(progress));

                    // ── Interacción completada ───────────────────────────────────────
                    if (gazeTimer >= requiredGazeTime)
                    {
                        interactionFired = true;
                        SetIndicatorAlpha(0f); // Ocultar al completar

                        OnInteraction?.Invoke(new InteractionContext
                        {
                            player = this.transform,
                            target = hit.transform,
                            objectType = objectType,
                            result = InteractionResult.Correct
                        });

                        if (CuartoDosGM.Instance.IsSceneInteractionClear())
                            OnCompleted?.Invoke();
                    }
                }
                else
                {
                    // Objeto no válido que influye en el escenario/usuario. Falta diferenciar más con los Default en el condicional interactionTrigger...
                    if (!targetObject)
                    {
                        OnInteraction?.Invoke(new InteractionContext
                        {
                            player = this.transform,
                            target = hit.transform,
                            objectType = objectType,
                            result = InteractionResult.Incorrect
                        }
                    );
                        interactionFired = true;
                    }

                    // Objeto único para cuando se terminan todos los objetos válidos del nivel. Última interacción.
                    if (hit.transform.tag.Equals("Book"))
                    {
#warning Esto está hardcodeado para largarlo funcionando rapido. En realidad debertía manejarse desde el libro mismo... Como lo hago con los post-it

                        DisableInteractionOnUITriggerStart();
                        bookUI.SetActive(true);
                        interactionFired = true;
                    }
                }
                interactionTrigger = true;
            }
        }
        else if (interactionTrigger) // Objetos que no son los principales para el libro, pero que PUEDEN desencadenar efectos en el entorno.
        {
            ResetGaze();

            OnInteraction?.Invoke(new InteractionContext
            {
                target = default,
                objectType = InteractionObjects.None,
                result = InteractionResult.Default
            });

            interactionTrigger = false;
        }
    }

    /// <summary>
    /// Reinicia el temporizador y el indicador visual.
    /// </summary>
    private void ResetGaze()
    {
        gazeTimer = 0f;
        isGazing = false;
        SetIndicatorAlpha(0f);
    }

    /// <summary>
    /// Asigna el alpha del indicador de mirada.
    /// </summary>
    private void SetIndicatorAlpha(float alpha)
    {
        if (gazeIndicator == null) return;
        Color c = gazeIndicator.color;
        c.a = alpha;
        gazeIndicator.color = c;
    }

    /// <summary>
    /// Verifica que el tag del objeto interactuable sea compatible con los objetos necesarios para interactuar y pasar al siguiente escenario.
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    private bool ParseTagToInteractiveValidation(string tag, out InteractionObjects result)
    {
        return Enum.TryParse(tag, true, out result) && CuartoDosGM.Instance.GetInteractiveObjectsRemaining().Contains(result);
    }

    // Por ahora utilizado para limpiar la ult referencia al clickear en quedarse en el nivel.
    public void CleanseOnStay()
    {
        StartCoroutine(CleanseOnStayDelay());
    }

    public IEnumerator CleanseOnStayDelay()
    {
        yield return new WaitForSeconds(3f);
        lastHit = null;
    }

    private IEnumerator CheckInteractionStateAfterDelay(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        // Faltaría volver a chequear que lo siga mirando...
        OnCompleted?.Invoke();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawSphere(Camera.main.transform.position, .25f);
    }

    /// <summary>
    /// Shows raycast debug in editor viewport and console.
    /// </summary>
    /// <param name="hit"></param>
    /// <param name="rayColor"></param>
    private void DebugRaycast(RaycastHit hit, Color rayColor)
    {
        Debug.DrawRay(Camera.main.transform.position, hit.transform.position - Camera.main.transform.position, rayColor);
        Debug.Log(hit.collider.name + " captado por raycast a una distancia de " + hit.distance);
    }

    public void DisableInteractionOnUITriggerStart(bool showMouse = true)
    {
        GetComponent<FirstPersonMovement>().enabled = false;
        vCam.enabled = false;
        if (showMouse)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void EnableInteractionOnUITriggerEnd()
    {
        GetComponent<FirstPersonMovement>().enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        vCam.enabled = true;
    }

    public void ChangeCamera(CinemachineVirtualCamera newCam)
    {
        if (vCam != null)
        {
            DisableInteractionOnUITriggerStart(false);
            vCam = newCam;
            maxDistance = 20f;
        }
    }

    public GameObject GetLastHit()
    {
        return lastHit;
    }

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}