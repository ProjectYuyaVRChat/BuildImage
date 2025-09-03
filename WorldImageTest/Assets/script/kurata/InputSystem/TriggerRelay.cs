using UdonSharp;
using UnityEngine;

public class TriggerRelay : UdonSharpBehaviour
{
    [SerializeField] private bool isOnce;
    private bool _isTriggered;

    public void Trigger()
    {
        if(isOnce && _isTriggered) return;
        _isTriggered = true;
        OnTrigger();
    }

    public virtual void OnTrigger()
    {
    }
}