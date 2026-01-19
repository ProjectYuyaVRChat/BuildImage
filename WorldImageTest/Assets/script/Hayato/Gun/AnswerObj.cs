using System;
using UdonSharp;
using Unity.VisualScripting;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AnswerObj : UdonSharpBehaviour
{
    [SerializeField] private GameObject entranceGate;
    [SerializeField] private GimmickManager gimmickManager;
    private bool isCleared = false;
    [SerializeField] private Animator anime;
    private AudioSource audioSource;
    [SerializeField] private AudioClip audioClip;

    private void Start()
    {
        entranceGate.SetActive(false);
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Bullet(Clone)")
        {
            if (entranceGate != null)
            {
                entranceGate.SetActive(true);
            }
            if (!isCleared)
            {
                if (gimmickManager != null)
                {
                    gimmickManager.ReportClear();
                }
                anime.SetTrigger("Break");
                audioSource.PlayOneShot(audioClip);
                isCleared = true;
                SendCustomEventDelayedSeconds(nameof(DeleteItem), 0.5f);
            }
        }
    }

    public void DeleteItem()
    {
        Destroy(gameObject);
    }
}
