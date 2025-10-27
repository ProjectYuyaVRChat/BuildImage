using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class PhotoTaker : UdonSharpBehaviour
{
    [SerializeField]private Camera worldCamera;

    [UdonSynced]private bool isCapturing = true;
    /*[SerializeField]private TextMeshProUGUI debugText;
    [SerializeField]private TextMeshProUGUI debugText2;
    [SerializeField]private TextMeshProUGUI debugText3;
    private float counter = 0;*/

    private void Start()
    {
        updateCameraState();
        if (Networking.IsOwner(gameObject))
        {
            RequestSerialization();
        }
    }
    public void ToggleCapturing()
    {
        if (!Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        
        updateCameraState();
        isCapturing = !isCapturing;
        RequestSerialization();
    }

    public override void OnDeserialization()
    {
        /*counter++;
        if (debugText2 != null)
        {
            debugText2.text = ($"PhotoTaker: OnDeserializationを受信しました。({counter}) isCapturingは {isCapturing} です。");
        }*/
        updateCameraState();
    }

    private void updateCameraState()
    {
        /*if (worldCamera == null)
        {
            debugText3.text = ("PhotoTaker: worldCameraがインスペクタで設定されていません。");
            return;
        }*/
        
        worldCamera.gameObject.SetActive(isCapturing);

        if (isCapturing)
        {
            Debug.Log("PhotoTaker: ライブキャプチャを再開");
            /*debugText.text = "PhotoTaker: ライブキャプチャを再開";*/
        }
        else
        {
            Debug.Log("PhotoTaker: 静止画として固定");
            /*debugText.text = "PhotoTaker: 静止画として固定";*/
        }
    }
}
