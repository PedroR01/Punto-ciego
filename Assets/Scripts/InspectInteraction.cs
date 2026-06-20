using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InspectInteraction : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Configuración desde el Inspector
    // -------------------------------------------------------------------------

    [Header("Posicionamiento")]
    [Tooltip("Distancia desde la cámara a la que se posiciona el objeto.")]
    [Range(0f, .5f)]
    [SerializeField] private float distanceFromCamera = 0.1f;

    [Tooltip("Desplazamiento adicional en espacio local de la cámara (X: izq/der, Y: arr/abajo).")]
    [SerializeField] private Vector3 inspectOffset = Vector3.zero;

    [Tooltip("Multiplicador de escala durante la inspección.")]
    [Range(0f, 4f)]
    [SerializeField] private float inspectScaleMultiplier = 2.5f;

    [Header("Temporizador")]
    [Tooltip("Segundos que el objeto permanece visible frente al jugador.")]
    [Range(1f, 8f)]
    [SerializeField] private float inspectionDuration = 4f;

    [Header("Animación de entrada")]
    [Tooltip("Duración en segundos del movimiento hacia la cámara.")]
    [Range(0f, 1.25f)]
    [SerializeField] private float animateInDuration = 0.5f;

    [Tooltip("Curva que define el ritmo del movimiento de entrada. Eje X: tiempo [0..1], Eje Y: progreso [0..1].")]
    [SerializeField] private AnimationCurve animateInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Animación de salida")]
    [Tooltip("Duración en segundos del movimiento de regreso a la posición original.")]
    [Range(0f, 1.25f)]
    [SerializeField] private float animateOutDuration = 0.35f;

    [Tooltip("Curva que define el ritmo del movimiento de salida.")]
    [SerializeField] private AnimationCurve animateOutCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    // -------------------------------------------------------------------------
    // Estado interno
    // -------------------------------------------------------------------------

    private Camera _mainCamera;
    private bool _isInspecting = false;
    private PlayerRaycastInteraction playerInteractions;
    private HashSet<GameObject> postItsReadedCollection = new HashSet<GameObject>();

    // -------------------------------------------------------------------------
    // Ciclo de vida
    // -------------------------------------------------------------------------

    private void Awake()
    {
        _mainCamera = Camera.main;
        postItsReadedCollection = new HashSet<GameObject>();
    }

    private void OnEnable()
    {
        CuartoDosGM.OnCorrectState += HandleInspectEvent;
        CuartoDosGM.OnIncorrectState += HandleInspectReadedEvent;
    }

    private void OnDisable()
    {
        CuartoDosGM.OnCorrectState -= HandleInspectEvent;
        CuartoDosGM.OnIncorrectState -= HandleInspectReadedEvent;
    }

    // -------------------------------------------------------------------------
    // Lógica principal
    // -------------------------------------------------------------------------

    private void HandleInspectEvent(InteractionContext context)
    {
        if (_isInspecting) return;

        if (context.target == null || context.player == null)
        {
            Debug.LogError("[InspectInteraction] Contexto inválido.");
            return;
        }

        playerInteractions = context.player.GetComponent<PlayerRaycastInteraction>();

        if (playerInteractions == null)
        {
            Debug.LogError("[InspectInteraction] No se encontró PlayerRaycastInteraction.");
            return;
        }

        StartCoroutine(InspectRoutine(context.target, playerInteractions));

        // Cambiamos el tag y guardamos la referencia al objeto del post-it leido para retomarlo si el jugador quiere volver a interactuar con él.
        context.target.tag = "PostItReaded";
        postItsReadedCollection.Add(context.target.gameObject);
    }

