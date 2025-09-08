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
    
    private bool isOn = false;

    private void Start()
    {
        targetBridgeF.SetActive(false);
        targetBridgeP.SetActive(false);
        count = timer;
    }

    private void Update()
    {
        if (isOn)
        {
            count -= Time.deltaTime;
            targetBridgeF.SetActive(true);
            targetBridgeP.SetActive(true);
        }

        if (count <= 0)
        {
            targetBridgeF.SetActive(false);
            targetBridgeP.SetActive(false);
        }
    }
    
    public override void Interact()
    {
        isOn = true;
    }
}
