using easyInputs;
using GorillaLocomotion;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrowMod : MonoBehaviour
{
    [Header("Keo.CS Made this")]
    [Header("No need for creds")]
    [Header("Have fun :)")]
    public EasyHand Grow;
    public EasyHand Shrink;
    public Rigidbody GorillaPlayer;
    public float MaxSize;
    public float MinSize;
    public float Speed;

    [Header("Reset Size")]
    public EasyHand ResetHand;
    public bool PrimaryButton;

    [Header("Debug / Test")]
    public bool TestGrow;
    public bool TestShrink;
    public bool ResetSize;

    private GameObject PlayerModel;
    private bool IsMaxReached;
    private bool IsMinReached;
    private Vector3 NormalPlayerSize;
    private Player PlayerScriptLOL;
    //I hope thats how you spell lengh but idk for sure
    private float NormalArmLengh;

    private void Start()
    {
        NormalPlayerSize = GorillaPlayer.transform.localScale;
        PlayerScriptLOL = GorillaPlayer.GetComponent<Player>();
        NormalArmLengh = PlayerScriptLOL.maxArmLength;
    }

    private void Update()
    {
        if (PlayerModel == null)
        {
            PlayerModel = GameObject.Find("Player(Clone)");
        }

        if (EasyInputs.GetTriggerButtonDown(Grow) && !IsMaxReached || TestGrow && !IsMaxReached)
        {
            Vector3 newScale = GorillaPlayer.transform.localScale + Vector3.one * Speed * Time.deltaTime;
            PlayerScriptLOL.maxArmLength += 1 * Speed * Time.deltaTime;
            GorillaPlayer.transform.localScale = newScale;
            PlayerModel.transform.localScale = newScale;
            IsReached(true, false);
        }
        else if (EasyInputs.GetTriggerButtonDown(Shrink) && !IsMinReached || TestShrink && !IsMinReached)
        {
            Vector3 newScale = GorillaPlayer.transform.localScale - Vector3.one * Speed * Time.deltaTime;
            PlayerScriptLOL.maxArmLength -= 1 * Speed * Time.deltaTime;
            GorillaPlayer.transform.localScale = newScale;
            PlayerModel.transform.localScale = newScale;
            IsReached(false, true);
        }

        if (PrimaryButton && EasyInputs.GetPrimaryButtonDown(ResetHand) || !PrimaryButton && EasyInputs.GetSecondaryButtonDown(ResetHand) || ResetSize)
        {
            GorillaPlayer.transform.localScale = NormalPlayerSize;
            PlayerScriptLOL.maxArmLength = NormalArmLengh;
            PlayerModel.transform.localScale = NormalPlayerSize;
        }
        IsReached(true, true);
    }

    public void IsReached(bool Max, bool Min)
    {
        if (Max)
        {
            if (GorillaPlayer.transform.localScale.x >= MaxSize)
            {
                IsMaxReached = true;
            }
            else
            {
                IsMaxReached = false;
            }
        }
        if (Min)
        {
            if (GorillaPlayer.transform.localScale.x <= MinSize)
            {
                IsMinReached = true;
            }
            else
            {
                IsMinReached = false;
            }
        }
    }
}
