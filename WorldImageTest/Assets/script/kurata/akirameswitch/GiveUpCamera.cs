
using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class GiveUpCamera : UdonSharpBehaviour
{
    [Header("有効化したいオブジェクト")]
    public GameObject targetObject;

    public override void Interact()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(true);
        }
    }

}
