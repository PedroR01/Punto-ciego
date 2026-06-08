using System;
using System.Collections.Generic;
using UnityEngine;

public class CuartoDosGM : MonoBehaviour
{
    public static CuartoDosGM Instance { get; private set; }

    public static event Action OnIncorrectState;

    //public static event Action OnCorrectState;
    public static event Action<InteractionContext> OnCorrectState;

    public static event Action OnDefaultState;

    private bool sceneCompleted = false;

    [SerializeField]
    private Transform closedBook;

    [SerializeField]
    private Transform openBook;

    [SerializeField]
    private List<InteractionObjects> sceneInteractionObjects;

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
            HandleSoundSfx(context.target);

            OnIncorrectState?.Invoke();
        }
        else if (context.result == InteractionResult.Correct)
        {
            HandleSoundSfx(context.target);

            sceneInteractionObjects.Remove(context.objectType);
            OnCorrectState?.Invoke(context);
        }
        else
            OnDefaultState?.Invoke();
    }

    public bool GetSceneCompleted() => sceneCompleted;

    public List<InteractionObjects> GetInteractiveObjectsRemaining() => sceneInteractionObjects;

    public bool IsSceneInteractionClear() => sceneInteractionObjects.Count <= 0;

#warning Hardcodeado, esto debería tener su propio componente/script asociado al evento de estado.

    private void HandleSoundSfx(Transform gameObject)
    {
        if (gameObject.TryGetComponent<AudioSource>(out var objSfx))
            if (!objSfx.isPlaying)
                objSfx.Play();
    }

    private void HandleSceneCompleted()
    {
        Debug.Log("Escenario terminado, abriendo libro...");

        sceneCompleted = true;
        closedBook.gameObject.SetActive(false);
        openBook.gameObject.SetActive(true);
    }
}