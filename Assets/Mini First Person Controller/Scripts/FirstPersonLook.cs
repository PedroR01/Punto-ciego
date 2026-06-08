using Cinemachine;
using UnityEngine;

public class FirstPersonLook : MonoBehaviour
{
    [SerializeField]
    private Transform characterOrientation;

    public CinemachineVirtualCamera characterVC;
    public CinemachineComponentBase characterCB;

    [SerializeField]
    private float sensitivity = 2.3f;

    [SerializeField]
    private float smoothing = 1.5f;

    private float xRotation;
    private float yRotation;

    private void Start()
    {
        // Lock the mouse cursor to the game screen.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        yRotation += mouseX;
        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        Debug.Log(characterVC.GetCinemachineComponent(CinemachineCore.Stage.Aim));
        //Debug.Log(characterCB.);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
        characterOrientation.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }
}