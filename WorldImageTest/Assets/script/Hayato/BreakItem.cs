using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
public class BreakItem : UdonSharpBehaviour
{
    [SerializeField] private GameObject nextRoom;

    private void Start()
    {
        nextRoom.SetActive(false);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "HummerHead")
        {
            nextRoom.SetActive(true);
            Destroy(gameObject);
        }
    }
}
