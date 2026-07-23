using UnityEngine;

public class MagicHatInteraction : MonoBehaviour
{
    [SerializeField]
    private BoxCollider tpTrigger;

    private void OnEnable()
    {
        CuartoDosGM.OnCorrectState += HandleTeleportation;
    }

    private void OnDisable()
    {
        CuartoDosGM.OnCorrectState -= HandleTeleportation;
    }

    private void HandleTeleportation(InteractionContext context)
    {
        if (context.target.tag.Equals("Hat"))
        {
            tpTrigger.enabled = true;
        }
    }
}