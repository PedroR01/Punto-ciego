using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class FirstPersonMovement : MonoBehaviour
{
    public static event Action OnJump;

    [SerializeField]
    private float speed = 5;

    [SerializeField]
    private GameObject playerParent;

    [SerializeField]
    private bool canJump = false;

    [SerializeField]
    private float jumpStrength = 2;

    private float defaul_jump_strength;

    private bool canRun = true;
    public bool IsGrounded { get; private set; }
    public bool IsSwiming { get; private set; }

    public bool IsRunning { get; private set; }

    [SerializeField]
    private float runSpeed = 9;

    private KeyCode runningKey = KeyCode.LeftShift;

    private Rigidbody rigidbody;

    private Transform cameraTransform;

    private void Awake()
    {
        if (!playerParent)
            playerParent = GameObject.Find("Player");

        rigidbody = GetComponent<Rigidbody>();
        rigidbody.freezeRotation = true;
        cameraTransform = Camera.main.transform;

        IsGrounded = false;
        defaul_jump_strength = jumpStrength;
    }

    private void OnEnable()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
    }

    private void OnDisable()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
    }

    private void Update()
    {
        IsRunning = canRun && Input.GetKey(runningKey);
    }

    private void FixedUpdate()
    {
        float targetMovingSpeed = IsRunning ? runSpeed : speed;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Dirección basada en la cámara
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        // Eliminar inclinación vertical
        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * vertical + horizontal * right) * targetMovingSpeed;

        rigidbody.velocity = new Vector3(
            moveDirection.x,
            rigidbody.velocity.y,
            moveDirection.z
        );
    }

    public void Jump()
    {
        if (canJump && IsGrounded)
        {
            IsGrounded = false;
            rigidbody.AddForce(Vector3.up * jumpStrength, ForceMode.Impulse);
            OnJump?.Invoke();
        }
    }

    public bool IsJumpEnabled => canJump;

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag.Equals("Floor"))
            IsGrounded = true;
        else if (other.gameObject.tag.Equals("Water"))
            IsSwiming = true;
    }

    private void OnCollisionStay(Collision other)
    {
        if (other.gameObject.tag.Equals("Floor"))
            IsGrounded = true;
        else if (other.gameObject.tag.Equals("Water"))
            IsSwiming = true;
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.tag.Equals("Water"))
            IsSwiming = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        IsGrounded = true;

        if (other.tag.Equals("Box"))
            jumpStrength = 6;
        else if (other.tag.Equals("Card"))
        {
            jumpStrength = 8;
            playerParent.transform.SetParent(other.transform, true);
        }
        else if (other.tag.Equals("Floor"))
            jumpStrength = 4;
    }

    private void OnTriggerExit(Collider other)
    {
        IsGrounded = false;
        jumpStrength = defaul_jump_strength;
        if (other.tag.Equals("Card"))
            playerParent.transform.SetParent(null);
    }
}