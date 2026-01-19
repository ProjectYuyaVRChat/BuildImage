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

    private void Start()
    {
        entranceGate.SetActive(false);
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
