using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

public class SyncPhotoCapture : UdonSharpBehaviour
{
    [Tooltip("撮影用のカメラを指定")]
    public Camera photoCamera;

    void Start()
    {
        if (photoCamera != null) photoCamera.gameObject.SetActive(false);
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(StartCapture));
    }
    

    // 全員のPCで実行されるメソッド（publicである必要があります）
    public void StartCapture()
    {
        if (photoCamera == null) return;

        // 1. カメラをオンにする
        photoCamera.gameObject.SetActive(true);

        // 2. 1フレーム後にカメラをオフにする処理を予約
        SendCustomEventDelayedFrames(nameof(StopCamera), 1);
    }

    public void StopCamera()
    {
        // 3. カメラをオフにする
        photoCamera.gameObject.SetActive(false);
    }
}