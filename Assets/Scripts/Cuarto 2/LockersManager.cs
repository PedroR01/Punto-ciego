using UnityEngine;

[System.Serializable]
internal struct Locker
{
    public Transform locker;
    public Light[] lights;
}

public class LockersManager : MonoBehaviour
{
    [Header("Orden de lockers")]
    [SerializeField]
    private Locker[] lockers;

    private int currentLockerIndex = 0;

    private void OnEnable()
    {
        CuartoDosGM.OnCorrectState += OnLockerInteracted;
    }

    private void OnDisable()
    {
        CuartoDosGM.OnCorrectState -= OnLockerInteracted;
    }

    public void OnLockerInteracted(InteractionContext actionTarget)
    {
        OpenLocker(actionTarget.target);

        // Encender las luces del siguiente
        if (currentLockerIndex < lockers.Length - 1)
        {
            lockers[currentLockerIndex].locker.GetComponent<BoxCollider>().enabled = false;
            currentLockerIndex++;
            lockers[currentLockerIndex].locker.GetComponent<BoxCollider>().enabled = true;
            foreach (var light in lockers[currentLockerIndex].lights)
                light.enabled = true;
        }
        else
            Debug.Log("Todos los lockers han sido abiertos");
    }

    private void OpenLocker(Transform lockerToOpen)
    {
        lockerToOpen.GetComponent<Animator>().SetTrigger("Open");
    }
}