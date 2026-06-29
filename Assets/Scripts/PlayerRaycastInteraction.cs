using Cinemachine;
using System;
using System.Collections;
using UnityEngine;

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

    [SerializeField]
    private CinemachineVirtualCamera vCam;

    [SerializeField]
    private GameObject bookUI;

    private void FixedUpdate()
    {
        CheckForColliders();
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

    /// <summary>
    /// Se encarga de las interacciones por medio de la vista (POV) del jugador.
    /// </summary>
    private void CheckForColliders()
    {
        ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.5f));

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, targetMask))
        {
            //DebugRaycast(hit, Color.red);
            if (hit.collider.gameObject != lastHit)
            {
                lastHit = hit.collider.gameObject;
                bool targetObject = ParseTagToInteractiveValidation(hit.transform.tag, out InteractionObjects objectType);
                OnInteraction?.Invoke(new InteractionContext
                {
                    player = this.transform,
                    target = hit.transform,
                    objectType = objectType,
                    result = targetObject ? InteractionResult.Correct : InteractionResult.Incorrect
                }
                );
                if (targetObject && CuartoDosGM.Instance.IsSceneInteractionClear())
                    OnCompleted?.Invoke();
                if (hit.transform.tag.Equals("Book"))
                {
#warning Esto está hardcodeado para largarlo funcionando rapido. En realidad debertía manejarse desde el libro mismo... Como lo hago con los post-it

                    GetComponent<FirstPersonMovement>().enabled = false;
                    vCam.enabled = false;
                    bookUI.SetActive(true);
                }

                //StartCoroutine(CheckInteractionStateAfterDelay(4f)); // Este tiempo tiene que ser igual al temblor de la camara con la silla en CameraShakeFeedback.cs
                interactionTrigger = true;
            }
        }
        else if (interactionTrigger) // Objetos que no son los principales para el libro, pero que pueden desencadenar efectos en el entorno.
        {
#warning Aca deberia haber una especie de espera o control para no disparar automaticamente eventos sin haber terminado la anterior interaccion.

            OnInteraction?.Invoke(new InteractionContext
            {
                target = hit.transform,
                objectType = InteractionObjects.None,
                result = InteractionResult.Default
            }
            );
            interactionTrigger = false;
        }
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

    public void DisableInteractionOnUITriggerStart()
    {
        GetComponent<FirstPersonMovement>().enabled = false;
        vCam.enabled = false;
        //this.enabled = false;
    }

    public void EnableInteractionOnUITriggerEnd()
    {
        GetComponent<FirstPersonMovement>().enabled = true;
        vCam.enabled = true;
        //this.enabled = true;
    }

    public void ChangeCamera(CinemachineVirtualCamera newCam)
    {
        if (vCam != null)
        {
            DisableInteractionOnUITriggerStart();
            vCam = newCam;
            maxDistance = 20f;
        }
    }

    public GameObject GetLastHit()
    {
        return lastHit;
    }
}