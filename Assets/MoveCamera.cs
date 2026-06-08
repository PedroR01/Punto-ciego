using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    [SerializeField]
    private Transform cameraPosition;

    [SerializeField]
    private Vector3 offsetPosition;

    private void Update()
    {
        transform.localPosition = cameraPosition.position - offsetPosition;
    }
}