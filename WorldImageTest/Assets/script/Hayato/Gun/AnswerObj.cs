using System;
using UdonSharp;
using Unity.VisualScripting;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AnswerObj : UdonSharpBehaviour
{
    [SerializeField] private GameObject entranceGate;

    private void Start()
    {
        entranceGate.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Bullet(Clone)")
        {
            entranceGate.SetActive(true);
        }
    }
}
