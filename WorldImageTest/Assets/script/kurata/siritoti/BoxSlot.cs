using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class BoxSlot : UdonSharpBehaviour
{
    [Header("この箱に入れる正解オブジェクト")]
    public GameObject correctObject;

    [HideInInspector]
    public bool isCorrect = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == correctObject)
        {
            isCorrect = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == correctObject)
        {
            isCorrect = false;
        }
    }
}
