
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class DoorController : UdonSharpBehaviour
{
    [Header("ドア設定")]
    [Tooltip("開くドアオブジェクト")]
    public GameObject door;
    
    [Header("デバッグ設定")]
    public bool showDebugInfo = true;
    
    public void OnPasswordSuccess()
    {
        if (door != null)
        {
            door.SetActive(false);
            
            if (showDebugInfo)
            {
                Debug.Log($"[DoorController] ドア '{door.name}' を開きました。");
            }
        }
        else
        {
            Debug.LogError("[DoorController] Doorオブジェクトが設定されていません！");
        }
    }
    
    // ドアを閉じるメソッド（必要に応じて）
    public void CloseDoor()
    {
        if (door != null)
        {
            door.SetActive(true);
            
            if (showDebugInfo)
            {
                Debug.Log($"[DoorController] ドア '{door.name}' を閉じました。");
            }
        }
    }
}
