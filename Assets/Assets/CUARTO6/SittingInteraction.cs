using Cinemachine;
using UnityEngine;

public class SittingInteraction : MonoBehaviour
{
    private bool isSit = false;

    [SerializeField]
    private CinemachineVirtualCamera benchCamera;

    private void OnEnable()
    {
        CuartoDosGM.OnCorrectState += HandleSitEvent;
    }

    private void OnDisable()
    {
        CuartoDosGM.OnCorrectState -= HandleSitEvent;
    }

    private void HandleSitEvent(InteractionContext context)
    {
        // context.player.position = context.target.position;
        if (!isSit)
        {
            context.player.GetComponent<PlayerRaycastInteraction>().ChangeCamera(benchCamera);
            isSit = true;
        }
    }
}