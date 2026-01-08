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
    public Animator button;
    
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
            targetBridgeF.SetActive(isObjectActive);
            targetBridgeP.SetActive(isObjectActive);
            RequestSerialization();
        }

        if (count <= 0)
        {
            isObjectActive = false;
            isOn = false;
            targetBridgeF.SetActive(isObjectActive);
            targetBridgeP.SetActive(isObjectActive);
            RequestSerialization();
            count = timer;
        }
    }
    
    public override void Interact()
    {
        if (!Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        button.SetTrigger("Push");
        isOn = true;
        isObjectActive = true;
        RequestSerialization();
    }
    
    public override void OnDeserialization()
    {
        // 同期されたisObjectActiveの値を使ってオブジェクトの状態を更新する
        targetBridgeF.SetActive(isObjectActive);
        targetBridgeP.SetActive(isObjectActive);
    }
}
