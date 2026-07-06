using System;
using System.Collections.Generic;
using UnityEngine;

public class CuartoDosGM : MonoBehaviour
{
    public static CuartoDosGM Instance { get; private set; }

    public static event Action OnIncorrectState;

    public static event Action<InteractionContext> OnCorrectState;

    public static event Action OnDefaultState;

    [SerializeField]
    private float interactionCounterUI;

    [SerializeField]
    private Transform closedBook;

    [SerializeField]
    private Transform openBook;

    [SerializeField]
    private List<InteractionObjects> sceneInteractionObjects;

    [SerializeField]
    private AudioManager audioMg;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        PlayerRaycastInteraction.OnInteraction += HandleInteraction;
        PlayerRaycastInteraction.OnCompleted += HandleSceneCompleted;
    }

    private void OnDisable()
    {
        PlayerRaycastInteraction.OnInteraction -= HandleInteraction;
        PlayerRaycastInteraction.OnCompleted -= HandleSceneCompleted;
    }

    private void HandleInteraction(InteractionContext context)
    {
        if (context.result == InteractionResult.Incorrect)
        {
            OnIncorrectState?.Invoke();
        }
        else if (context.result == InteractionResult.Correct)
        {
            HandleSoundSfx(context.target);

            context.target.GetComponent<UIInteractionTransition>().ActivateColor(); // AGREGRAR SCRIPT A C/U DE LOS INTERACTUABLES PARA GM
            sceneInteractionObjects.Remove(context.objectType);
            OnCorrectState?.Invoke(context);
        }
        else
            OnDefaultState?.Invoke();
    }

    public List<InteractionObjects> GetInteractiveObjectsRemaining() => sceneInteractionObjects;

    public bool IsSceneInteractionClear() => sceneInteractionObjects.Count <= 0;

#warning Hardcodeado, esto debería tener su propio componente/script asociado al evento de estado.

    private void HandleSoundSfx(Transform gameObject)
    {
        AudioSource sound = gameObject.GetComponent<AudioSource>();
        if (sound)
        {
            if (!audioMg.isActiveAndEnabled)
                audioMg.enabled = true;

            audioMg.StopAllWithFadeOut(); // Se apaga automaticamente
            if (!sound.isPlaying)
                sound.Play();
        }
    }

    private void HandleSceneCompleted()
    {
        Debug.Log("Escenario terminado, abriendo libro...");

        closedBook.gameObject.SetActive(false);
        openBook.gameObject.SetActive(true);
    }
}