#warning Hay algún caso limite que pueda provocar un loop de inspección de post-its no deseados?

    private void HandleInspectReadedEvent()
    {
        // Como c# es shortcircuit, junto las evaluaciones terminales en un solo condicional.
        if (!playerInteractions || !playerInteractions.GetLastHit().tag.Equals("PostItReaded") || !postItsReadedCollection.Contains(playerInteractions.GetLastHit()))
            return;
        else
        {
            postItsReadedCollection.TryGetValue(playerInteractions.GetLastHit(), out GameObject postIt);
            if (postIt)
                StartCoroutine(InspectRoutine(postIt.transform, playerInteractions));
        }
    }

    private IEnumerator InspectRoutine(Transform target, PlayerRaycastInteraction playerInteractions)
    {
        _isInspecting = true;

        // --- Guardar estado original ---
        Vector3 originalPosition = target.position;
        Quaternion originalRotation = target.rotation;
        Vector3 originalScale = target.localScale;

        // --- Calcular estado destino ---
        Vector3 targetPosition = CalculateInspectPosition();
        Quaternion targetRotation = _mainCamera.transform.rotation;
        Vector3 targetScale = originalScale * inspectScaleMultiplier;

        // --- Deshabilitar controles antes de animar ---
        playerInteractions.DisableInteractionOnUITriggerStart();
        playerInteractions.enabled = false;
        Rigidbody playerRb = playerInteractions.transform.GetComponent<Rigidbody>();
        RigidbodyConstraints defaultConstraints = new RigidbodyConstraints();
        defaultConstraints = playerRb.constraints;
        playerRb.constraints = RigidbodyConstraints.FreezePosition;

        // --- Fase 1: Animación de entrada ---
        yield return StartCoroutine(AnimateTransform(
            target,
            originalPosition, targetPosition,
            originalRotation, targetRotation,
            originalScale, targetScale,
            animateInDuration,
            animateInCurve
        ));

        // --- Fase 2: Esperar mientras el jugador lee ---
        yield return new WaitForSeconds(inspectionDuration);

        // --- Fase 3: Animación de salida ---
        yield return StartCoroutine(AnimateTransform(
            target,
            targetPosition, originalPosition,
            targetRotation, originalRotation,
            targetScale, originalScale,
            animateOutDuration,
            animateOutCurve
        ));

        // --- Restaurar controles ---
        playerRb.constraints = defaultConstraints;
        playerInteractions.enabled = true;
        playerInteractions.EnableInteractionOnUITriggerEnd();

        _isInspecting = false;
    }

    // -------------------------------------------------------------------------
    // Corrutina de animación genérica
    // -------------------------------------------------------------------------

    /// <summary>
    /// Interpola posición, rotación y escala de un Transform desde un estado
    /// origen hasta un estado destino, usando una AnimationCurve para el ritmo.
    /// Al ser genérica, sirve tanto para la entrada como para la salida.
    /// </summary>
    private IEnumerator AnimateTransform(
        Transform target,
        Vector3 fromPosition, Vector3 toPosition,
        Quaternion fromRotation, Quaternion toRotation,
        Vector3 fromScale, Vector3 toScale,
        float duration,
        AnimationCurve curve)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Progreso lineal normalizado entre 0 y 1.
            float t = elapsed / duration;

            // Progreso transformado por la curva: aquí nace el ease in/out.
            float tCurved = curve.Evaluate(t);

            // Interpolamos cada propiedad usando el t orgánico.
            target.position = Vector3.Lerp(fromPosition, toPosition, tCurved);
            target.rotation = Quaternion.Slerp(fromRotation, toRotation, tCurved);
            target.localScale = Vector3.Lerp(fromScale, toScale, tCurved);

            // Avanzamos el tiempo y cedemos el control hasta el próximo frame.
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Aplicamos los valores finales exactos para eliminar cualquier
        // error de punto flotante acumulado en el loop.
        target.position = toPosition;
        target.rotation = toRotation;
        target.localScale = toScale;
    }

    // -------------------------------------------------------------------------
    // Cálculo de posición destino
    // -------------------------------------------------------------------------

    private Vector3 CalculateInspectPosition()
    {
        Transform cam = _mainCamera.transform;

        Vector3 basePosition = cam.position + cam.forward * distanceFromCamera;
        basePosition += cam.right * inspectOffset.x;
        basePosition += cam.up * inspectOffset.y;
        basePosition += cam.forward * inspectOffset.z;

        return basePosition;
    }
}