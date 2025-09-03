using UnityEngine;

public class TargetDeactivateRelay : TriggerRelay
{
    [SerializeField] private GameObject[] targets;

    public override void OnTrigger()
    {
        foreach (var target in targets)
        {
            if(target.activeSelf) target.SetActive(false);
        }
    }
}