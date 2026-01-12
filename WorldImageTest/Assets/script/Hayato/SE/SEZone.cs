using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

public class SEZone : UdonSharpBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private AudioClip sound;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        audioSource.PlayOneShot(sound);
    }
}
