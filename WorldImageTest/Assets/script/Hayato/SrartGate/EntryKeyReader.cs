using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class EntryKeyReader : UdonSharpBehaviour
{
    [SerializeField] private int openID;
    [SerializeField] private Animator gate;
    private AudioSource audioSource;
    [SerializeField] private AudioClip open;
    [SerializeField] private AudioClip scan;

    [UdonSynced(UdonSyncMode.None)] private bool isGateOpen;
    

    private void Start()
    {
        if (open != null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        var card = other.GetComponent<CardKey>();
        if (card == null) return;

        int id = card.keyID;
        audioSource.PlayOneShot(scan);

        if (openID == id)
        {
            Networking.SetOwner(Networking.LocalPlayer,gameObject);
            UpdateGate();
        }
    }

    public override void OnDeserialization()
    {
        UpdateGate();
    }

    public void UpdateGate()
    {
        if (!isGateOpen)
        {
            if (open != null)
            {
                audioSource.PlayOneShot(open);
            }
            isGateOpen = true;
            RequestSerialization();
        }
        gate.SetTrigger("Open");
    }
}
