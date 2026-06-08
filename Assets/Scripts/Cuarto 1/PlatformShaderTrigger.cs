using UnityEngine;

public class PlatformShaderTrigger : MonoBehaviour
{
    [Header("Glow Settings")]
    [SerializeField] private float fillDuration = .5f;

    private Material materialInstance;
    private float fillAmount = 1f;
    private bool playerOnTop = false;

    private void Awake()
    {
        materialInstance = GetComponent<Renderer>().material;
        materialInstance.SetFloat("_FillAmount", 1f);
    }

    private void Update()
    {
        float target = playerOnTop ? 0f : 1f;
        float speed = Time.deltaTime / fillDuration;

        fillAmount = Mathf.MoveTowards(fillAmount, target, speed);
        materialInstance.SetFloat("_FillAmount", fillAmount);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            playerOnTop = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            playerOnTop = false;
    }
}