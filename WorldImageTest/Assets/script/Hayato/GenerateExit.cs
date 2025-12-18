using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

public class GenerateExit : UdonSharpBehaviour
{
    [SerializeField] private GameObject teleportGate;
    [SerializeField] private AudioClip spawnKeySE;
    public Animator key;
    public Animator button;

    private AudioSource _audioSource;

    private void Start()
    {
        teleportGate.SetActive(false);
        _audioSource = GetComponent<AudioSource>();
    }

    public override void Interact()
    {
        teleportGate.SetActive(true);
        _audioSource.PlayOneShot(spawnKeySE);
        key.SetTrigger("Open");
        button.SetTrigger("Push");
    }
}
