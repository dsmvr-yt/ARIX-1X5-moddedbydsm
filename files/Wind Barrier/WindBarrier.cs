using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WindBarrier : MonoBehaviour
{
    [Header("Set layer to water and turn mesh renderer off")]
    public Rigidbody GorillaPlayer;
    [Header("If Y is negative it sends u down!")]
    public Vector3 Force = new Vector3(0, -40, 0);
    [Space]
    [Header("AudioStuff")]
    public AudioClip sound;
    public bool SpaceAudio;
    private AudioSource source;

    public void Start()
    {
        source = gameObject.AddComponent<AudioSource>();
        source.clip = sound;
        source.playOnAwake = false;

        if (SpaceAudio)
        {
            source.rolloffMode = AudioRolloffMode.Linear;
            source.spatialBlend = 1;
            source.maxDistance = 10;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody)
        {
            GorillaPlayer.AddForce(Force, ForceMode.Impulse);
            source.PlayOneShot(sound);
        }
    }
}
