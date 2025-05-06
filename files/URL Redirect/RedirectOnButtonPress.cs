using UnityEngine;
using UnityEngine.SceneManagement;

public class RedirectOnButtonPress : MonoBehaviour
{
    [Header("THIS SCRIPT IS MADE BY SIMPLISTIC VR PLEASE DONT SAY ITS YOURS")]
    public string websiteURL; // The URL of the website you want to redirect to
    public GameObject buttonObject; // The object that, when pressed, will trigger the redirect

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == buttonObject)
        {
            Application.OpenURL(websiteURL);
        }
    }
}