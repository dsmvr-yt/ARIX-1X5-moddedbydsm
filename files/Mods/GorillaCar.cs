using UnityEngine;
using easyInputs;

// -----------------------------------------------------------------
// GorillaCar - OMG CAR VROOOOOOOOOOOOOOM
// -----------------------------------------------------------------
// MADE BY: The Kachow
// DO NOT STEAL! CREDIT ME!
// -----------------------------------------------------------------

public class GorillaCar : MonoBehaviour
{
    [Header("GorillaCar Properties")]
    [SerializeField]
    private Rigidbody GorillaPlayer;

    [Range(0f, 500f)]
    [SerializeField]
    private float VroomVroomSpeed = 10f;

    [SerializeField]
    private Transform CarDirection;

    void Update()
    {
        if (EasyInputs.GetTriggerButtonDown(EasyHand.RightHand))
        {
            Vector3 forceDirection = CarDirection.forward.normalized * VroomVroomSpeed;
            forceDirection.y = 0f;
            GorillaPlayer.AddForceAtPosition(forceDirection, CarDirection.position, ForceMode.Acceleration);
        }

        if (EasyInputs.GetTriggerButtonDown(EasyHand.LeftHand))
        {
            Vector3 forceDirection = -CarDirection.forward.normalized * VroomVroomSpeed;
            forceDirection.y = 0f;
            GorillaPlayer.AddForceAtPosition(forceDirection, CarDirection.position, ForceMode.Acceleration);
        }
    }
}
