using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class EntryKeyReader : UdonSharpBehaviour
{
    [SerializeField] private int openID;
    [SerializeField] private Animator gate;

    [UdonSynced(UdonSyncMode.None)] private bool isGateOpen;
    
    private void OnTriggerEnter(Collider other)
    {
        var card = other.GetComponent<CardKey>();
        if (card == null) return;

        int id = card.keyID;

        if (openID == id)
        {
            Networking.SetOwner(Networking.LocalPlayer,gameObject);
            isGateOpen = true;
            RequestSerialization();
            UpdateGate();
        }
    }

    public override void OnDeserialization()
    {
        UpdateGate();
    }

    public void UpdateGate()
    {
        gate.SetTrigger("Open");
    }
}
