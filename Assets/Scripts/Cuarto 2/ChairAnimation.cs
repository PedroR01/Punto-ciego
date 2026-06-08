using System.Collections;
using UnityEngine;

public class ChairAnimation : MonoBehaviour
{
    [SerializeField]
    private Transform anchorTransform;

    private void OnEnable()
    {
        CuartoDosGM.OnIncorrectState += MoveToAnchor;
    }

    private void OnDisable()
    {
        CuartoDosGM.OnIncorrectState -= MoveToAnchor;
    }

    public void MoveToAnchor()
    {
        if (anchorTransform.gameObject.activeSelf)
        {
            StartCoroutine(MoveRoutine());
            anchorTransform.gameObject.SetActive(false);
        }
    }

    private IEnumerator MoveRoutine()
    {
        float duration = .3f;
        float time = 0;

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        while (time < duration)
        {
            float t = time / duration;

            transform.position = Vector3.Lerp(startPos, anchorTransform.position, t);
            transform.rotation = Quaternion.Slerp(startRot, anchorTransform.rotation, t);

            time += Time.deltaTime;
            yield return null;
        }

        transform.position = anchorTransform.position;
        transform.rotation = anchorTransform.rotation;
    }
}