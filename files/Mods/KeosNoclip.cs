using UnityEngine;
using easyInputs;
using System.Collections.Generic;

public class KeosNoclip : MonoBehaviour
{
    [Header("This script was made by Keo.cs")]
    [Header("You do not have to give credits")]
    public string ExcluteTag;
    public GameObject leftController;
    public GameObject rightController;
    public EasyHand hand;
    public List<Collider> additionalExemptColliders = new List<Collider>();

    private List<Collider> disabledColliders = new List<Collider>();
    private bool isNoclipActive = false;

    void Update()
    {
        if (EasyInputs.GetTriggerButtonTouched(hand))
        {
            if (!isNoclipActive)
            {
                Collider[] allColliders = FindObjectsOfType<Collider>();
                foreach (Collider col in allColliders)
                {
                    if (col.gameObject != leftController && col.gameObject != rightController && !col.CompareTag(ExcluteTag) && !additionalExemptColliders.Contains(col))
                    {
                        col.enabled = false;
                        disabledColliders.Add(col);
                    }
                }
                isNoclipActive = true;
            }
        }
        if (!EasyInputs.GetTriggerButtonTouched(hand))
        {
            if (isNoclipActive)
            {
                foreach (Collider col in disabledColliders)
                {
                    col.enabled = true;
                }
                disabledColliders.Clear();
                isNoclipActive = false;
            }
        }
    }
}
