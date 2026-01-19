using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

public class SELoop : UdonSharpBehaviour
{
    // Start is called before the first frame update
   private AudioSource audioSource;
   [SerializeField] private AudioClip audioClip;

   private void Start()
   {
       audioSource = GetComponent<AudioSource>();
       audioSource.PlayOneShot(audioClip);
   }
   
   
}
