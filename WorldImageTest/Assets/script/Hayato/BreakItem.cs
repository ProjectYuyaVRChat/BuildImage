using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class BreakItem : UdonSharpBehaviour
{
    public GameObject HummerHead;
    [Tooltip("同期先の球体")] 
    public GameObject[] m_SyncTargets;
    
    private void Start()
    {
        if (m_SyncTargets != null)
        {
            foreach (var target in m_SyncTargets)
            {
                if (target == null) continue;
                if (target.GetComponent<SyncObject>() != null)
                {
                    Debug.LogWarning("SyncObjectの同期先「{target.name}」にも同スクリプトが付いてるけど大丈夫そ？");
                    break;
                }
            }
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == HummerHead)
        {
            if (m_SyncTargets != null)
            {
                foreach (var target in m_SyncTargets)
                {
                    if (target == null) continue;
                    Destroy(gameObject);
                    Destroy(target);
                }
            }
        }
    }
}
