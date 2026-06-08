using UnityEngine;

public class LockerAnimation : MonoBehaviour
{
    [Header("Luces del locker")]
    [SerializeField]
    private Light[] lockerLights;

    private void Start()
    {
        SetLights(false);
    }

    private void OnEnable()
    {
        CuartoDosGM.OnCorrectState += OpenLocker;
    }

    private void OpenLocker(InteractionContext lockerToOpen)
    {
#warning Estos esta hardcodeado, tranquilamente el llamado del animator podria efectuarlo el mismo manager que controla las luces...

        lockerToOpen.target.GetComponent<Animator>().SetTrigger("Open");
    }

    public void SetLights(bool state)
    {
        foreach (Light light in lockerLights)
        {
            light.enabled = state;
        }
    }
}