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
    public KeyCase keyCase;

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
        keyCase.isMove = true;
    }
}
