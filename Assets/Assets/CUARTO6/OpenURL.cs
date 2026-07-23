using UnityEngine;

public class OpenURL : MonoBehaviour
{
    [SerializeField]
    private string url = "https://unity.com";

    public void IrAWeb()
    {
        Application.OpenURL(url);
    }
}