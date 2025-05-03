using UnityEngine;
using UnityEngine.UI;

public class LongArms : MonoBehaviour
{
    [Header("This script was made by Keo.cs")]
    [Header("You do not have to give credits")]
    public GameObject objectToResize; 
    public Vector3 newSize = new Vector3(1f, 1f, 1f);

    private void Start()
    {
       
        if (objectToResize == null)
        {
            Debug.LogError("Please assign an object to resize in the inspector.");
        }
        else
        {
            
            Button button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(ResizeObject);
            }
            else
            {
                Debug.LogError("No Button component found on the GameObject.");
            }
        }
    }

   
    public void ResizeObject()
    {
        if (objectToResize != null)
        {
            objectToResize.transform.localScale = newSize; 
        }
        else
        {
            Debug.LogError("No object to resize assigned.");
        }
    }
}
