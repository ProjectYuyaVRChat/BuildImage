using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class PhotoTaker : UdonSharpBehaviour
{
    [SerializeField] private Camera worldCamera;

    [UdonSynced]private bool isCapturing = true;
    [SerializeField]private TextMeshProUGUI debugText;

    public void ToggleCapturing()
    {
        if (!Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        
        isCapturing = !isCapturing;
        
        RequestSerialization();
        updateCameraState();
    }

    public override void OnDeserialization()
    {
        updateCameraState();
    }

    private void updateCameraState()
    {
        worldCamera.gameObject.SetActive(isCapturing);
        RequestSerialization();

        if (isCapturing)
        {
            Debug.Log("PhotoTaker: ライブキャプチャを再開");
            debugText.text = "PhotoTaker: ライブキャプチャを再開";
        }
        else
        {
            Debug.Log("PhatoTaker: 静止画として固定");
            debugText.text = "PhatoTaker: 静止画として固定";
        }
    }
}
