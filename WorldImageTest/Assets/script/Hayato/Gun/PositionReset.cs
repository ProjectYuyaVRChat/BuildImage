using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDKBase;
using VRC.Udon;

public class PositionReset : UdonSharpBehaviour
{
    [SerializeField] private GameObject target;
    private Vector3 resetPosition;
    private Quaternion resetRotate;
    private Rigidbody targetRigdbody;
    public Animator button;

    private void Start()
    {
        if (target == null)
        {
            target = this.gameObject;
        }
        resetPosition = target.transform.position;
        resetRotate = target.transform.rotation;
        targetRigdbody = target.GetComponent<Rigidbody>();
    }

    public override void Interact()
    {
        if (!Networking.IsOwner(target))
        {
            Networking.SetOwner(Networking.LocalPlayer, target);
        }
        button.SetTrigger("Push");
        target.transform.SetPositionAndRotation(resetPosition,resetRotate);

        if (targetRigdbody != null)
        {
            targetRigdbody.velocity = Vector3.zero;
            targetRigdbody.angularVelocity = Vector3.zero;
        }
    }
}
