using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class PhotoTaker : UdonSharpBehaviour
{
    [SerializeField] private Camera worldCamera;

    // 初期値はライブビュー(true)
    [UdonSynced] private bool isCapturing = true;

    private void Start()
    {
        updateCameraState();
    }

    // ボタンに割り当てるメソッド
    public void ToggleCapturing()
    {
        // 既に撮影済み(false)なら何もしない
        if (!isCapturing) return;

        if (!Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        
        // 1. フラグを「撮影済み(false)」にする
        isCapturing = false;
        
        // 2. 他のプレイヤーへ同期
        RequestSerialization();

        // 3. 自分のカメラ状態を更新
        updateCameraState();
    }

    // (オプション) リセット用ボタンを作るならこれを割り当てる
    public void ResetCamera()
    {
        if (!isCapturing) // 停止中なら
        {
            if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);
            isCapturing = true; // 再開する
            RequestSerialization();
            updateCameraState();
        }
    }

    public override void OnDeserialization()
    {
        updateCameraState();
    }

    private void updateCameraState()
    {
        if (worldCamera != null)
        {
            worldCamera.gameObject.SetActive(isCapturing);
        }
    }
}
