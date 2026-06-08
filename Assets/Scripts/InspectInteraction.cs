using UnityEngine;

public class InspectInteraction : MonoBehaviour
{
    private bool inspectioning = false;
    private Transform objectInspect;

    [SerializeField]
    private Vector3 offset = Vector3.zero;

    private void OnEnable()
    {
        CuartoDosGM.OnCorrectState += inspectObject;
    }

    private void Update()
    {
        if (inspectioning)
            objectInspect.position = Camera.main.transform.position - offset;
    }

    private void inspectObject(InteractionContext objectToInspect)
    {
        objectInspect = objectToInspect.target;
        inspectioning = true;
    }
}