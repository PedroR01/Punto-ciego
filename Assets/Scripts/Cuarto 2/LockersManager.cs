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

    public void OnLockerInteracted(InteractionContext actionTarget)
    {
        // Verificar si es el locker correcto
        if (lockers[currentLockerIndex].locker != actionTarget.target)
        {
            Debug.Log("Locker incorrecto.");
            return;
        }

        Debug.Log($"Locker correcto: {actionTarget.target}");
        OpenLocker(actionTarget.target);

        // Encender las luces del siguiente
        if (currentLockerIndex < lockers.Length - 1)
        {
            currentLockerIndex++;
            foreach (var light in lockers[currentLockerIndex].lights)
            {
                Debug.Log($"Encendiendo la luz - {light} - de {lockers[currentLockerIndex].locker}");
                light.enabled = true;
            }
        }
        else
            Debug.Log("Todos los lockers han sido abiertos");
    }

    private void OpenLocker(Transform lockerToOpen)
    {
        lockerToOpen.GetComponent<Animator>().SetTrigger("Open");
    }
}