using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PSwitch : UdonSharpBehaviour
{
    [SerializeField] private GameObject targetBridgeF;
    [SerializeField] private GameObject targetBridgeP;
    [SerializeField] private float timer = 5f;
    private float count;
    
    [UdonSynced]private bool isOn = false;
    [UdonSynced]
    public bool isObjectActive = false;

    private void Start()
    {
        targetBridgeF.SetActive(isObjectActive);
        targetBridgeP.SetActive(isObjectActive);
        count = timer;
    }

    private void Update()
    {
        if (isOn)
        {
            count -= Time.deltaTime;
            isObjectActive = true;
            RequestSerialization();
            targetBridgeF.SetActive(isObjectActive);
            targetBridgeP.SetActive(isObjectActive);
        }

        if (count <= 0)
        {
            isObjectActive = false;
            isOn = false;
            RequestSerialization();
            targetBridgeF.SetActive(isObjectActive);
            targetBridgeP.SetActive(isObjectActive);
            count = timer;
        }
    }
    
    public override void Interact()
    {
        isOn = true;
        isObjectActive = true;
        RequestSerialization();
    }
}
