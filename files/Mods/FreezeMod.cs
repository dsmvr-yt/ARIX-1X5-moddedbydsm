using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using easyInputs;
using Photon.VR;

public class FreezeMod : MonoBehaviour
{
    // Start is called before the first frame update
    public PhotonVRManager MyManager;
    public EasyHand HandChoice;
    private bool IsOn = false;
        
    

    // Update is called once per frame
    void Update()
    {
        if (EasyInputs.GetSecondaryButtonDown(HandChoice) && (IsOn == false))
        {
            MyManager.LocalPlayer.enabled = false;
            IsOn = true;
        }
            else
            {
                if(EasyInputs.GetSecondaryButtonDown(HandChoice)&&(IsOn == true))
                {
                MyManager.LocalPlayer.enabled = true;
                IsOn = false;
            }
            }
        }
        
    }

